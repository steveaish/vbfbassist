import { useCallback, useEffect, useMemo, useState } from 'react';
import { Alert, Box, Button, IconButton, Paper, Stack, TextField, Typography } from '@mui/material';
import { Add, ChevronLeft, ChevronRight, Today } from '@mui/icons-material';
import dayjs from 'dayjs';
import { api, ApiErrorMessage } from '../utils/ApiUtils';
import { useIsMobile } from '../utils/GridUtils';
import EventDialog from './EventDialog';
import { textOn } from './calendarUtils';

const weekdays = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

const timeLabel = (e) => (e.start ? `${e.start}${e.end ? `–${e.end}` : ''} ` : '');

// Brigade calendar (Laravel /calendar): month grid on desktop, agenda list on phones
export default function Calendar() {
  const isMobile = useIsMobile();
  const [month, setMonth] = useState(dayjs().startOf('month'));
  const [items, setItems] = useState([]);
  const [canEdit, setCanEdit] = useState(false);
  const [dialog, setDialog] = useState(null); // { id } to view/edit, { date } to add
  const [message, setMessage] = useState(null);

  const load = useCallback(() => {
    api.get('/calendar', { params: { year: month.year(), month: month.month() + 1 } })
      .then((r) => { setItems(r.data.items); setCanEdit(r.data.canedit); })
      .catch((err) => setMessage({ severity: 'error', text: ApiErrorMessage(err) }));
  }, [month]);

  useEffect(() => { api.post('/calendar/opened').catch(() => {}); }, []);
  useEffect(() => { load(); }, [load]);

  const byDate = useMemo(() => items.reduce((acc, e) => {
    (acc[e.date] = acc[e.date] ?? []).push(e);
    return acc;
  }, {}), [items]);

  // Grid cells: blanks before the 1st, then each day
  const cells = useMemo(() => {
    const blanks = Array(month.day()).fill(null);
    const days = Array.from({ length: month.daysInMonth() }, (_, i) => month.date(i + 1));
    return [...blanks, ...days];
  }, [month]);

  const today = dayjs().format('YYYY-MM-DD');

  const eventChip = (e, compact) => (
    <Box
      key={e.id}
      component="button"
      type="button"
      onClick={() => setDialog({ id: e.id })}
      sx={{
        display: 'block', width: '100%', textAlign: 'left', border: 1, borderColor: 'divider', borderRadius: 1,
        px: 0.75, py: compact ? 0.25 : 0.75, mb: 0.5, cursor: 'pointer', font: 'inherit',
        fontSize: compact ? '0.75rem' : '0.9rem', bgcolor: e.colour, color: textOn(e.colour),
        overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: compact ? 'nowrap' : 'normal'
      }}
    >
      {timeLabel(e)}{e.description}
    </Box>
  );

  return (
    <div className="vbfb-page">
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }} flexWrap="wrap" useFlexGap>
        <h3 className="vbfb-page-title" style={{ margin: 0, flexGrow: 1 }}>Calendar</h3>
        <Stack direction="row" alignItems="center" spacing={0.5}>
          <IconButton aria-label="Previous month" onClick={() => setMonth(month.subtract(1, 'month'))}><ChevronLeft /></IconButton>
          <TextField
            type="month" size="small" value={month.format('YYYY-MM')} inputProps={{ 'aria-label': 'Month' }} sx={{ width: 170 }}
            onChange={(e) => e.target.value && setMonth(dayjs(`${e.target.value}-01`))}
          />
          <IconButton aria-label="Next month" onClick={() => setMonth(month.add(1, 'month'))}><ChevronRight /></IconButton>
        </Stack>
        <Button size="small" startIcon={<Today />} onClick={() => setMonth(dayjs().startOf('month'))}>Today</Button>
        {canEdit && <Button variant="contained" size="small" startIcon={<Add />} onClick={() => setDialog({ date: today })}>Add</Button>}
      </Stack>

      {message && <Alert severity={message.severity} sx={{ mb: 2 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      {isMobile ? (
        <Stack spacing={1}>
          {Object.keys(byDate).length === 0 && <Typography color="text.secondary">Nothing on this month.</Typography>}
          {Object.entries(byDate).map(([date, events]) => (
            <Paper key={date} variant="outlined" sx={{ p: 1.5, borderLeft: 4, borderLeftColor: date === today ? 'secondary.main' : 'primary.main' }}>
              <Typography sx={{ fontWeight: 500, mb: 0.5 }}>{dayjs(date).format('dddd D MMMM')}</Typography>
              {events.map((e) => eventChip(e, false))}
            </Paper>
          ))}
        </Stack>
      ) : (
        <Paper sx={{ overflow: 'hidden' }}>
          <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)' }}>
            {weekdays.map((d) => (
              <Box key={d} sx={{ p: 1, textAlign: 'center', fontWeight: 500, bgcolor: 'var(--vbfb-green-light)', color: 'primary.dark' }}>{d}</Box>
            ))}
            {cells.map((d, i) => {
              if (!d) return <Box key={`b${i}`} sx={{ borderTop: 1, borderRight: 1, borderColor: 'divider', bgcolor: 'grey.50' }} />;
              const key = d.format('YYYY-MM-DD');
              return (
                <Box key={key} sx={{ minHeight: 110, p: 0.5, borderTop: 1, borderRight: 1, borderColor: 'divider', minWidth: 0 }}>
                  <Stack direction="row" alignItems="center" sx={{ mb: 0.5 }}>
                    <Typography variant="body2" sx={{
                      fontWeight: key === today ? 700 : 400,
                      color: key === today ? 'secondary.main' : 'text.primary',
                      flexGrow: 1
                    }}>
                      {d.date()}
                    </Typography>
                    {canEdit && (
                      <IconButton size="small" aria-label={`Add event on ${d.format('D MMMM')}`} sx={{ p: 0.25 }} onClick={() => setDialog({ date: key })}>
                        <Add fontSize="inherit" />
                      </IconButton>
                    )}
                  </Stack>
                  {(byDate[key] ?? []).map((e) => eventChip(e, true))}
                </Box>
              );
            })}
          </Box>
        </Paper>
      )}

      {dialog && (
        <EventDialog
          id={dialog.id}
          date={dialog.date}
          canEdit={canEdit}
          onClose={(changed) => { setDialog(null); if (changed) load(); }}
        />
      )}
    </div>
  );
}
