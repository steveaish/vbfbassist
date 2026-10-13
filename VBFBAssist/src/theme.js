import { createTheme } from '@mui/material/styles';

// Brand colours sampled from the Bush Fire Service logo - keep in sync with index.css
export const vbfbTheme = createTheme({
  palette: {
    primary: {
      main: '#12654A',
      dark: '#0B4A35',
      light: '#3E8A70',
      contrastText: '#FFFFFF'
    },
    secondary: {
      main: '#F61814',
      dark: '#C8110E',
      contrastText: '#FFFFFF'
    },
    error: {
      main: '#C8110E'
    },
    background: {
      default: '#F5F7F6'
    }
  },
  shape: {
    borderRadius: 8
  },
  typography: {
    fontFamily: 'Roboto, system-ui, -apple-system, "Segoe UI", sans-serif'
  }
});
