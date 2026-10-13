import { useCallback, useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import { useNavigate } from 'react-router-dom';
import {
  Alert, Box, Button, Checkbox, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle,
  IconButton, List, ListItemButton, ListItemText, MenuItem, Paper, Stack, TextField, Tooltip, Typography
} from '@mui/material';
import { Add, ArrowBack, ArrowDownward, ArrowUpward, Delete, Save } from '@mui/icons-material';
import { api, ApiErrorMessage } from '../utils/ApiUtils';
import { ConfirmDialog } from '../components/Modals';

// Settings > Vehicles (Laravel /veh/veh): vehicles and each vehicle's ordered checklist
export default function VehicleSetup() {
  const navigate = useNavigate();
  const [vehicles, setVehicles] = useState([]);
  const [editId, setEditId] = useState(null);
  const [adding, setAdding] = useState(null);
  const [message, setMessage] = useState(null);

  const load = useCallback(() => {
    api.get('/vehiclesetup/vehicles').then((r) => setVehicles(r.data)).catch((err) => setMessage({ severity: 'error', text: ApiErrorMessage(err) }));
  }, []);

  useEffect(() => { api.post('/vehiclesetup/opened').catch(() => {}); load(); }, [load]);

  const add = async () => {
    try {
      const r = await api.post('/vehiclesetup/vehicles', adding);
      setAdding(null);
      setEditId(r.data.id);
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  if (editId !== null) return <VehicleChecklist id={editId} onClose={() => { setEditId(null); load(); }} />;

  return (
    <div className="vbfb-page">
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
        <Button startIcon={<ArrowBack />} onClick={() => navigate('/settings')}>Settings</Button>
        <h3 className="vbfb-page-title" style={{ margin: 0, flexGrow: 1 }}>Vehicles</h3>
        <Button variant="contained" startIcon={<Add />} onClick={() => setAdding({ Name: '', NumberPlate: '' })}>Add</Button>
      </Stack>
      {message && <Alert severity={message.severity} sx={{ mb: 2 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      <Paper sx={{ maxWidth: 900 }}>
        <List disablePadding>
          {vehicles.map((v) => (
            <ListItemButton key={v.id} divider onClick={() => setEditId(v.id)}>
              <ListItemText primary={v.Name} secondary={v.NumberPlate} />
            </ListItemButton>
          ))}
        </List>
      </Paper>

      <Dialog open={!!adding} onClose={() => setAdding(null)} fullWidth maxWidth="sm">
        <DialogTitle>Add Vehicle</DialogTitle>
        <DialogContent>
          <TextField autoFocus fullWidth margin="dense" label="Vehicle name" value={adding?.Name ?? ''} onChange={(e) => setAdding({ ...adding, Name: e.target.value })} />
          <TextField fullWidth margin="dense" label="Number plate / details" value={adding?.NumberPlate ?? ''} onChange={(e) => setAdding({ ...adding, NumberPlate: e.target.value })} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAdding(null)}>Cancel</Button>
          <Button variant="contained" onClick={add}>Add</Button>
        </DialogActions>
      </Dialog>
    </div>
  );
}

// One vehicle: name/plate, then sections in order with their items and weekly/monthly flags
function VehicleChecklist({ id, onClose }) {
  const [data, setData] = useState(null);
  const [form, setForm] = useState({ Name: '', NumberPlate: '' });
  const [masters, setMasters] = useState({ sections: [], items: [] });
  const [picker, setPicker] = useState(null); // { kind: 'section' } | { kind: 'item', sectionid }
  const [pickValue, setPickValue] = useState('');
  const [confirm, setConfirm] = useState(null);
  const [message, setMessage] = useState(null);

  const load = useCallback(async () => {
    try {
      const [v, s, i] = await Promise.all([api.get(`/vehiclesetup/vehicles/${id}`), api.get('/vehiclesetup/sections'), api.get('/vehiclesetup/items')]);
      setData(v.data);
      setForm({ Name: v.data.vehicle.Name ?? '', NumberPlate: v.data.vehicle.NumberPlate ?? '' });
      setMasters({ sections: s.data, items: i.data });
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  }, [id]);

  useEffect(() => { load(); }, [load]);

  // Run a change then reload the checklist
  const act = async (fn) => {
    try {
      await fn();
      const v = await api.get(`/vehiclesetup/vehicles/${id}`);
      setData(v.data);
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  const saveVehicle = () => act(async () => {
    await api.put(`/vehiclesetup/vehicles/${id}`, form);
    setMessage({ severity: 'success', text: 'Vehicle saved.' });
  });

  const addPicked = () => {
    const p = picker;
    setPicker(null);
    act(() => p.kind === 'section'
      ? api.post(`/vehiclesetup/vehicles/${id}/sections`, { sectionid: pickValue })
      : api.post(`/vehiclesetup/vehicles/${id}/items`, { sectionid: p.sectionid, itemid: pickValue }));
    setPickValue('');
  };

  const deleteVehicle = async () => {
    setConfirm(null);
    try {
      await api.delete(`/vehiclesetup/vehicles/${id}`);
      onClose();
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  if (!data) return <div className="vbfb-page">{message ? <Alert severity="error">{message.text}</Alert> : <CircularProgress />}</div>;

  const usedSections = data.sections.map((s) => s.sectionid);
  const pickerOptions = picker?.kind === 'section'
    ? masters.sections.filter((s) => !usedSections.includes(s.id))
    : masters.items;

  const moveButtons = (url) => (
    <>
      <Tooltip title="Move up"><IconButton size="small" onClick={() => act(() => api.post(url, { direction: 'up' }))}><ArrowUpward fontSize="small" /></IconButton></Tooltip>
      <Tooltip title="Move down"><IconButton size="small" onClick={() => act(() => api.post(url, { direction: 'down' }))}><ArrowDownward fontSize="small" /></IconButton></Tooltip>
    </>
  );

  return (
    <div className="vbfb-page">
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
        <Button startIcon={<ArrowBack />} onClick={onClose}>Vehicles</Button>
        <h3 className="vbfb-page-title" style={{ margin: 0 }}>{data.vehicle.Name}</h3>
      </Stack>
      {message && <Alert severity={message.severity} sx={{ mb: 2 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      <Paper sx={{ p: 2, mb: 2, maxWidth: 900 }}>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
          <TextField size="small" fullWidth label="Vehicle name" value={form.Name} onChange={(e) => setForm({ ...form, Name: e.target.value })} />
          <TextField size="small" fullWidth label="Number plate / details" value={form.NumberPlate} onChange={(e) => setForm({ ...form, NumberPlate: e.target.value })} />
          <Button variant="contained" startIcon={<Save />} onClick={saveVehicle}>Save</Button>
        </Stack>
      </Paper>

      <Stack direction="row" alignItems="center" sx={{ maxWidth: 900, mb: 1 }}>
        <Typography sx={{ fontWeight: 500, flexGrow: 1 }}>Checklist</Typography>
        <Button startIcon={<Add />} onClick={() => setPicker({ kind: 'section' })}>Add Section</Button>
      </Stack>

      <Stack spacing={1.5} sx={{ maxWidth: 900 }}>
        {data.sections.map((s) => (
          <Paper key={s.id} variant="outlined">
            <Stack direction="row" alignItems="center" sx={{ px: 1.5, py: 1, bgcolor: 'var(--vbfb-green-light)' }}>
              <Typography sx={{ fontWeight: 500, flexGrow: 1 }}>{s.Name}</Typography>
              {moveButtons(`/vehiclesetup/vehiclesections/${s.id}/move`)}
              <Tooltip title="Remove section from this vehicle">
                <IconButton size="small" onClick={() => setConfirm({ message: `Remove ${s.Name} from this vehicle?`, onConfirm: () => { setConfirm(null); act(() => api.delete(`/vehiclesetup/vehiclesections/${s.id}`)); } })}>
                  <Delete fontSize="small" />
                </IconButton>
              </Tooltip>
            </Stack>
            <Box sx={{ px: 1.5, pb: 1 }}>
              <Stack direction="row" sx={{ py: 0.5, color: 'text.secondary', fontSize: '0.8rem' }}>
                <Box sx={{ flexGrow: 1 }}>Item</Box>
                <Box sx={{ width: 64, textAlign: 'center' }}>Weekly</Box>
                <Box sx={{ width: 64, textAlign: 'center' }}>Monthly</Box>
                <Box sx={{ width: 108 }} />
              </Stack>
              {s.items.map((it) => (
                <Stack key={it.id} direction="row" alignItems="center" sx={{ borderTop: 1, borderColor: 'divider' }}>
                  <Typography variant="body2" sx={{ flexGrow: 1, py: 0.5 }}>{it.Name}</Typography>
                  <Box sx={{ width: 64, textAlign: 'center' }}>
                    <Checkbox size="small" checked={it.weekly === 'Y'} inputProps={{ 'aria-label': 'Weekly' }}
                      onChange={(e) => act(() => api.put(`/vehiclesetup/vehicleitems/${it.id}`, { weekly: e.target.checked ? 'Y' : 'N' }))} />
                  </Box>
                  <Box sx={{ width: 64, textAlign: 'center' }}>
                    <Checkbox size="small" checked={it.monthly === 'Y'} inputProps={{ 'aria-label': 'Monthly' }}
                      onChange={(e) => act(() => api.put(`/vehiclesetup/vehicleitems/${it.id}`, { monthly: e.target.checked ? 'Y' : 'N' }))} />
                  </Box>
                  <Box sx={{ width: 108, textAlign: 'right' }}>
                    {moveButtons(`/vehiclesetup/vehicleitems/${it.id}/move`)}
                    <IconButton size="small" aria-label="Remove item" onClick={() => act(() => api.delete(`/vehiclesetup/vehicleitems/${it.id}`))}><Delete fontSize="small" /></IconButton>
                  </Box>
                </Stack>
              ))}
              <Button size="small" startIcon={<Add />} onClick={() => setPicker({ kind: 'item', sectionid: s.sectionid })}>Add Item</Button>
            </Box>
          </Paper>
        ))}
      </Stack>

      <Button color="error" startIcon={<Delete />} sx={{ mt: 3 }}
        onClick={() => setConfirm({ message: `Delete ${data.vehicle.Name}? Past checks are kept.`, onConfirm: deleteVehicle })}>
        Delete Vehicle
      </Button>

      <Dialog open={!!picker} onClose={() => setPicker(null)} fullWidth maxWidth="sm">
        <DialogTitle>{picker?.kind === 'section' ? 'Add Section' : 'Add Item'}</DialogTitle>
        <DialogContent>
          <TextField select fullWidth margin="dense" label={picker?.kind === 'section' ? 'Section' : 'Item'} value={pickValue} onChange={(e) => setPickValue(e.target.value)}>
            {pickerOptions.map((o) => <MenuItem key={o.id} value={o.id}>{o.Name}</MenuItem>)}
          </TextField>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            Not in the list? Add it under Settings &gt; Vehicle {picker?.kind === 'section' ? 'Sections' : 'Items'} first.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPicker(null)}>Cancel</Button>
          <Button variant="contained" disabled={!pickValue} onClick={addPicked}>Add</Button>
        </DialogActions>
      </Dialog>

      <ConfirmDialog open={!!confirm} title="Please confirm" message={confirm?.message} danger confirmText="Delete"
        onConfirm={() => confirm?.onConfirm()} onClose={() => setConfirm(null)} />
    </div>
  );
}

VehicleChecklist.propTypes = {
  id: PropTypes.number.isRequired,
  onClose: PropTypes.func.isRequired
};
