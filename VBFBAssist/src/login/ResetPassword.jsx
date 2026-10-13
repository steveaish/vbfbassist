import { useState } from 'react';
import { Alert, Box, Button, Card, CardContent, TextField, Typography } from '@mui/material';
import { api, ApiErrorMessage } from '../utils/ApiUtils';
import logo from '../images/logo.png';

// /resetpassword?id=..&token=.. - opened from the emailed link, works without being signed in
export default function ResetPassword() {
  const params = new URLSearchParams(window.location.search);
  const id = Number(params.get('id'));
  const token = params.get('token') ?? '';

  const [password, setPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const [result, setResult] = useState(null);
  const [busy, setBusy] = useState(false);

  const submit = async (e) => {
    e.preventDefault();
    if (password !== confirm) {
      setResult({ severity: 'error', text: 'The passwords do not match.' });
      return;
    }
    setBusy(true);
    try {
      const r = await api.post('/login/resetpassword', { id, token, password });
      setResult({ severity: 'success', text: r.data.message, done: true });
    }
    catch (err) {
      setResult({ severity: 'error', text: ApiErrorMessage(err) });
    }
    setBusy(false);
  };

  return (
    <Box sx={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', p: 2, bgcolor: 'background.default' }}>
      <Card sx={{ width: '100%', maxWidth: 420, borderRadius: 3, boxShadow: 6 }}>
        <CardContent sx={{ p: { xs: 3, sm: 4 } }}>
          <Box sx={{ textAlign: 'center', mb: 2 }}>
            <img src={logo} alt="Bush Fire Service W.A." style={{ height: 80, width: 'auto' }} />
            <Typography variant="h6" sx={{ mt: 1, color: 'primary.dark' }}>Choose a new password</Typography>
          </Box>

          {result && <Alert severity={result.severity} sx={{ mb: 2 }}>{result.text}</Alert>}

          {result?.done ? (
            <Button variant="contained" fullWidth href="/">Sign in</Button>
          ) : !id || !token ? (
            <Alert severity="error">This link is incomplete. Please use the link from your email, or ask for a new one.</Alert>
          ) : (
            <form onSubmit={submit}>
              <TextField fullWidth margin="normal" type="password" label="New password" autoComplete="new-password" autoFocus
                value={password} onChange={(e) => setPassword(e.target.value)} helperText="At least 6 characters" />
              <TextField fullWidth margin="normal" type="password" label="Confirm new password" autoComplete="new-password"
                value={confirm} onChange={(e) => setConfirm(e.target.value)} />
              <Button type="submit" variant="contained" fullWidth size="large" sx={{ mt: 2 }} disabled={busy || password.length < 6 || !confirm}>
                Set password
              </Button>
            </form>
          )}
        </CardContent>
      </Card>
    </Box>
  );
}
