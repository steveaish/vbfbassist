import { useEffect, useMemo, useState } from 'react';
import {
  ColumnDirective, ColumnsDirective, GridComponent, Inject, Page, Sort
} from '@syncfusion/ej2-react-grids';
import { Query } from '@syncfusion/ej2-data';
import { Box, Button, Checkbox, CircularProgress, FormControlLabel, InputAdornment, Paper, Stack, TextField } from '@mui/material';
import { ArrowBack, Search } from '@mui/icons-material';
import { api } from '../utils/ApiUtils';
import { createGridDataManager, DESKTOP_ONLY } from '../utils/GridUtils';
import { isCurrentUserAdmin } from '../utils/LoginUtils';
import MemberDetails from './MemberDetails';
import ProfileEdit from '../profiles/ProfileEdit';

// Member directory for people with View Members (Laravel /members). Admins can jump to the full profile.
export default function Members() {
  const [search, setSearch] = useState('');
  const [showInactive, setShowInactive] = useState(false);
  const [query, setQuery] = useState(new Query());
  const [viewId, setViewId] = useState(null);
  const [member, setMember] = useState(null);
  const [editing, setEditing] = useState(false);

  const dataManager = useMemo(() => createGridDataManager('/members/list'), []);

  useEffect(() => { api.post('/members/opened').catch(() => {}); }, []);

  useEffect(() => {
    const t = setTimeout(() => {
      setQuery(new Query().addParams('search', search).addParams('showinactive', showInactive ? 'Y' : 'N'));
    }, 300);
    return () => clearTimeout(t);
  }, [search, showInactive]);

  useEffect(() => {
    if (viewId === null) return;
    setMember(null);
    api.get(`/members/${viewId}`).then((r) => setMember(r.data)).catch(() => setViewId(null));
  }, [viewId]);

  if (editing) {
    return <ProfileEdit id={viewId} onClose={() => { setEditing(false); setViewId(null); }} />;
  }

  if (viewId !== null) {
    return (
      <div className="vbfb-page">
        <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
          <Button startIcon={<ArrowBack />} onClick={() => setViewId(null)}>Back</Button>
          <h3 className="vbfb-page-title" style={{ margin: 0, flexGrow: 1 }}>
            {member ? `${member.firstname ?? ''} ${member.lastname ?? ''}` : ''}
          </h3>
          {isCurrentUserAdmin() && <Button variant="outlined" onClick={() => setEditing(true)}>Edit Profile</Button>}
        </Stack>
        <Paper sx={{ p: { xs: 2, sm: 3 }, maxWidth: 720 }}>
          {member ? <MemberDetails member={member} /> : <CircularProgress />}
        </Paper>
      </div>
    );
  }

  return (
    <div className="vbfb-page">
      <h3 className="vbfb-page-title">Members</h3>
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
          <ColumnDirective field="mobilephone" headerText="Mobile" width="140" />
          <ColumnDirective field="CrewName" headerText="Crew" width="120" hideAtMedia={DESKTOP_ONLY} />
          <ColumnDirective field="alltypes" headerText="Type" width="260" hideAtMedia={DESKTOP_ONLY} allowSorting={false} />
        </ColumnsDirective>
        <Inject services={[Page, Sort]} />
      </GridComponent>
    </div>
  );
}
