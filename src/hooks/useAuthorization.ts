import { useMemo } from 'react'
import { useAuth } from '../providers/AuthProvider'

type Role = 'Admin' | 'Manager' | 'Assistant' | 'Staff'

export const useAuthorization = () => {
  const { roles } = useAuth()
  const isAdmin = useMemo(() => roles.includes('Admin'), [roles])
  const canManageTasks = useMemo(() => roles.some((role) => ['Admin', 'Manager', 'Assistant'].includes(role)), [roles])
  const canUploadDocuments = useMemo(() => roles.some((role) => ['Admin', 'Manager', 'Assistant', 'Staff'].includes(role)), [roles])

  const hasRole = (role: Role) => roles.includes(role)

  return {
    isAdmin,
    canManageTasks,
    canUploadDocuments,
    hasRole,
  }
}
