import PropTypes from 'prop-types';
import { Box } from '@mui/material';

// "123 characters · 1 SMS" - texts over 160 characters are split into 153 character parts
export default function SmsLength({ text }) {
  const parts = text.length <= 160 ? 1 : Math.ceil(text.length / 153);
  return <Box component="span">{text.length} characters · {parts} SMS{parts > 1 ? ' each' : ''}</Box>;
}

SmsLength.propTypes = {
  text: PropTypes.string.isRequired
};
