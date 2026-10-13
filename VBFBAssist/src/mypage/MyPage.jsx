import { useCallback, useEffect, useState } from 'react';
import { Alert, CircularProgress, Paper, Tab, Tabs } from '@mui/material';
import { api, ApiErrorMessage } from '../utils/ApiUtils';
import MemberDetails from '../members/MemberDetails';
import PpeTable, { PpeButton } from '../components/PpeTable';

// The logged-in member's own details and PPE (Laravel /mypage)
export default function MyPage() {
  const [tab, setTab] = useState(0);
  const [data, setData] = useState(null);
  const [message, setMessage] = useState(null);

  const load = useCallback(() => {
    api.get('/mypage').then((r) => setData(r.data)).catch((err) => setMessage({ severity: 'error', text: ApiErrorMessage(err) }));
  }, []);

  useEffect(() => { load(); }, [load]);

  const ppeAction = async (row, action) => {
    try {
      await api.post(`/ppestock/${row.id}/${action}`);
      setMessage({ severity: 'success', text: action === 'acknowledge' ? `Thanks - receipt of ${row.Item} acknowledged.` : `Thanks - ${row.Item} checked.` });
      load();
    }
    catch (err) {
      setMessage({ severity: 'error', text: ApiErrorMessage(err) });
    }
  };

  if (!data) return <div className="vbfb-page">{message ? <Alert severity="error">{message.text}</Alert> : <CircularProgress />}</div>;

  if (!data.profile) {
    return (
      <div className="vbfb-page">
        <h3 className="vbfb-page-title">My Page</h3>
        <Alert severity="info">There is no member profile with your email address. Please ask an administrator to set one up.</Alert>
      </div>
    );
  }

  const current = data.ppe.filter((p) => p.deleted !== 'Y' && p.returned !== 'Y');

  return (
    <div className="vbfb-page">
      <h3 className="vbfb-page-title">{data.profile.firstname} {data.profile.lastname}</h3>
      {message && <Alert severity={message.severity} sx={{ mb: 2 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      <Paper sx={{ mb: 2 }}>
        <Tabs value={tab} onChange={(e, v) => setTab(v)}>
          <Tab label="Details" />
          <Tab label={`PPE (${current.length})`} />
        </Tabs>
      </Paper>

      <Paper sx={{ p: { xs: 2, sm: 3 } }}>
        {tab === 0 && <MemberDetails member={data.profile} />}
        {tab === 1 && (
          <PpeTable
            rows={current}
            emptyText="You have no PPE issued."
            actions={(r) => r.acknowledged !== 'Y'
              ? <PpeButton variant="contained" onClick={() => ppeAction(r, 'acknowledge')}>Acknowledge receipt</PpeButton>
              : <PpeButton onClick={() => ppeAction(r, 'check')}>Check now</PpeButton>}
          />
        )}
      </Paper>
    </div>
  );
}
