import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuth } from '../../../providers/AuthProvider'
import api from '../../../services/api'
import type { TaskSummary } from './useTasks'

interface CreateTaskPayload {
  title: string
  description?: string
  assigneeUserId: string
  dueDateUtc: string
  priority: TaskSummary['priority']
  requiredGroupIds: string[]
}

export const useCreateTask = () => {
  const { companyId } = useAuth()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: CreateTaskPayload) => {
      if (!companyId) throw new Error('Şirket seçilmedi')
      const response = await api.post(`/companies/${companyId}/tasks`, payload, {
        headers: { 'X-Company-Id': companyId },
      })
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] })
    },
  })
}
