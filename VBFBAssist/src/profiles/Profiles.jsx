import { useEffect, useMemo, useRef, useState } from 'react';
import {
  ColumnDirective, ColumnsDirective, ExcelExport, GridComponent, Inject, Page, Resize, Sort
} from '@syncfusion/ej2-react-grids';
import { Query } from '@syncfusion/ej2-data';
import {
  Box, Button, Checkbox, FormControlLabel, InputAdornment, ListItemText, MenuItem, Stack, TextField
} from '@mui/material';
import { Add, FileDownload, Search } from '@mui/icons-material';
import { api } from '../utils/ApiUtils';
import { createGridDataManager, DESKTOP_ONLY } from '../utils/GridUtils';
import { typeFilterOptions } from './profileOptions';
import ProfileEdit from './ProfileEdit';

// Admin list of member profiles (Laravel /profiles). Row click opens the edit screen.
export default function Profiles() {
  const grid = useRef(null);
  const [editId, setEditId] = useState(null); // null = list, 0 = new, n = edit
  const [crews, setCrews] = useState([]);
  const [search, setSearch] = useState('');
  const [showInactive, setShowInactive] = useState(false);
  const [types, setTypes] = useState([]);
  const [crewFilter, setCrewFilter] = useState([]);

  const dataManager = useMemo(() => createGridDataManager('/profiles/list'), []);

  useEffect(() => {
    api.post('/profiles/opened').catch(() => {});
    api.get('/profiles/lookups').then((r) => setCrews(r.data.crews)).catch(() => {});
  }, []);

  // Filters go to the API as DataManager params; debounce the search box
  const [query, setQuery] = useState(new Query());
  useEffect(() => {
    const t = setTimeout(() => {
      setQuery(new Query()
        .addParams('search', search)
        .addParams('showinactive', showInactive ? 'Y' : 'N')
        .addParams('types', types)
        .addParams('crews', crewFilter));
    }, 300);
    return () => clearTimeout(t);
  }, [search, showInactive, types, crewFilter]);

  const exportExcel = () => {
    grid.current?.excelExport({ fileName: 'Profiles.xlsx', exportType: 'AllPages', includeHiddenColumn: true });
  };

  const closeEdit = () => {
    setEditId(null);
    grid.current?.refresh();
  };

  if (editId !== null) {
    return <ProfileEdit id={editId} onClose={closeEdit} />;
  }

  const activeTemplate = (row) => (row.isactive === 'Y' ? 'Yes' : 'No');

  return (
    <div className="vbfb-page">
      <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ sm: 'center' }} spacing={1} sx={{ mb: 2 }}>
        <h3 className="vbfb-page-title" style={{ margin: 0 }}>Profiles</h3>
        <Stack direction="row" spacing={1}>
          <Button variant="outlined" startIcon={<FileDownload />} onClick={exportExcel}>Excel</Button>
          <Button variant="contained" startIcon={<Add />} onClick={() => setEditId(0)}>New Profile</Button>
        </Stack>
      </Stack>

      <Box sx={{ display: 'grid', gap: 1, gridTemplateColumns: { xs: '1fr', md: '2fr 1.5fr 1.5fr auto' }, mb: 2, alignItems: 'center' }}>
        <TextField
          size="small"
          placeholder="Search name or phone"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          InputProps={{ startAdornment: <InputAdornment position="start"><Search /></InputAdornment> }}
        />
        <TextField
          select size="small" label="Type"
          SelectProps={{ multiple: true, renderValue: (v) => v.map((x) => typeFilterOptions.find((o) => o.value === x)?.label).join(', ') }}
          value={types}
          onChange={(e) => setTypes(e.target.value)}
        >
          {typeFilterOptions.map((o) => (
            <MenuItem key={o.value} value={o.value}>
              <Checkbox size="small" checked={types.includes(o.value)} />
              <ListItemText primary={o.label} />
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select size="small" label="Crew"
          SelectProps={{ multiple: true, renderValue: (v) => v.map((x) => crews.find((c) => c.seqno === x)?.CrewName).join(', ') }}
          value={crewFilter}
          onChange={(e) => setCrewFilter(e.target.value)}
        >
          {crews.map((c) => (
            <MenuItem key={c.seqno} value={c.seqno}>
              <Checkbox size="small" checked={crewFilter.includes(c.seqno)} />
              <ListItemText primary={c.CrewName} />
            </MenuItem>
          ))}
        </TextField>
        <FormControlLabel control={<Checkbox checked={showInactive} onChange={(e) => setShowInactive(e.target.checked)} />} label="Show inactive" />
      </Box>

      <GridComponent
        ref={grid}
        dataSource={dataManager}
        query={query}
        allowPaging
        allowSorting
        allowExcelExport
        allowResizing
        pageSettings={{ pageSize: 50, pageSizes: [25, 50, 100, 500] }}
        rowSelected={(e) => setEditId(e.data.id)}
        enableHover
      >
        <ColumnsDirective>
          <ColumnDirective field="firstname" headerText="First Name" width="110" />
          <ColumnDirective field="lastname" headerText="Last Name" width="110" />
          <ColumnDirective field="mobilephone" headerText="Mobile" width="115" />
          <ColumnDirective field="homephone" headerText="Home Phone" width="130" hideAtMedia={DESKTOP_ONLY} />
          <ColumnDirective field="CrewName" headerText="Crew" width="110" hideAtMedia={DESKTOP_ONLY} />
          <ColumnDirective field="alltypes" headerText="Type" width="220" hideAtMedia={DESKTOP_ONLY} allowSorting={false} />
          <ColumnDirective field="Notes" headerText="Notes" width="200" hideAtMedia={DESKTOP_ONLY} allowSorting={false} />
          <ColumnDirective field="isactive" headerText="Active" width="80" template={activeTemplate} hideAtMedia={DESKTOP_ONLY} />
          {/* Export-only columns (Laravel's CSV export had these) */}
          <ColumnDirective field="email" headerText="Email" visible={false} />
          <ColumnDirective field="dfesnumber" headerText="DFES Number" visible={false} />
          <ColumnDirective field="address" headerText="Address" visible={false} />
          <ColumnDirective field="dlicense" headerText="Driver Licence" visible={false} />
          <ColumnDirective field="dob" headerText="DOB" visible={false} />
          <ColumnDirective field="sex" headerText="Sex" visible={false} />
          <ColumnDirective field="workphone" headerText="Work Phone" visible={false} />
          <ColumnDirective field="startdate" headerText="Start Date" visible={false} />
          <ColumnDirective field="bartactive" headerText="BART" visible={false} />
          <ColumnDirective field="prefcomm" headerText="Pref Comm" visible={false} />
          <ColumnDirective field="caneditcalendar" headerText="Can Edit Calendar" visible={false} />
          <ColumnDirective field="canbulksms" headerText="Can Bulk SMS" visible={false} />
          <ColumnDirective field="caregivername" headerText="Caregiver" visible={false} />
          <ColumnDirective field="caregivermobile" headerText="Caregiver Mobile" visible={false} />
          <ColumnDirective field="caregiverhome" headerText="Caregiver Home" visible={false} />
          <ColumnDirective field="caregiverwork" headerText="Caregiver Work" visible={false} />
          <ColumnDirective field="cananyoneelsepickup" headerText="Anyone Else Pick Up" visible={false} />
          <ColumnDirective field="whocanpickup" headerText="Who Can Pick Up" visible={false} />
          <ColumnDirective field="whocannotpickup" headerText="Who Cannot Pick Up" visible={false} />
          <ColumnDirective field="allergies" headerText="Allergies" visible={false} />
          <ColumnDirective field="allergydetails" headerText="Allergy Details" visible={false} />
          <ColumnDirective field="medicalyn" headerText="Medical" visible={false} />
          <ColumnDirective field="medicaldetails" headerText="Medical Details" visible={false} />
          <ColumnDirective field="photossocialmedia" headerText="Photos on Social Media" visible={false} />
          <ColumnDirective field="cadetleader" headerText="Cadet Leader" visible={false} />
        </ColumnsDirective>
        <Inject services={[Page, Sort, ExcelExport, Resize]} />
      </GridComponent>
    </div>
  );
}
