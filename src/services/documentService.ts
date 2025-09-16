import api from './api';

export interface Document {
  id: number;
  name: string;
  originalName: string;
  fileSize: number;
  fileExtension: string;
  documentTypeId: number;
  documentTypeName: string;
  uploadedById: number;
  uploadedByName: string;
  companyId: number;
  companyName: string;
  uploadDate: string;
}

export interface DocumentSearchRequest {
  searchTerm?: string;
  documentTypeId?: number;
  companyId?: number;
  dateFrom?: string;
  dateTo?: string;
  page?: number;
  pageSize?: number;
}

export interface DocumentSearchResponse {
  documents: Document[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export const documentService = {
  async getAll(): Promise<Document[]> {
    const response = await api.get('/documents');
    return response.data;
  },

  async getById(id: number): Promise<Document> {
    const response = await api.get(`/documents/${id}`);
    return response.data;
  },

  async upload(file: File, documentTypeId: number): Promise<Document> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('documentTypeId', documentTypeId.toString());

    const response = await api.post('/documents/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  async download(id: number): Promise<Blob> {
    const response = await api.get(`/documents/${id}/download`, {
      responseType: 'blob',
    });
    return response.data;
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/documents/${id}`);
  },

  async search(request: DocumentSearchRequest): Promise<DocumentSearchResponse> {
    const response = await api.post('/documents/search', request);
    return response.data;
  },
};