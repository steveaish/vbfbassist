import PropTypes from 'prop-types';
import { Box, Button, Chip, Divider, Stack, Typography } from '@mui/material';
import { Call, Email, Sms } from '@mui/icons-material';
import { yesNo } from '../utils/GridUtils';
import { membershipFlags } from '../profiles/profileOptions';

const accessLabels = [
  { field: 'isadmin', label: 'Admin' },
  { field: 'IsVehicleMaint', label: 'Vehicle Maint' },
  { field: 'cadetleader', label: 'Cadet Leader' },
  { field: 'isfacilitator', label: 'BFR Facilitator' },
  { field: 'iscoordinator', label: 'BFR Coordinator' }
];

// Read-only member card (Laravel members/view and the Details tab of My Page)
export default function MemberDetails({ member }) {
  const row = (label, value) => value ? (
    <Box sx={{ display: 'flex', py: 0.75 }}>
      <Typography sx={{ width: 140, flexShrink: 0 }} color="text.secondary">{label}</Typography>
      <Typography>{value}</Typography>
    </Box>
  ) : null;

  const chips = (flags) => flags.filter((f) => member[f.field] === 'Y').map((f) => <Chip key={f.field} label={f.label} size="small" sx={{ mr: 0.5, mb: 0.5 }} />);
  const access = chips(accessLabels);
  const membership = chips(membershipFlags);

  return (
    <Box>
      <Stack direction="row" spacing={1} sx={{ mb: 2, flexWrap: 'wrap', gap: 1 }}>
        {member.mobilephone && <Button variant="contained" startIcon={<Call />} href={`tel:${member.mobilephone}`}>Mobile</Button>}
        {member.mobilephone && <Button variant="outlined" startIcon={<Sms />} href={`sms:${member.mobilephone}`}>SMS</Button>}
        {member.homephone && <Button variant="outlined" startIcon={<Call />} href={`tel:${member.homephone}`}>Home</Button>}
        {member.email && <Button variant="outlined" startIcon={<Email />} href={`mailto:${member.email}`}>Email</Button>}
      </Stack>

      {row('Email', member.email)}
      {row('Mobile', member.mobilephone)}
      {row('Home Phone', member.homephone)}
      {row('Male / Female', member.sex)}
      {row('Active', yesNo(member.isactive))}
      {row('Driver Licence', member.dlicense)}
      {row('DFES Number', member.dfesnumber)}
      {row('BART Active', yesNo(member.bartactive))}
      {row('Pref Comm', member.prefcomm)}
      {row('Crew', member.CrewName)}

      {access.length > 0 && (
        <>
          <Divider sx={{ my: 1.5 }} />
          <Typography sx={{ fontWeight: 500, mb: 1 }}>Access</Typography>
          {access}
        </>
      )}
      {membership.length > 0 && (
        <>
          <Divider sx={{ my: 1.5 }} />
          <Typography sx={{ fontWeight: 500, mb: 1 }}>Membership</Typography>
          {membership}
        </>
      )}
    </Box>
  );
}

MemberDetails.propTypes = {
  member: PropTypes.object.isRequired
};
