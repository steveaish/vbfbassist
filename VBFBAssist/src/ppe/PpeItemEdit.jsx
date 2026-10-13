import { useCallback, useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import {
  Alert, Box, Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle, IconButton,
  Paper, Stack, Tab, Table, TableBody, TableCell, TableHead, TableRow, Tabs, TextField
} from '@mui/material';
import { Add, ArrowBack, Delete, Edit, Save } from '@mui/icons-material';
import { api, ApiErrorMessage } from '../utils/ApiUtils';
import { formatDate } from '../utils/GridUtils';
import { ConfirmDialog } from '../components/Modals';

// One PPE item: rename, styles (sizes) with stocktake quantities, and movement history
export default function PpeItemEdit({ id, onClose }) {
  const [tab, setTab] = useState(0);
  const [item, setItem] = useState(null);
  const [name, setName] = useState('');
  const [styles, setStyles] = useState([]);
  const [counts, setCounts] = useState({}); // style id -> quantity being typed
  const [history, setHistory] = useState([]);
  const [styleDialog, setStyleDialog] = useState(null); // { id?, Name, quantity }
  const [confirm, setConfirm] = useState(null);
  const [message, setMessage] = useState(null);

  const load = useCallback(async () => {
    try {
      const [d, h] = await Promise.all([api.get(`/ppeitems/${id}`), api.get(`/ppeitems/${id}/history`)]);
      setItem(d.data.item);
      setName(d.data.item.Name ?? '');
      setStyles(d.data.styles);
      setCounts(Object.fromEntries(d.data.styles.map((s) => [s.id, String(s.quantity)])));
      setHistory(h.data);
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  }, [id]);

  useEffect(() => { load(); }, [load]);

  const act = async (fn, success) => {
    try {
      await fn();
      if (success) setMessage({ severity: 'success', text: success });
      load();
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  const saveStyle = () => {
    const s = styleDialog;
    setStyleDialog(null);
    act(() => s.id
      ? api.put(`/ppeitems/styles/${s.id}`, { Name: s.Name })
      : api.post(`/ppeitems/${id}/styles`, { Name: s.Name, quantity: Number(s.quantity) || 0 }));
  };

  if (!item) return <div className="vbfb-page">{message ? <Alert severity="error">{message.text}</Alert> : <CircularProgress />}</div>;

  const total = styles.reduce((sum, s) => sum + (s.quantity ?? 0), 0);

  return (
    <div className="vbfb-page">
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
        <Button startIcon={<ArrowBack />} onClick={onClose}>PPE Items</Button>
        <h3 className="vbfb-page-title" style={{ margin: 0 }}>{item.Name}</h3>
      </Stack>
      {message && <Alert severity={message.severity} sx={{ mb: 2 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      <Paper sx={{ p: 2, mb: 2, maxWidth: 900 }}>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
          <TextField size="small" fullWidth label="Item name" value={name} onChange={(e) => setName(e.target.value)} />
          <Button variant="contained" startIcon={<Save />} onClick={() => act(() => api.put(`/ppeitems/${id}`, { Name: name }), 'Item saved.')}>Save</Button>
        </Stack>
      </Paper>

      <Paper sx={{ mb: 2, maxWidth: 900 }}>
        <Tabs value={tab} onChange={(e, v) => setTab(v)}>
          <Tab label={`Styles (${total} in stock)`} />
          <Tab label="History" />
        </Tabs>
      </Paper>

      {tab === 0 && (
        <Paper sx={{ maxWidth: 900 }}>
          <Box sx={{ overflowX: 'auto' }}>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Style</TableCell>
                  <TableCell>In stock</TableCell>
                  <TableCell align="right">Issued</TableCell>
                  <TableCell />
                </TableRow>
              </TableHead>
              <TableBody>
                {styles.map((s) => {
                  const changed = counts[s.id] !== String(s.quantity);
                  return (
                    <TableRow key={s.id}>
                      <TableCell>{s.Name}</TableCell>
                      <TableCell sx={{ whiteSpace: 'nowrap' }}>
                        <TextField size="small" type="number" inputProps={{ min: 0, 'aria-label': `Quantity of ${s.Name}` }} sx={{ width: 90 }}
                          value={counts[s.id] ?? ''} onChange={(e) => setCounts({ ...counts, [s.id]: e.target.value })} />
                        {changed && (
                          <Button size="small" sx={{ ml: 1 }} variant="contained"
                            onClick={() => act(() => api.post(`/ppeitems/styles/${s.id}/quantity`, { quantity: Number(counts[s.id]) }), `${s.Name} stock updated.`)}>
                            Update
                          </Button>
                        )}
                      </TableCell>
                      <TableCell align="right">{s.Issued}</TableCell>
                      <TableCell align="right" sx={{ whiteSpace: 'nowrap' }}>
                        <IconButton size="small" aria-label="Rename style" onClick={() => setStyleDialog({ id: s.id, Name: s.Name })}><Edit fontSize="small" /></IconButton>
                        <IconButton size="small" aria-label="Delete style"
                          onClick={() => setConfirm({ message: `Delete style ${s.Name}?`, onConfirm: () => { setConfirm(null); act(() => api.delete(`/ppeitems/styles/${s.id}`)); } })}>
                          <Delete fontSize="small" />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </Box>
          <Button startIcon={<Add />} sx={{ m: 1 }} onClick={() => setStyleDialog({ Name: '', quantity: '0' })}>Add Style</Button>
        </Paper>
      )}

      {tab === 1 && (
        <Paper sx={{ maxWidth: 900 }}>
          <Box sx={{ overflowX: 'auto' }}>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Date</TableCell>
                  <TableCell>Style</TableCell>
                  <TableCell>From</TableCell>
                  <TableCell>To</TableCell>
                  <TableCell align="right">Qty</TableCell>
                  <TableCell>Notes</TableCell>
                  <TableCell>By</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {history.map((h) => (
                  <TableRow key={h.id}>
                    <TableCell>{formatDate(h.historydate)}</TableCell>
                    <TableCell>{h.StyleName}</TableCell>
                    <TableCell>{h.FromLocation}</TableCell>
                    <TableCell>{h.ToLocation}</TableCell>
                    <TableCell align="right">{h.Quantity}</TableCell>
                    <TableCell>{h.notes}</TableCell>
                    <TableCell>{h.Profile}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </Box>
        </Paper>
      )}

      <Button color="error" startIcon={<Delete />} sx={{ mt: 3 }}
        onClick={() => setConfirm({ message: `Delete ${item.Name}? Items already issued stay on members' profiles.`, onConfirm: () => { setConfirm(null); act(() => api.delete(`/ppeitems/${id}`)).then(onClose); } })}>
        Delete Item
      </Button>

      <Dialog open={!!styleDialog} onClose={() => setStyleDialog(null)} fullWidth maxWidth="xs">
        <DialogTitle>{styleDialog?.id ? 'Rename Style' : 'Add Style'}</DialogTitle>
        <DialogContent>
          <TextField autoFocus fullWidth margin="dense" label="Style / size" value={styleDialog?.Name ?? ''} onChange={(e) => setStyleDialog({ ...styleDialog, Name: e.target.value })} />
          {!styleDialog?.id && (
            <TextField fullWidth margin="dense" type="number" label="Quantity in stock" inputProps={{ min: 0 }}
              value={styleDialog?.quantity ?? ''} onChange={(e) => setStyleDialog({ ...styleDialog, quantity: e.target.value })} />
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setStyleDialog(null)}>Cancel</Button>
          <Button variant="contained" onClick={saveStyle} disabled={!styleDialog?.Name?.trim()}>Save</Button>
        </DialogActions>
      </Dialog>

      <ConfirmDialog open={!!confirm} title="Please confirm" message={confirm?.message} danger confirmText="Delete"
        onConfirm={() => confirm?.onConfirm()} onClose={() => setConfirm(null)} />
    </div>
  );
}

PpeItemEdit.propTypes = {
  id: PropTypes.number.isRequired,
  onClose: PropTypes.func.isRequired
};
