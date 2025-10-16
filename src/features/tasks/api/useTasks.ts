import { useQuery } from '@tanstack/react-query'
import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'
import timezone from 'dayjs/plugin/timezone'
import { useAuth } from '../../../providers/AuthProvider'
import api from '../../../services/api'

export interface TaskSummary {
  id: string
  title: string
  assigneeUserId: string
  dueDateUtc: string
  priority: 'Low' | 'Normal' | 'High' | 'Critical'
  status: 'Open' | 'InProgress' | 'Review' | 'Completed' | 'Cancelled'
}

export const useTasks = (status?: TaskSummary['status']) => {
  const { companyId } = useAuth()

  return useQuery({
    queryKey: ['tasks', companyId, status],
    queryFn: async () => {
      if (!companyId) return []
      const response = await api.get<TaskSummary[]>(`/companies/${companyId}/tasks`, {
        params: { status },
        headers: { 'X-Company-Id': companyId },
      })
      return Array.isArray(response.data) ? response.data : (response.data as any).data ?? []
    },
    placeholderData: [],
  })
}

dayjs.extend(utc)
dayjs.extend(timezone)

export const formatDueDate = (value: string, timeZoneId: string) => {
  return dayjs.utc(value).tz(timeZoneId).format('DD.MM.YYYY HH:mm')
}
