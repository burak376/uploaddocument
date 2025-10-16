import { useState } from 'react'
import { Box, Button, Card, CardContent, CardHeader, FormControl, InputLabel, MenuItem, Select, Stack, Typography } from '@mui/material'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../providers/AuthProvider'

const demoCompanies = [
  { id: '00000000-0000-0000-0000-000000000000', name: 'Demo Holding' },
  { id: '11111111-1111-1111-1111-111111111111', name: 'İkinci Şirket' },
]

export const CompanySelectionPage = () => {
  const [companyId, setCompanyId] = useState(demoCompanies[0]?.id)
  const navigate = useNavigate()
  const { login } = useAuth()

  const handleContinue = () => {
    if (!companyId) return
    login('demo-token', companyId, ['Admin'])
    navigate('/tasks')
  }

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', minHeight: '100vh' }}>
      <Card sx={{ width: 400 }}>
        <CardHeader title="Şirket Seçimi" subheader="Erişmek istediğiniz firmayı seçin" />
        <CardContent>
          <Stack spacing={2}>
            <FormControl fullWidth>
              <InputLabel id="company-select">Şirket</InputLabel>
              <Select labelId="company-select" value={companyId} label="Şirket" onChange={(event) => setCompanyId(event.target.value)}>
                {demoCompanies.map((company) => (
                  <MenuItem key={company.id} value={company.id}>
                    {company.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <Button variant="contained" onClick={handleContinue}>
              Devam Et
            </Button>
            <Typography variant="caption" color="text.secondary">
              Tüm veriler şirket bazlı izole edilir. Seçtiğiniz şirket için gerekli yetkilere sahip olmanız gerekir.
            </Typography>
          </Stack>
        </CardContent>
      </Card>
    </Box>
  )
}
