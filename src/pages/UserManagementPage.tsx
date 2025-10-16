import { Box, Button, Card, CardContent, Chip, Stack, TextField, Typography } from '@mui/material'
import { DataGrid, GridColDef } from '@mui/x-data-grid'
import { useMemo, useState } from 'react'

interface UserRow {
  id: string
  email: string
  fullName: string
  roles: string[]
}

const initialUsers: UserRow[] = [
  { id: 'user-1', email: 'admin@example.com', fullName: 'Admin Kullanıcı', roles: ['Admin'] },
  { id: 'user-2', email: 'manager@example.com', fullName: 'Müdür', roles: ['Manager'] },
]

export const UserManagementPage = () => {
  const [users, setUsers] = useState(initialUsers)
  const [inviteEmail, setInviteEmail] = useState('')

  const columns = useMemo<GridColDef[]>(
    () => [
      { field: 'fullName', headerName: 'Ad Soyad', flex: 1 },
      { field: 'email', headerName: 'E-posta', flex: 1 },
      {
        field: 'roles',
        headerName: 'Roller',
        flex: 1,
        renderCell: (params) => (
          <Stack direction="row" spacing={1}>
            {params.value.map((role: string) => (
              <Chip key={role} label={role} size="small" />
            ))}
          </Stack>
        ),
      },
    ],
    [],
  )

  const handleInvite = () => {
    if (!inviteEmail) return
    const id = typeof crypto !== 'undefined' && 'randomUUID' in crypto ? crypto.randomUUID() : `user-${Date.now()}`
    setUsers((prev) => [...prev, { id, email: inviteEmail, fullName: inviteEmail.split('@')[0], roles: ['Staff'] }])
    setInviteEmail('')
  }

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h4" gutterBottom>
          Kullanıcı & Rol Yönetimi
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Şirkete kullanıcı davet edin ve RBAC matrisine göre rol atamalarını yönetin.
        </Typography>
      </Box>
      <Card>
        <CardContent>
          <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} alignItems="center">
            <TextField label="E-posta daveti" value={inviteEmail} onChange={(event) => setInviteEmail(event.target.value)} sx={{ flex: 1 }} />
            <Button variant="contained" onClick={handleInvite}>
              Daveti Gönder
            </Button>
          </Stack>
        </CardContent>
      </Card>
      <Card>
        <CardContent>
          <div style={{ height: 360, width: '100%' }}>
            <DataGrid rows={users} columns={columns} getRowId={(row) => row.id} />
          </div>
        </CardContent>
      </Card>
    </Stack>
  )
}
