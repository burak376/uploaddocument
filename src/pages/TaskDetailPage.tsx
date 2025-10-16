import { useMemo } from 'react'
import { Alert, Box, Card, CardContent, Chip, Divider, Grid, Stack, Typography } from '@mui/material'
import { useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { useAuth } from '../providers/AuthProvider'
import { formatDueDate } from '../features/tasks/api/useTasks'
import api from '../services/api'

interface TaskDocumentView {
  id: string
  documentTypeId: string
  fileName: string
  status: 'Uploaded' | 'Approved' | 'Rejected'
  notes?: string
}

interface TaskDetailView {
  id: string
  title: string
  description?: string
  assigneeUserId: string
  dueDateUtc: string
  priority: string
  status: string
  requiredGroupIds: string[]
  documents: TaskDocumentView[]
}

export const TaskDetailPage = () => {
  const { taskId } = useParams<{ taskId: string }>()
  const { companyId } = useAuth()

  const { data, isLoading, isError } = useQuery({
    queryKey: ['task', companyId, taskId],
    enabled: Boolean(companyId && taskId),
    queryFn: async () => {
      const response = await api.get<TaskDetailView>(`/companies/${companyId}/tasks/${taskId}`, {
        headers: { 'X-Company-Id': companyId },
      })
      return response.data
    },
  })

  const missingDocuments = useMemo(() => {
    if (!data) return []
    const required = new Set(data.requiredGroupIds)
    const uploadedTypes = new Set(data.documents.filter((doc) => doc.status !== 'Rejected').map((doc) => doc.documentTypeId))
    return [...required].filter((groupId) => !uploadedTypes.has(groupId))
  }, [data])

  if (isLoading) {
    return <Typography>Yükleniyor...</Typography>
  }

  if (isError || !data) {
    return <Alert severity="error">Görev bilgileri alınamadı.</Alert>
  }

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h4" gutterBottom>
          {data.title}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Termin tarihi: {formatDueDate(data.dueDateUtc, 'Europe/Istanbul')} (şirket zaman dilimi)
        </Typography>
      </Box>
      <Card>
        <CardContent>
          <Grid container spacing={2}>
            <Grid item xs={12} md={6}>
              <Typography variant="subtitle2" color="text.secondary">
                Durum
              </Typography>
              <Typography variant="h6">{data.status}</Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="subtitle2" color="text.secondary">
                Öncelik
              </Typography>
              <Typography variant="h6">{data.priority}</Typography>
            </Grid>
            <Grid item xs={12}>
              <Typography variant="subtitle2" color="text.secondary">
                Açıklama
              </Typography>
              <Typography>{data.description ?? 'Açıklama girilmemiş.'}</Typography>
            </Grid>
          </Grid>
        </CardContent>
      </Card>
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Belgeler
          </Typography>
          <Stack spacing={2}>
            {data.documents.map((document) => (
              <Box key={document.id}>
                <Typography variant="subtitle1">{document.fileName}</Typography>
                <Chip label={document.status} color={document.status === 'Approved' ? 'success' : document.status === 'Rejected' ? 'error' : 'default'} size="small" />
                {document.notes && (
                  <Typography variant="caption" color="text.secondary" display="block">
                    Not: {document.notes}
                  </Typography>
                )}
              </Box>
            ))}
            {data.documents.length === 0 && <Typography>Kayıtlı belge yok.</Typography>}
          </Stack>
          <Divider sx={{ my: 3 }} />
          <Typography variant="subtitle1" gutterBottom>
            Eksik belgeler
          </Typography>
          {missingDocuments.length === 0 ? (
            <Alert severity="success">Tüm belge gereksinimleri karşılandı.</Alert>
          ) : (
            <Stack direction="row" spacing={1} flexWrap="wrap">
              {missingDocuments.map((groupId) => (
                <Chip key={groupId} label={groupId} color="warning" />
              ))}
            </Stack>
          )}
        </CardContent>
      </Card>
    </Stack>
  )
}
