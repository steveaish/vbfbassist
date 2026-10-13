import { useCallback, useEffect, useState } from 'react';
import {
  Alert, Button, Card, CardActionArea, CardContent, Chip, Dialog, DialogActions, DialogContent,
  DialogTitle, Stack, TextField, Typography
} from '@mui/material';
import { Add } from '@mui/icons-material';
import { api, ApiErrorMessage } from '../utils/ApiUtils';
import { formatDate } from '../utils/GridUtils';
import BulkSmsMessage from './BulkSmsMessage';
import SmsLength from './SmsLength';

// Bulk SMS messages (Laravel /bulksms)
export default function BulkSms() {
  const [messages, setMessages] = useState([]);
  const [openId, setOpenId] = useState(null);
  const [creating, setCreating] = useState(null);
  const [message, setMessage] = useState(null);

  const load = useCallback(() => {
    api.get('/bulksms').then((r) => setMessages(r.data)).catch((err) => setMessage({ severity: 'error', text: ApiErrorMessage(err) }));
  }, []);

  useEffect(() => { api.post('/bulksms/opened').catch(() => {}); load(); }, [load]);

  const create = async () => {
    try {
      const r = await api.post('/bulksms', { message: creating });
      setCreating(null);
      setOpenId(r.data.id);
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  if (openId !== null) return <BulkSmsMessage id={openId} onClose={() => { setOpenId(null); load(); }} />;

  return (
    <div className="vbfb-page">
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
        <h3 className="vbfb-page-title" style={{ margin: 0 }}>Bulk SMS</h3>
        <Button variant="contained" startIcon={<Add />} onClick={() => setCreating('')}>New Message</Button>
      </Stack>
      {message && <Alert severity={message.severity} sx={{ mb: 2 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      <Stack spacing={1}>
        {messages.map((m) => (
          <Card key={m.seqno} variant="outlined">
            <CardActionArea onClick={() => setOpenId(m.seqno)}>
              <CardContent sx={{ py: 1.25, '&:last-child': { pb: 1.25 } }}>
                <Stack direction="row" spacing={1} alignItems="flex-start">
                  <Typography sx={{ flexGrow: 1, overflow: 'hidden', textOverflow: 'ellipsis', display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical' }}>
                    {m.message}
                  </Typography>
                  {m.sent === 'Y'
                    ? <Chip size="small" color="success" label={`Sent ${formatDate(m.senttime)}`} />
                    : <Chip size="small" label="Draft" />}
                </Stack>
                <Typography variant="caption" color="text.secondary">{m.Recipients} recipient(s)</Typography>
              </CardContent>
            </CardActionArea>
          </Card>
        ))}
      </Stack>

      <Dialog open={creating !== null} onClose={() => setCreating(null)} fullWidth maxWidth="sm">
        <DialogTitle>New SMS Message</DialogTitle>
        <DialogContent>
          <TextField autoFocus fullWidth multiline minRows={4} margin="dense" label="Message" value={creating ?? ''} onChange={(e) => setCreating(e.target.value)}
            helperText={<SmsLength text={creating ?? ''} />} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCreating(null)}>Cancel</Button>
          <Button variant="contained" onClick={create} disabled={!creating?.trim()}>Next: choose recipients</Button>
        </DialogActions>
      </Dialog>
    </div>
  );
}

