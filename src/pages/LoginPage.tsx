import { useState } from 'react'
import { Box, Button, Paper, Stack, TextField, Typography } from '@mui/material'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../providers/AuthProvider'

export const LoginPage = () => {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const navigate = useNavigate()
  const { login } = useAuth()

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    login('demo-token', '00000000-0000-0000-0000-000000000000', ['Admin'])
    navigate('/company')
  }

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', minHeight: '100vh' }}>
      <Paper elevation={4} component="form" onSubmit={handleSubmit} sx={{ p: 4, width: 360 }}>
        <Stack spacing={2}>
          <Typography variant="h5" textAlign="center">
            Sisteme Giriş
          </Typography>
          <TextField label="E-posta" type="email" required value={email} onChange={(event) => setEmail(event.target.value)} />
          <TextField label="Parola" type="password" required value={password} onChange={(event) => setPassword(event.target.value)} />
          <Button type="submit" variant="contained" size="large">
            Giriş Yap
          </Button>
        </Stack>
      </Paper>
    </Box>
  )
}
