import { useCallback, useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import {
  Alert, Box, Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle, Grid, MenuItem,
  Paper, Stack, TextField, Typography
} from '@mui/material';
import { Add, ArrowBack, Call, Delete } from '@mui/icons-material';
import dayjs from 'dayjs';
import { api, ApiErrorMessage } from '../utils/ApiUtils';
import { formatDate, useIsMobile } from '../utils/GridUtils';

// One BFR street: logged activities and residents' phone numbers (Laravel bfr/activities)
export default function BfrStreet({ id, onClose }) {
  const isMobile = useIsMobile();
  const [data, setData] = useState(null);
  const [lookups, setLookups] = useState(null);
  const [form, setForm] = useState(null); // activity being added/edited
  const [confirmDelete, setConfirmDelete] = useState(false);
  const [message, setMessage] = useState(null);
  const [error, setError] = useState(null);

  const load = useCallback(() => {
    api.get(`/bfr/streets/${id}`).then((r) => setData(r.data)).catch((err) => setMessage({ severity: 'error', text: ApiErrorMessage(err) }));
  }, [id]);

  useEffect(() => {
    load();
    api.get('/bfr/lookups').then((r) => setLookups(r.data)).catch(() => {});
  }, [load]);

  const startAdd = () => {
    setError(null);
    setConfirmDelete(false);
    setForm({
      streetid: id, activityid: '', profileid: lookups?.members.some((m) => m.id === lookups.myprofileid) ? lookups.myprofileid : '',
      activitydate: dayjs().format('YYYY-MM-DD'), starttime: '', duration: 1, attendeed: '', notes: ''
    });
  };

  const startEdit = (a) => {
    setError(null);
    setConfirmDelete(false);
    setForm({
      id: a.id, streetid: a.streetid, activityid: a.activityid ?? '', profileid: a.profileid ?? '',
      activitydate: a.activitydate ? dayjs(a.activitydate).format('YYYY-MM-DD') : '', starttime: a.starttime ?? '',
      duration: a.duration ?? '', attendeed: a.attendeed ?? '', notes: a.notes ?? ''
    });
  };

  const save = async () => {
    const body = {
      ...form,
      activityid: form.activityid === '' ? -1 : form.activityid,
      profileid: form.profileid === '' ? null : form.profileid,
      duration: form.duration === '' ? 0 : Number(form.duration),
      attendeed: form.attendeed === '' ? null : Number(form.attendeed)
    };
    try {
      if (form.id) await api.put(`/bfr/activities/${form.id}`, body);
      else await api.post('/bfr/activities', body);
      setForm(null);
      load();
    }
    catch (err) {
      setError(ApiErrorMessage(err));
    }
  };

  const remove = async () => {
    try {
      await api.delete(`/bfr/activities/${form.id}`);
      setForm(null);
      load();
    }
    catch (err) {
      setError(ApiErrorMessage(err));
    }
  };

  if (!data) return <div className="vbfb-page">{message ? <Alert severity="error">{message.text}</Alert> : <CircularProgress />}</div>;

  const set = (field) => (e) => setForm({ ...form, [field]: e.target.value });

  return (
    <div className="vbfb-page">
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }} flexWrap="wrap" useFlexGap>
        <Button startIcon={<ArrowBack />} onClick={onClose}>Streets</Button>
        <Box sx={{ flexGrow: 1 }}>
          <Typography variant="h6" sx={{ color: 'primary.dark', lineHeight: 1.2 }}>{data.street.StreetName}</Typography>
          <Typography variant="body2" color="text.secondary">{data.street.AreaName}</Typography>
        </Box>
        <Button variant="contained" startIcon={<Add />} onClick={startAdd} disabled={!lookups}>Add Activity</Button>
      </Stack>

      {message && <Alert severity={message.severity} sx={{ mb: 2 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      <Grid container spacing={2}>
        <Grid item xs={12} md={8}>
          <Typography sx={{ fontWeight: 500, mb: 1 }}>Activities ({data.activities.length})</Typography>
          {data.activities.length === 0 && <Typography color="text.secondary">No activities logged for this street yet.</Typography>}
          <Stack spacing={1}>
            {data.activities.map((a) => (
              <Paper key={a.id} variant="outlined" sx={{ p: 1.5, cursor: 'pointer', '&:hover': { bgcolor: 'action.hover' } }} onClick={() => startEdit(a)}>
                <Stack direction="row" justifyContent="space-between" spacing={1}>
                  <Typography fontWeight={500}>{a.ActivityDesc ?? 'Not set'}</Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ whiteSpace: 'nowrap' }}>{formatDate(a.activitydate)}</Typography>
                </Stack>
                <Typography variant="body2" color="text.secondary">
                  {a.MemberName}
                  {a.starttime && <> · {a.starttime}</>}
                  {a.duration ? <> · {a.duration} hr</> : null}
                  {a.attendeed ? <> · {a.attendeed} attended</> : null}
                </Typography>
                {a.notes && <Typography variant="body2" sx={{ mt: 0.5, whiteSpace: 'pre-wrap' }}>{a.notes}</Typography>}
              </Paper>
            ))}
          </Stack>
        </Grid>

        <Grid item xs={12} md={4}>
          <Typography sx={{ fontWeight: 500, mb: 1 }}>Residents</Typography>
          <Paper variant="outlined" sx={{ p: 1.5 }}>
            {data.phonelist.length === 0 && <Typography variant="body2" color="text.secondary">No members live in this street.</Typography>}
            {data.phonelist.map((p) => (
              <Box key={p.id} sx={{ mb: 1 }}>
                <Typography>{p.firstname} {p.lastname}</Typography>
                <Stack direction="row" spacing={1}>
                  {p.mobilephone && <Button size="small" startIcon={<Call />} href={`tel:${p.mobilephone}`}>{p.mobilephone}</Button>}
                  {p.homephone && <Button size="small" startIcon={<Call />} href={`tel:${p.homephone}`}>{p.homephone}</Button>}
                </Stack>
              </Box>
            ))}
          </Paper>
        </Grid>
      </Grid>

      <Dialog open={!!form} onClose={() => setForm(null)} fullWidth maxWidth="sm" fullScreen={isMobile}>
        <DialogTitle>{form?.id ? 'Edit Activity' : 'Add Activity'}</DialogTitle>
        <DialogContent>
          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
          {form && lookups && (
            <Grid container columnSpacing={2}>
              <Grid item xs={12}>
                <TextField select fullWidth margin="dense" label="Street" value={form.streetid} onChange={set('streetid')}>
                  {lookups.streets.map((s) => <MenuItem key={s.id} value={s.id}>{s.AreaName} - {s.StreetName}</MenuItem>)}
                </TextField>
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField select fullWidth margin="dense" label="Activity" value={form.activityid} onChange={set('activityid')}>
                  <MenuItem value="">Not set</MenuItem>
                  {lookups.activitytypes.map((t) => <MenuItem key={t.id} value={t.id}>{t.ActivityDesc}</MenuItem>)}
                </TextField>
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField select fullWidth margin="dense" label="Member" value={form.profileid} onChange={set('profileid')}>
                  <MenuItem value="">Not set</MenuItem>
                  {lookups.members.map((m) => <MenuItem key={m.id} value={m.id}>{m.name}</MenuItem>)}
                </TextField>
              </Grid>
              <Grid item xs={12} sm={4}><TextField fullWidth margin="dense" type="date" label="Date" InputLabelProps={{ shrink: true }} value={form.activitydate} onChange={set('activitydate')} /></Grid>
              <Grid item xs={6} sm={3}><TextField fullWidth margin="dense" type="time" label="Start" InputLabelProps={{ shrink: true }} value={form.starttime} onChange={set('starttime')} /></Grid>
              <Grid item xs={6} sm={2}><TextField fullWidth margin="dense" type="number" label="Hours" inputProps={{ min: 0, step: 0.5 }} value={form.duration} onChange={set('duration')} /></Grid>
              <Grid item xs={12} sm={3}><TextField fullWidth margin="dense" type="number" label="Attended" inputProps={{ min: 0 }} value={form.attendeed} onChange={set('attendeed')} /></Grid>
              <Grid item xs={12}><TextField fullWidth margin="dense" multiline minRows={3} label="Notes" value={form.notes} onChange={set('notes')} /></Grid>
            </Grid>
          )}
          {confirmDelete && (
            <Alert severity="warning" sx={{ mt: 2 }} action={<Button color="inherit" size="small" onClick={remove}>Delete</Button>}>Delete this activity?</Alert>
          )}
        </DialogContent>
        <DialogActions>
          {form?.id && !confirmDelete && <Button color="error" startIcon={<Delete />} sx={{ mr: 'auto' }} onClick={() => setConfirmDelete(true)}>Delete</Button>}
          <Button onClick={() => setForm(null)}>Cancel</Button>
          <Button variant="contained" onClick={save} disabled={!form?.activitydate}>Save</Button>
        </DialogActions>
      </Dialog>
    </div>
  );
}

BfrStreet.propTypes = {
  id: PropTypes.number.isRequired,
  onClose: PropTypes.func.isRequired
};
