import { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import {
  Alert, Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle, Divider,
  List, ListItemButton, ListItemText, MenuItem, Stack, TextField, Typography
} from '@mui/material';
import { Delete, Download, OpenInNew, UploadFile } from '@mui/icons-material';
import { api, ApiErrorMessage, ApiFileUrl } from '../utils/ApiUtils';
import { formatDate, useIsMobile } from '../utils/GridUtils';
import FilePicker from '../components/FilePicker';

// Open / edit / upload a document. id 0 = upload a new document.
export default function DocumentDialog({ id, types, canEdit, onClose }) {
  const isMobile = useIsMobile();
  const isNew = id === 0;
  const [doc, setDoc] = useState(isNew ? { documentname: '', description: '', Type: 'Not Specified', Author: '' } : null);
  const [history, setHistory] = useState([]);
  const [file, setFile] = useState(null);
  const [newVersion, setNewVersion] = useState(null);
  const [confirmDelete, setConfirmDelete] = useState(false);
  const [error, setError] = useState(null);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    if (isNew) return;
    api.get(`/documents/${id}`)
      .then((r) => { setDoc(r.data.document); setHistory(r.data.history); })
      .catch((err) => setError(ApiErrorMessage(err)));
  }, [id, isNew]);

  const set = (field) => (e) => setDoc({ ...doc, [field]: e.target.value });

  const run = async (fn) => {
    setBusy(true);
    setError(null);
    try {
      await fn();
      onClose(true);
    }
    catch (err) {
      setError(ApiErrorMessage(err));
      setBusy(false);
    }
  };

  const upload = () => run(() => {
    const fd = new FormData();
    fd.append('file', file);
    ['documentname', 'description', 'Type', 'Author'].forEach((f) => fd.append(f, doc[f] ?? ''));
    return api.post('/documents', fd);
  });

  const save = () => run(async () => {
    await api.put(`/documents/${id}`, doc);
    if (newVersion) {
      const fd = new FormData();
      fd.append('file', newVersion);
      await api.post(`/documents/${id}/file`, fd);
    }
  });

  const open = (path, download) => window.open(ApiFileUrl(path, download ? '&download=Y' : ''), '_blank', 'noopener');

  const editable = canEdit;

  return (
    <Dialog open onClose={() => onClose(false)} fullWidth maxWidth="sm" fullScreen={isMobile}>
      <DialogTitle>{isNew ? 'Upload Document' : doc?.documentname ?? ''}</DialogTitle>
      <DialogContent>
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        {!doc ? <CircularProgress /> : (
          <>
            {!isNew && (
              <Stack direction="row" spacing={1} sx={{ mb: 2 }}>
                <Button variant="contained" startIcon={<OpenInNew />} onClick={() => open(`/documents/${id}/file`)}>Open</Button>
                <Button variant="outlined" startIcon={<Download />} onClick={() => open(`/documents/${id}/file`, true)}>Download</Button>
              </Stack>
            )}

            {editable ? (
              <>
                <TextField fullWidth margin="dense" label="Document name" value={doc.documentname ?? ''} onChange={set('documentname')} />
                <TextField fullWidth margin="dense" label="Description" multiline minRows={2} value={doc.description ?? ''} onChange={set('description')} />
                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={{ sm: 2 }}>
                  <TextField select fullWidth margin="dense" label="Type" value={doc.Type ?? 'Not Specified'} onChange={set('Type')}>
                    {types.map((t) => <MenuItem key={t} value={t}>{t}</MenuItem>)}
                  </TextField>
                  <TextField fullWidth margin="dense" label="Author" value={doc.Author ?? ''} onChange={set('Author')} />
                </Stack>
                {isNew
                  ? <FilePicker file={file} onChange={setFile} label="Choose file" />
                  : <FilePicker file={newVersion} onChange={setNewVersion} label="Upload a new version" />}
              </>
            ) : (
              <>
                {doc.description && <Typography sx={{ mb: 1 }}>{doc.description}</Typography>}
                <Typography variant="body2" color="text.secondary">
                  {doc.Type && doc.Type !== 'Not Specified' && <>{doc.Type} · </>}
                  {doc.Author && <>{doc.Author} · </>}
                  Version {doc.versionnumber ?? 1} · {formatDate(doc.lastupdated ?? doc.createddate)}
                </Typography>
              </>
            )}

            {!isNew && (
              <Typography variant="caption" color="text.secondary" component="div" sx={{ mt: 1 }}>
                File: {doc.originalfilename}
              </Typography>
            )}

            {history.length > 0 && (
              <>
                <Divider sx={{ my: 2 }} />
                <Typography sx={{ fontWeight: 500 }}>Previous versions</Typography>
                <List dense disablePadding>
                  {history.map((h) => (
                    <ListItemButton key={h.seqno} onClick={() => open(`/documents/history/${h.seqno}/file`)}>
                      <ListItemText primary={`Version ${h.versionnumber ?? ''} - ${h.originalfilename ?? ''}`} secondary={formatDate(h.lastupdated ?? h.createddate)} />
                      <OpenInNew fontSize="small" color="action" />
                    </ListItemButton>
                  ))}
                </List>
              </>
            )}

            {confirmDelete && (
              <Alert severity="warning" sx={{ mt: 2 }} action={<Button color="inherit" size="small" onClick={() => run(() => api.delete(`/documents/${id}`))}>Delete</Button>}>
                Remove this document from the library?
              </Alert>
            )}
          </>
        )}
      </DialogContent>
      <DialogActions>
        {!isNew && editable && !confirmDelete && <Button color="error" startIcon={<Delete />} sx={{ mr: 'auto' }} onClick={() => setConfirmDelete(true)}>Delete</Button>}
        <Button onClick={() => onClose(false)}>{editable ? 'Cancel' : 'Close'}</Button>
        {editable && isNew && <Button variant="contained" startIcon={<UploadFile />} onClick={upload} disabled={busy || !file || !doc?.documentname?.trim()}>Upload</Button>}
        {editable && !isNew && <Button variant="contained" onClick={save} disabled={busy || !doc?.documentname?.trim()}>Save</Button>}
      </DialogActions>
    </Dialog>
  );
}

DocumentDialog.propTypes = {
  id: PropTypes.number.isRequired,
  types: PropTypes.arrayOf(PropTypes.string).isRequired,
  canEdit: PropTypes.bool,
  onClose: PropTypes.func.isRequired
};
