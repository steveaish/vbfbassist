import { useCallback, useEffect, useMemo, useState } from 'react';
import PropTypes from 'prop-types';
import {
  Alert, Box, Button, Chip, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle,
  LinearProgress, MenuItem, Paper, Stack, TextField, ToggleButton, ToggleButtonGroup, Typography
} from '@mui/material';
import { ArrowBack, Check, Close, Delete, Email, PictureAsPdf, Remove, ReportProblem, TaskAlt } from '@mui/icons-material';
import { api, ApiErrorMessage, ApiQueryAuth, GetApiUrl } from '../utils/ApiUtils';
import { formatDate } from '../utils/GridUtils';
import { ConfirmDialog } from '../components/Modals';
import { checkStatus, results, SIGNED_OFF } from './vehicleStatus';

const DEFECT_REMEDY = 'To be reported to defect report';

// One vehicle check: tap Pass / Fail per item (saved immediately), give fails a remedy, then sign off.
// Replaces Laravel's separate edit / "complete checks" / sign-off pages.
export default function VehicleCheck({ id, onClose }) {
  const [check, setCheck] = useState(null);
  const [lines, setLines] = useState([]);
  const [resolutions, setResolutions] = useState([]);
  const [filter, setFilter] = useState('all');
  const [message, setMessage] = useState(null);
  const [signing, setSigning] = useState(false);
  const [code, setCode] = useState('');
  const [signError, setSignError] = useState(null);
  const [confirmDelete, setConfirmDelete] = useState(false);
  const [busy, setBusy] = useState(false);
  const [commentOpen, setCommentOpen] = useState([]); // line ids with the comment box opened by hand

  const load = useCallback(() => {
    api.get(`/vehiclechecks/${id}`)
      .then((r) => {
        setCheck(r.data.check);
        setLines(r.data.lines);
        setResolutions(r.data.resolutions);
        if (r.data.check.status >= 2) setFilter('review');
      })
      .catch((err) => setMessage({ severity: 'error', text: ApiErrorMessage(err) }));
  }, [id]);

  useEffect(() => { load(); }, [load]);

  const readOnly = check?.status >= SIGNED_OFF;

  // Save one line's changed fields; optimistic, rolled back if the API refuses
  const saveLine = async (line, changes) => {
    const before = lines;
    setLines(lines.map((l) => (l.id === line.id ? { ...l, ...changes } : l)));
    try {
      const r = await api.put(`/vehiclechecks/${id}/lines`, [{ id: line.id, ...changes }]);
      setCheck((c) => ({ ...c, status: r.data.status }));
    }
    catch (err) {
      setLines(before);
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  const signOff = async () => {
    setBusy(true);
    try {
      const r = await api.post(`/vehiclechecks/${id}/signoff`, { signoffcode: code });
      setSigning(false);
      setCode('');
      setMessage(r.data.emailed
        ? { severity: 'success', text: 'Signed off. The report has been emailed to you.' }
        : { severity: 'warning', text: `Signed off. ${r.data.emailError}` });
      load();
    }
    catch (err) {
      setSignError(ApiErrorMessage(err));
    }
    setBusy(false);
  };

  const emailReport = async () => {
    setBusy(true);
    try {
      const r = await api.post(`/vehiclechecks/${id}/emailreport`);
      setMessage({ severity: 'success', text: r.data.message });
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
    setBusy(false);
  };

  const remove = async () => {
    setConfirmDelete(false);
    try {
      await api.delete(`/vehiclechecks/${id}`);
      onClose();
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  const openPdf = (which) => window.open(`${GetApiUrl()}/vehiclechecks/${id}/${which}?${ApiQueryAuth()}`, '_blank', 'noopener');

  const counts = useMemo(() => ({
    total: lines.length,
    done: lines.filter((l) => l.checked !== results.notChecked).length,
    fails: lines.filter((l) => l.checked === results.fail).length,
    needRemedy: lines.filter((l) => l.checked === results.fail && !l.remedy).length,
    defects: lines.filter((l) => l.checked !== results.pass && l.remedy === DEFECT_REMEDY).length
  }), [lines]);

  if (!check) return <div className="vbfb-page">{message ? <Alert severity="error">{message.text}</Alert> : <CircularProgress />}</div>;

  const st = checkStatus[check.status] ?? checkStatus[0];
  const shown = filter === 'review' ? lines.filter((l) => l.checked !== results.pass) : lines;

  const guidance = {
    0: 'Work through each item and tap Pass or Fail. Every tap is saved straight away.',
    1: `${counts.total - counts.done} item(s) still to check.`,
    2: `${counts.needRemedy} failed item(s) need a remedy before this check can be signed off.`,
    3: 'All items are checked. Enter a sign off code to complete the check.',
    4: `Signed off by ${check.signedoffname ?? ''}. This check is read only.`
  }[check.status];

  return (
    <div className="vbfb-page">
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 1 }}>
        <Button startIcon={<ArrowBack />} onClick={onClose}>Back</Button>
        <Box sx={{ flexGrow: 1, minWidth: 0 }}>
          <Typography variant="h6" sx={{ color: 'primary.dark', lineHeight: 1.2 }}>{check.Name}</Typography>
          <Typography variant="body2" color="text.secondary">
            {check.monthly === 'Y' ? 'Monthly' : 'Weekly'} check · {formatDate(check.createddate)}
            {check.CreatedByName && <> · {check.CreatedByName}</>}
          </Typography>
        </Box>
        <Chip label={st.label} color={st.color} />
      </Stack>

      <LinearProgress variant="determinate" value={counts.total ? (100 * counts.done) / counts.total : 0} sx={{ height: 8, borderRadius: 4, my: 1 }} />
      <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
        {counts.done} of {counts.total} checked · {counts.fails} failed
      </Typography>

      <Alert severity={check.status === 3 ? 'success' : 'info'} sx={{ mb: 1 }}>{guidance}</Alert>
      {message && <Alert severity={message.severity} sx={{ mb: 1 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      <ToggleButtonGroup exclusive size="small" value={filter} onChange={(e, v) => v && setFilter(v)} sx={{ mb: 1.5 }}>
        <ToggleButton value="all">All items ({counts.total})</ToggleButton>
        <ToggleButton value="review">Not passed ({counts.total - lines.filter((l) => l.checked === results.pass).length})</ToggleButton>
      </ToggleButtonGroup>

      <Stack spacing={1}>
        {shown.map((line, i) => {
          const newSection = i === 0 || shown[i - 1].sectionname !== line.sectionname;
          const isFail = line.checked === results.fail;
          return (
            <Box key={line.id}>
              {newSection && <Typography sx={{ fontWeight: 500, color: 'primary.dark', mt: i === 0 ? 0 : 1.5, mb: 0.5 }}>{line.sectionname}</Typography>}
              <Paper variant="outlined" sx={{
                p: 1.5,
                borderLeft: 4,
                borderLeftColor: line.checked === results.pass ? 'success.main' : isFail ? 'error.main' : 'grey.400'
              }}>
                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} alignItems={{ sm: 'center' }}>
                  <Typography sx={{ flexGrow: 1 }}>{line.itemname}</Typography>
                  <ToggleButtonGroup
                    exclusive
                    disabled={readOnly}
                    value={line.checked}
                    onChange={(e, v) => v && v !== line.checked && saveLine(line, { checked: v })}
                    sx={{ alignSelf: { xs: 'stretch', sm: 'auto' }, '& .MuiToggleButton-root': { flex: 1, py: 1.2, px: 2 } }}
                  >
                    <ToggleButton value={results.pass} color="success" aria-label="Pass"><Check sx={{ mr: 0.5 }} />Pass</ToggleButton>
                    <ToggleButton value={results.fail} color="error" aria-label="Fail"><Close sx={{ mr: 0.5 }} />Fail</ToggleButton>
                    <ToggleButton value={results.notChecked} aria-label="Not checked"><Remove /></ToggleButton>
                  </ToggleButtonGroup>
                </Stack>

                {(isFail || line.comments || line.remedy || commentOpen.includes(line.id)) && (
                  <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} sx={{ mt: 1 }}>
                    <CommentField line={line} readOnly={readOnly} onSave={(comments) => saveLine(line, { comments })} />
                    {line.checked !== results.pass && (
                      <TextField
                        select size="small" label="Remedy" value={line.remedy ?? ''} disabled={readOnly}
                        error={isFail && !line.remedy}
                        onChange={(e) => saveLine(line, { remedy: e.target.value || null })}
                        sx={{ minWidth: { sm: 280 } }}
                      >
                        <MenuItem value="">Not set</MenuItem>
                        {resolutions.map((r) => <MenuItem key={r} value={r}>{r}</MenuItem>)}
                      </TextField>
                    )}
                  </Stack>
                )}
                {!isFail && !line.comments && !line.remedy && !commentOpen.includes(line.id) && !readOnly && (
                  <Button size="small" sx={{ mt: 0.5 }} onClick={() => setCommentOpen([...commentOpen, line.id])}>
                    Add comment
                  </Button>
                )}
              </Paper>
            </Box>
          );
        })}
        {shown.length === 0 && <Typography color="text.secondary">Everything here has passed.</Typography>}
      </Stack>

      <Paper sx={{ mt: 2, p: 1.5, position: 'sticky', bottom: 0, zIndex: 2 }} elevation={4}>
        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
          {check.status === 3 && <Button variant="contained" startIcon={<TaskAlt />} onClick={() => { setSignError(null); setSigning(true); }}>Sign Off</Button>}
          <Button variant="outlined" startIcon={<PictureAsPdf />} onClick={() => openPdf('report')}>Report</Button>
          {counts.defects > 0 && <Button variant="outlined" startIcon={<ReportProblem />} onClick={() => openPdf('defectreport')}>Defect Report</Button>}
          {check.status >= 2 && <Button variant="outlined" startIcon={<Email />} onClick={emailReport} disabled={busy}>Email Report</Button>}
          {!readOnly && <Button color="error" startIcon={<Delete />} onClick={() => setConfirmDelete(true)}>Delete</Button>}
        </Stack>
      </Paper>

      <Dialog open={signing} onClose={() => setSigning(false)} fullWidth maxWidth="xs">
        <DialogTitle>Sign off {check.Name}</DialogTitle>
        <DialogContent>
          {signError && <Alert severity="error" sx={{ mb: 1 }}>{signError}</Alert>}
          <Typography variant="body2" sx={{ mb: 1 }}>Enter your vehicle maintenance sign off code. The report will be emailed to you.</Typography>
          <TextField
            autoFocus fullWidth label="Sign off code" type="password" autoComplete="off"
            value={code} onChange={(e) => setCode(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && code && signOff()}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSigning(false)}>Cancel</Button>
          <Button variant="contained" onClick={signOff} disabled={!code || busy}>Sign Off</Button>
        </DialogActions>
      </Dialog>

      <ConfirmDialog
        open={confirmDelete}
        title="Delete vehicle check"
        message="Delete this check? The vehicle will then be free for a new check."
        danger
        confirmText="Delete"
        onConfirm={remove}
        onClose={() => setConfirmDelete(false)}
      />
    </div>
  );
}

VehicleCheck.propTypes = {
  id: PropTypes.number.isRequired,
  onClose: PropTypes.func.isRequired
};

// Comment box that saves when you leave it, so typing doesn't call the API on every key
function CommentField({ line, readOnly, onSave }) {
  const [value, setValue] = useState((line.comments ?? '').trim());
  useEffect(() => { setValue((line.comments ?? '').trim()); }, [line.comments]);

  return (
    <TextField
      size="small" label="Comment" fullWidth disabled={readOnly}
      value={value}
      onChange={(e) => setValue(e.target.value)}
      onBlur={() => value !== (line.comments ?? '').trim() && onSave(value || null)}
    />
  );
}

CommentField.propTypes = {
  line: PropTypes.object.isRequired,
  readOnly: PropTypes.bool,
  onSave: PropTypes.func.isRequired
};
