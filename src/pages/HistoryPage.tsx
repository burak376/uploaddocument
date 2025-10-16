import { useQuery } from '@tanstack/react-query'
import { Box, Card, CardContent, Stack, Typography } from '@mui/material'
import { DataGrid, GridColDef } from '@mui/x-data-grid'
import { useAuth } from '../providers/AuthProvider'
import api from '../services/api'

interface AuditRow {
  id: string
  userId: string
  eventType: string
  entityType: string
  entityId?: string
  createdAtUtc: string
  data: string
}

export const HistoryPage = () => {
  const { companyId } = useAuth()

  const { data = [], isLoading } = useQuery({
    queryKey: ['history', companyId],
    enabled: Boolean(companyId),
    queryFn: async () => {
      const response = await api.get<AuditRow[]>(`/companies/${companyId}/history`, {
        headers: { 'X-Company-Id': companyId },
      })
      return response.data
    },
  })

  const columns: GridColDef[] = [
    { field: 'eventType', headerName: 'Olay', flex: 0.8 },
    { field: 'entityType', headerName: 'Kaynak', flex: 0.6 },
    { field: 'entityId', headerName: 'Kaynak Id', flex: 0.8 },
    { field: 'userId', headerName: 'Kullanıcı', flex: 0.8 },
    { field: 'createdAtUtc', headerName: 'Tarih (UTC)', flex: 0.8 },
  ]

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h4" gutterBottom>
          Audit Log
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Kritik aksiyonlar Tenant izolasyonuyla kayıt altına alınır. PII içeren bilgiler maskeleme kurallarına tabidir.
        </Typography>
      </Box>
      <Card>
        <CardContent>
          <div style={{ height: 420, width: '100%' }}>
            <DataGrid rows={data} columns={columns} loading={isLoading} getRowId={(row) => row.id} />
          </div>
        </CardContent>
      </Card>
    </Stack>
  )
}
