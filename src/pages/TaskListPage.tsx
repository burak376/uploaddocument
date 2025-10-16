import { useMemo, useState } from 'react'
import { Box, Card, CardContent, Chip, FormControl, Grid, InputLabel, MenuItem, Select, Stack, Typography } from '@mui/material'
import { DataGrid, GridColDef } from '@mui/x-data-grid'
import { Link as RouterLink } from 'react-router-dom'
import { useTasks, type TaskSummary, formatDueDate } from '../features/tasks/api/useTasks'

const statusOptions: Array<{ value: TaskSummary['status']; label: string; color: 'default' | 'primary' | 'warning' | 'success' | 'error' }> = [
  { value: 'Open', label: 'Açık', color: 'primary' },
  { value: 'InProgress', label: 'Devam', color: 'warning' },
  { value: 'Review', label: 'İnceleme', color: 'default' },
  { value: 'Completed', label: 'Tamamlandı', color: 'success' },
  { value: 'Cancelled', label: 'İptal', color: 'error' },
]

export const TaskListPage = () => {
  const [statusFilter, setStatusFilter] = useState<TaskSummary['status'] | ''>('')
  const { data: tasks, isLoading } = useTasks(statusFilter || undefined)
  const columns = useMemo<GridColDef[]>(
    () => [
      {
        field: 'title',
        headerName: 'Başlık',
        flex: 1,
        renderCell: (params) => (
          <Typography component={RouterLink} to={`/tasks/${params.row.id}`} color="primary" sx={{ textDecoration: 'none' }}>
            {params.value}
          </Typography>
        ),
      },
      {
        field: 'dueDateUtc',
        headerName: 'Termin (Şirket TZ)',
        flex: 0.8,
        valueGetter: (params) => formatDueDate(params.value, 'Europe/Istanbul'),
      },
      {
        field: 'priority',
        headerName: 'Öncelik',
        flex: 0.5,
      },
      {
        field: 'status',
        headerName: 'Durum',
        flex: 0.6,
        renderCell: (params) => {
          const badge = statusOptions.find((option) => option.value === params.value)
          return <Chip label={badge?.label ?? params.value} color={badge?.color} variant="outlined" />
        },
      },
    ],
    [],
  )

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h4" gutterBottom>
          Görevler
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Görev durumuna göre filtreleyin, termin tarihlerini şirket zaman diliminde görüntüleyin.
        </Typography>
      </Box>
      <Card>
        <CardContent>
          <Grid container spacing={2} sx={{ mb: 2 }}>
            <Grid item xs={12} md={4}>
              <FormControl fullWidth>
                <InputLabel>Durum</InputLabel>
                <Select value={statusFilter} label="Durum" onChange={(event) => setStatusFilter(event.target.value as TaskSummary['status'] | '')}>
                  <MenuItem value="">
                    Tümü
                  </MenuItem>
                  {statusOptions.map((option) => (
                    <MenuItem key={option.value} value={option.value}>
                      {option.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
          </Grid>
          <div style={{ height: 480, width: '100%' }}>
            <DataGrid rows={tasks ?? []} columns={columns} loading={isLoading} disableRowSelectionOnClick getRowId={(row) => row.id} />
          </div>
        </CardContent>
      </Card>
    </Stack>
  )
}
