import { Link } from 'react-router-dom';
import { Card, CardActionArea, CardContent, Grid, Typography } from '@mui/material';
import { Checklist, DirectionsCar, Groups, School, ViewList, Workspaces } from '@mui/icons-material';
import ListEditor from '../components/ListEditor';

// Settings home (Laravel /settings). More lists arrive with later phases.
const items = [
  { path: '/settings/crews', label: 'Crews', text: 'Member crews used on profiles.', icon: Groups },
  { path: '/settings/training', label: 'Training Items', text: 'Types of training. This is for cadets only.', icon: School },
  { path: '/settings/cadetcrews', label: 'Cadet Crews', text: 'Crews cadets are placed in.', icon: Workspaces },
  { path: '/settings/vehicles', label: 'Vehicles', text: 'Vehicles and the checklist for each one.', icon: DirectionsCar },
  { path: '/settings/vehiclesections', label: 'Vehicle Sections', text: 'Sections a vehicle checklist is grouped into.', icon: ViewList },
  { path: '/settings/vehicleitems', label: 'Vehicle Items', text: 'Items that can be added to a checklist section.', icon: Checklist }
];

export default function Settings() {
  return (
    <div className="vbfb-page">
      <h3 className="vbfb-page-title">Settings</h3>
      <Grid container spacing={2}>
        {items.map((i) => (
          <Grid item xs={12} sm={6} md={4} key={i.path}>
            <Card sx={{ height: '100%' }}>
              <CardActionArea component={Link} to={i.path} sx={{ height: '100%' }}>
                <CardContent>
                  <i.icon sx={{ fontSize: 36, color: 'primary.main' }} />
                  <Typography variant="h6">{i.label}</Typography>
                  <Typography variant="body2" color="text.secondary">{i.text}</Typography>
                </CardContent>
              </CardActionArea>
            </Card>
          </Grid>
        ))}
      </Grid>
    </div>
  );
}

export function CrewsSettings() {
  return <ListEditor title="Crews" endpoint="/crews" fields={[{ field: 'CrewName', label: 'Name', list: true }]} />;
}

export function CadetCrewsSettings() {
  return <ListEditor title="Cadet Crews" endpoint="/cadetcrews" fields={[{ field: 'CrewName', label: 'Name', list: true }]} />;
}

export function TrainingSettings() {
  return (
    <ListEditor
      title="Training Items"
      endpoint="/training"
      newItem={{ IsActive: 'Y' }}
      fields={[
        { field: 'name', label: 'Name', list: true },
        { field: 'resources', label: 'Resources', type: 'multiline' },
        { field: 'Description', label: 'Description', type: 'multiline' },
        { field: 'IsActive', label: 'Active', type: 'yesno', list: true }
      ]}
    />
  );
}

export function VehicleSectionsSettings() {
  return <ListEditor title="Vehicle Sections" endpoint="/vehiclesetup/sections" fields={[{ field: 'Name', label: 'Name', list: true }]} />;
}

export function VehicleItemsSettings() {
  return <ListEditor title="Vehicle Items" endpoint="/vehiclesetup/items" fields={[{ field: 'Name', label: 'Name', list: true }]} />;
}
