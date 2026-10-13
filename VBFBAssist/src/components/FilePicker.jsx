import PropTypes from 'prop-types';
import { Button, Stack, Typography } from '@mui/material';
import { AttachFile } from '@mui/icons-material';

// File chooser button showing the chosen file name (works with the phone camera/files picker too)
export default function FilePicker({ file, onChange, label = 'Choose file', accept }) {
  return (
    <Stack direction="row" spacing={1} alignItems="center" sx={{ mt: 1.5 }}>
      <Button variant="outlined" component="label" startIcon={<AttachFile />}>
        {label}
        <input hidden type="file" accept={accept} onChange={(e) => onChange(e.target.files?.[0] ?? null)} />
      </Button>
      <Typography variant="body2" color="text.secondary" sx={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
        {file ? `${file.name} (${Math.ceil(file.size / 1024)} KB)` : 'No file chosen'}
      </Typography>
    </Stack>
  );
}

FilePicker.propTypes = {
  file: PropTypes.object,
  onChange: PropTypes.func.isRequired,
  label: PropTypes.string,
  accept: PropTypes.string
};
