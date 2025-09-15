import React from 'react';
import { LogOut, User, Building2 } from 'lucide-react';
import { useAuth } from '../../contexts/AuthContext';

const Header: React.FC = () => {
  const { user, logout } = useAuth();

  const handleLogout = () => {
    logout();
  };

  return (
    <header className="bg-white shadow-sm border-b border-gray-200">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          <div className="flex items-center">
            <Building2 className="h-8 w-8 text-blue-600" />
            <h1 className="ml-3 text-xl font-semibold text-gray-900">
              Belge Yönetim Sistemi
            </h1>
          </div>
          
          <div className="flex items-center space-x-4">
            <div className="flex items-center space-x-3">
              <User className="h-5 w-5 text-gray-400" />
              <div className="text-sm">
                <div className="font-medium text-gray-900">
                  {user?.firstName} {user?.lastName}
                </div>
                <div className="text-gray-500">
                  {user?.role === 'SuperAdmin' ? 'Süper Admin' :
                   user?.role === 'CompanyAdmin' ? 'Firma Admin' : 'Kullanıcı'}
                  {user?.companyName && ` - ${user.companyName}`}
                </div>
              </div>
            </div>
            
            <button
              onClick={handleLogout}
              className="flex items-center space-x-2 px-3 py-2 text-sm text-gray-700 hover:text-gray-900 hover:bg-gray-100 rounded-md transition-colors"
            >
              <LogOut className="h-4 w-4" />
              <span>Çıkış</span>
            </button>
          </div>
        </div>
      </div>
    </header>
  );
};

export default Header;