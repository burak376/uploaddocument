import React, { createContext, useContext, useState, useEffect } from 'react';

interface User {
  id: string;
  username: string;
  firstName: string;
  lastName: string;
  email: string;
  role: 'SuperAdmin' | 'CompanyAdmin' | 'User';
  companyId?: string;
  companyName?: string;
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  login: (username: string, password: string) => Promise<boolean>;
  logout: () => void;
  loading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Mock kullanıcı verileri
const mockUsers: User[] = [
  {
    id: '1',
    username: 'superadmin',
    firstName: 'Super',
    lastName: 'Admin',
    email: 'admin@system.com',
    role: 'SuperAdmin'
  },
  {
    id: '2',
    username: 'bugibo_admin',
    firstName: 'Bugibo',
    lastName: 'Admin',
    email: 'admin@bugibo.com',
    role: 'CompanyAdmin',
    companyId: '1',
    companyName: 'Bugibo Yazılım'
  },
  {
    id: '3',
    username: 'burak',
    firstName: 'Burak',
    lastName: 'Kullanıcı',
    email: 'burak@bugibo.com',
    role: 'User',
    companyId: '1',
    companyName: 'Bugibo Yazılım'
  }
];

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Sayfa yenilendiğinde oturum kontrolü
    const savedUser = localStorage.getItem('user');
    if (savedUser) {
      setUser(JSON.parse(savedUser));
    }
    setLoading(false);
  }, []);

  const login = async (username: string, password: string): Promise<boolean> => {
    setLoading(true);
    
    // Mock authentication - gerçek uygulamada API çağrısı olacak
    const foundUser = mockUsers.find(u => u.username === username);
    
    if (foundUser && password === '12345') {
      setUser(foundUser);
      localStorage.setItem('user', JSON.stringify(foundUser));
      setLoading(false);
      return true;
    }
    
    setLoading(false);
    return false;
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem('user');
  };

  return (
    <AuthContext.Provider value={{
      user,
      isAuthenticated: !!user,
      login,
      logout,
      loading
    }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};