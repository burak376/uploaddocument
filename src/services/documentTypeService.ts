import api from './api';

export interface DocumentType {
  id: number;
  name: string;
  description?: string;
  allowedExtensions: string[];
  maxFileSize: number;
  companyId?: number;
  companyName?: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateDocumentTypeRequest {
  name: string;
  description?: string;
  allowedExtensions: string[];
  maxFileSize: number;
  companyId?: number;
  isActive: boolean;
}

export const documentTypeService = {
  async getAll(): Promise<DocumentType[]> {
    const response = await api.get('/documenttypes');
    return response.data;
  },

  async getById(id: number): Promise<DocumentType> {
    const response = await api.get(`/documenttypes/${id}`);
    return response.data;
  },

  async create(documentType: CreateDocumentTypeRequest): Promise<DocumentType> {
    const response = await api.post('/documenttypes', documentType);
    return response.data;
  },

  async update(id: number, documentType: CreateDocumentTypeRequest): Promise<DocumentType> {
    const response = await api.put(`/documenttypes/${id}`, documentType);
    return response.data;
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/documenttypes/${id}`);
  },
};