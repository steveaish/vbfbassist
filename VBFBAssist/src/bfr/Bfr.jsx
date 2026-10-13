import { useCallback, useEffect, useState } from 'react';
import {
  Alert, Box, Card, CardActionArea, CardContent, InputAdornment, MenuItem, Stack, TextField, Typography
} from '@mui/material';
import { Search } from '@mui/icons-material';
import { api, ApiErrorMessage } from '../utils/ApiUtils';
import BfrStreet from './BfrStreet';

// Bush Fire Ready streets (Laravel /bfr): facilitators see every street, coordinators their assigned ones
export default function Bfr() {
  const [search, setSearch] = useState('');
  const [area, setArea] = useState('ALL');
  const [data, setData] = useState({ streets: [], areas: [], seesall: true });
  const [streetId, setStreetId] = useState(null);
  const [message, setMessage] = useState(null);

  const load = useCallback((s, a) => {
    api.get('/bfr/streets', { params: { search: s, area: a } })
      .then((r) => setData(r.data))
      .catch((err) => setMessage({ severity: 'error', text: ApiErrorMessage(err) }));
  }, []);

  useEffect(() => { api.post('/bfr/opened').catch(() => {}); }, []);
  useEffect(() => {
    const t = setTimeout(() => load(search, area), 300);
    return () => clearTimeout(t);
  }, [search, area, load]);

  if (streetId !== null) return <BfrStreet id={streetId} onClose={() => { setStreetId(null); load(search, area); }} />;

  // Group the street cards under their area
  const groups = data.streets.reduce((acc, s) => {
    (acc[s.AreaName] = acc[s.AreaName] ?? []).push(s);
    return acc;
  }, {});

  const count = (label, n) => (
    <Box sx={{ textAlign: 'center', minWidth: 52 }}>
      <Typography fontWeight={500} color={n > 0 ? 'primary.main' : 'text.disabled'}>{n}</Typography>
      <Typography variant="caption" color="text.secondary">{label}</Typography>
    </Box>
  );

  return (
    <div className="vbfb-page">
      <h3 className="vbfb-page-title">Bush Fire Ready</h3>
      {!data.seesall && <Alert severity="info" sx={{ mb: 2 }}>Showing the streets assigned to you.</Alert>}

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} sx={{ mb: 2 }}>
        <TextField size="small" placeholder="Search street or area" value={search} onChange={(e) => setSearch(e.target.value)} sx={{ flex: 1, maxWidth: { sm: 360 } }}
          InputProps={{ startAdornment: <InputAdornment position="start"><Search /></InputAdornment> }} />
        <TextField select size="small" label="Area" value={area} onChange={(e) => setArea(e.target.value)} sx={{ minWidth: 240 }}>
          <MenuItem value="ALL">All areas</MenuItem>
          {data.areas.map((a) => <MenuItem key={a} value={a}>{a}</MenuItem>)}
        </TextField>
      </Stack>

      {message && <Alert severity={message.severity} sx={{ mb: 2 }} onClose={() => setMessage(null)}>{message.text}</Alert>}
      {data.streets.length === 0 && <Typography color="text.secondary">No streets found.</Typography>}

      {Object.entries(groups).map(([areaName, streets]) => (
        <Box key={areaName} sx={{ mb: 2 }}>
          <Typography sx={{ fontWeight: 500, color: 'primary.dark', mb: 1 }}>{areaName}</Typography>
          <Box sx={{ display: 'grid', gap: 1, gridTemplateColumns: { xs: '1fr', md: '1fr 1fr', lg: '1fr 1fr 1fr' } }}>
            {streets.map((s) => (
              <Card key={s.id} variant="outlined">
                <CardActionArea onClick={() => setStreetId(s.id)}>
                  <CardContent sx={{ py: 1.25, '&:last-child': { pb: 1.25 } }}>
                    <Stack direction="row" alignItems="center" spacing={1}>
                      <Typography sx={{ flexGrow: 1 }}>{s.StreetName}</Typography>
                      {count('Month', s.ThisMonth)}
                      {count('3 Mths', s.ThreeMonths)}
                      {count('Year', s.ThisYear)}
                    </Stack>
                  </CardContent>
                </CardActionArea>
              </Card>
            ))}
          </Box>
        </Box>
      ))}
    </div>
  );
}
