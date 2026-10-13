import { Link } from 'react-router-dom';
import { Card, CardActionArea, CardContent, Grid, Typography } from '@mui/material';
import {
  Badge, CalendarMonth, Description, DirectionsCar, Groups, HealthAndSafety,
  LocalFireDepartment, Person, School, Sms
} from '@mui/icons-material';
import { hasPermission } from '../utils/LoginUtils';
import { menuItems } from '../menu';
import DashboardWidgets from './DashboardWidgets';

const icons = {
  mypage: Person,
  members: Groups,
  profiles: Badge,
  cadets: School,
  bfr: LocalFireDepartment,
  vehicles: DirectionsCar,
  ppe: HealthAndSafety,
  calendar: CalendarMonth,
  documents: Description,
  bulksms: Sms
};

export default function Home() {
  const tiles = menuItems.filter((m) => !m.permission || hasPermission(m.permission));

  return (
    <div className="vbfb-page">
      <h3 className="vbfb-page-title">Welcome, {sessionStorage.getItem('firstname')}</h3>

      <Grid container spacing={2}>
        {tiles.map((tile) => {
          const Icon = icons[tile.key] ?? Description;
          return (
            <Grid item xs={6} sm={4} md={3} key={tile.key}>
              <Card sx={{ height: '100%' }}>
                <CardActionArea component={Link} to={tile.path} sx={{ height: '100%' }}>
                  <CardContent sx={{ textAlign: 'center', py: 3 }}>
                    <Icon sx={{ fontSize: 44, color: 'primary.main' }} />
                    <Typography sx={{ mt: 1, fontWeight: 500 }}>{tile.label}</Typography>
                  </CardContent>
                </CardActionArea>
              </Card>
            </Grid>
          );
        })}
      </Grid>
      <DashboardWidgets />
    </div>
  );
}
