import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Alert, Autocomplete, Box, Button, Checkbox, Chip, Dialog, DialogActions, DialogContent, DialogTitle,
  FormControlLabel, InputAdornment, Paper, Stack, Switch, Tab, Tabs, TextField, Typography
} from '@mui/material';
import { Email, PersonAdd, Search } from '@mui/icons-material';
import { api, ApiErrorMessage } from '../utils/ApiUtils';
import { formatDate, useIsMobile } from '../utils/GridUtils';

// Admin > Users (Laravel /admin/users): logins, the profile each one maps to, and creating logins for members
export default function AdminUsers() {
  const isMobile = useIsMobile();
  const [users, setUsers] = useState([]);
  const [candidates, setCandidates] = useState([]);
  const [search, setSearch] = useState('');
  const [showInactive, setShowInactive] = useState(false);
  const [editing, setEditing] = useState(null);
  const [creating, setCreating] = useState(null);
  const [message, setMessage] = useState(null);
  const [error, setError] = useState(null);

  const load = useCallback(() => {
    Promise.all([api.get('/adminusers'), api.get('/adminusers/candidates')])
      .then(([u, c]) => { setUsers(u.data); setCandidates(c.data); })
      .catch((err) => setMessage({ severity: 'error', text: ApiErrorMessage(err) }));
  }, []);

  useEffect(() => { load(); }, [load]);

  const shown = useMemo(() => {
    const s = search.trim().toLowerCase();
    return users.filter((u) => (showInactive || u.status === 1) &&
      (!s || `${u.name} ${u.email} ${u.ProfileName ?? ''}`.toLowerCase().includes(s)));
  }, [users, search, showInactive]);

  const saveEdit = async () => {
    setError(null);
    try {
      await api.put(`/adminusers/${editing.id}`, editing);
      setEditing(null);
      setMessage({ severity: 'success', text: `${editing.name} has been updated.` });
      load();
    }
    catch (err) {
      setError(ApiErrorMessage(err));
    }
  };

  const sendLink = async (u) => {
    try {
      const r = await api.post(`/adminusers/${u.id}/sendlink`);
      setMessage({ severity: 'success', text: r.data.message });
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  const create = async () => {
    setError(null);
    try {
      const r = await api.post('/adminusers', creating);
      setCreating(null);
      setMessage(r.data.emailError
        ? { severity: 'warning', text: r.data.emailError }
        : { severity: 'success', text: `Login created for ${creating.name}${creating.sendlink ? ' and a set-password link emailed.' : '.'}` });
      load();
    }
    catch (err) {
      setError(ApiErrorMessage(err));
    }
  };

  const openCreate = () => {
    setError(null);
    setCreating({ mode: candidates.length ? 'profile' : 'manual', name: '', email: '', password: '', admin: false, sendlink: true });
  };

  return (
    <div className="vbfb-page">
      <Stack direction="row" justifyContent="space-between" alignItems="center" flexWrap="wrap" useFlexGap spacing={1} sx={{ mb: 2 }}>
        <h3 className="vbfb-page-title" style={{ margin: 0 }}>Users</h3>
        <Button variant="contained" startIcon={<PersonAdd />} onClick={openCreate}>New Login</Button>
      </Stack>

      {candidates.length > 0 && (
        <Alert severity="info" sx={{ mb: 2 }} action={<Button color="inherit" size="small" onClick={openCreate}>Create</Button>}>
          {candidates.length} active member(s) have an email on their profile but no login yet.
        </Alert>
      )}
      {message && <Alert severity={message.severity} sx={{ mb: 2 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      <Stack direction="row" spacing={2} alignItems="center" sx={{ mb: 2 }} flexWrap="wrap" useFlexGap>
        <TextField size="small" placeholder="Search name or email" value={search} onChange={(e) => setSearch(e.target.value)} sx={{ flex: '1 1 260px', maxWidth: 420 }}
          InputProps={{ startAdornment: <InputAdornment position="start"><Search /></InputAdornment> }} />
        <FormControlLabel control={<Checkbox checked={showInactive} onChange={(e) => setShowInactive(e.target.checked)} />} label="Show deactivated" />
      </Stack>

      <Stack spacing={1}>
        {shown.map((u) => (
          <Paper key={u.id} variant="outlined" sx={{ p: 1.5, cursor: 'pointer', '&:hover': { bgcolor: 'action.hover' }, opacity: u.status === 1 ? 1 : 0.6 }}
            onClick={() => { setError(null); setEditing({ id: u.id, name: u.name ?? '', email: u.email ?? '', status: u.status === 1, admin: u.admin === 1, password: '' }); }}>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={{ sm: 2 }} alignItems={{ sm: 'center' }}>
              <Box sx={{ flexGrow: 1, minWidth: 0 }}>
                <Typography fontWeight={500}>{u.name}</Typography>
                <Typography variant="body2" color="text.secondary" sx={{ overflow: 'hidden', textOverflow: 'ellipsis' }}>{u.email}</Typography>
              </Box>
              <Typography variant="body2" color="text.secondary" sx={{ minWidth: 150 }}>
                {u.LastLogin ? `Last login ${formatDate(u.LastLogin)}` : 'No login recorded'}
              </Typography>
              <Stack direction="row" spacing={0.5} sx={{ mt: { xs: 0.5, sm: 0 } }}>
                {u.admin === 1 && <Chip size="small" color="primary" label="Site admin" />}
                {u.status !== 1 && <Chip size="small" label="Deactivated" />}
                {!u.profileid && <Chip size="small" color="warning" label="No profile" />}
                {u.profileid && u.ProfileActive !== 'Y' && <Chip size="small" color="warning" label="Profile inactive" />}
              </Stack>
            </Stack>
          </Paper>
        ))}
      </Stack>

      <Dialog open={!!editing} onClose={() => setEditing(null)} fullWidth maxWidth="sm" fullScreen={isMobile}>
        <DialogTitle>Edit Login</DialogTitle>
        <DialogContent>
          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
          {editing && (
            <>
              <TextField fullWidth margin="dense" label="Name" value={editing.name} onChange={(e) => setEditing({ ...editing, name: e.target.value })} />
              <TextField fullWidth margin="dense" label="Email (sign-in)" value={editing.email} onChange={(e) => setEditing({ ...editing, email: e.target.value })}
                helperText="Permissions come from the member profile with this email." />
              <TextField fullWidth margin="dense" type="password" label="New password" autoComplete="new-password" value={editing.password}
                onChange={(e) => setEditing({ ...editing, password: e.target.value })} helperText="Leave blank to keep the current password." />
              <FormControlLabel control={<Switch checked={editing.status} onChange={(e) => setEditing({ ...editing, status: e.target.checked })} />} label="Active (can sign in)" />
              <FormControlLabel control={<Switch checked={editing.admin} onChange={(e) => setEditing({ ...editing, admin: e.target.checked })} />} label="Site admin (this Users screen)" />
              <Box sx={{ mt: 1 }}>
                <Button startIcon={<Email />} onClick={() => sendLink(editing)} disabled={!editing.status}>Email a set-password link</Button>
              </Box>
            </>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditing(null)}>Cancel</Button>
          <Button variant="contained" onClick={saveEdit} disabled={!editing?.name?.trim() || !editing?.email?.trim()}>Save</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={!!creating} onClose={() => setCreating(null)} fullWidth maxWidth="sm" fullScreen={isMobile}>
        <DialogTitle>New Login</DialogTitle>
        <DialogContent>
          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
          {creating && (
            <>
              <Tabs value={creating.mode} onChange={(e, v) => setCreating({ ...creating, mode: v, name: '', email: '' })} sx={{ mb: 1 }}>
                <Tab value="profile" label={`For a member (${candidates.length})`} disabled={candidates.length === 0} />
                <Tab value="manual" label="Enter details" />
              </Tabs>
              {creating.mode === 'profile' ? (
                <Autocomplete
                  options={candidates}
                  getOptionLabel={(c) => `${c.firstname} ${c.lastname} - ${c.email}`}
                  onChange={(e, c) => setCreating({ ...creating, name: c ? `${c.firstname} ${c.lastname}` : '', email: c?.email ?? '' })}
                  renderInput={(p) => <TextField {...p} margin="dense" label="Member" />}
                />
              ) : (
                <>
                  <TextField fullWidth margin="dense" label="Name" value={creating.name} onChange={(e) => setCreating({ ...creating, name: e.target.value })} />
                  <TextField fullWidth margin="dense" label="Email (sign-in)" value={creating.email} onChange={(e) => setCreating({ ...creating, email: e.target.value })}
                    helperText="Use the same email as their member profile so their permissions apply." />
                </>
              )}
              <FormControlLabel sx={{ mt: 1 }} control={<Switch checked={creating.sendlink} onChange={(e) => setCreating({ ...creating, sendlink: e.target.checked, password: '' })} />}
                label="Email them a link to choose their own password" />
              {!creating.sendlink && (
                <TextField fullWidth margin="dense" type="password" label="Password" autoComplete="new-password" value={creating.password}
                  onChange={(e) => setCreating({ ...creating, password: e.target.value })} helperText="At least 6 characters" />
              )}
              <FormControlLabel control={<Switch checked={creating.admin} onChange={(e) => setCreating({ ...creating, admin: e.target.checked })} />} label="Site admin" />
            </>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCreating(null)}>Cancel</Button>
          <Button variant="contained" onClick={create}
            disabled={!creating?.name?.trim() || !creating?.email?.trim() || (!creating?.sendlink && (creating?.password ?? '').length < 6)}>
            Create
          </Button>
        </DialogActions>
      </Dialog>
    </div>
  );
}
