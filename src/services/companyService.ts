import api from './api';

export interface Company {
  id: number;
  name: string;
  taxNumber: string;
  address?: string;
  phone?: string;
  email: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateCompanyRequest {
  name: string;
  taxNumber: string;
  address?: string;
  phone?: string;
  email: string;
  isActive: boolean;
}

export const companyService = {
  async getAll(): Promise<Company[]> {
    const response = await api.get('/companies');
    return response.data;
  },

  async getById(id: number): Promise<Company> {
    const response = await api.get(`/companies/${id}`);
    return response.data;
  },

  async create(company: CreateCompanyRequest): Promise<Company> {
    const response = await api.post('/companies', company);
    return response.data;
  },

  async update(id: number, company: CreateCompanyRequest): Promise<Company> {
    const response = await api.put(`/companies/${id}`, company);
    return response.data;
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/companies/${id}`);
  },
};