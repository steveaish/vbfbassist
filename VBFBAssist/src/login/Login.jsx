import { useState } from 'react';
import {
  Alert, Box, Button, Card, CardContent, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle,
  IconButton, InputAdornment, Link, TextField, Typography
} from '@mui/material';
import { Email, Lock, LoginRounded, Visibility, VisibilityOff } from '@mui/icons-material';
import { api, ApiErrorMessage, SaveSession } from '../utils/ApiUtils';
import logo from '../images/logo.png';

export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [forgot, setForgot] = useState(null); // null closed, { email, result }

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);
    try {
      const response = await api.post('/login', { email, password });
      SaveSession(response.data);
      window.location.replace('/');
    }
    catch (err) {
      setError(ApiErrorMessage(err, 'Unable to reach the server. Please try again.'));
      setIsLoading(false);
    }
  };

  const sendReset = async () => {
    try {
      const r = await api.post('/login/forgotpassword', { email: forgot.email });
      setForgot({ ...forgot, result: { severity: 'success', text: r.data.message } });
    }
    catch (err) {
      setForgot({ ...forgot, result: { severity: 'error', text: ApiErrorMessage(err) } });
    }
  };

  return (
    <Box sx={{
      minHeight: '100vh',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      p: 2,
      background: 'linear-gradient(135deg, #12654A 0%, #0B4A35 55%, #F61814 55.2%, #C8110E 100%)'
    }}>
      <Card sx={{ width: '100%', maxWidth: 420, borderRadius: 3, boxShadow: 6 }}>
        <CardContent sx={{ p: { xs: 3, sm: 4 } }}>
          <Box sx={{ textAlign: 'center', mb: 3 }}>
            <img src={logo} alt="Bush Fire Service W.A." style={{ height: 110, width: 'auto' }} />
            <Typography variant="h5" sx={{ mt: 1, color: 'primary.dark', fontWeight: 500 }}>
              VBFB Assist
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Bedfordale Volunteer Bush Fire Brigade
            </Typography>
          </Box>

          <form onSubmit={handleSubmit} noValidate>
            {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

            <TextField
              label="Email"
              type="email"
              autoComplete="username"
              fullWidth
              margin="normal"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              autoFocus
              InputProps={{
                startAdornment: <InputAdornment position="start"><Email color="action" /></InputAdornment>
              }}
            />

            <TextField
              label="Password"
              type={showPassword ? 'text' : 'password'}
              autoComplete="current-password"
              fullWidth
              margin="normal"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              InputProps={{
                startAdornment: <InputAdornment position="start"><Lock color="action" /></InputAdornment>,
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton aria-label={showPassword ? 'Hide password' : 'Show password'} onClick={() => setShowPassword(!showPassword)} edge="end">
                      {showPassword ? <VisibilityOff /> : <Visibility />}
                    </IconButton>
                  </InputAdornment>
                )
              }}
            />

            <Button
              type="submit"
              variant="contained"
              size="large"
              fullWidth
              disabled={isLoading || !email || !password}
              startIcon={isLoading ? <CircularProgress size={20} color="inherit" /> : <LoginRounded />}
              sx={{ mt: 3, py: 1.5 }}
            >
              {isLoading ? 'Signing in...' : 'Sign in'}
            </Button>
            <Box sx={{ textAlign: 'center', mt: 2 }}>
              <Link component="button" type="button" variant="body2" onClick={() => setForgot({ email, result: null })}>
                Forgot your password?
              </Link>
            </Box>
          </form>
        </CardContent>
      </Card>

      <Dialog open={!!forgot} onClose={() => setForgot(null)} fullWidth maxWidth="xs">
        <DialogTitle>Reset your password</DialogTitle>
        <DialogContent>
          {forgot?.result ? (
            <Alert severity={forgot.result.severity}>{forgot.result.text}</Alert>
          ) : (
            <>
              <Typography variant="body2" sx={{ mb: 1 }}>Enter the email you sign in with and we will email you a link to choose a new password.</Typography>
              <TextField autoFocus fullWidth type="email" label="Email" value={forgot?.email ?? ''}
                onChange={(e) => setForgot({ ...forgot, email: e.target.value })} />
            </>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setForgot(null)}>{forgot?.result ? 'Close' : 'Cancel'}</Button>
          {!forgot?.result && <Button variant="contained" onClick={sendReset} disabled={!forgot?.email}>Send link</Button>}
        </DialogActions>
      </Dialog>
    </Box>
  );
}
