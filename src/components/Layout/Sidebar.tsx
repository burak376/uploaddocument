import React from 'react';
import { NavLink } from 'react-router-dom';
import { 
  LayoutDashboard, 
  Building2, 
  Users, 
  FileType, 
  Upload, 
  FileText, 
  Search,
  Lock
} from 'lucide-react';
import { useAuth } from '../../contexts/AuthContext';

const Sidebar: React.FC = () => {
  const { user } = useAuth();

  const navItems = [
    {
      name: 'Dashboard',
      path: '/dashboard',
      icon: LayoutDashboard,
      roles: ['SuperAdmin', 'CompanyAdmin', 'User']
    },
    {
      name: 'Firmalar',
      path: '/companies',
      icon: Building2,
      roles: ['SuperAdmin']
    },
    {
      name: 'Kullanıcılar',
      path: '/users',
      icon: Users,
      roles: ['SuperAdmin', 'CompanyAdmin']
    },
    {
      name: 'Belge Türleri',
      path: '/document-types',
      icon: FileType,
      roles: ['SuperAdmin', 'CompanyAdmin']
    },
    {
      name: 'Belge Yükle',
      path: '/upload',
      icon: Upload,
      roles: ['CompanyAdmin', 'User']
    },
    {
      name: 'Belgelerim',
      path: '/my-documents',
      icon: FileText,
      roles: ['CompanyAdmin', 'User']
    },
    {
      name: 'Belge Ara',
      path: '/search',
      icon: Search,
      roles: ['SuperAdmin', 'CompanyAdmin']
    },
    {
      name: 'Şifre Değiştir',
      path: '/change-password',
      icon: Lock,
      roles: ['SuperAdmin', 'CompanyAdmin', 'User']
    }
  ];

  const filteredNavItems = navItems.filter(item => 
    user && item.roles.includes(user.role)
  );

  return (
    <nav className="bg-gray-50 w-64 h-full overflow-y-auto">
      <div className="p-4">
        <ul className="space-y-2">
          {filteredNavItems.map((item) => {
            const Icon = item.icon;
            return (
              <li key={item.path}>
                <NavLink
                  to={item.path}
                  className={({ isActive }) =>
                    `flex items-center space-x-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors ${
                      isActive
                        ? 'bg-blue-100 text-blue-700'
                        : 'text-gray-700 hover:bg-gray-100 hover:text-gray-900'
                    }`
                  }
                >
                  <Icon className="h-5 w-5" />
                  <span>{item.name}</span>
                </NavLink>
              </li>
            );
          })}
        </ul>
      </div>
    </nav>
  );
};

export default Sidebar;