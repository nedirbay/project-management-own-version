import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Workspace, WorkspaceForm } from '@/types'
import { ElMessage } from 'element-plus'
import { useAuthStore } from './auth'
import api from '@/api'

export const useWorkspacesStore = defineStore('workspaces', () => {
  // State
  const workspaces = ref<Workspace[]>([])
  const loading = ref(false)
  const currentWorkspaceId = ref<string | null>(null)

  // Computed
  const allWorkspaces = computed(() => workspaces.value)
  const currentWorkspace = computed(() =>
    workspaces.value.find((w) => w.id === currentWorkspaceId.value),
  )
  const workspaceCount = computed(() => workspaces.value.length)

  // Get workspaces for current user
  const myWorkspaces = computed(() => {
    const authStore = useAuthStore()
    const userId = authStore.user?.id
    const userRole = authStore.user?.role
    if (!userId) return []

    // Admin sees all workspaces
    if (userRole === 'admin') {
      return workspaces.value
    }

    // WorkspaceAdmin sees only their assigned workspaces
    if (userRole === 'workspaceAdmin') {
      return workspaces.value.filter((w) => w.adminId === userId)
    }

    // Member sees workspaces they are part of
    return workspaces.value.filter(
      (w) => w.ownerId === userId || w.adminId === userId || w.memberIds.includes(userId),
    )
  })

  // Helper function to save to localStorage
  const saveToStorage = () => {
    localStorage.setItem('workspaces', JSON.stringify(workspaces.value))
    if (currentWorkspaceId.value) {
      localStorage.setItem('currentWorkspaceId', currentWorkspaceId.value)
    }
  }

  // Initialize from localStorage
  const initWorkspaces = () => {
    const stored = localStorage.getItem('workspaces')
    if (stored) {
      workspaces.value = JSON.parse(stored)
    }

    const storedCurrentId = localStorage.getItem('currentWorkspaceId')
    if (storedCurrentId && workspaces.value.some((w) => w.id === storedCurrentId)) {
      currentWorkspaceId.value = storedCurrentId
    }
  }

  // Actions
  const fetchWorkspaces = async () => {
    loading.value = true
    try {
      const res = await api.get('/workspaces')
      const list = res.data as any[]
      workspaces.value = list.map((w: any): Workspace => ({
        id: w.id,
        name: w.name,
        description: w.description,
        ownerId: w.adminId ?? '', // API doesn't return ownerId in list; fall back to adminId
        adminId: w.adminId,
        memberIds: [], // will be populated when fetching workspace details
        color: w.color,
        icon: w.icon,
        createdAt: w.createdAt,
        updatedAt: w.createdAt,
      }))
      saveToStorage()
    } catch (error) {
      ElMessage.error('Workspaces alınırken hata oluştu')
    } finally {
      loading.value = false
    }
  }

  // Fetch workspace details (members, projects)
  const fetchWorkspaceDetail = async (id: string): Promise<Workspace | null> => {
    loading.value = true
    try {
      const res = await api.get(`/workspaces/${id}`)
      const w = res.data as any
      const workspace: Workspace = {
        id: w.id,
        name: w.name,
        description: w.description,
        ownerId: w.ownerId ?? w.adminId ?? '',
        adminId: w.adminId,
        memberIds: (w.members || []).map((m: any) => m.id),
        color: w.color,
        icon: w.icon,
        createdAt: w.createdAt,
        updatedAt: w.createdAt,
      }

      const index = workspaces.value.findIndex((x) => x.id === workspace.id)
      if (index === -1) {
        workspaces.value.push(workspace)
      } else {
        workspaces.value[index] = { ...workspaces.value[index], ...workspace }
      }

      saveToStorage()
      return workspace
    } catch (error) {
      ElMessage.error('Workspace detayları alınırken hata oluştu')
      return null
    } finally {
      loading.value = false
    }
  }

  const getWorkspaceById = (id: string): Workspace | undefined => {
    return workspaces.value.find((w) => w.id === id)
  }

  const createWorkspace = async (form: WorkspaceForm): Promise<boolean> => {
    loading.value = true
    try {
      const authStore = useAuthStore()
      if (!authStore.canManageWorkspaces) {
        ElMessage.error('Bu işlemi yapmak için yetkiniz yok')
        return false
      }
      const userId = authStore.user?.id

      if (!userId) {
        ElMessage.error('Kullanıcı bulunamadı')
        return false
      }

      // Check if workspace name already exists
      const existing = workspaces.value.find((w) => w.name === form.name)
      if (existing) {
        ElMessage.error('Bu isimde bir workspace zaten mevcut')
        return false
      }

      // Call backend to create workspace
      const payload = {
        name: form.name,
        description: form.description,
        color: form.color,
        memberIds: form.memberIds || [],
      }

      const res = await api.post('/workspaces', payload)
      const created = res.data as any

      const createdWorkspace: Workspace = {
        id: created.id,
        name: created.name,
        description: created.description,
        ownerId: created.ownerId ?? created.adminId ?? userId,
        adminId: created.adminId,
        memberIds: [],
        color: created.color,
        icon: created.icon,
        createdAt: created.createdAt,
        updatedAt: created.createdAt,
      }

      workspaces.value.push(createdWorkspace)
      saveToStorage()

      if (workspaces.value.length === 1) {
        currentWorkspaceId.value = createdWorkspace.id
      }

      ElMessage.success('Workspace başarıyla oluşturuldu')
      return true
    } catch (error) {
      ElMessage.error('Workspace oluşturulurken hata oluştu')
      return false
    } finally {
      loading.value = false
    }
  }

  const updateWorkspace = async (id: string, form: Partial<WorkspaceForm>): Promise<boolean> => {
    loading.value = true
    try {
      const authStore = useAuthStore()
      if (!authStore.canManageWorkspaces) {
        ElMessage.error('Bu işlemi yapmak için yetkiniz yok')
        return false
      }
      const index = workspaces.value.findIndex((w) => w.id === id)
      if (index === -1) {
        ElMessage.error('Workspace bulunamadı')
        return false
      }
      const existing = workspaces.value[index]!

      // Check if name is being changed and already exists
      if (form.name && form.name !== existing.name) {
        const duplicate = workspaces.value.find((w) => w.name === form.name)
        if (duplicate) {
          ElMessage.error('Bu isimde bir workspace zaten mevcut')
          return false
        }
      }

      // Call backend to update
      const payload = {
        name: form.name ?? existing.name,
        description: form.description ?? existing.description,
        color: form.color ?? existing.color,
      }

      const res = await api.put(`/workspaces/${id}`, payload)
      const updated = res.data as any

      workspaces.value[index] = {
        ...existing,
        name: updated.name,
        description: updated.description,
        color: updated.color,
        updatedAt: updated.createdAt,
      }

      saveToStorage()
      ElMessage.success('Workspace başarıyla güncellendi')
      return true
    } catch (error) {
      ElMessage.error('Workspace güncellenirken hata oluştu')
      return false
    } finally {
      loading.value = false
    }
  }

  const deleteWorkspace = async (id: string): Promise<boolean> => {
    loading.value = true
    try {
      const authStore = useAuthStore()
      if (!authStore.isAdmin) {
        ElMessage.error('Sadece Admin workspace silebilir')
        return false
      }
      // Call backend to delete
      await api.delete(`/workspaces/${id}`)

      const index = workspaces.value.findIndex((w) => w.id === id)
      if (index !== -1) workspaces.value.splice(index, 1)

      if (currentWorkspaceId.value === id) {
        currentWorkspaceId.value = workspaces.value.length > 0 ? workspaces.value[0]!.id : null
      }

      saveToStorage()
      ElMessage.success('Workspace başarıyla silindi')
      return true
    } catch (error) {
      ElMessage.error('Workspace silinirken hata oluştu')
      return false
    } finally {
      loading.value = false
    }
  }

  const addMember = async (workspaceId: string, userId: string): Promise<boolean> => {
    loading.value = true
    try {
      const authStore = useAuthStore()
      if (!authStore.canManageWorkspaces) {
        ElMessage.error('Bu işlemi yapmak için yetkiniz yok')
        return false
      }
      // Call backend
      await api.post(`/workspaces/${workspaceId}/members`, { userId })

      // Refresh workspace details to get updated members
      await fetchWorkspaceDetail(workspaceId)

      ElMessage.success('Üye başarıyla eklendi')
      return true
    } catch (error) {
      ElMessage.error('Üye eklenirken hata oluştu')
      return false
    } finally {
      loading.value = false
    }
  }

  const removeMember = async (workspaceId: string, userId: string): Promise<boolean> => {
    loading.value = true
    try {
      const authStore = useAuthStore()
      if (!authStore.canManageWorkspaces) {
        ElMessage.error('Bu işlemi yapmak için yetkiniz yok')
        return false
      }
      // Call backend
      await api.delete(`/workspaces/${workspaceId}/members/${userId}`)

      // Refresh workspace details
      await fetchWorkspaceDetail(workspaceId)

      ElMessage.success('Üye başarıyla çıkarıldı')
      return true
    } catch (error) {
      ElMessage.error('Üye çıkarılırken hata oluştu')
      return false
    } finally {
      loading.value = false
    }
  }

  const setCurrentWorkspace = (id: string | null) => {
    currentWorkspaceId.value = id
    if (id) {
      localStorage.setItem('currentWorkspaceId', id)
    } else {
      localStorage.removeItem('currentWorkspaceId')
    }
  }

  // Initialize on store creation
  initWorkspaces()

  return {
    // State
    workspaces,
    loading,
    currentWorkspaceId,
    // Computed
    allWorkspaces,
    currentWorkspace,
    myWorkspaces,
    workspaceCount,
    // Actions
    fetchWorkspaces,
    getWorkspaceById,
    createWorkspace,
    updateWorkspace,
    deleteWorkspace,
    addMember,
    removeMember,
    fetchWorkspaceDetail,
    setCurrentWorkspace,
  }
})
