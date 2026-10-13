import PropTypes from 'prop-types';
import { Box, Button, Chip, Table, TableBody, TableCell, TableHead, TableRow, Typography } from '@mui/material';
import { formatDate, useIsMobile } from '../utils/GridUtils';

// PPE held by a member (ppe_stock rows). actions(row) returns the buttons for that row.
export default function PpeTable({ rows, actions, emptyText = 'No PPE issued.' }) {
  const isMobile = useIsMobile();

  if (rows.length === 0) return <Typography color="text.secondary" sx={{ py: 2 }}>{emptyText}</Typography>;

  const status = (r) => {
    if (r.deleted === 'Y') return <Chip size="small" label="Retired" />;
    if (r.returned === 'Y') return <Chip size="small" label="Returned" />;
    return <Chip size="small" color="primary" label="Issued" />;
  };

  // Phones get stacked cards instead of a wide table
  if (isMobile) {
    return (
      <Box sx={{ display: 'grid', gap: 1 }}>
        {rows.map((r) => (
          <Box key={r.id} sx={{ border: 1, borderColor: 'divider', borderRadius: 2, p: 1.5, bgcolor: 'background.paper' }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Typography fontWeight={500}>{r.Item} - {r.Style}</Typography>
              {status(r)}
            </Box>
            <Typography variant="body2" color="text.secondary">
              Issued {formatDate(r.issuedate)}
              {r.laststocktakedate && <> · Checked {formatDate(r.laststocktakedate)}</>}
              {r.returndate && <> · Ended {formatDate(r.returndate)}</>}
            </Typography>
            {r.notes && <Typography variant="body2">{r.notes}</Typography>}
            <Box sx={{ display: 'flex', gap: 1, mt: 1, flexWrap: 'wrap' }}>{actions(r)}</Box>
          </Box>
        ))}
      </Box>
    );
  }

  return (
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell>Item</TableCell>
          <TableCell>Style</TableCell>
          <TableCell>Issued</TableCell>
          <TableCell>Returned / Retired</TableCell>
          <TableCell>Last PPE Check</TableCell>
          <TableCell>Notes</TableCell>
          <TableCell>Status</TableCell>
          <TableCell align="right" />
        </TableRow>
      </TableHead>
      <TableBody>
        {rows.map((r) => (
          <TableRow key={r.id} hover>
            <TableCell>{r.Item}</TableCell>
            <TableCell>{r.Style}</TableCell>
            <TableCell>{formatDate(r.issuedate)}</TableCell>
            <TableCell>{formatDate(r.returndate)}</TableCell>
            <TableCell>{formatDate(r.laststocktakedate)}</TableCell>
            <TableCell>{r.notes}</TableCell>
            <TableCell>{status(r)}</TableCell>
            <TableCell align="right" sx={{ whiteSpace: 'nowrap' }}>{actions(r)}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}

PpeTable.propTypes = {
  rows: PropTypes.array.isRequired,
  actions: PropTypes.func.isRequired,
  emptyText: PropTypes.string
};

// Convenience for action buttons
export function PpeButton(props) {
  return <Button size="small" variant="outlined" sx={{ ml: 1 }} {...props} />;
}
