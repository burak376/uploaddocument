import { useState } from 'react'
import { Alert, Box, Button, Stack, TextField, Typography } from '@mui/material'

export interface DocumentUploadFormProps {
  documentTypeId: string
  onSubmit: (file: File, note?: string) => Promise<void> | void
  maxSizeMb?: number
}

export const DocumentUploadForm = ({ documentTypeId, onSubmit, maxSizeMb = 10 }: DocumentUploadFormProps) => {
  const [file, setFile] = useState<File | null>(null)
  const [note, setNote] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!file) {
      setError('Lütfen bir dosya seçin')
      return
    }

    if (file.size > maxSizeMb * 1024 * 1024) {
      setError(`Dosya boyutu ${maxSizeMb} MB limitini aşıyor`)
      return
    }

    setError(null)
    await onSubmit(file, note)
    setSuccess(true)
  }

  return (
    <Box component="form" onSubmit={handleSubmit}>
      <Stack spacing={2}>
        <Typography variant="subtitle1">Belge yükle</Typography>
        <input
          data-testid="file-input"
          type="file"
          accept="application/pdf,image/*"
          onChange={(event) => setFile(event.target.files?.[0] ?? null)}
        />
        <TextField label="Not" value={note} onChange={(event) => setNote(event.target.value)} multiline minRows={2} />
        <Button type="submit" variant="contained" disabled={!file}>
          Yükle
        </Button>
        {error && <Alert severity="error" data-testid="error-message">{error}</Alert>}
        {success && <Alert severity="success">Belge yüklendi.</Alert>}
      </Stack>
    </Box>
  )
}
