import { describe, expect, it, vi } from 'vitest'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { DocumentUploadForm } from '../features/documents/components/DocumentUploadForm'

const createFile = (name: string, sizeInBytes: number, type: string) => {
  const file = new File(['x'.repeat(sizeInBytes)], name, { type })
  Object.defineProperty(file, 'size', { value: sizeInBytes })
  return file
}

describe('DocumentUploadForm', () => {
  it('validates required file and max size', async () => {
    const handleSubmit = vi.fn()
    render(<DocumentUploadForm documentTypeId="doc" onSubmit={handleSubmit} maxSizeMb={1} />)

    fireEvent.click(screen.getByRole('button', { name: /yükle/i }))
    expect(await screen.findByTestId('error-message')).toHaveTextContent('Lütfen bir dosya seçin')

    const oversizedFile = createFile('large.pdf', 2 * 1024 * 1024, 'application/pdf')
    const input = screen.getByTestId('file-input') as HTMLInputElement
    fireEvent.change(input, { target: { files: [oversizedFile] } })
    fireEvent.click(screen.getByRole('button', { name: /yükle/i }))
    expect(await screen.findByTestId('error-message')).toHaveTextContent('Dosya boyutu')

    const validFile = createFile('doc.pdf', 0.5 * 1024 * 1024, 'application/pdf')
    fireEvent.change(input, { target: { files: [validFile] } })
    fireEvent.click(screen.getByRole('button', { name: /yükle/i }))

    await waitFor(() => expect(handleSubmit).toHaveBeenCalled())
  })
})
