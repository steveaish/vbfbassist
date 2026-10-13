import { useState } from 'react';
import { Alert, Button, Card, CardContent, TextField } from '@mui/material';
import { api, ApiErrorMessage } from '../utils/ApiUtils';

export default function ChangePassword() {
  const [form, setForm] = useState({ currentpassword: '', newpassword: '', confirm: '' });
  const [message, setMessage] = useState(null);
  const [saving, setSaving] = useState(false);

  const update = (field) => (e) => setForm({ ...form, [field]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (form.newpassword !== form.confirm) {
      setMessage({ severity: 'error', text: 'The new passwords do not match.' });
      return;
    }
    setSaving(true);
    try {
      await api.post('/login/changepassword', { currentpassword: form.currentpassword, newpassword: form.newpassword });
      setMessage({ severity: 'success', text: 'Your password has been changed.' });
      setForm({ currentpassword: '', newpassword: '', confirm: '' });
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
    setSaving(false);
  };

  return (
    <div className="vbfb-page">
      <h3 className="vbfb-page-title">Change Password</h3>
      <Card sx={{ maxWidth: 480 }}>
        <CardContent>
          <form onSubmit={handleSubmit}>
            {message && <Alert severity={message.severity} sx={{ mb: 2 }}>{message.text}</Alert>}
            <TextField label="Current password" type="password" autoComplete="current-password" fullWidth margin="normal" value={form.currentpassword} onChange={update('currentpassword')} />
            <TextField label="New password" type="password" autoComplete="new-password" fullWidth margin="normal" value={form.newpassword} onChange={update('newpassword')} helperText="At least 6 characters" />
            <TextField label="Confirm new password" type="password" autoComplete="new-password" fullWidth margin="normal" value={form.confirm} onChange={update('confirm')} />
            <Button type="submit" variant="contained" sx={{ mt: 2 }} disabled={saving || !form.currentpassword || !form.newpassword}>
              Change password
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
