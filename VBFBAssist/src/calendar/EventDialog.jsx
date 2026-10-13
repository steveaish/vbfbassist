import { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import {
  Alert, Box, Button, Checkbox, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle,
  FormControlLabel, Grid, MenuItem, TextField, Typography
} from '@mui/material';
import { Delete } from '@mui/icons-material';
import dayjs from 'dayjs';
import { api, ApiErrorMessage } from '../utils/ApiUtils';
import { useIsMobile } from '../utils/GridUtils';
import { eventColours, textOn } from './calendarUtils';

// View / add / edit one calendar event. Non-editors get a read-only view.
export default function EventDialog({ id, date, canEdit, onClose }) {
  const isMobile = useIsMobile();
  const isNew = !id;
  const [form, setForm] = useState(isNew
    ? { date, start: '19:00', end: '', description: '', longdesc: '', colour: '#ffffff', repeat: 'None', every: 1, count: 4 }
    : null);
  const [seriesCount, setSeriesCount] = useState(1);
  const [applyToFollowing, setApplyToFollowing] = useState(false);
  const [confirmDelete, setConfirmDelete] = useState(false);
  const [error, setError] = useState(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (isNew) return;
    api.get(`/calendar/${id}`)
      .then((r) => {
        setForm({ ...r.data.item, start: r.data.item.start ?? '', end: r.data.item.end ?? '', longdesc: r.data.item.longdesc ?? '' });
        setSeriesCount(r.data.seriesCount);
      })
      .catch((err) => setError(ApiErrorMessage(err)));
  }, [id, isNew]);

  const set = (field) => (e) => setForm({ ...form, [field]: e.target.value });

  const save = async () => {
    setSaving(true);
    setError(null);
    try {
      if (isNew) await api.post('/calendar', form);
      else await api.put(`/calendar/${id}`, { ...form, applytofollowing: applyToFollowing });
      onClose(true);
    }
    catch (err) {
      setError(ApiErrorMessage(err));
      setSaving(false);
    }
  };

  const remove = async (series) => {
    try {
      await api.delete(`/calendar/${id}`, { params: { series: series ? 'Y' : 'N' } });
      onClose(true);
    }
    catch (err) {
      setError(ApiErrorMessage(err));
    }
  };

  const readOnly = !canEdit;
  const title = isNew ? 'Add Event' : readOnly ? (form?.description ?? '') : 'Edit Event';

  return (
    <Dialog open onClose={() => onClose(false)} fullWidth maxWidth="sm" fullScreen={isMobile}>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        {!form ? <CircularProgress /> : readOnly ? (
          <Box>
            <Typography sx={{ fontWeight: 500 }}>{dayjs(form.date).format('dddd D MMMM YYYY')}</Typography>
            {form.start && <Typography color="text.secondary">{form.start}{form.end ? ` – ${form.end}` : ''}</Typography>}
            {form.longdesc && <Typography sx={{ mt: 2, whiteSpace: 'pre-wrap' }}>{form.longdesc}</Typography>}
          </Box>
        ) : (
          <>
            <Grid container columnSpacing={2}>
              <Grid item xs={12}><TextField fullWidth margin="dense" label="Label" value={form.description} onChange={set('description')} autoFocus={isNew} /></Grid>
              <Grid item xs={12} sm={4}><TextField fullWidth margin="dense" type="date" label="Date" InputLabelProps={{ shrink: true }} value={form.date} onChange={set('date')} /></Grid>
              <Grid item xs={6} sm={4}><TextField fullWidth margin="dense" type="time" label="Start time" InputLabelProps={{ shrink: true }} value={form.start} onChange={set('start')} /></Grid>
              <Grid item xs={6} sm={4}><TextField fullWidth margin="dense" type="time" label="End time" InputLabelProps={{ shrink: true }} value={form.end} onChange={set('end')} /></Grid>
              <Grid item xs={12}><TextField fullWidth margin="dense" multiline minRows={3} label="Description" value={form.longdesc} onChange={set('longdesc')} /></Grid>
            </Grid>

            <Typography variant="body2" color="text.secondary" sx={{ mt: 1, mb: 0.5 }}>Colour</Typography>
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, alignItems: 'center' }}>
              {eventColours.map((c) => (
                <Box key={c} component="button" type="button" aria-label={`Colour ${c}`} onClick={() => setForm({ ...form, colour: c })}
                  sx={{
                    width: 32, height: 32, borderRadius: '50%', bgcolor: c, cursor: 'pointer', border: 2,
                    borderColor: form.colour === c ? 'primary.main' : 'divider', outline: form.colour === c ? '2px solid' : 'none', outlineColor: 'primary.main'
                  }} />
              ))}
              <TextField type="color" size="small" value={form.colour} onChange={set('colour')} sx={{ width: 64 }} inputProps={{ 'aria-label': 'Custom colour' }} />
              <Box sx={{ px: 1, py: 0.5, borderRadius: 1, bgcolor: form.colour, color: textOn(form.colour), border: 1, borderColor: 'divider' }}>
                {form.description || 'Preview'}
              </Box>
            </Box>

            {isNew ? (
              <Grid container columnSpacing={2} sx={{ mt: 1 }}>
                <Grid item xs={12} sm={4}>
                  <TextField select fullWidth margin="dense" label="Repeat" value={form.repeat} onChange={set('repeat')}>
                    <MenuItem value="None">Does not repeat</MenuItem>
                    <MenuItem value="Day">Every x days</MenuItem>
                    <MenuItem value="Week">Every x weeks</MenuItem>
                    <MenuItem value="Month">Every x months</MenuItem>
                  </TextField>
                </Grid>
                {form.repeat !== 'None' && (
                  <>
                    <Grid item xs={6} sm={4}><TextField fullWidth margin="dense" type="number" label="Every" inputProps={{ min: 1 }} value={form.every} onChange={set('every')} /></Grid>
                    <Grid item xs={6} sm={4}><TextField fullWidth margin="dense" type="number" label="More times" inputProps={{ min: 1, max: 104 }} value={form.count} onChange={set('count')} helperText="after this one" /></Grid>
                  </>
                )}
              </Grid>
            ) : seriesCount > 1 && (
              <FormControlLabel sx={{ mt: 1 }}
                control={<Checkbox checked={applyToFollowing} onChange={(e) => setApplyToFollowing(e.target.checked)} />}
                label="Also apply the time, colour and text to the later events in this series" />
            )}

            {confirmDelete && (
              <Alert severity="warning" sx={{ mt: 2 }} action={
                <>
                  <Button color="inherit" size="small" onClick={() => remove(false)}>This one</Button>
                  {seriesCount > 1 && <Button color="inherit" size="small" onClick={() => remove(true)}>All {seriesCount}</Button>}
                </>
              }>
                Delete {seriesCount > 1 ? 'this event or the whole series' : 'this event'}?
              </Alert>
            )}
          </>
        )}
      </DialogContent>
      <DialogActions>
        {!isNew && canEdit && !confirmDelete && <Button color="error" startIcon={<Delete />} onClick={() => setConfirmDelete(true)} sx={{ mr: 'auto' }}>Delete</Button>}
        <Button onClick={() => onClose(false)}>{readOnly ? 'Close' : 'Cancel'}</Button>
        {!readOnly && <Button variant="contained" onClick={save} disabled={saving || !form?.description?.trim()}>Save</Button>}
      </DialogActions>
    </Dialog>
  );
}

EventDialog.propTypes = {
  id: PropTypes.number,
  date: PropTypes.string,
  canEdit: PropTypes.bool,
  onClose: PropTypes.func.isRequired
};
