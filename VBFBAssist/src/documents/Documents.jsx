import { useCallback, useEffect, useState } from 'react';
import {
  Alert, Box, Button, Card, CardActionArea, CardContent, Chip, InputAdornment, Stack, TextField, Typography
} from '@mui/material';
import { Description, Search, UploadFile } from '@mui/icons-material';
import { api, ApiErrorMessage } from '../utils/ApiUtils';
import { formatDate } from '../utils/GridUtils';
import DocumentDialog from './DocumentDialog';

// Brigade documents library (Laravel /documents)
export default function Documents() {
  const [search, setSearch] = useState('');
  const [data, setData] = useState({ documents: [], types: [], canedit: false });
  const [dialog, setDialog] = useState(null); // { id } open, { id: 0 } upload
  const [message, setMessage] = useState(null);

  const load = useCallback((s) => {
    api.get('/documents', { params: { search: s } })
      .then((r) => setData(r.data))
      .catch((err) => setMessage({ severity: 'error', text: ApiErrorMessage(err) }));
  }, []);

  useEffect(() => { api.post('/documents/opened').catch(() => {}); }, []);
  useEffect(() => {
    const t = setTimeout(() => load(search), 300);
    return () => clearTimeout(t);
  }, [search, load]);

  return (
    <div className="vbfb-page">
      <Stack direction="row" justifyContent="space-between" alignItems="center" flexWrap="wrap" useFlexGap spacing={1} sx={{ mb: 2 }}>
        <h3 className="vbfb-page-title" style={{ margin: 0 }}>Documents</h3>
        {data.canedit && <Button variant="contained" startIcon={<UploadFile />} onClick={() => setDialog({ id: 0 })}>Upload</Button>}
      </Stack>

      <TextField
        size="small" placeholder="Search name, description, author or type" value={search} onChange={(e) => setSearch(e.target.value)}
        sx={{ mb: 2, width: '100%', maxWidth: 480 }}
        InputProps={{ startAdornment: <InputAdornment position="start"><Search /></InputAdornment> }}
      />

      {message && <Alert severity={message.severity} sx={{ mb: 2 }} onClose={() => setMessage(null)}>{message.text}</Alert>}

      <Box sx={{ display: 'grid', gap: 1.5, gridTemplateColumns: { xs: '1fr', md: '1fr 1fr', lg: '1fr 1fr 1fr' } }}>
        {data.documents.map((d) => (
          <Card key={d.seqno}>
            <CardActionArea onClick={() => setDialog({ id: d.seqno })} sx={{ height: '100%' }}>
              <CardContent>
                <Stack direction="row" spacing={1.5} alignItems="flex-start">
                  <Description color="primary" />
                  <Box sx={{ minWidth: 0, flexGrow: 1 }}>
                    <Typography fontWeight={500}>{d.documentname}</Typography>
                    {d.description && <Typography variant="body2" color="text.secondary">{d.description}</Typography>}
                    <Typography variant="caption" color="text.secondary">
                      v{d.versionnumber ?? 1} · {formatDate(d.lastupdated ?? d.createddate)}{d.Author && <> · {d.Author}</>}
                    </Typography>
                  </Box>
                  {d.Type && d.Type !== 'Not Specified' && <Chip size="small" label={d.Type} />}
                </Stack>
              </CardContent>
            </CardActionArea>
          </Card>
        ))}
      </Box>
      {data.documents.length === 0 && <Typography color="text.secondary">No documents found.</Typography>}

      {dialog && (
        <DocumentDialog
          id={dialog.id}
          types={data.types}
          canEdit={data.canedit}
          onClose={(changed) => { setDialog(null); if (changed) load(search); }}
        />
      )}
    </div>
  );
}
