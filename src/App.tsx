import { Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider, useAuth } from './providers/AuthProvider'
import { AppLayout } from './components/layout/AppLayout'
import { LoginPage } from './pages/LoginPage'
import { CompanySelectionPage } from './pages/CompanySelectionPage'
import { TaskListPage } from './pages/TaskListPage'
import { TaskCreatePage } from './pages/TaskCreatePage'
import { TaskDetailPage } from './pages/TaskDetailPage'
import { DocumentSetupPage } from './pages/DocumentSetupPage'
import { UserManagementPage } from './pages/UserManagementPage'
import { HistoryPage } from './pages/HistoryPage'
import { ReportsPage } from './pages/ReportsPage'
import { ReactNode } from 'react'

const RequireAuth = ({ children }: { children: ReactNode }) => {
  const { isAuthenticated } = useAuth()
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }
  return <>{children}</>
}

const RequireCompany = ({ children }: { children: ReactNode }) => {
  const { companyId } = useAuth()
  if (!companyId) {
    return <Navigate to="/company" replace />
  }
  return <>{children}</>
}

const AppRoutes = () => (
  <Routes>
    <Route path="/login" element={<LoginPage />} />
    <Route
      path="/company"
      element={
        <RequireAuth>
          <CompanySelectionPage />
        </RequireAuth>
      }
    />
    <Route
      path="/"
      element={
        <RequireAuth>
          <RequireCompany>
            <AppLayout />
          </RequireCompany>
        </RequireAuth>
      }
    >
      <Route index element={<Navigate to="tasks" replace />} />
      <Route path="tasks" element={<TaskListPage />} />
      <Route path="tasks/new" element={<TaskCreatePage />} />
      <Route path="tasks/:taskId" element={<TaskDetailPage />} />
      <Route path="document-setup" element={<DocumentSetupPage />} />
      <Route path="admin/users" element={<UserManagementPage />} />
      <Route path="history" element={<HistoryPage />} />
      <Route path="reports" element={<ReportsPage />} />
    </Route>
    <Route path="*" element={<Navigate to="/" replace />} />
  </Routes>
)

function App() {
  return (
    <AuthProvider>
      <AppRoutes />
    </AuthProvider>
  )
}

export default App
