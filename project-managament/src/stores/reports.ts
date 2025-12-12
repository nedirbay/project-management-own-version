import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { DailyReport, DailyReportForm } from '@/types'
import { ElMessage } from 'element-plus'
import { useAuthStore } from './auth'
import api from '@/api'

export const useReportsStore = defineStore('reports', () => {
  // State
  const reports = ref<DailyReport[]>([])
  const loading = ref(false)

  // Computed
  const allReports = computed(() => reports.value)
  const reportCount = computed(() => reports.value.length)

  // Get reports for current user
  const myReports = computed(() => {
    const authStore = useAuthStore()
    const userId = authStore.user?.id
    if (!userId) return []
    return reports.value.filter((r) => r.userId === userId)
  })

  // Get today's report for current user
  const todayReport = computed(() => {
    const authStore = useAuthStore()
    const userId = authStore.user?.id
    if (!userId) return null

    const today = new Date().toISOString().split('T')[0]
    return reports.value.find((r) => r.userId === userId && r.date === today)
  })

  // Get reports by date range
  const getReportsByDateRange = (startDate: string, endDate: string): DailyReport[] => {
    return reports.value.filter((r) => r.date >= startDate && r.date <= endDate)
  }

  // Get reports by workspace
  const getReportsByWorkspace = (workspaceId: string): DailyReport[] => {
    return reports.value.filter((r) => r.workspaceId === workspaceId)
  }

  // Get reports by user
  const getReportsByUser = (userId: string): DailyReport[] => {
    return reports.value.filter((r) => r.userId === userId)
  }

  // Get weekly reports for current user
  const weeklyReports = computed(() => {
    const authStore = useAuthStore()
    const userId = authStore.user?.id
    if (!userId) return []

    const now = new Date()
    const weekAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000)
    const weekAgoStr = weekAgo.toISOString().split('T')[0]
    const todayStr = now.toISOString().split('T')[0]

    return reports.value.filter(
      (r) => r.userId === userId && r.date >= weekAgoStr && r.date <= todayStr,
    )
  })

  // Calculate total hours for a user in date range
  const getTotalHours = (userId: string, startDate: string, endDate: string): number => {
    const userReports = reports.value.filter(
      (r) => r.userId === userId && r.date >= startDate && r.date <= endDate,
    )
    return userReports.reduce((total, report) => total + report.hoursWorked, 0)
  }

  // Helper function to save to localStorage
  const saveToStorage = () => {
    localStorage.setItem('reports', JSON.stringify(reports.value))
  }

  // Initialize from localStorage
  const initReports = () => {
    const stored = localStorage.getItem('reports')
    if (stored) {
      reports.value = JSON.parse(stored)
    }
  }

  // Actions
  const fetchReports = async () => {
    loading.value = true
    try {
      const res = await api.get('/dailyreports')
      const list = res.data as any[]
      reports.value = list.map((r: any): DailyReport => ({
        id: r.id,
        userId: r.userId,
        date: r.date,
        workspaceId: r.workspaceId,
        projectId: r.projectId,
        tasksCompleted: r.yumusesCompleted || [],
        workDescription: r.workDescription,
        hoursWorked: 0,
        blockers: '',
        notes: r.notes,
        createdAt: r.createdAt,
        updatedAt: r.createdAt,
      }))
      saveToStorage()
    } catch (error) {
      ElMessage.error('Raporlar alınırken hata oluştu')
    } finally {
      loading.value = false
    }
  }

  const getReportById = (id: string): DailyReport | undefined => {
    return reports.value.find((r) => r.id === id)
  }

  const createReport = async (form: any): Promise<boolean> => {
    loading.value = true
    try {
      const authStore = useAuthStore()
      const userId = form.userId || authStore.user?.id

      if (!userId) {
        ElMessage.error('Kullanıcı bulunamadı')
        return false
      }

      const reportDate = form.date || new Date().toISOString().split('T')[0]

      // Check if report already exists for this date
      const existingReport = reports.value.find(
        (r) => r.userId === userId && r.date === reportDate && r.workspaceId === form.workspaceId,
      )

      if (existingReport) {
        ElMessage.error("Bu tarih için bu workspace'te zaten bir rapor oluşturdunuz")
        return false
      }

      const newReport: DailyReport = {
        id: 'report-' + Date.now() + '-' + Math.random().toString(36).substr(2, 9),
        userId,
        date: reportDate,
        workspaceId: form.workspaceId,
        projectId: form.projectId,
        tasksCompleted: form.tasksCompleted || [],
        workDescription: form.workDescription,
        hoursWorked: form.hoursWorked || 0,
        blockers: form.blockers || '',
        notes: form.notes || '',
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      }

      reports.value.push(newReport)
      saveToStorage()
      ElMessage.success('Günlük rapor başarıyla oluşturuldu')
      return true
    } catch (error) {
      ElMessage.error('Rapor oluşturulurken hata oluştu')
      return false
    } finally {
      loading.value = false
    }
  }

  const updateReport = async (id: string, form: Partial<DailyReportForm>): Promise<boolean> => {
    loading.value = true
    try {
      const index = reports.value.findIndex((r) => r.id === id)
      if (index === -1) {
        ElMessage.error('Rapor bulunamadı')
        return false
      }

      reports.value[index] = {
        ...reports.value[index],
        ...form,
        updatedAt: new Date().toISOString(),
      }

      saveToStorage()
      ElMessage.success('Rapor başarıyla güncellendi')
      return true
    } catch (error) {
      ElMessage.error('Rapor güncellenirken hata oluştu')
      return false
    } finally {
      loading.value = false
    }
  }

  const deleteReport = async (id: string): Promise<boolean> => {
    loading.value = true
    try {
      const index = reports.value.findIndex((r) => r.id === id)
      if (index === -1) {
        ElMessage.error('Rapor bulunamadı')
        return false
      }

      reports.value.splice(index, 1)
      saveToStorage()
      ElMessage.success('Rapor başarıyla silindi')
      return true
    } catch (error) {
      ElMessage.error('Rapor silinirken hata oluştu')
      return false
    } finally {
      loading.value = false
    }
  }

  // Get statistics for dashboard
  const getUserStats = (userId: string, days: number = 30) => {
    const now = new Date()
    const pastDate = new Date(now.getTime() - days * 24 * 60 * 60 * 1000)
    const pastDateStr = pastDate.toISOString().split('T')[0]
    const todayStr = now.toISOString().split('T')[0]

    const userReports = reports.value.filter(
      (r) => r.userId === userId && r.date >= pastDateStr && r.date <= todayStr,
    )

    const totalHours = userReports.reduce((sum, r) => sum + r.hoursWorked, 0)
    const totalReports = userReports.length
    const avgHoursPerDay = totalReports > 0 ? totalHours / totalReports : 0

    return {
      totalHours,
      totalReports,
      avgHoursPerDay: Math.round(avgHoursPerDay * 10) / 10,
      reportsWithBlockers: userReports.filter((r) => r.blockers && r.blockers.length > 0).length,
    }
  }

  // Get team statistics for workspace
  const getWorkspaceStats = (workspaceId: string, days: number = 30) => {
    const now = new Date()
    const pastDate = new Date(now.getTime() - days * 24 * 60 * 60 * 1000)
    const pastDateStr = pastDate.toISOString().split('T')[0]
    const todayStr = now.toISOString().split('T')[0]

    const workspaceReports = reports.value.filter(
      (r) => r.workspaceId === workspaceId && r.date >= pastDateStr && r.date <= todayStr,
    )

    const totalHours = workspaceReports.reduce((sum, r) => sum + r.hoursWorked, 0)
    const totalReports = workspaceReports.length
    const uniqueUsers = new Set(workspaceReports.map((r) => r.userId)).size

    return {
      totalHours,
      totalReports,
      activeUsers: uniqueUsers,
      avgHoursPerReport: totalReports > 0 ? Math.round((totalHours / totalReports) * 10) / 10 : 0,
    }
  }

  // Initialize on store creation
  initReports()

  return {
    // State
    reports,
    loading,
    // Computed
    allReports,
    reportCount,
    myReports,
    todayReport,
    weeklyReports,
    // Actions
    fetchReports,
    getReportById,
    getReportsByDateRange,
    getReportsByWorkspace,
    getReportsByUser,
    getTotalHours,
    createReport,
    updateReport,
    deleteReport,
    getUserStats,
    getWorkspaceStats,
  }
})
