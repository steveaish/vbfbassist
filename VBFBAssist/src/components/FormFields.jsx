import PropTypes from 'prop-types';
import { FormControlLabel, MenuItem, Switch, TextField } from '@mui/material';

// Small wrappers so edit screens bind straight to a record of DB column names:
// <TextInput form={form} setForm={setForm} field="firstname" label="First Name" />

export function TextInput({ form, setForm, field, label, ...rest }) {
  return (
    <TextField
      label={label}
      fullWidth
      size="small"
      margin="dense"
      value={form[field] ?? ''}
      onChange={(e) => setForm({ ...form, [field]: e.target.value })}
      {...rest}
    />
  );
}

TextInput.propTypes = {
  form: PropTypes.object.isRequired,
  setForm: PropTypes.func.isRequired,
  field: PropTypes.string.isRequired,
  label: PropTypes.string.isRequired
};

// Y/N column shown as a switch
export function YesNoSwitch({ form, setForm, field, label }) {
  return (
    <FormControlLabel
      sx={{ display: 'flex', my: 0.5 }}
      control={
        <Switch
          checked={form[field] === 'Y'}
          onChange={(e) => setForm({ ...form, [field]: e.target.checked ? 'Y' : 'N' })}
        />
      }
      label={label}
    />
  );
}

YesNoSwitch.propTypes = {
  form: PropTypes.object.isRequired,
  setForm: PropTypes.func.isRequired,
  field: PropTypes.string.isRequired,
  label: PropTypes.string.isRequired
};

// options: [{ value, label }]
export function SelectInput({ form, setForm, field, label, options, ...rest }) {
  return (
    <TextField
      select
      label={label}
      fullWidth
      size="small"
      margin="dense"
      value={form[field] ?? ''}
      onChange={(e) => setForm({ ...form, [field]: e.target.value })}
      {...rest}
    >
      {options.map((o) => <MenuItem key={String(o.value)} value={o.value}>{o.label}</MenuItem>)}
    </TextField>
  );
}

SelectInput.propTypes = {
  form: PropTypes.object.isRequired,
  setForm: PropTypes.func.isRequired,
  field: PropTypes.string.isRequired,
  label: PropTypes.string.isRequired,
  options: PropTypes.arrayOf(PropTypes.shape({ value: PropTypes.any, label: PropTypes.string })).isRequired
};
