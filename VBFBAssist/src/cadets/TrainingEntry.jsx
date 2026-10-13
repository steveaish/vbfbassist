import { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import {
  Alert, Box, Button, Checkbox, CircularProgress, FormControlLabel, Grid, MenuItem, Paper, Stack, TextField, Typography
} from '@mui/material';
import { ArrowBack, Save } from '@mui/icons-material';
import dayjs from 'dayjs';
import { api, ApiErrorMessage } from '../utils/ApiUtils';

// "Record Training Event" (Laravel cadets/trainingadd): one training item for several cadets at once
export default function TrainingEntry({ onClose }) {
  const [lists, setLists] = useState(null);
  const [form, setForm] = useState({ thedate: dayjs().format('YYYY-MM-DD'), trainingid: '', unitleader: '', hours: '' });
  const [selected, setSelected] = useState([]);
  const [message, setMessage] = useState(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    api.get('/cadets/trainingentry')
      .then((r) => {
        setLists(r.data);
        // Default the unit leader to the person recording, when they are one
        if (r.data.unitleaders.some((u) => u.id === r.data.myprofileid)) {
          setForm((f) => ({ ...f, unitleader: r.data.myprofileid }));
        }
      })
      .catch((err) => setMessage({ severity: 'error', text: ApiErrorMessage(err) }));
  }, []);

  const toggle = (id) => setSelected(selected.includes(id) ? selected.filter((x) => x !== id) : [...selected, id]);

  const save = async () => {
    setSaving(true);
    try {
      const r = await api.post('/cadets/training', {
        ...form,
        hours: form.hours === '' ? null : Number(form.hours),
        unitleader: form.unitleader === '' ? null : form.unitleader,
        cadets: selected
      });
      setMessage({ severity: 'success', text: `Training recorded for ${r.data.added} cadet(s).` });
      setSelected([]);
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
    setSaving(false);
  };

  if (!lists) return <div className="vbfb-page">{message ? <Alert severity="error">{message.text}</Alert> : <CircularProgress />}</div>;

  const set = (field) => (e) => setForm({ ...form, [field]: e.target.value });

  return (
    <div className="vbfb-page">
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
        <Button startIcon={<ArrowBack />} onClick={onClose}>Back</Button>
        <h3 className="vbfb-page-title" style={{ margin: 0 }}>Record Training Event</h3>
      </Stack>

      {message && <Alert severity={message.severity} sx={{ mb: 2 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      <Paper sx={{ p: { xs: 2, sm: 3 }, mb: 2 }}>
        <Grid container columnSpacing={2}>
          <Grid item xs={6} sm={3}>
            <TextField label="Date" type="date" fullWidth size="small" margin="dense" value={form.thedate} onChange={set('thedate')} InputLabelProps={{ shrink: true }} />
          </Grid>
          <Grid item xs={6} sm={2}>
            <TextField label="Hours" type="number" inputProps={{ step: 0.5, min: 0 }} fullWidth size="small" margin="dense" value={form.hours} onChange={set('hours')} />
          </Grid>
          <Grid item xs={12} sm={7}>
            <TextField select label="Training completed" fullWidth size="small" margin="dense" value={form.trainingid} onChange={set('trainingid')}>
              {lists.training.map((t) => <MenuItem key={t.id} value={t.id}>{t.name}</MenuItem>)}
            </TextField>
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField select label="Unit leader" fullWidth size="small" margin="dense" value={form.unitleader} onChange={set('unitleader')}>
              <MenuItem value="">Not set</MenuItem>
              {lists.unitleaders.map((u) => <MenuItem key={u.id} value={u.id}>{u.name}</MenuItem>)}
            </TextField>
          </Grid>
        </Grid>
      </Paper>

      <Paper sx={{ p: { xs: 2, sm: 3 } }}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography sx={{ fontWeight: 500 }}>Cadets ({selected.length} selected)</Typography>
          <Button size="small" onClick={() => setSelected(selected.length === lists.cadets.length ? [] : lists.cadets.map((c) => c.id))}>
            {selected.length === lists.cadets.length ? 'Clear all' : 'Select all'}
          </Button>
        </Stack>
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', md: '1fr 1fr 1fr' } }}>
          {lists.cadets.map((c) => (
            <FormControlLabel key={c.id} control={<Checkbox checked={selected.includes(c.id)} onChange={() => toggle(c.id)} />} label={c.name} />
          ))}
        </Box>
      </Paper>

      <Stack direction="row" spacing={1} sx={{ mt: 2 }}>
        <Button variant="contained" startIcon={<Save />} onClick={save} disabled={saving || !form.trainingid || selected.length === 0}>Record</Button>
        <Button onClick={onClose}>Done</Button>
      </Stack>
    </div>
  );
}

TrainingEntry.propTypes = {
  onClose: PropTypes.func.isRequired
};
