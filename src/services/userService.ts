import api from './api';

export interface User {
  id: number;
  username: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  companyId?: number;
  companyName?: string;
  isActive: boolean;
}

export interface CreateUserRequest {
  username: string;
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  role: number; // 1: SuperAdmin, 2: CompanyAdmin, 3: User
  companyId?: number;
  isActive: boolean;
}

export interface UpdateUserRequest {
  username: string;
  firstName: string;
  lastName: string;
  email: string;
  role: number;
  companyId?: number;
  isActive: boolean;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export const userService = {
  async getAll(): Promise<User[]> {
    const response = await api.get('/users');
    return response.data;
  },

  async getById(id: number): Promise<User> {
    const response = await api.get(`/users/${id}`);
    return response.data;
  },

  async create(user: CreateUserRequest): Promise<User> {
    const response = await api.post('/users', user);
    return response.data;
  },

  async update(id: number, user: UpdateUserRequest): Promise<User> {
    const response = await api.put(`/users/${id}`, user);
    return response.data;
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/users/${id}`);
  },

  async changePassword(id: number, request: ChangePasswordRequest): Promise<void> {
    await api.post(`/users/${id}/change-password`, request);
  },
};