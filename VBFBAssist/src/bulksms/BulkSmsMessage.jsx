import { useCallback, useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import {
  Alert, Box, Button, Checkbox, CircularProgress, Dialog, DialogActions, DialogContent, DialogContentText,
  DialogTitle, FormControlLabel, Grid, IconButton, MenuItem, Paper, Stack, TextField, Typography
} from '@mui/material';
import { ArrowBack, Delete, PersonAdd, Save, Send } from '@mui/icons-material';
import { api, ApiErrorMessage } from '../utils/ApiUtils';
import { formatDate } from '../utils/GridUtils';
import { ConfirmDialog } from '../components/Modals';
import SmsLength from './SmsLength';

const filterLabels = {
  FirstLine: '1st Line Firefighters',
  SecondLine: '2nd Line Firefighters',
  President: 'President',
  Captain: 'Captain',
  Lieutenant: 'Lieutenants',
  OtherOfficer: 'Other Officers',
  Cadet: 'Cadets',
  CadetLeader: 'Cadet Leaders',
  LifeMember: 'Life Members',
  AuxMember: 'Auxiliary Members'
};

// One message: edit the text, choose recipients, send. Sent messages are read only.
export default function BulkSmsMessage({ id, onClose }) {
  const [data, setData] = useState(null);
  const [text, setText] = useState('');
  const [filter, setFilter] = useState('');
  const [selected, setSelected] = useState([]);
  const [confirm, setConfirm] = useState(null);
  const [sending, setSending] = useState(false);
  const [message, setMessage] = useState(null);

  const load = useCallback(() => {
    api.get(`/bulksms/${id}`, { params: { filter: filter || undefined } })
      .then((r) => { setData(r.data); setText(r.data.message.message ?? ''); setSelected([]); })
      .catch((err) => setMessage({ severity: 'error', text: ApiErrorMessage(err) }));
  }, [id, filter]);

  useEffect(() => { load(); }, [load]);

  const act = async (fn, success) => {
    try {
      const r = await fn();
      if (success) setMessage({ severity: 'success', text: typeof success === 'function' ? success(r) : success });
      load();
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  const send = async () => {
    setConfirm(null);
    setSending(true);
    try {
      const r = await api.post(`/bulksms/${id}/send`);
      setMessage({ severity: 'success', text: `Sent to ${r.data.sent} recipient(s).` });
      load();
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
    setSending(false);
  };

  if (!data) return <div className="vbfb-page">{message ? <Alert severity="error">{message.text}</Alert> : <CircularProgress />}</div>;

  const sent = data.message.sent === 'Y';
  const toggle = (pid) => setSelected(selected.includes(pid) ? selected.filter((x) => x !== pid) : [...selected, pid]);

  return (
    <div className="vbfb-page">
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
        <Button startIcon={<ArrowBack />} onClick={onClose}>Messages</Button>
        <h3 className="vbfb-page-title" style={{ margin: 0, flexGrow: 1 }}>{sent ? `Sent ${formatDate(data.message.senttime)}` : 'Draft SMS'}</h3>
      </Stack>
      {message && <Alert severity={message.severity} sx={{ mb: 2 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      <Paper sx={{ p: 2, mb: 2 }}>
        <TextField fullWidth multiline minRows={3} label="Message" value={text} onChange={(e) => setText(e.target.value)} disabled={sent}
          helperText={<SmsLength text={text} />} />
        {!sent && (
          <Stack direction="row" spacing={1} sx={{ mt: 1 }} flexWrap="wrap" useFlexGap>
            <Button startIcon={<Save />} disabled={text === data.message.message || !text.trim()}
              onClick={() => act(() => api.put(`/bulksms/${id}`, { message: text }), 'Message saved.')}>Save text</Button>
            <Button variant="contained" startIcon={sending ? <CircularProgress size={18} color="inherit" /> : <Send />}
              disabled={sending || data.recipients.length === 0 || text !== data.message.message}
              onClick={() => setConfirm('send')}>
              Send to {data.recipients.length}
            </Button>
            <Box sx={{ flexGrow: 1 }} />
            <Button color="error" startIcon={<Delete />} onClick={() => setConfirm('delete')}>Delete</Button>
          </Stack>
        )}
        {!sent && text !== data.message.message && <Typography variant="caption" color="warning.main">Save the text before sending.</Typography>}
      </Paper>

      <Grid container spacing={2}>
        <Grid item xs={12} md={sent ? 12 : 6}>
          <Paper sx={{ p: 2 }}>
            <Typography sx={{ fontWeight: 500, mb: 1 }}>Recipients ({data.recipients.length})</Typography>
            {data.recipients.length === 0 && <Typography variant="body2" color="text.secondary">Nobody yet - add members from the list.</Typography>}
            {data.recipients.map((r) => (
              <Stack key={r.seqno} direction="row" alignItems="center" sx={{ borderBottom: 1, borderColor: 'divider', py: 0.25 }}>
                <Typography sx={{ flexGrow: 1 }}>{r.firstname} {r.lastname}</Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mr: 1 }}>{r.mobilephone}</Typography>
                {!sent && (
                  <IconButton size="small" aria-label={`Remove ${r.firstname}`} onClick={() => act(() => api.delete(`/bulksms/recipients/${r.seqno}`))}>
                    <Delete fontSize="small" />
                  </IconButton>
                )}
              </Stack>
            ))}
          </Paper>
        </Grid>

        {!sent && (
          <Grid item xs={12} md={6}>
            <Paper sx={{ p: 2 }}>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} alignItems={{ sm: 'center' }} sx={{ mb: 1 }}>
                <Typography sx={{ fontWeight: 500, flexGrow: 1 }}>Add members ({data.available.length})</Typography>
                <TextField select size="small" label="Membership" value={filter} onChange={(e) => setFilter(e.target.value)} sx={{ minWidth: 200 }}>
                  <MenuItem value="">All active members</MenuItem>
                  {data.filters.map((f) => <MenuItem key={f} value={f}>{filterLabels[f] ?? f}</MenuItem>)}
                </TextField>
              </Stack>
              <Stack direction="row" spacing={1} sx={{ mb: 1 }}>
                <Button size="small" variant="contained" startIcon={<PersonAdd />} disabled={selected.length === 0}
                  onClick={() => act(() => api.post(`/bulksms/${id}/recipients`, { profileids: selected }), (r) => `Added ${r.data.added}.`)}>
                  Add selected ({selected.length})
                </Button>
                <Button size="small" disabled={data.available.length === 0}
                  onClick={() => act(() => api.post(`/bulksms/${id}/recipients`, { all: true, filter: filter || null }), (r) => `Added ${r.data.added}.`)}>
                  Add all {data.available.length}
                </Button>
              </Stack>
              <Box sx={{ maxHeight: 420, overflowY: 'auto' }}>
                {data.available.map((p) => (
                  <FormControlLabel key={p.id} sx={{ display: 'flex', mr: 0 }}
                    control={<Checkbox size="small" checked={selected.includes(p.id)} onChange={() => toggle(p.id)} />}
                    label={<><span>{p.firstname} {p.lastname}</span> <Typography component="span" variant="body2" color="text.secondary">{p.mobilephone}</Typography></>} />
                ))}
              </Box>
            </Paper>
          </Grid>
        )}
      </Grid>

      <Dialog open={confirm === 'send'} onClose={() => setConfirm(null)} maxWidth="xs" fullWidth>
        <DialogTitle>Send SMS?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            This sends the message to <b>{data.recipients.length}</b> mobile number(s) now. It can&apos;t be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirm(null)}>Cancel</Button>
          <Button variant="contained" onClick={send}>Send</Button>
        </DialogActions>
      </Dialog>

      <ConfirmDialog open={confirm === 'delete'} title="Delete message" message="Delete this draft and its recipient list?" danger confirmText="Delete"
        onConfirm={() => { setConfirm(null); act(() => api.delete(`/bulksms/${id}`)).then(onClose); }} onClose={() => setConfirm(null)} />
    </div>
  );
}

BulkSmsMessage.propTypes = {
  id: PropTypes.number.isRequired,
  onClose: PropTypes.func.isRequired
};
