import { useState } from 'react'
import { Box, Button, Card, CardContent, Grid, List, ListItem, ListItemText, Stack, TextField, Typography } from '@mui/material'

const initialTypes = [
  { id: 'doc-identity', name: 'T.C. Kimlik' },
  { id: 'doc-license', name: 'Ehliyet' },
  { id: 'doc-bank', name: 'Banka Hesap Dökümü' },
]

export const DocumentSetupPage = () => {
  const [documentTypes, setDocumentTypes] = useState(initialTypes)
  const [newType, setNewType] = useState('')

  const handleAddType = () => {
    if (!newType.trim()) return
    const id = typeof crypto !== 'undefined' && 'randomUUID' in crypto ? crypto.randomUUID() : `doc-${Date.now()}`
    setDocumentTypes((prev) => [...prev, { id, name: newType }])
    setNewType('')
  }

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h4" gutterBottom>
          Belge Tipleri & Grupları
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Belge tiplerini yönetin ve görev şablonlarında kullanılacak grupları oluşturun.
        </Typography>
      </Box>
      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Belge Tipleri
              </Typography>
              <Stack direction="row" spacing={2} component="form" onSubmit={(event) => { event.preventDefault(); handleAddType() }}>
                <TextField label="Yeni Belge Tipi" value={newType} onChange={(event) => setNewType(event.target.value)} fullWidth />
                <Button type="submit" variant="contained">
                  Ekle
                </Button>
              </Stack>
              <List>
                {documentTypes.map((type) => (
                  <ListItem key={type.id} disableGutters>
                    <ListItemText primary={type.name} secondary={type.id} />
                  </ListItem>
                ))}
              </List>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Belge Grupları
              </Typography>
              <Typography variant="body2">
                Gruplara belge tiplerini sürükleyip bırakarak görev gereksinimlerini hızla tanımlayabilirsiniz. Örnek: "Muhasebe Grubu" içine Banka Hesap Dökümü ekleyin.
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Stack>
  )
}
