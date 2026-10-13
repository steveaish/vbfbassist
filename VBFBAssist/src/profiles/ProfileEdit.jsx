import { useCallback, useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import {
  Accordion, AccordionDetails, AccordionSummary, Alert, Box, Button, Checkbox, CircularProgress,
  FormControlLabel, Grid, MenuItem, Paper, Stack, Tab, Tabs, TextField, Typography
} from '@mui/material';
import { ArrowBack, Delete, ExpandMore, Save } from '@mui/icons-material';
import { api, ApiErrorMessage } from '../utils/ApiUtils';
import { useIsMobile } from '../utils/GridUtils';
import { SelectInput, TextInput, YesNoSwitch } from '../components/FormFields';
import { yesNoNotSetOptions, yesNoOptions } from '../components/formOptions';
import { ConfirmDialog } from '../components/Modals';
import PpeTable, { PpeButton } from '../components/PpeTable';
import {
  accessFlags, bfrFlags, licenceOptions, medalFlags, membershipFlags, prefCommOptions, sexOptions
} from './profileOptions';

const newProfile = { isactive: 'Y', sex: 'Not Specified', dlicense: 'None', crew: -1, streetid: -1 };

// Admin edit of one profile, tabs as in Laravel: Details, Access, Membership, Bush Fire Ready, Medals, PPE
export default function ProfileEdit({ id, onClose }) {
  const isNew = id === 0;
  const isMobile = useIsMobile();
  const [tab, setTab] = useState(0);
  const [form, setForm] = useState(isNew ? newProfile : null);
  const [lookups, setLookups] = useState({ crews: [], streets: [], ppestyles: [] });
  const [allowedStreets, setAllowedStreets] = useState([]);
  const [ppe, setPpe] = useState([]);
  const [issueStyle, setIssueStyle] = useState('');
  const [message, setMessage] = useState(null);
  const [saving, setSaving] = useState(false);
  const [confirm, setConfirm] = useState(null);

  const load = useCallback(async () => {
    try {
      const [lk, pr] = await Promise.all([
        api.get('/profiles/lookups'),
        isNew ? Promise.resolve(null) : api.get(`/profiles/${id}`)
      ]);
      setLookups(lk.data);
      if (pr) {
        setForm({ ...pr.data.profile, dob: pr.data.dob_iso, startdate: pr.data.startdate_iso });
        setAllowedStreets(pr.data.allowedstreets);
        setPpe(pr.data.ppe);
      }
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  }, [id, isNew]);

  useEffect(() => { load(); }, [load]);

  const save = async () => {
    setSaving(true);
    setMessage(null);
    try {
      const body = { ...form, allowedstreets: allowedStreets };
      if (isNew) {
        await api.post('/profiles', body);
        onClose();
        return;
      }
      await api.put(`/profiles/${id}`, body);
      setMessage({ severity: 'success', text: `${form.firstname ?? ''} has been updated.` });
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
    setSaving(false);
  };

  const remove = async () => {
    setConfirm(null);
    try {
      await api.delete(`/profiles/${id}`);
      onClose();
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  const ppeAction = async (url, body) => {
    try {
      await api.post(url, body);
      const r = await api.get(`/profiles/${id}`);
      setPpe(r.data.ppe);
      const lk = await api.get('/profiles/lookups');
      setLookups(lk.data);
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  if (!form) {
    return <div className="vbfb-page"><CircularProgress /></div>;
  }

  const flagGrid = (flags) => (
    <Grid container columnSpacing={2}>
      {flags.map((f) => (
        <Grid item xs={12} sm={6} md={4} key={f.field}>
          <YesNoSwitch form={form} setForm={setForm} field={f.field} label={f.label} />
        </Grid>
      ))}
    </Grid>
  );

  const crewOptions = [{ value: -1, label: 'Not set' }, ...lookups.crews.map((c) => ({ value: c.seqno, label: c.CrewName }))];
  const streetOptions = [{ value: -1, label: 'Not set' }, ...lookups.streets.map((s) => ({ value: s.id, label: `${s.AreaName} - ${s.StreetName}` }))];

  // BFR streets grouped by area, for the "streets this member can see" checklist
  const areas = lookups.streets.reduce((acc, s) => {
    (acc[s.AreaName] = acc[s.AreaName] ?? []).push(s);
    return acc;
  }, {});
  const toggleStreet = (streetId) => setAllowedStreets(
    allowedStreets.includes(streetId) ? allowedStreets.filter((x) => x !== streetId) : [...allowedStreets, streetId]
  );
  const toggleArea = (streets, on) => {
    const ids = streets.map((s) => s.id);
    setAllowedStreets(on ? [...new Set([...allowedStreets, ...ids])] : allowedStreets.filter((x) => !ids.includes(x)));
  };

  const current = ppe.filter((p) => p.deleted !== 'Y' && p.returned !== 'Y');
  const history = ppe.filter((p) => p.deleted === 'Y' || p.returned === 'Y');
  const ppeActions = (r) => (r.deleted !== 'Y' && r.returned !== 'Y') && (
    <>
      <PpeButton onClick={() => setConfirm({ title: 'Return item', message: `Return ${r.Item} - ${r.Style} to the stock cupboard?`, onConfirm: () => { setConfirm(null); ppeAction(`/ppestock/${r.id}/return`); } })}>Return</PpeButton>
      <PpeButton color="error" onClick={() => setConfirm({ title: 'Retire item', message: `Retire ${r.Item} - ${r.Style}? It will not go back into stock.`, danger: true, onConfirm: () => { setConfirm(null); ppeAction(`/ppestock/${r.id}/retire`); } })}>Retire</PpeButton>
    </>
  );

  const tabs = ['Details', 'Access', 'Membership', 'Bush Fire Ready', 'Medals'];
  if (!isNew) tabs.push('PPE');

  return (
    <div className="vbfb-page">
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 1 }}>
        <Button startIcon={<ArrowBack />} onClick={onClose}>Back</Button>
        <h3 className="vbfb-page-title" style={{ margin: 0 }}>
          {isNew ? 'New Profile' : `${form.firstname ?? ''} ${form.lastname ?? ''}`}
        </h3>
      </Stack>

      {message && <Alert severity={message.severity} sx={{ mb: 2 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      <Paper sx={{ mb: 2 }}>
        <Tabs value={tab} onChange={(e, v) => setTab(v)} variant="scrollable" scrollButtons="auto" allowScrollButtonsMobile>
          {tabs.map((t) => <Tab key={t} label={t} />)}
        </Tabs>
      </Paper>

      <Paper sx={{ p: { xs: 2, sm: 3 } }}>
        {tab === 0 && (
          <Grid container columnSpacing={2}>
            <Grid item xs={12} sm={6}><TextInput form={form} setForm={setForm} field="firstname" label="First Name" /></Grid>
            <Grid item xs={12} sm={6}><TextInput form={form} setForm={setForm} field="lastname" label="Last Name" /></Grid>
            <Grid item xs={12} sm={6}><TextInput form={form} setForm={setForm} field="email" label="Email" type="email" /></Grid>
            <Grid item xs={6} sm={3}><TextInput form={form} setForm={setForm} field="dob" label="D.O.B" type="date" InputLabelProps={{ shrink: true }} /></Grid>
            <Grid item xs={6} sm={3}><TextInput form={form} setForm={setForm} field="startdate" label="Start Date" type="date" InputLabelProps={{ shrink: true }} /></Grid>
            <Grid item xs={6} sm={3}><SelectInput form={form} setForm={setForm} field="sex" label="Male / Female" options={sexOptions} /></Grid>
            <Grid item xs={6} sm={3}><SelectInput form={form} setForm={setForm} field="isactive" label="Active" options={yesNoOptions} /></Grid>
            <Grid item xs={12} sm={6}><SelectInput form={form} setForm={setForm} field="crew" label="Crew" options={crewOptions} /></Grid>
            <Grid item xs={12} sm={4}><TextInput form={form} setForm={setForm} field="mobilephone" label="Mobile Phone" type="tel" /></Grid>
            <Grid item xs={6} sm={4}><TextInput form={form} setForm={setForm} field="homephone" label="Home Phone" type="tel" /></Grid>
            <Grid item xs={6} sm={4}><TextInput form={form} setForm={setForm} field="workphone" label="Work Phone" type="tel" /></Grid>
            <Grid item xs={12}><TextInput form={form} setForm={setForm} field="address" label="Address" multiline minRows={2} /></Grid>
            <Grid item xs={12}><SelectInput form={form} setForm={setForm} field="streetid" label="BFR Street" options={streetOptions} /></Grid>
            <Grid item xs={6} sm={3}><SelectInput form={form} setForm={setForm} field="dlicense" label="Driver Licence" options={licenceOptions} /></Grid>
            <Grid item xs={6} sm={3}><TextInput form={form} setForm={setForm} field="dfesnumber" label="DFES Number" /></Grid>
            <Grid item xs={6} sm={3}><SelectInput form={form} setForm={setForm} field="bartactive" label="BART Active" options={yesNoNotSetOptions} /></Grid>
            <Grid item xs={6} sm={3}><SelectInput form={form} setForm={setForm} field="prefcomm" label="Preferred Comms" options={prefCommOptions} /></Grid>
            <Grid item xs={12}><TextInput form={form} setForm={setForm} field="comments" label="Comments" multiline minRows={3} /></Grid>
          </Grid>
        )}

        {tab === 1 && (
          <>
            {flagGrid(accessFlags)}
            <Box sx={{ maxWidth: 320, mt: 1 }}>
              <TextInput form={form} setForm={setForm} field="signoffcode" label="Vehicle Maintenance Signoff Code" />
            </Box>
          </>
        )}

        {tab === 2 && flagGrid(membershipFlags)}

        {tab === 3 && (
          <>
            {flagGrid(bfrFlags)}
            <Typography sx={{ mt: 2, mb: 1, fontWeight: 500 }}>Streets this member can see</Typography>
            {Object.entries(areas).map(([area, streets]) => {
              const selected = streets.filter((s) => allowedStreets.includes(s.id)).length;
              return (
                <Accordion key={area} disableGutters>
                  <AccordionSummary expandIcon={<ExpandMore />}>
                    <Typography sx={{ flexGrow: 1 }}>{area}</Typography>
                    <Typography color="text.secondary" sx={{ mr: 1 }}>{selected} / {streets.length}</Typography>
                  </AccordionSummary>
                  <AccordionDetails>
                    <Button size="small" onClick={() => toggleArea(streets, selected < streets.length)}>
                      {selected < streets.length ? 'Select all' : 'Clear all'}
                    </Button>
                    <Grid container>
                      {streets.map((s) => (
                        <Grid item xs={12} sm={6} md={4} key={s.id}>
                          <FormControlLabel control={<Checkbox checked={allowedStreets.includes(s.id)} onChange={() => toggleStreet(s.id)} />} label={s.StreetName} />
                        </Grid>
                      ))}
                    </Grid>
                  </AccordionDetails>
                </Accordion>
              );
            })}
          </>
        )}

        {tab === 4 && flagGrid(medalFlags)}

        {tab === 5 && !isNew && (
          <>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} sx={{ mb: 2 }}>
              <TextField select size="small" label="Issue item" value={issueStyle} onChange={(e) => setIssueStyle(e.target.value)} sx={{ minWidth: { sm: 360 } }}>
                {lookups.ppestyles.map((s) => (
                  <MenuItem key={s.id} value={s.id}>{s.ItemName} - {s.StyleName} ({s.quantity ?? 0} in stock)</MenuItem>
                ))}
              </TextField>
              <Button variant="contained" disabled={!issueStyle} onClick={() => { ppeAction('/ppestock/issue', { profileid: id, styleid: issueStyle }); setIssueStyle(''); }}>
                Issue
              </Button>
            </Stack>
            <Typography sx={{ fontWeight: 500, mb: 1 }}>Currently issued</Typography>
            <PpeTable rows={current} actions={ppeActions} />
            {history.length > 0 && (
              <>
                <Typography sx={{ fontWeight: 500, mt: 3, mb: 1 }}>Returned / retired</Typography>
                <PpeTable rows={history} actions={() => null} />
              </>
            )}
          </>
        )}
      </Paper>

      {tab !== 5 && (
        <Stack direction="row" spacing={1} sx={{ mt: 2, position: isMobile ? 'sticky' : 'static', bottom: 0, bgcolor: 'background.default', py: 1 }}>
          <Button variant="contained" startIcon={<Save />} onClick={save} disabled={saving}>Save</Button>
          <Button onClick={onClose}>Cancel</Button>
          <Box sx={{ flexGrow: 1 }} />
          {!isNew && (
            <Button color="error" startIcon={<Delete />} onClick={() => setConfirm({
              title: 'Delete profile',
              message: 'Permanently delete this profile? To keep their history, set Active to No instead.',
              danger: true,
              onConfirm: remove
            })}>
              Delete
            </Button>
          )}
        </Stack>
      )}

      <ConfirmDialog
        open={!!confirm}
        title={confirm?.title ?? ''}
        message={confirm?.message}
        danger={confirm?.danger}
        onConfirm={() => confirm?.onConfirm()}
        onClose={() => setConfirm(null)}
      />
    </div>
  );
}

ProfileEdit.propTypes = {
  id: PropTypes.number.isRequired,
  onClose: PropTypes.func.isRequired
};
