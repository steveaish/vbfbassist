import { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import { Link } from 'react-router-dom';
import { Alert, Box, Card, CardContent, Grid, Stack, Tooltip, Typography } from '@mui/material';
import { api } from '../utils/ApiUtils';

// Chart bars: a brand-family green that passes the chart colour checks (chroma, lightness band,
// contrast on white) - the logo green #12654A is too low in chroma and reads grey as a mark.
const BAR = '#1A8A5E';

// Home page widgets from the reports table (Laravel home dashboard)
export default function DashboardWidgets() {
  const [widgets, setWidgets] = useState(null);

  useEffect(() => {
    api.get('/dashboard').then((r) => setWidgets(r.data)).catch(() => setWidgets([]));
  }, []);

  if (!widgets || widgets.length === 0) return null;

  return (
    <Grid container spacing={2} sx={{ mt: 1 }}>
      {widgets.map((w) => (
        <Grid item xs={12} md={6} lg={w.type === 'grid' ? 6 : 4} key={w.id}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Typography variant="h6" sx={{ fontSize: '1rem', fontWeight: 500, mb: 1.5 }}>{w.name}</Typography>
              {w.error && <Alert severity="warning">{w.error}</Alert>}
              {!w.error && (w.type === 'grid'
                ? <ListWidget rows={w.rows} labelColumn={w.column_label} dataColumn={w.column_data} />
                : <BarList rows={w.rows} unit={w.column_data} />)}
            </CardContent>
          </Card>
        </Grid>
      ))}
    </Grid>
  );
}

// Horizontal bars sorted largest first, value at the bar end, tooltip with the share of the total.
// Replaces Laravel's doughnuts: lengths on a common baseline compare more accurately than angles.
function BarList({ rows, unit }) {
  const data = rows
    .map((r) => ({ label: String(r.TheLabel ?? 'Not set'), value: Number(r.Count ?? 0) }))
    .filter((d) => d.value > 0)
    .sort((a, b) => b.value - a.value);
  if (data.length === 0) return <Typography variant="body2" color="text.secondary">No data.</Typography>;

  const max = Math.max(...data.map((d) => d.value));
  const total = data.reduce((s, d) => s + d.value, 0);

  return (
    <Stack spacing={1} role="list" aria-label="Bar chart">
      {data.map((d) => (
        <Tooltip key={d.label} placement="top" arrow
          title={`${d.label}: ${d.value} ${unit ? unit.toLowerCase() : ''} (${Math.round((100 * d.value) / total)}% of ${total})`}>
          <Box role="listitem" sx={{ display: 'grid', gridTemplateColumns: 'minmax(90px, 40%) 1fr', alignItems: 'center', gap: 1, py: 0.25, cursor: 'default' }}>
            <Typography variant="body2" sx={{ color: 'text.secondary', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{d.label}</Typography>
            <Stack direction="row" alignItems="center" spacing={0.75}>
              <Box sx={{ height: 14, width: `${Math.max(2, (100 * d.value) / max)}%`, bgcolor: BAR, borderRadius: '0 4px 4px 0' }} />
              <Typography variant="body2" sx={{ color: 'text.primary', fontVariantNumeric: 'tabular-nums' }}>{d.value}</Typography>
            </Stack>
          </Box>
        </Tooltip>
      ))}
    </Stack>
  );
}

BarList.propTypes = {
  rows: PropTypes.array.isRequired,
  unit: PropTypes.string
};

// Simple list (e.g. Upcoming Events). Laravel links like /calendar/item/5 go to the new calendar.
function ListWidget({ rows, labelColumn, dataColumn }) {
  if (rows.length === 0) return <Typography variant="body2" color="text.secondary">Nothing to show.</Typography>;
  const label = labelColumn ?? Object.keys(rows[0])[0];
  const data = dataColumn ?? Object.keys(rows[0])[1];

  return (
    <Stack divider={<Box sx={{ borderTop: 1, borderColor: 'divider' }} />}>
      {rows.map((r, i) => {
        const content = (
          <Stack direction="row" spacing={2} sx={{ py: 0.75 }}>
            <Typography variant="body2" sx={{ color: 'text.secondary', minWidth: 130 }}>{String(r[label] ?? '')}</Typography>
            <Typography variant="body2">{String(r[data] ?? '')}</Typography>
          </Stack>
        );
        return r.html_link?.startsWith('/calendar')
          ? <Box key={i} component={Link} to="/calendar" sx={{ color: 'inherit', textDecoration: 'none' }}>{content}</Box>
          : <Box key={i}>{content}</Box>;
      })}
    </Stack>
  );
}

ListWidget.propTypes = {
  rows: PropTypes.array.isRequired,
  labelColumn: PropTypes.string,
  dataColumn: PropTypes.string
};
