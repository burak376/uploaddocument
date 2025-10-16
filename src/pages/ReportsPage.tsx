import { Box, Card, CardContent, Divider, List, ListItem, ListItemText, Stack, Typography } from '@mui/material'

const overdueTasks = [
  { id: 'task-1', title: 'Muhasebe Belgeleri', dueDate: '2024-09-01', assignee: 'Ayşe Yılmaz' },
]

const missingDocuments = [
  { taskId: 'task-2', taskTitle: 'Personel Evrak Tamamlama', assignee: 'Mehmet Demir', missing: ['Ehliyet', 'İkametgah'] },
]

export const ReportsPage = () => {
  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h4" gutterBottom>
          Raporlar
        </Typography>
        <Typography variant="body2" color="text.secondary">
          SLA ihlalleri, eksik belgeler ve görev ilerleme özetlerine hızlıca erişin.
        </Typography>
      </Box>
      <Card>
        <CardContent>
          <Typography variant="h6">Süresi Geçen Görevler</Typography>
          <List>
            {overdueTasks.map((task) => (
              <ListItem key={task.id}>
                <ListItemText primary={task.title} secondary={`Termin: ${task.dueDate} | Atanan: ${task.assignee}`} />
              </ListItem>
            ))}
          </List>
        </CardContent>
      </Card>
      <Card>
        <CardContent>
          <Typography variant="h6">Eksik Belge Özeti</Typography>
          <List>
            {missingDocuments.map((item) => (
              <ListItem key={item.taskId}>
                <ListItemText primary={item.taskTitle} secondary={`Atanan: ${item.assignee} | Eksik: ${item.missing.join(', ')}`} />
              </ListItem>
            ))}
          </List>
        </CardContent>
      </Card>
      <Divider />
      <Typography variant="caption" color="text.secondary">
        Raporlar, Hangfire job’ları tarafından güncellenen audit log ve görev verilerini kullanarak oluşturulur.
      </Typography>
    </Stack>
  )
}
