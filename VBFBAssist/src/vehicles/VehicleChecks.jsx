import { useCallback, useEffect, useState } from 'react';
import {
  Alert, Box, Button, Card, CardActionArea, CardContent, Chip, Dialog, DialogActions, DialogContent,
  DialogTitle, FormControlLabel, MenuItem, Stack, Switch, TextField, Typography
} from '@mui/material';
import { Add, DirectionsCar } from '@mui/icons-material';
import { api, ApiErrorMessage } from '../utils/ApiUtils';
import { formatDate } from '../utils/GridUtils';
import { checkStatus } from './vehicleStatus';
import VehicleCheck from './VehicleCheck';

// Vehicle checks list (Laravel /vehicles). Cards rather than a grid so it works one-handed on a phone.
export default function VehicleChecks() {
  const [checks, setChecks] = useState(null);
  const [showSignedOff, setShowSignedOff] = useState(false);
  const [openId, setOpenId] = useState(null);
  const [creating, setCreating] = useState(null);
  const [message, setMessage] = useState(null);

  const load = useCallback(() => {
    api.get('/vehiclechecks', { params: { showsignedoff: showSignedOff ? 'Y' : 'N' } })
      .then((r) => setChecks(r.data))
      .catch((err) => setMessage({ severity: 'error', text: ApiErrorMessage(err) }));
  }, [showSignedOff]);

  useEffect(() => { api.post('/vehiclechecks/opened').catch(() => {}); }, []);
  useEffect(() => { load(); }, [load]);

  const startNew = async () => {
    try {
      const r = await api.get('/vehiclechecks/newcheck');
      setCreating({
        lists: r.data,
        vehicleid: r.data.vehicles[0]?.id ?? '',
        createdby: r.data.checkers.some((c) => c.id === r.data.myprofileid) ? r.data.myprofileid : '',
        monthly: 'Y'
      });
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  const create = async () => {
    try {
      const r = await api.post('/vehiclechecks', {
        vehicleid: creating.vehicleid,
        createdby: creating.createdby || null,
        monthly: creating.monthly
      });
      setCreating(null);
      setOpenId(r.data.id);
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  if (openId !== null) {
    return <VehicleCheck id={openId} onClose={() => { setOpenId(null); load(); }} />;
  }

  return (
    <div className="vbfb-page">
      <Stack direction="row" justifyContent="space-between" alignItems="center" flexWrap="wrap" useFlexGap spacing={1} sx={{ mb: 1 }}>
        <h3 className="vbfb-page-title" style={{ margin: 0 }}>Vehicle Maintenance</h3>
        <Button variant="contained" startIcon={<Add />} onClick={startNew}>New Check</Button>
      </Stack>
      <FormControlLabel
        control={<Switch checked={showSignedOff} onChange={(e) => setShowSignedOff(e.target.checked)} />}
        label="Show signed off checks"
        sx={{ mb: 1 }}
      />

      {message && <Alert severity={message.severity} sx={{ mb: 2 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      {checks && checks.length === 0 && (
        <Alert severity="info">No checks in progress. Tap <b>New Check</b> to start one.</Alert>
      )}

      <Box sx={{ display: 'grid', gap: 1.5, gridTemplateColumns: { xs: '1fr', md: '1fr 1fr', lg: '1fr 1fr 1fr' } }}>
        {(checks ?? []).map((c) => {
          const st = checkStatus[c.status] ?? checkStatus[0];
          return (
            <Card key={c.id}>
              <CardActionArea onClick={() => setOpenId(c.id)}>
                <CardContent>
                  <Stack direction="row" spacing={1.5} alignItems="flex-start">
                    <DirectionsCar color="primary" sx={{ mt: 0.3 }} />
                    <Box sx={{ flexGrow: 1, minWidth: 0 }}>
                      <Typography fontWeight={500}>{c.Name}</Typography>
                      <Typography variant="body2" color="text.secondary">
                        {c.monthly === 'Y' ? 'Monthly' : 'Weekly'} · {formatDate(c.createddate)}
                        {c.CreatedByName && <> · {c.CreatedByName}</>}
                      </Typography>
                      {c.signedoffname && <Typography variant="body2" color="text.secondary">Signed off by {c.signedoffname}</Typography>}
                    </Box>
                    <Chip size="small" label={st.label} color={st.color} />
                  </Stack>
                </CardContent>
              </CardActionArea>
            </Card>
          );
        })}
      </Box>

      <Dialog open={!!creating} onClose={() => setCreating(null)} fullWidth maxWidth="xs">
        <DialogTitle>New Vehicle Check</DialogTitle>
        <DialogContent>
          {creating && creating.lists.vehicles.length === 0 ? (
            <Alert severity="info">Every vehicle already has a check in progress. Please complete those first.</Alert>
          ) : creating && (
            <>
              <TextField select fullWidth margin="dense" label="Vehicle" value={creating.vehicleid} onChange={(e) => setCreating({ ...creating, vehicleid: e.target.value })}>
                {creating.lists.vehicles.map((v) => <MenuItem key={v.id} value={v.id}>{v.Name}</MenuItem>)}
              </TextField>
              <TextField select fullWidth margin="dense" label="Check type" value={creating.monthly} onChange={(e) => setCreating({ ...creating, monthly: e.target.value })}>
                <MenuItem value="Y">Monthly</MenuItem>
                <MenuItem value="N">Weekly</MenuItem>
              </TextField>
              <TextField select fullWidth margin="dense" label="Checked by" value={creating.createdby} onChange={(e) => setCreating({ ...creating, createdby: e.target.value })}>
                <MenuItem value="">Not set</MenuItem>
                {creating.lists.checkers.map((p) => <MenuItem key={p.id} value={p.id}>{p.name}</MenuItem>)}
              </TextField>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                Vehicles with a check in progress are not listed. Complete the previous check before starting a new one.
              </Typography>
            </>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCreating(null)}>Cancel</Button>
          <Button variant="contained" onClick={create} disabled={!creating?.vehicleid}>Start Check</Button>
        </DialogActions>
      </Dialog>
    </div>
  );
}
