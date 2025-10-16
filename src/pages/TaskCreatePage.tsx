import { useState } from 'react'
import { Alert, Box, Button, Card, CardContent, Chip, Grid, Stack, TextField, Typography } from '@mui/material'
import { DesktopDateTimePicker } from '@mui/x-date-pickers/DesktopDateTimePicker'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs'
import dayjs, { Dayjs } from 'dayjs'
import { useCreateTask } from '../features/tasks/api/useCreateTask'

const demoGroups = [
  { id: 'group-1', name: 'Muhasebe Grubu' },
  { id: 'group-2', name: 'İnsan Kaynakları' },
]

const demoUsers = [
  { id: 'user-1', name: 'Ayşe Yılmaz' },
  { id: 'user-2', name: 'Mehmet Demir' },
]

export const TaskCreatePage = () => {
  const createTask = useCreateTask()
  const [selectedGroups, setSelectedGroups] = useState<string[]>([])
  const [dueDate, setDueDate] = useState<Dayjs | null>(dayjs().add(3, 'day'))

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const data = new FormData(event.currentTarget)
    const payload = {
      title: String(data.get('title') ?? ''),
      description: String(data.get('description') ?? ''),
      assigneeUserId: String(data.get('assigneeUserId') ?? ''),
      dueDateUtc: dueDate?.toISOString() ?? dayjs().toISOString(),
      priority: (data.get('priority') as 'Low' | 'Normal' | 'High' | 'Critical') ?? 'Normal',
      requiredGroupIds: selectedGroups,
    }
    createTask.mutate(payload)
  }

  const toggleGroup = (groupId: string) => {
    setSelectedGroups((prev) => (prev.includes(groupId) ? prev.filter((id) => id !== groupId) : [...prev, groupId]))
  }

  return (
    <Box component="form" onSubmit={handleSubmit}>
      <Stack spacing={3}>
        <Box>
          <Typography variant="h4" gutterBottom>
            Görev Oluştur
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Gerekli belge gruplarını seçtiğinizde, görev alan kullanıcı bu gruptaki tüm belge tiplerini yüklemekle yükümlüdür.
          </Typography>
        </Box>
        <Card>
          <CardContent>
            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <TextField name="title" label="Başlık" required fullWidth />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField name="assigneeUserId" label="Atanacak Kullanıcı" select fullWidth SelectProps={{ native: true }} defaultValue={demoUsers[0]?.id}>
                  {demoUsers.map((user) => (
                    <option key={user.id} value={user.id}>
                      {user.name}
                    </option>
                  ))}
                </TextField>
              </Grid>
              <Grid item xs={12} md={6}>
                <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="tr">
                  <DesktopDateTimePicker label="Termin (UTC)" value={dueDate} onChange={setDueDate} slotProps={{ textField: { fullWidth: true } }} />
                </LocalizationProvider>
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField name="priority" label="Öncelik" select fullWidth SelectProps={{ native: true }} defaultValue="Normal">
                  <option value="Low">Düşük</option>
                  <option value="Normal">Normal</option>
                  <option value="High">Yüksek</option>
                  <option value="Critical">Kritik</option>
                </TextField>
              </Grid>
              <Grid item xs={12}>
                <TextField name="description" label="Açıklama" fullWidth multiline minRows={3} />
              </Grid>
            </Grid>
          </CardContent>
        </Card>
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Gerekli Belge Grupları
            </Typography>
            <Stack direction="row" spacing={1} flexWrap="wrap">
              {demoGroups.map((group) => (
                <Chip key={group.id} label={group.name} color={selectedGroups.includes(group.id) ? 'primary' : 'default'} onClick={() => toggleGroup(group.id)} variant={selectedGroups.includes(group.id) ? 'filled' : 'outlined'} sx={{ mb: 1 }} />
              ))}
            </Stack>
            {selectedGroups.length === 0 && <Alert severity="info" sx={{ mt: 2 }}>En az bir belge grubu seçmeniz gerekir.</Alert>}
          </CardContent>
        </Card>
        <Box display="flex" justifyContent="flex-end">
          <Button type="submit" variant="contained" size="large" disabled={createTask.isPending}>
            Görevi Kaydet
          </Button>
        </Box>
        {createTask.isError && <Alert severity="error">Görev oluşturulurken bir hata oluştu: {(createTask.error as Error).message}</Alert>}
        {createTask.isSuccess && <Alert severity="success">Görev başarıyla oluşturuldu.</Alert>}
      </Stack>
    </Box>
  )
}
