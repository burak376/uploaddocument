import { createContext, useContext, useMemo, useState, type ReactNode } from 'react'

type Role = 'Admin' | 'Manager' | 'Assistant' | 'Staff'

interface AuthState {
  isAuthenticated: boolean
  companyId?: string
  roles: Role[]
  login: (token: string, companyId: string, roles: Role[]) => void
  logout: () => void
}

const AuthContext = createContext<AuthState | undefined>(undefined)

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [isAuthenticated, setAuthenticated] = useState(false)
  const [companyId, setCompanyId] = useState<string | undefined>(undefined)
  const [roles, setRoles] = useState<Role[]>([])

  const value = useMemo<AuthState>(() => ({
    isAuthenticated,
    companyId,
    roles,
    login: (_token, tenantId, assignedRoles) => {
      setAuthenticated(true)
      setCompanyId(tenantId)
      setRoles(assignedRoles)
    },
    logout: () => {
      setAuthenticated(false)
      setCompanyId(undefined)
      setRoles([])
    },
  }), [isAuthenticated, companyId, roles])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export const useAuth = () => {
  const value = useContext(AuthContext)
  if (!value) {
    throw new Error('useAuth must be used inside AuthProvider')
  }
  return value
}
