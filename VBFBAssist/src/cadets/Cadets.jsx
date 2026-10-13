import { useEffect, useMemo, useRef, useState } from 'react';
import { ColumnDirective, ColumnsDirective, GridComponent, Inject, Page, Sort } from '@syncfusion/ej2-react-grids';
import { Query } from '@syncfusion/ej2-data';
import { Box, Button, Checkbox, FormControlLabel, InputAdornment, Stack, TextField } from '@mui/material';
import { PlaylistAdd, Search } from '@mui/icons-material';
import { api } from '../utils/ApiUtils';
import { createGridDataManager, DESKTOP_ONLY } from '../utils/GridUtils';
import CadetEdit from './CadetEdit';
import TrainingEntry from './TrainingEntry';

// Cadet list for cadet leaders (Laravel /cadets). Admins see the caregiver columns too, as before.
export default function Cadets() {
  const grid = useRef(null);
  const [search, setSearch] = useState('');
  const [showInactive, setShowInactive] = useState(false);
  const [query, setQuery] = useState(new Query());
  const [viewId, setViewId] = useState(null);
  const [recording, setRecording] = useState(false);

  const dataManager = useMemo(() => createGridDataManager('/cadets/list'), []);

  useEffect(() => { api.post('/cadets/opened').catch(() => {}); }, []);

  useEffect(() => {
    const t = setTimeout(() => {
      setQuery(new Query().addParams('search', search).addParams('showinactive', showInactive ? 'Y' : 'N'));
    }, 300);
    return () => clearTimeout(t);
  }, [search, showInactive]);

  if (recording) return <TrainingEntry onClose={() => setRecording(false)} />;
  if (viewId !== null) return <CadetEdit id={viewId} onClose={() => { setViewId(null); grid.current?.refresh(); }} />;

  return (
    <div className="vbfb-page">
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
        <h3 className="vbfb-page-title" style={{ margin: 0 }}>Cadets</h3>
        <Button variant="contained" startIcon={<PlaylistAdd />} onClick={() => setRecording(true)}>Record Training</Button>
      </Stack>

      <Box sx={{ display: 'flex', gap: 2, mb: 2, flexWrap: 'wrap', alignItems: 'center' }}>
        <TextField
          size="small"
          placeholder="Search name or phone"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          sx={{ flex: '1 1 260px', maxWidth: 420 }}
          InputProps={{ startAdornment: <InputAdornment position="start"><Search /></InputAdornment> }}
        />
        <FormControlLabel control={<Checkbox checked={showInactive} onChange={(e) => setShowInactive(e.target.checked)} />} label="Show inactive" />
      </Box>

      <GridComponent
        ref={grid}
        dataSource={dataManager}
        query={query}
        allowPaging
        allowSorting
        pageSettings={{ pageSize: 50 }}
        rowSelected={(e) => setViewId(e.data.id)}
        enableHover
      >
        <ColumnsDirective>
          <ColumnDirective field="fullname" headerText="Name" width="200" />
          <ColumnDirective field="mobilephone" headerText="Mobile" width="130" hideAtMedia={DESKTOP_ONLY} />
          <ColumnDirective field="CrewName" headerText="Crew" width="120" />
          <ColumnDirective field="caregivername" headerText="Caregiver" width="160" hideAtMedia={DESKTOP_ONLY} />
          <ColumnDirective field="caregivermobile" headerText="Caregiver Mobile" width="140" hideAtMedia={DESKTOP_ONLY} />
        </ColumnsDirective>
        <Inject services={[Page, Sort]} />
      </GridComponent>
    </div>
  );
}
