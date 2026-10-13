import { useCallback, useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import {
  Alert, Box, Button, CircularProgress, Grid, IconButton, Paper, Stack, Tab, Table, TableBody,
  TableCell, TableHead, TableRow, Tabs, Typography
} from '@mui/material';
import { ArrowBack, Delete, FileDownload, OpenInNew, Save, UploadFile } from '@mui/icons-material';
import { api, ApiErrorMessage, ApiFileUrl } from '../utils/ApiUtils';
import { formatDate, yesNo } from '../utils/GridUtils';
import { isCurrentUserAdmin } from '../utils/LoginUtils';
import { SelectInput, TextInput } from '../components/FormFields';
import { yesNoNotSetOptions } from '../components/formOptions';
import { ConfirmDialog } from '../components/Modals';
import FilePicker from '../components/FilePicker';
import { exportCadet } from './exportCadet';

// One cadet: Details / Medical / Training. Admins edit (Laravel cadets/edit), leaders view (cadets/view).
export default function CadetEdit({ id, onClose }) {
  const canEdit = isCurrentUserAdmin();
  const [tab, setTab] = useState(0);
  const [form, setForm] = useState(null);
  const [training, setTraining] = useState([]);
  const [documents, setDocuments] = useState([]);
  const [upload, setUpload] = useState({ file: null, description: '' });
  const [deleteDoc, setDeleteDoc] = useState(null);
  const [crews, setCrews] = useState([]);
  const [message, setMessage] = useState(null);
  const [saving, setSaving] = useState(false);
  const [deleteRow, setDeleteRow] = useState(null);
  const [exporting, setExporting] = useState(false);

  const load = useCallback(() => {
    api.get(`/cadets/${id}`)
      .then((r) => {
        setForm(r.data.cadet);
        setTraining(r.data.training);
        setDocuments(r.data.documents);
        setCrews(r.data.cadetcrews);
      })
      .catch((err) => setMessage({ severity: 'error', text: ApiErrorMessage(err) }));
  }, [id]);

  useEffect(() => { load(); }, [load]);

  const save = async () => {
    setSaving(true);
    try {
      await api.put(`/cadets/${id}`, form);
      setMessage({ severity: 'success', text: `${form.firstname ?? ''} has been updated.` });
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
    setSaving(false);
  };

  const uploadDocument = async () => {
    try {
      const fd = new FormData();
      fd.append('file', upload.file);
      fd.append('description', upload.description ?? '');
      await api.post(`/cadets/${id}/documents`, fd);
      setUpload({ file: null, description: '' });
      load();
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  const removeDocument = async () => {
    const d = deleteDoc;
    setDeleteDoc(null);
    try {
      await api.delete(`/cadets/documents/${d.id}`);
      load();
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  const removeTraining = async () => {
    const row = deleteRow;
    setDeleteRow(null);
    try {
      await api.delete(`/cadets/training/${row.id}`);
      load();
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  const handleExport = async () => {
    setExporting(true);
    try {
      await exportCadet(api, form, training, documents, crews);
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
    setExporting(false);
  };

  if (!form) return <div className="vbfb-page">{message ? <Alert severity="error">{message.text}</Alert> : <CircularProgress />}</div>;

  // Read-only rows for cadet leaders
  const show = (label, field, fmt = (v) => v) => (
    <Box sx={{ py: 0.75 }}>
      <Typography variant="body2" color="text.secondary">{label}</Typography>
      <Typography>{fmt(form[field]) || '-'}</Typography>
    </Box>
  );
  const text = (field, label, props = {}) => canEdit
    ? <TextInput form={form} setForm={setForm} field={field} label={label} {...props} />
    : show(label, field);
  const yn = (field, label) => canEdit
    ? <SelectInput form={form} setForm={setForm} field={field} label={label} options={yesNoNotSetOptions} />
    : show(label, field, yesNo);

  const crewOptions = [{ value: '', label: 'Not set' }, ...crews.map((c) => ({ value: c.seqno, label: c.CrewName }))];
  const totalHours = training.reduce((sum, t) => sum + (t.hours ?? 0), 0);

  return (
    <div className="vbfb-page">
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 1 }}>
        <Button startIcon={<ArrowBack />} onClick={onClose}>Back</Button>
        <h3 className="vbfb-page-title" style={{ margin: 0, flexGrow: 1 }}>{form.firstname} {form.lastname}</h3>
        <Button variant="outlined" startIcon={<FileDownload />} onClick={handleExport} disabled={exporting}>
          {exporting ? 'Exporting…' : 'Export'}
        </Button>
      </Stack>

      {message && <Alert severity={message.severity} sx={{ mb: 2 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      <Paper sx={{ mb: 2 }}>
        <Tabs value={tab} onChange={(e, v) => setTab(v)} variant="scrollable" allowScrollButtonsMobile>
          <Tab label="Details" />
          <Tab label="Medical" />
          <Tab label={`Training (${training.length})`} />
          <Tab label={`Documents (${documents.length})`} />
        </Tabs>
      </Paper>

      <Paper sx={{ p: { xs: 2, sm: 3 } }}>
        {tab === 0 && (
          <Grid container columnSpacing={2}>
            <Grid item xs={12} sm={6}>{text('firstname', 'First Name')}</Grid>
            <Grid item xs={12} sm={6}>{text('lastname', 'Last Name')}</Grid>
            <Grid item xs={12} sm={6}>{text('email', 'Email', { type: 'email' })}</Grid>
            <Grid item xs={12} sm={6}>
              {canEdit
                ? <SelectInput form={form} setForm={setForm} field="cadetcrew" label="Crew" options={crewOptions} />
                : show('Crew', 'CrewName')}
            </Grid>
            <Grid item xs={12} sm={6}>{text('caregivername', 'Caregiver Name')}</Grid>
            <Grid item xs={12} sm={6}>{text('caregivermobile', 'Caregiver Mobile', { type: 'tel' })}</Grid>
            <Grid item xs={12} sm={6}>{text('caregiverhome', 'Caregiver Home Phone', { type: 'tel' })}</Grid>
            <Grid item xs={12} sm={6}>{text('caregiverwork', 'Caregiver Work Phone', { type: 'tel' })}</Grid>
            <Grid item xs={12} sm={6}>{yn('photossocialmedia', 'Can we put photos on social media?')}</Grid>
            <Grid item xs={12} sm={6}>{yn('cananyoneelsepickup', 'Will anyone else pick up or drop off?')}</Grid>
            <Grid item xs={12} sm={6}>{text('whocanpickup', 'Who can pick up (max 2)')}</Grid>
            <Grid item xs={12} sm={6}>{text('whocannotpickup', 'Who CANNOT pick up')}</Grid>
          </Grid>
        )}

        {tab === 1 && (
          <Grid container columnSpacing={2}>
            <Grid item xs={12} sm={4}>{yn('allergies', 'Any allergies?')}</Grid>
            <Grid item xs={12} sm={8}>{text('allergydetails', 'Allergy details, emergency medication and dose', { multiline: true, minRows: 2 })}</Grid>
            <Grid item xs={12} sm={4}>{yn('medicalyn', 'Medical or physical conditions?')}</Grid>
            <Grid item xs={12} sm={8}>{text('medicaldetails', 'Medications or aids used to manage it', { multiline: true, minRows: 2 })}</Grid>
          </Grid>
        )}

        {tab === 2 && (
          training.length === 0
            ? <Typography color="text.secondary">No training recorded.</Typography>
            : (
              <>
                <Typography sx={{ mb: 1 }}>Total hours: <b>{totalHours}</b></Typography>
                <Box sx={{ overflowX: 'auto' }}>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Date</TableCell>
                        <TableCell>Training</TableCell>
                        <TableCell>Unit Leader</TableCell>
                        <TableCell align="right">Hours</TableCell>
                        {canEdit && <TableCell />}
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {training.map((t) => (
                        <TableRow key={t.id}>
                          <TableCell>{formatDate(t.thedate)}</TableCell>
                          <TableCell>{t.Training}</TableCell>
                          <TableCell>{t.UnitLeader}</TableCell>
                          <TableCell align="right">{t.hours}</TableCell>
                          {canEdit && (
                            <TableCell align="right" padding="checkbox">
                              <IconButton size="small" aria-label="Delete training entry" onClick={() => setDeleteRow(t)}><Delete fontSize="small" /></IconButton>
                            </TableCell>
                          )}
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </Box>
              </>
            )
        )}

        {tab === 3 && (
          <>
            {documents.length === 0 && <Typography color="text.secondary">No documents uploaded.</Typography>}
            <Stack spacing={1}>
              {documents.map((d) => (
                <Stack key={d.id} direction="row" alignItems="center" spacing={1} sx={{ borderBottom: 1, borderColor: 'divider', pb: 1 }}>
                  <Box sx={{ flexGrow: 1, minWidth: 0 }}>
                    <Typography>{d.description || d.origfilename}</Typography>
                    <Typography variant="caption" color="text.secondary">{d.origfilename} · {formatDate(d.created_at)}</Typography>
                  </Box>
                  <Button size="small" startIcon={<OpenInNew />} onClick={() => window.open(ApiFileUrl(`/cadets/documents/${d.id}/file`), '_blank', 'noopener')}>Open</Button>
                  {canEdit && <IconButton size="small" aria-label="Delete document" onClick={() => setDeleteDoc(d)}><Delete fontSize="small" /></IconButton>}
                </Stack>
              ))}
            </Stack>
            {canEdit && (
              <Box sx={{ mt: 2 }}>
                <Typography sx={{ fontWeight: 500 }}>Upload a document</Typography>
                <TextInput form={upload} setForm={setUpload} field="description" label="Description (e.g. Consent form)" />
                <FilePicker file={upload.file} onChange={(file) => setUpload({ ...upload, file })} />
                <Button variant="contained" startIcon={<UploadFile />} sx={{ mt: 1.5 }} disabled={!upload.file} onClick={uploadDocument}>Upload</Button>
              </Box>
            )}
          </>
        )}
      </Paper>

      {canEdit && tab < 2 && (
        <Stack direction="row" spacing={1} sx={{ mt: 2 }}>
          <Button variant="contained" startIcon={<Save />} onClick={save} disabled={saving}>Save</Button>
          <Button onClick={onClose}>Cancel</Button>
        </Stack>
      )}

      <ConfirmDialog
        open={!!deleteRow}
        title="Delete training entry"
        message={deleteRow ? `Remove ${deleteRow.Training} on ${formatDate(deleteRow.thedate)}?` : ''}
        danger
        confirmText="Delete"
        onConfirm={removeTraining}
        onClose={() => setDeleteRow(null)}
      />

      <ConfirmDialog
        open={!!deleteDoc}
        title="Delete document"
        message={deleteDoc ? `Remove ${deleteDoc.description || deleteDoc.origfilename}?` : ''}
        danger
        confirmText="Delete"
        onConfirm={removeDocument}
        onClose={() => setDeleteDoc(null)}
      />
    </div>
  );
}

CadetEdit.propTypes = {
  id: PropTypes.number.isRequired,
  onClose: PropTypes.func.isRequired
};
