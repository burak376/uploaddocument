import React, { useState } from 'react';
import { Users as UsersIcon, Plus, Edit, Trash2, User } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { useApp } from '../contexts/AppContext';
import Modal from '../components/Common/Modal';
import toast from 'react-hot-toast';

interface User {
  id: string;
  username: string;
  firstName: string;
  lastName: string;
  email: string;
  role: 'SuperAdmin' | 'CompanyAdmin' | 'User';
  companyId?: string;
  companyName?: string;
  isActive: boolean;
}

const Users: React.FC = () => {
  const { user: currentUser } = useAuth();
  const { companies } = useApp();
  const [users, setUsers] = useState<User[]>([
    {
      id: '1',
      username: 'superadmin',
      firstName: 'Super',
      lastName: 'Admin',
      email: 'admin@system.com',
      role: 'SuperAdmin',
      isActive: true
    },
    {
      id: '2',
      username: 'bugibo_admin',
      firstName: 'Bugibo',
      lastName: 'Admin',
      email: 'admin@bugibo.com',
      role: 'CompanyAdmin',
      companyId: '1',
      companyName: 'Bugibo Yazılım',
      isActive: true
    },
    {
      id: '3',
      username: 'burak',
      firstName: 'Burak',
      lastName: 'Kullanıcı',
      email: 'burak@bugibo.com',
      role: 'User',
      companyId: '1',
      companyName: 'Bugibo Yazılım',
      isActive: true
    }
  ]);

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [formData, setFormData] = useState({
    username: '',
    firstName: '',
    lastName: '',
    email: '',
    role: 'User' as 'SuperAdmin' | 'CompanyAdmin' | 'User',
    companyId: '',
    isActive: true
  });

  // Kullanıcıları filtrele - SuperAdmin tüm kullanıcıları, CompanyAdmin sadece kendi firmasındakileri görebilir
  const filteredUsers = users.filter(user => {
    if (currentUser?.role === 'SuperAdmin') {
      return true;
    }
    if (currentUser?.role === 'CompanyAdmin') {
      return user.companyId === currentUser.companyId;
    }
    return false;
  });

  const handleOpenModal = (user?: User) => {
    if (user) {
      setEditingUser(user);
      setFormData({
        username: user.username,
        firstName: user.firstName,
        lastName: user.lastName,
        email: user.email,
        role: user.role,
        companyId: user.companyId || '',
        isActive: user.isActive
      });
    } else {
      setEditingUser(null);
      setFormData({
        username: '',
        firstName: '',
        lastName: '',
        email: '',
        role: currentUser?.role === 'SuperAdmin' ? 'CompanyAdmin' : 'User',
        companyId: currentUser?.role === 'CompanyAdmin' ? currentUser.companyId || '' : '',
        isActive: true
      });
    }
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingUser(null);
    setFormData({
      username: '',
      firstName: '',
      lastName: '',
      email: '',
      role: 'User',
      companyId: '',
      isActive: true
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.username || !formData.firstName || !formData.lastName || !formData.email) {
      toast.error('Zorunlu alanları doldurun');
      return;
    }

    if (formData.role !== 'SuperAdmin' && !formData.companyId) {
      toast.error('Firma seçimi zorunludur');
      return;
    }

    const companyName = companies.find(c => c.id === formData.companyId)?.name;

    if (editingUser) {
      setUsers(prev => prev.map(u => u.id === editingUser.id ? {
        ...u,
        ...formData,
        companyName: formData.role === 'SuperAdmin' ? undefined : companyName
      } : u));
      toast.success('Kullanıcı başarıyla güncellendi');
    } else {
      const newUser: User = {
        id: Date.now().toString(),
        ...formData,
        companyName: formData.role === 'SuperAdmin' ? undefined : companyName
      };
      setUsers(prev => [...prev, newUser]);
      toast.success('Kullanıcı başarıyla eklendi');
    }
    
    handleCloseModal();
  };

  const handleDelete = (user: User) => {
    if (user.id === currentUser?.id) {
      toast.error('Kendi hesabınızı silemezsiniz');
      return;
    }

    if (window.confirm(`${user.firstName} ${user.lastName} kullanıcısını silmek istediğinizden emin misiniz?`)) {
      setUsers(prev => prev.filter(u => u.id !== user.id));
      toast.success('Kullanıcı başarıyla silindi');
    }
  };

  const getRoleText = (role: string) => {
    switch (role) {
      case 'SuperAdmin': return 'Süper Admin';
      case 'CompanyAdmin': return 'Firma Admin';
      case 'User': return 'Kullanıcı';
      default: return role;
    }
  };

  const getRoleColor = (role: string) => {
    switch (role) {
      case 'SuperAdmin': return 'bg-purple-100 text-purple-800';
      case 'CompanyAdmin': return 'bg-blue-100 text-blue-800';
      case 'User': return 'bg-green-100 text-green-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Kullanıcılar</h1>
          <p className="text-gray-600">
            {currentUser?.role === 'SuperAdmin' 
              ? 'Tüm sistem kullanıcılarını yönetin'
              : 'Firma kullanıcılarını yönetin'
            }
          </p>
        </div>
        <button
          onClick={() => handleOpenModal()}
          className="flex items-center space-x-2 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
        >
          <Plus className="h-4 w-4" />
          <span>Yeni Kullanıcı</span>
        </button>
      </div>

      <div className="bg-white rounded-lg shadow-sm border border-gray-200">
        {filteredUsers.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Kullanıcı
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Kullanıcı Adı
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Rol
                  </th>
                  {currentUser?.role === 'SuperAdmin' && (
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Firma
                    </th>
                  )}
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Durum
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    İşlemler
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredUsers.map((user) => (
                  <tr key={user.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <div className="p-2 bg-blue-100 rounded-lg mr-3">
                          <User className="h-5 w-5 text-blue-600" />
                        </div>
                        <div>
                          <div className="text-sm font-medium text-gray-900">
                            {user.firstName} {user.lastName}
                          </div>
                          <div className="text-sm text-gray-500">
                            {user.email}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {user.username}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getRoleColor(user.role)}`}>
                        {getRoleText(user.role)}
                      </span>
                    </td>
                    {currentUser?.role === 'SuperAdmin' && (
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {user.companyName || '-'}
                      </td>
                    )}
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${
                        user.isActive 
                          ? 'bg-green-100 text-green-800' 
                          : 'bg-red-100 text-red-800'
                      }`}>
                        {user.isActive ? 'Aktif' : 'Pasif'}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <div className="flex items-center justify-end space-x-2">
                        <button
                          onClick={() => handleOpenModal(user)}
                          className="p-2 text-blue-600 hover:bg-blue-100 rounded-lg transition-colors"
                          title="Düzenle"
                        >
                          <Edit className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => handleDelete(user)}
                          className="p-2 text-red-600 hover:bg-red-100 rounded-lg transition-colors"
                          title="Sil"
                          disabled={user.id === currentUser?.id}
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="text-center py-12">
            <UsersIcon className="h-16 w-16 text-gray-400 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">
              Henüz kullanıcı bulunmuyor
            </h3>
            <p className="text-gray-600 mb-4">
              İlk kullanıcıyı eklemek için butona tıklayın.
            </p>
            <button
              onClick={() => handleOpenModal()}
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
            >
              İlk Kullanıcıyı Ekle
            </button>
          </div>
        )}
      </div>

      {/* Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        title={editingUser ? 'Kullanıcı Düzenle' : 'Yeni Kullanıcı Ekle'}
        maxWidth="lg"
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Ad *
              </label>
              <input
                type="text"
                value={formData.firstName}
                onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="Adını girin"
                required
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Soyad *
              </label>
              <input
                type="text"
                value={formData.lastName}
                onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="Soyadını girin"
                required
              />
            </div>
          </div>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Kullanıcı Adı *
              </label>
              <input
                type="text"
                value={formData.username}
                onChange={(e) => setFormData({ ...formData, username: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="Kullanıcı adını girin"
                required
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                E-posta *
              </label>
              <input
                type="email"
                value={formData.email}
                onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="E-posta adresini girin"
                required
              />
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Rol *
              </label>
              <select
                value={formData.role}
                onChange={(e) => setFormData({ ...formData, role: e.target.value as any })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                required
              >
                {currentUser?.role === 'SuperAdmin' && (
                  <>
                    <option value="SuperAdmin">Süper Admin</option>
                    <option value="CompanyAdmin">Firma Admin</option>
                  </>
                )}
                <option value="User">Kullanıcı</option>
              </select>
            </div>
            {formData.role !== 'SuperAdmin' && (
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Firma *
                </label>
                <select
                  value={formData.companyId}
                  onChange={(e) => setFormData({ ...formData, companyId: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  required
                  disabled={currentUser?.role === 'CompanyAdmin'}
                >
                  <option value="">Firma seçin</option>
                  {companies
                    .filter(c => currentUser?.role === 'SuperAdmin' || c.id === currentUser?.companyId)
                    .map(company => (
                    <option key={company.id} value={company.id}>
                      {company.name}
                    </option>
                  ))}
                </select>
              </div>
            )}
          </div>

          <div className="flex items-center">
            <input
              type="checkbox"
              id="isActive"
              checked={formData.isActive}
              onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
              className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
            />
            <label htmlFor="isActive" className="ml-2 block text-sm text-gray-900">
              Aktif kullanıcı
            </label>
          </div>

          <div className="flex justify-end space-x-3 pt-4">
            <button
              type="button"
              onClick={handleCloseModal}
              className="px-4 py-2 text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200 transition-colors"
            >
              İptal
            </button>
            <button
              type="submit"
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
            >
              {editingUser ? 'Güncelle' : 'Ekle'}
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default Users;