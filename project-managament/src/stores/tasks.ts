import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Task, TaskForm, TaskFilter, SubTask, Comment, KanbanColumn, TaskStatus } from '@/types'
import { ElMessage } from 'element-plus'
import { useAuthStore } from './auth'
import { useProjectsStore } from './projects'
import api from '@/api'

export const useTasksStore = defineStore('tasks', () => {
  // State
  const tasks = ref<Task[]>([])
  const loading = ref(false)

  // Computed
  const allTasks = computed(() => tasks.value)
  const taskCount = computed(() => tasks.value.length)

  // Get tasks by status
  const todoTasks = computed(() => tasks.value.filter((t) => t.status === 'todo'))
  const inProgressTasks = computed(() => tasks.value.filter((t) => t.status === 'in-progress'))
  const reviewTasks = computed(() => tasks.value.filter((t) => t.status === 'review'))
  const doneTasks = computed(() => tasks.value.filter((t) => t.status === 'done'))

  // Get tasks for current user
  const myTasks = computed(() => {
    const authStore = useAuthStore()
    const userId = authStore.user?.id
    if (!userId) return []
    return tasks.value.filter((t) => t.assigneeIds.includes(userId))
  })

  // Get overdue tasks
  const overdueTasks = computed(() => {
    const now = new Date()
    return tasks.value.filter(
      (t) => t.dueDate && new Date(t.dueDate) < now && t.status !== 'done'
    )
  })

  // Get completed tasks
  const completedTasks = computed(() => tasks.value.filter((t) => t.status === 'done'))

  // Kanban columns
  const kanbanColumns = computed((): KanbanColumn[] => [
    {
      id: 'todo',
      title: 'Yapılacak',
      tasks: todoTasks.value,
      color: '#909399',
    },
    {
      id: 'in-progress',
      title: 'Devam Ediyor',
      tasks: inProgressTasks.value,
      color: '#409EFF',
    },
    {
      id: 'review',
      title: 'İncelemede',
      tasks: reviewTasks.value,
      color: '#E6A23C',
    },
    {
      id: 'done',
      title: 'Tamamlandı',
      tasks: doneTasks.value,
      color: '#67C23A',
    },
  ])

  // Helper function to save to localStorage
  const saveToStorage = () => {
    localStorage.setItem('tasks', JSON.stringify(tasks.value))
  }

  // Initialize from localStorage
  const initTasks = () => {
    const stored = localStorage.getItem('tasks')
    if (stored) {
      tasks.value = JSON.parse(stored)
    }
  }

  // Actions
  const fetchTasks = async () => {
    loading.value = true
    try {
      const res = await api.get('/yumus')
      const list = res.data as any[]
      const mapStatus = (s: string): TaskStatus => {
        const m: Record<string, TaskStatus> = {
          Todo: 'todo',
          InProgress: 'in-progress',
          Review: 'review',
          Done: 'done',
        }
        return m[s] || 'todo'
      }

      tasks.value = list.map((t: any): Task => ({
        id: t.id,
        title: t.title,
        description: t.description,
        projectId: t.projectId,
        assigneeIds: (t.assignees || []).map((u: any) => u.id),
        status: mapStatus(t.status),
        priority: (t.priority || 'medium').toLowerCase(),
        dueDate: t.dueDate,
        estimatedHours: t.estimatedHours,
        actualHours: t.actualHours || 0,
        tags: t.tags || [],
        subtasks: [],
        attachments: [],
        comments: [],
        createdBy: '',
        createdAt: t.createdAt,
        updatedAt: t.createdAt,
      }))
      saveToStorage()
    } catch (error) {
      ElMessage.error('Task’lar alınırken hata oluştu')
    } finally {
      loading.value = false
    }
  }

  const getTaskById = (id: string): Task | undefined => {
    return tasks.value.find((t) => t.id === id)
  }

  const getTasksByProject = (projectId: string): Task[] => {
    return tasks.value.filter((t) => t.projectId === projectId)
  }

  const getTasksByUser = (userId: string): Task[] => {
    return tasks.value.filter((t) => t.assigneeIds.includes(userId))
  }

  const createTask = async (form: TaskForm): Promise<boolean> => {
    loading.value = true
    try {
      const authStore = useAuthStore()
      const userId = authStore.user?.id

      if (!userId) {
        ElMessage.error('Kullanıcı bulunamadı')
        return false
      }

      const newTask: Task = {
        id: 'task-' + Date.now() + '-' + Math.random().toString(36).substr(2, 9),
        title: form.title,
        description: form.description,
        projectId: form.projectId,
        assigneeIds: form.assigneeIds || [],
        status: form.status || 'todo',
        priority: form.priority || 'medium',
        dueDate: form.dueDate,
        estimatedHours: form.estimatedHours,
        actualHours: 0,
        tags: form.tags || [],
        subtasks: [],
        attachments: [],
        comments: [],
        createdBy: userId,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      }

      tasks.value.push(newTask)
      saveToStorage()

      // Update project progress
      updateProjectProgress(form.projectId)

      ElMessage.success('Task başarıyla oluşturuldu')
      return true
    } catch (error) {
      ElMessage.error('Task oluşturulurken hata oluştu')
      return false
    } finally {
      loading.value = false
    }
  }

  const updateTask = async (id: string, updates: Partial<Task>): Promise<boolean> => {
    loading.value = true
    try {
      const index = tasks.value.findIndex((t) => t.id === id)
      if (index === -1) {
        ElMessage.error('Task bulunamadı')
        return false
      }

      const oldProjectId = tasks.value[index].projectId

      tasks.value[index] = {
        ...tasks.value[index],
        ...updates,
        updatedAt: new Date().toISOString(),
      }

      saveToStorage()

      // Update project progress
      updateProjectProgress(tasks.value[index].projectId)
      if (oldProjectId !== tasks.value[index].projectId) {
        updateProjectProgress(oldProjectId)
      }

      ElMessage.success('Task başarıyla güncellendi')
      return true
    } catch (error) {
      ElMessage.error('Task güncellenirken hata oluştu')
      return false
    } finally {
      loading.value = false
    }
  }

  const deleteTask = async (id: string): Promise<boolean> => {
    loading.value = true
    try {
      const index = tasks.value.findIndex((t) => t.id === id)
      if (index === -1) {
        ElMessage.error('Task bulunamadı')
        return false
      }

      const projectId = tasks.value[index].projectId
      tasks.value.splice(index, 1)
      saveToStorage()

      // Update project progress
      updateProjectProgress(projectId)

      ElMessage.success('Task başarıyla silindi')
      return true
    } catch (error) {
      ElMessage.error('Task silinirken hata oluştu')
      return false
    } finally {
      loading.value = false
    }
  }

  const updateTaskStatus = async (id: string, status: TaskStatus): Promise<boolean> => {
    try {
      const task = tasks.value.find((t) => t.id === id)
      if (!task) {
        ElMessage.error('Task bulunamadı')
        return false
      }

      task.status = status
      task.updatedAt = new Date().toISOString()

      saveToStorage()

      // Update project progress
      updateProjectProgress(task.projectId)

      return true
    } catch (error) {
      ElMessage.error('Task durumu güncellenirken hata oluştu')
      return false
    }
  }

  const moveTask = async (taskId: string, newStatus: TaskStatus): Promise<boolean> => {
    return updateTaskStatus(taskId, newStatus)
  }

  const addSubTask = async (taskId: string, title: string): Promise<boolean> => {
    try {
      const task = tasks.value.find((t) => t.id === taskId)
      if (!task) {
        ElMessage.error('Task bulunamadı')
        return false
      }

      const newSubTask: SubTask = {
        id: 'subtask-' + Date.now() + '-' + Math.random().toString(36).substr(2, 9),
        title,
        completed: false,
        createdAt: new Date().toISOString(),
      }

      task.subtasks.push(newSubTask)
      task.updatedAt = new Date().toISOString()

      saveToStorage()
      ElMessage.success('Alt görev başarıyla eklendi')
      return true
    } catch (error) {
      ElMessage.error('Alt görev eklenirken hata oluştu')
      return false
    }
  }

  const toggleSubTask = async (taskId: string, subtaskId: string): Promise<boolean> => {
    try {
      const task = tasks.value.find((t) => t.id === taskId)
      if (!task) {
        ElMessage.error('Task bulunamadı')
        return false
      }

      const subtask = task.subtasks.find((st) => st.id === subtaskId)
      if (!subtask) {
        ElMessage.error('Alt görev bulunamadı')
        return false
      }

      subtask.completed = !subtask.completed
      task.updatedAt = new Date().toISOString()

      saveToStorage()

      // Update project progress
      updateProjectProgress(task.projectId)

      return true
    } catch (error) {
      ElMessage.error('Alt görev güncellenirken hata oluştu')
      return false
    }
  }

  const deleteSubTask = async (taskId: string, subtaskId: string): Promise<boolean> => {
    try {
      const task = tasks.value.find((t) => t.id === taskId)
      if (!task) {
        ElMessage.error('Task bulunamadı')
        return false
      }

      const index = task.subtasks.findIndex((st) => st.id === subtaskId)
      if (index === -1) {
        ElMessage.error('Alt görev bulunamadı')
        return false
      }

      task.subtasks.splice(index, 1)
      task.updatedAt = new Date().toISOString()

      saveToStorage()
      ElMessage.success('Alt görev başarıyla silindi')
      return true
    } catch (error) {
      ElMessage.error('Alt görev silinirken hata oluştu')
      return false
    }
  }

  const addComment = async (taskId: string, text: string): Promise<boolean> => {
    try {
      const authStore = useAuthStore()
      const userId = authStore.user?.id

      if (!userId) {
        ElMessage.error('Kullanıcı bulunamadı')
        return false
      }

      const task = tasks.value.find((t) => t.id === taskId)
      if (!task) {
        ElMessage.error('Task bulunamadı')
        return false
      }

      const newComment: Comment = {
        id: 'comment-' + Date.now() + '-' + Math.random().toString(36).substr(2, 9),
        text,
        userId,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      }

      task.comments.push(newComment)
      task.updatedAt = new Date().toISOString()

      saveToStorage()
      ElMessage.success('Yorum başarıyla eklendi')
      return true
    } catch (error) {
      ElMessage.error('Yorum eklenirken hata oluştu')
      return false
    }
  }

  const deleteComment = async (taskId: string, commentId: string): Promise<boolean> => {
    try {
      const task = tasks.value.find((t) => t.id === taskId)
      if (!task) {
        ElMessage.error('Task bulunamadı')
        return false
      }

      const index = task.comments.findIndex((c) => c.id === commentId)
      if (index === -1) {
        ElMessage.error('Yorum bulunamadı')
        return false
      }

      task.comments.splice(index, 1)
      task.updatedAt = new Date().toISOString()

      saveToStorage()
      ElMessage.success('Yorum başarıyla silindi')
      return true
    } catch (error) {
      ElMessage.error('Yorum silinirken hata oluştu')
      return false
    }
  }

  const updateProjectProgress = (projectId: string) => {
    const projectsStore = useProjectsStore()
    const projectTasks = tasks.value.filter((t) => t.projectId === projectId)

    if (projectTasks.length === 0) {
      projectsStore.updateProjectProgress(projectId, 0)
      return
    }

    const completedCount = projectTasks.filter((t) => t.status === 'done').length
    const progress = Math.round((completedCount / projectTasks.length) * 100)

    projectsStore.updateProjectProgress(projectId, progress)
  }

  const filterTasks = (filter: TaskFilter): Task[] => {
    let filtered = tasks.value

    if (filter.projectId) {
      filtered = filtered.filter((t) => t.projectId === filter.projectId)
    }

    if (filter.status && filter.status.length > 0) {
      filtered = filtered.filter((t) => filter.status!.includes(t.status))
    }

    if (filter.priority && filter.priority.length > 0) {
      filtered = filtered.filter((t) => filter.priority!.includes(t.priority))
    }

    if (filter.assigneeIds && filter.assigneeIds.length > 0) {
      filtered = filtered.filter((t) =>
        filter.assigneeIds!.some((id) => t.assigneeIds.includes(id))
      )
    }

    if (filter.overdue) {
      const now = new Date()
      filtered = filtered.filter(
        (t) => t.dueDate && new Date(t.dueDate) < now && t.status !== 'done'
      )
    }

    if (filter.search) {
      const search = filter.search.toLowerCase()
      filtered = filtered.filter(
        (t) =>
          t.title.toLowerCase().includes(search) ||
          t.description.toLowerCase().includes(search) ||
          t.tags.some((tag) => tag.toLowerCase().includes(search))
      )
    }

    return filtered
  }

  const getKanbanData = (projectId?: string): KanbanColumn[] => {
    const projectTasks = projectId
      ? tasks.value.filter((t) => t.projectId === projectId)
      : tasks.value

    return [
      {
        id: 'todo',
        title: 'Yapılacak',
        tasks: projectTasks.filter((t) => t.status === 'todo'),
        color: '#909399',
      },
      {
        id: 'in-progress',
        title: 'Devam Ediyor',
        tasks: projectTasks.filter((t) => t.status === 'in-progress'),
        color: '#409EFF',
      },
      {
        id: 'review',
        title: 'İncelemede',
        tasks: projectTasks.filter((t) => t.status === 'review'),
        color: '#E6A23C',
      },
      {
        id: 'done',
        title: 'Tamamlandı',
        tasks: projectTasks.filter((t) => t.status === 'done'),
        color: '#67C23A',
      },
    ]
  }

  const assignTask = async (taskId: string, userIds: string[]): Promise<boolean> => {
    try {
      const task = tasks.value.find((t) => t.id === taskId)
      if (!task) {
        ElMessage.error('Task bulunamadı')
        return false
      }

      task.assigneeIds = userIds
      task.updatedAt = new Date().toISOString()

      saveToStorage()
      ElMessage.success('Task başarıyla atandı')
      return true
    } catch (error) {
      ElMessage.error('Task atanırken hata oluştu')
      return false
    }
  }

  // Initialize on store creation
  initTasks()

  return {
    // State
    tasks,
    loading,
    // Computed
    allTasks,
    taskCount,
    todoTasks,
    inProgressTasks,
    reviewTasks,
    doneTasks,
    myTasks,
    overdueTasks,
    completedTasks,
    kanbanColumns,
    // Actions
    fetchTasks,
    getTaskById,
    getTasksByProject,
    getTasksByUser,
    createTask,
    updateTask,
    deleteTask,
    updateTaskStatus,
    moveTask,
    addSubTask,
    toggleSubTask,
    deleteSubTask,
    addComment,
    deleteComment,
    filterTasks,
    getKanbanData,
    assignTask,
  }
})
