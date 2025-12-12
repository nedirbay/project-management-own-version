import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Project, ProjectForm, ProjectFilter } from '@/types'
import { ElMessage } from 'element-plus'
import { useAuthStore } from './auth'
import { useWorkspacesStore } from './workspaces'
import api from '@/api'

export const useProjectsStore = defineStore('projects', () => {
  // State
  const projects = ref<Project[]>([])
  const loading = ref(false)

  // Computed
  const allProjects = computed(() => projects.value)
  const projectCount = computed(() => projects.value.length)

  // Get projects for current workspace
  const workspaceProjects = computed(() => {
    const workspacesStore = useWorkspacesStore()
    const workspaceId = workspacesStore.currentWorkspaceId
    if (!workspaceId) return []
    return projects.value.filter((p) => p.workspaceId === workspaceId)
  })

  // Get projects for current user
  const myProjects = computed(() => {
    const authStore = useAuthStore()
    const userId = authStore.user?.id
    const userRole = authStore.user?.role
    if (!userId) return []

    // Admin sees all projects
    if (userRole === 'admin') {
      return projects.value
    }

    // WorkspaceAdmin sees projects in their workspaces
    if (userRole === 'workspaceAdmin') {
      const workspacesStore = useWorkspacesStore()
      const myWorkspaceIds = workspacesStore.myWorkspaces.map((w) => w.id)
      return projects.value.filter((p) => myWorkspaceIds.includes(p.workspaceId))
    }

    // Member sees only assigned projects
    return projects.value.filter((p) => p.ownerId === userId || p.memberIds.includes(userId))
  })

  // Get active projects
  const activeProjects = computed(() => projects.value.filter((p) => p.status === 'active'))

  // Get completed projects
  const completedProjects = computed(() => projects.value.filter((p) => p.status === 'completed'))

  // Helper function to save to localStorage
  const saveToStorage = () => {
    localStorage.setItem('projects', JSON.stringify(projects.value))
  }

  // Initialize from localStorage
  const initProjects = () => {
    const stored = localStorage.getItem('projects')
    if (stored) {
      projects.value = JSON.parse(stored)
    }
  }

  // Actions
  const fetchProjects = async () => {
    loading.value = true
    try {
      const res = await api.get('/projects')
      const list = res.data as any[]
      projects.value = list.map((p: any): Project => ({
        id: p.id,
        name: p.name,
        description: p.description,
        workspaceId: p.workspaceId,
        ownerId: '',
        memberIds: [],
        status: (p.status || 'planning').toLowerCase(),
        priority: (p.priority || 'medium').toLowerCase(),
        startDate: p.startDate,
        endDate: p.endDate,
        progress: p.progress,
        color: p.color,
        tags: p.tags || [],
        createdAt: p.createdAt,
        updatedAt: p.createdAt,
      }))
      saveToStorage()
    } catch (error) {
      ElMessage.error('Projeler alınırken hata oluştu')
    } finally {
      loading.value = false
    }
  }

  const getProjectById = (id: string): Project | undefined => {
    return projects.value.find((p) => p.id === id)
  }

  const getProjectsByWorkspace = (workspaceId: string): Project[] => {
    return projects.value.filter((p) => p.workspaceId === workspaceId)
  }

  const createProject = async (form: ProjectForm): Promise<boolean> => {
    loading.value = true
    try {
      const authStore = useAuthStore()
      const userId = authStore.user?.id
      const userRole = authStore.user?.role

      if (!userId) {
        ElMessage.error('Kullanıcı bulunamadı')
        return false
      }

      // Check permissions
      if (userRole === 'member') {
        ElMessage.error('Bu işlem için yetkiniz yok')
        return false
      }

      // WorkspaceAdmin can only create in their workspaces
      if (userRole === 'workspaceAdmin') {
        const workspacesStore = useWorkspacesStore()
        const workspace = workspacesStore.getWorkspaceById(form.workspaceId)
        if (!workspace || workspace.adminId !== userId) {
          ElMessage.error("Bu workspace'te proje oluşturma yetkiniz yok")
          return false
        }
      }

      // Check if project name already exists in workspace
      const existing = projects.value.find(
        (p) => p.name === form.name && p.workspaceId === form.workspaceId,
      )
      if (existing) {
        ElMessage.error("Bu workspace'te aynı isimde bir proje zaten mevcut")
        return false
      }

      const newProject: Project = {
        id: 'proj-' + Date.now() + '-' + Math.random().toString(36).substr(2, 9),
        name: form.name,
        description: form.description,
        workspaceId: form.workspaceId,
        ownerId: userId,
        memberIds: form.memberIds || [],
        status: form.status || 'planning',
        priority: form.priority || 'medium',
        startDate: form.startDate,
        endDate: form.endDate,
        progress: 0,
        color: form.color || '#409EFF',
        tags: form.tags || [],
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      }

      projects.value.push(newProject)
      saveToStorage()
      ElMessage.success('Proje başarıyla oluşturuldu')
      return true
    } catch (error) {
      ElMessage.error('Proje oluşturulurken hata oluştu')
      return false
    } finally {
      loading.value = false
    }
  }

  const updateProject = async (id: string, form: Partial<ProjectForm>): Promise<boolean> => {
    loading.value = true
    try {
      const authStore = useAuthStore()
      const userId = authStore.user?.id
      const userRole = authStore.user?.role

      const index = projects.value.findIndex((p) => p.id === id)
      if (index === -1) {
        ElMessage.error('Proje bulunamadı')
        return false
      }

      // Check permissions
      if (userRole === 'member') {
        ElMessage.error('Bu işlem için yetkiniz yok')
        return false
      }

      // WorkspaceAdmin can only update their workspace projects
      if (userRole === 'workspaceAdmin') {
        const workspacesStore = useWorkspacesStore()
        const workspace = workspacesStore.getWorkspaceById(projects.value[index].workspaceId)
        if (!workspace || workspace.adminId !== userId) {
          ElMessage.error('Bu projeyi güncelleme yetkiniz yok')
          return false
        }
      }

      // Check if name is being changed and already exists
      if (form.name && form.name !== projects.value[index].name) {
        const existing = projects.value.find(
          (p) =>
            p.name === form.name &&
            p.workspaceId === projects.value[index].workspaceId &&
            p.id !== id,
        )
        if (existing) {
          ElMessage.error("Bu workspace'te aynı isimde bir proje zaten mevcut")
          return false
        }
      }

      projects.value[index] = {
        ...projects.value[index],
        ...form,
        updatedAt: new Date().toISOString(),
      }

      saveToStorage()
      ElMessage.success('Proje başarıyla güncellendi')
      return true
    } catch (error) {
      ElMessage.error('Proje güncellenirken hata oluştu')
      return false
    } finally {
      loading.value = false
    }
  }

  const deleteProject = async (id: string): Promise<boolean> => {
    loading.value = true
    try {
      const authStore = useAuthStore()
      const userId = authStore.user?.id
      const userRole = authStore.user?.role

      const index = projects.value.findIndex((p) => p.id === id)
      if (index === -1) {
        ElMessage.error('Proje bulunamadı')
        return false
      }

      // Check permissions
      if (userRole === 'member') {
        ElMessage.error('Bu işlem için yetkiniz yok')
        return false
      }

      // WorkspaceAdmin can only delete their workspace projects
      if (userRole === 'workspaceAdmin') {
        const workspacesStore = useWorkspacesStore()
        const workspace = workspacesStore.getWorkspaceById(projects.value[index].workspaceId)
        if (!workspace || workspace.adminId !== userId) {
          ElMessage.error('Bu projeyi silme yetkiniz yok')
          return false
        }
      }

      projects.value.splice(index, 1)
      saveToStorage()
      ElMessage.success('Proje başarıyla silindi')
      return true
    } catch (error) {
      ElMessage.error('Proje silinirken hata oluştu')
      return false
    } finally {
      loading.value = false
    }
  }

  const updateProjectProgress = async (id: string, progress: number): Promise<boolean> => {
    try {
      const project = projects.value.find((p) => p.id === id)
      if (!project) {
        ElMessage.error('Proje bulunamadı')
        return false
      }

      project.progress = Math.min(100, Math.max(0, progress))
      project.updatedAt = new Date().toISOString()

      // Auto-complete project if progress reaches 100
      if (project.progress === 100 && project.status !== 'completed') {
        project.status = 'completed'
      }

      saveToStorage()
      return true
    } catch (error) {
      ElMessage.error('Proje ilerlemesi güncellenirken hata oluştu')
      return false
    }
  }

  const addMember = async (projectId: string, userId: string): Promise<boolean> => {
    loading.value = true
    try {
      const project = projects.value.find((p) => p.id === projectId)
      if (!project) {
        ElMessage.error('Proje bulunamadı')
        return false
      }

      if (project.memberIds.includes(userId)) {
        ElMessage.warning('Kullanıcı zaten üye')
        return false
      }

      project.memberIds.push(userId)
      project.updatedAt = new Date().toISOString()

      saveToStorage()
      ElMessage.success('Üye başarıyla eklendi')
      return true
    } catch (error) {
      ElMessage.error('Üye eklenirken hata oluştu')
      return false
    } finally {
      loading.value = false
    }
  }

  const removeMember = async (projectId: string, userId: string): Promise<boolean> => {
    loading.value = true
    try {
      const project = projects.value.find((p) => p.id === projectId)
      if (!project) {
        ElMessage.error('Proje bulunamadı')
        return false
      }

      // Prevent removing the owner
      if (project.ownerId === userId) {
        ElMessage.error('Proje sahibi çıkarılamaz')
        return false
      }

      const index = project.memberIds.indexOf(userId)
      if (index === -1) {
        ElMessage.warning('Kullanıcı üye değil')
        return false
      }

      project.memberIds.splice(index, 1)
      project.updatedAt = new Date().toISOString()

      saveToStorage()
      ElMessage.success('Üye başarıyla çıkarıldı')
      return true
    } catch (error) {
      ElMessage.error('Üye çıkarılırken hata oluştu')
      return false
    } finally {
      loading.value = false
    }
  }

  const filterProjects = (filter: ProjectFilter): Project[] => {
    let filtered = projects.value

    if (filter.workspaceId) {
      filtered = filtered.filter((p) => p.workspaceId === filter.workspaceId)
    }

    if (filter.status && filter.status.length > 0) {
      filtered = filtered.filter((p) => filter.status!.includes(p.status))
    }

    if (filter.priority && filter.priority.length > 0) {
      filtered = filtered.filter((p) => filter.priority!.includes(p.priority))
    }

    if (filter.memberIds && filter.memberIds.length > 0) {
      filtered = filtered.filter((p) =>
        filter.memberIds!.some((id) => p.memberIds.includes(id) || p.ownerId === id),
      )
    }

    if (filter.search) {
      const search = filter.search.toLowerCase()
      filtered = filtered.filter(
        (p) =>
          p.name.toLowerCase().includes(search) ||
          p.description.toLowerCase().includes(search) ||
          p.tags.some((tag) => tag.toLowerCase().includes(search)),
      )
    }

    return filtered
  }

  // Initialize on store creation
  initProjects()

  return {
    // State
    projects,
    loading,
    // Computed
    allProjects,
    projectCount,
    workspaceProjects,
    myProjects,
    activeProjects,
    completedProjects,
    // Actions
    fetchProjects,
    getProjectById,
    getProjectsByWorkspace,
    createProject,
    updateProject,
    deleteProject,
    updateProjectProgress,
    addMember,
    removeMember,
    filterProjects,
  }
})
