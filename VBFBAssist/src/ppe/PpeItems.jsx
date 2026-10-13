import { useCallback, useEffect, useState } from 'react';
import {
  Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, Paper, Stack, Table, TableBody,
  TableCell, TableHead, TableRow, TextField
} from '@mui/material';
import { Add } from '@mui/icons-material';
import { api, ApiErrorMessage } from '../utils/ApiUtils';
import PpeItemEdit from './PpeItemEdit';

// PPE stock cupboard items (Laravel /ppe/itm)
export default function PpeItems() {
  const [items, setItems] = useState([]);
  const [editId, setEditId] = useState(null);
  const [adding, setAdding] = useState(null);
  const [message, setMessage] = useState(null);

  const load = useCallback(() => {
    api.get('/ppeitems').then((r) => setItems(r.data)).catch((err) => setMessage({ severity: 'error', text: ApiErrorMessage(err) }));
  }, []);

  useEffect(() => { api.post('/ppeitems/opened').catch(() => {}); load(); }, [load]);

  const add = async () => {
    try {
      const r = await api.post('/ppeitems', { Name: adding });
      setAdding(null);
      setEditId(r.data.id);
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  if (editId !== null) return <PpeItemEdit id={editId} onClose={() => { setEditId(null); load(); }} />;

  return (
    <div className="vbfb-page">
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
        <h3 className="vbfb-page-title" style={{ margin: 0 }}>PPE Items</h3>
        <Button variant="contained" startIcon={<Add />} onClick={() => setAdding('')}>Add Item</Button>
      </Stack>
      {message && <Alert severity={message.severity} sx={{ mb: 2 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      <Paper sx={{ maxWidth: 900 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Item</TableCell>
              <TableCell align="right">In Stock</TableCell>
              <TableCell align="right">Issued</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {items.map((i) => (
              <TableRow key={i.id} hover sx={{ cursor: 'pointer' }} onClick={() => setEditId(i.id)}>
                <TableCell>{i.Name}</TableCell>
                <TableCell align="right">{i.InStock}</TableCell>
                <TableCell align="right">{i.Issued}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Paper>

      <Dialog open={adding !== null} onClose={() => setAdding(null)} fullWidth maxWidth="xs">
        <DialogTitle>Add PPE Item</DialogTitle>
        <DialogContent>
          <TextField autoFocus fullWidth margin="dense" label="Item name" value={adding ?? ''} onChange={(e) => setAdding(e.target.value)} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAdding(null)}>Cancel</Button>
          <Button variant="contained" onClick={add} disabled={!adding?.trim()}>Add</Button>
        </DialogActions>
      </Dialog>
    </div>
  );
}
