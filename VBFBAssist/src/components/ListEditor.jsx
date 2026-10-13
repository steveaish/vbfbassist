import { useCallback, useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import {
  Alert, Button, Chip, Dialog, DialogActions, DialogContent, DialogTitle, IconButton, Paper, Stack,
  Table, TableBody, TableCell, TableHead, TableRow
} from '@mui/material';
import { Add, ArrowBack, Delete } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { api, ApiErrorMessage } from '../utils/ApiUtils';
import { TextInput, YesNoSwitch } from './FormFields';
import { ConfirmDialog } from './Modals';

/**
 * Small settings lists (crews, training items...): GET/POST <endpoint>, PUT/DELETE <endpoint>/{id}.
 * fields: [{ field, label, type: 'text' | 'multiline' | 'yesno', list: true to show as a column }]
 */
export default function ListEditor({ title, endpoint, fields, idField = 'id', newItem = {} }) {
  const navigate = useNavigate();
  const [rows, setRows] = useState([]);
  const [editing, setEditing] = useState(null);
  const [deleting, setDeleting] = useState(null);
  const [message, setMessage] = useState(null);

  const load = useCallback(() => {
    api.get(endpoint).then((r) => setRows(r.data)).catch((err) => setMessage({ severity: 'error', text: ApiErrorMessage(err) }));
  }, [endpoint]);

  useEffect(() => { load(); }, [load]);

  const save = async () => {
    try {
      if (editing[idField]) await api.put(`${endpoint}/${editing[idField]}`, editing);
      else await api.post(endpoint, editing);
      setEditing(null);
      load();
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  const remove = async () => {
    const row = deleting;
    setDeleting(null);
    try {
      const r = await api.delete(`${endpoint}/${row[idField]}`);
      if (r.data?.message) setMessage({ severity: 'info', text: r.data.message });
      load();
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  const listFields = fields.filter((f) => f.list);
  const display = (row, f) => f.type === 'yesno'
    ? <Chip size="small" label={row[f.field] === 'Y' ? 'Yes' : 'No'} color={row[f.field] === 'Y' ? 'primary' : 'default'} />
    : row[f.field];

  return (
    <div className="vbfb-page">
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
        <Button startIcon={<ArrowBack />} onClick={() => navigate('/settings')}>Settings</Button>
        <h3 className="vbfb-page-title" style={{ margin: 0, flexGrow: 1 }}>{title}</h3>
        <Button variant="contained" startIcon={<Add />} onClick={() => setEditing({ ...newItem })}>Add</Button>
      </Stack>

      {message && <Alert severity={message.severity} sx={{ mb: 2 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      <Paper sx={{ maxWidth: 900 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              {listFields.map((f) => <TableCell key={f.field}>{f.label}</TableCell>)}
              <TableCell />
            </TableRow>
          </TableHead>
          <TableBody>
            {rows.map((row) => (
              <TableRow key={row[idField]} hover sx={{ cursor: 'pointer' }} onClick={() => setEditing({ ...row })}>
                {listFields.map((f) => <TableCell key={f.field}>{display(row, f)}</TableCell>)}
                <TableCell align="right" padding="checkbox">
                  <IconButton size="small" aria-label="Delete" onClick={(e) => { e.stopPropagation(); setDeleting(row); }}>
                    <Delete fontSize="small" />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Paper>

      <Dialog open={!!editing} onClose={() => setEditing(null)} fullWidth maxWidth="sm">
        <DialogTitle>{editing?.[idField] ? 'Edit' : 'Add'} {title.replace(/s$/, '')}</DialogTitle>
        <DialogContent>
          {editing && fields.map((f) => (
            f.type === 'yesno'
              ? <YesNoSwitch key={f.field} form={editing} setForm={setEditing} field={f.field} label={f.label} />
              : <TextInput key={f.field} form={editing} setForm={setEditing} field={f.field} label={f.label}
                  multiline={f.type === 'multiline'} minRows={f.type === 'multiline' ? 3 : undefined} autoFocus={f === fields[0]} />
          ))}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditing(null)}>Cancel</Button>
          <Button variant="contained" onClick={save}>Save</Button>
        </DialogActions>
      </Dialog>

      <ConfirmDialog
        open={!!deleting}
        title={`Delete ${title.replace(/s$/, '').toLowerCase()}`}
        message={deleting ? `Delete "${deleting[fields[0].field]}"?` : ''}
        danger
        confirmText="Delete"
        onConfirm={remove}
        onClose={() => setDeleting(null)}
      />
    </div>
  );
}

ListEditor.propTypes = {
  title: PropTypes.string.isRequired,
  endpoint: PropTypes.string.isRequired,
  fields: PropTypes.arrayOf(PropTypes.shape({
    field: PropTypes.string.isRequired,
    label: PropTypes.string.isRequired,
    type: PropTypes.oneOf(['text', 'multiline', 'yesno']),
    list: PropTypes.bool
  })).isRequired,
  idField: PropTypes.string,
  newItem: PropTypes.object
};
