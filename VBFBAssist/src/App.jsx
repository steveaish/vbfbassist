import { BrowserRouter, Navigate, NavLink, Route, Routes } from 'react-router-dom';
import { Container, Nav, Navbar, NavDropdown } from 'react-bootstrap';
import { ThemeProvider, CssBaseline } from '@mui/material';
import { registerLicense } from '@syncfusion/ej2-base';
import PropTypes from 'prop-types';

import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-icons/font/bootstrap-icons.css';
import '@syncfusion/ej2-base/styles/material.css';
import '@syncfusion/ej2-buttons/styles/material.css';
import '@syncfusion/ej2-calendars/styles/material.css';
import '@syncfusion/ej2-dropdowns/styles/material.css';
import '@syncfusion/ej2-inputs/styles/material.css';
import '@syncfusion/ej2-navigations/styles/material.css';
import '@syncfusion/ej2-popups/styles/material.css';
import '@syncfusion/ej2-splitbuttons/styles/material.css';
import '@syncfusion/ej2-react-grids/styles/material.css';

import { vbfbTheme } from './theme';
import { menuItems } from './menu';
import { hasPermission, isCurrentUserAdmin, isSiteAdmin } from './utils/LoginUtils';
import { api, ClearSession } from './utils/ApiUtils';
import logo from './images/logo.png';

import Login from './login/Login';
import ResetPassword from './login/ResetPassword';
import Home from './home/Home';
import ChangePassword from './myprofile/ChangePassword';
import MyPage from './mypage/MyPage';
import Members from './members/Members';
import Profiles from './profiles/Profiles';
import Cadets from './cadets/Cadets';
import VehicleChecks from './vehicles/VehicleChecks';
import VehicleSetup from './vehicles/VehicleSetup';
import PpeItems from './ppe/PpeItems';
import Calendar from './calendar/Calendar';
import Documents from './documents/Documents';
import Bfr from './bfr/Bfr';
import BulkSms from './bulksms/BulkSms';
import AdminUsers from './admin/AdminUsers';
import Settings, {
  CadetCrewsSettings, CrewsSettings, TrainingSettings, VehicleItemsSettings, VehicleSectionsSettings
} from './settings/Settings';

// Screen for each menu key
const screens = {
  mypage: MyPage,
  members: Members,
  profiles: Profiles,
  cadets: Cadets,
  vehicles: VehicleChecks,
  ppe: PpeItems,
  calendar: Calendar,
  documents: Documents,
  bfr: Bfr,
  bulksms: BulkSms
};

registerLicense('Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXZeeXZVQmdZWUN/XkBWYEg=');

async function logout() {
  try {
    await api.post('/login/logout');
  }
  catch {
    // the session is cleared locally either way
  }
  ClearSession();
  window.location.replace('/');
}

// Routes the user can't see redirect home rather than showing an error
function Guard({ permission, children }) {
  if (permission && !hasPermission(permission)) return <Navigate to="/" replace />;
  return children;
}

Guard.propTypes = {
  permission: PropTypes.oneOfType([PropTypes.string, PropTypes.arrayOf(PropTypes.string)]),
  children: PropTypes.node
};

export default function App({ children }) {
  // The emailed reset link works whether or not someone is signed in on this browser
  if (window.location.pathname === '/resetpassword') {
    return (
      <ThemeProvider theme={vbfbTheme}>
        <CssBaseline />
        <ResetPassword />
      </ThemeProvider>
    );
  }

  if (sessionStorage.getItem('loggedIn') !== 'true') {
    return (
      <ThemeProvider theme={vbfbTheme}>
        <CssBaseline />
        <Login />
      </ThemeProvider>
    );
  }

  const visibleMenu = menuItems.filter((m) => !m.permission || hasPermission(m.permission));

  return (
    <ThemeProvider theme={vbfbTheme}>
      <CssBaseline />
      <BrowserRouter>
        <Navbar expand="xl" variant="dark" className="vbfb-navbar" collapseOnSelect>
          <Container fluid>
            <Navbar.Brand as={NavLink} to="/" className="d-flex align-items-center gap-2">
              <img src={logo} alt="" />
              <span>VBFB Assist</span>
            </Navbar.Brand>
            <Navbar.Toggle aria-controls="vbfb-nav" />
            <Navbar.Collapse id="vbfb-nav">
              <Nav className="me-auto">
                {visibleMenu.map((m) => (
                  <Nav.Link as={NavLink} to={m.path} key={m.key} eventKey={m.key}>
                    {m.key === 'mypage' ? sessionStorage.getItem('firstname') || m.label : m.label}
                  </Nav.Link>
                ))}
              </Nav>
              <Nav>
                <NavDropdown title={<><i className="bi bi-person-circle me-1" />{sessionStorage.getItem('username')}</>} align="end" id="vbfb-user-menu">
                  <NavDropdown.Item as={NavLink} to="/changepassword" eventKey="changepassword">Change password</NavDropdown.Item>
                  {isCurrentUserAdmin() && <NavDropdown.Item as={NavLink} to="/settings" eventKey="settings">Settings</NavDropdown.Item>}
                  {isSiteAdmin() && <NavDropdown.Item as={NavLink} to="/admin" eventKey="admin">Users</NavDropdown.Item>}
                  <NavDropdown.Divider />
                  <NavDropdown.Item onClick={logout}>Log out</NavDropdown.Item>
                </NavDropdown>
              </Nav>
            </Navbar.Collapse>
          </Container>
        </Navbar>

        <main>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/changepassword" element={<ChangePassword />} />

            {menuItems.map((m) => {
              const Screen = screens[m.key];
              return (
                <Route
                  key={m.key}
                  path={m.path + '/*'}
                  element={
                    <Guard permission={m.permission}>
                      <Screen />
                    </Guard>
                  }
                />
              );
            })}

            <Route path="/settings" element={<Guard permission="isadmin"><Settings /></Guard>} />
            <Route path="/settings/crews" element={<Guard permission="isadmin"><CrewsSettings /></Guard>} />
            <Route path="/settings/cadetcrews" element={<Guard permission="isadmin"><CadetCrewsSettings /></Guard>} />
            <Route path="/settings/training" element={<Guard permission="isadmin"><TrainingSettings /></Guard>} />
            <Route path="/settings/vehicles" element={<Guard permission="isadmin"><VehicleSetup /></Guard>} />
            <Route path="/settings/vehiclesections" element={<Guard permission="isadmin"><VehicleSectionsSettings /></Guard>} />
            <Route path="/settings/vehicleitems" element={<Guard permission="isadmin"><VehicleItemsSettings /></Guard>} />
            <Route path="/admin" element={<Guard permission="siteadmin"><AdminUsers /></Guard>} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </main>

        {children}
      </BrowserRouter>
    </ThemeProvider>
  );
}

App.propTypes = {
  children: PropTypes.node
};
