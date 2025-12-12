<template>
  <div class="kanban-view">
    <div class="page-header">
      <div class="header-content">
        <h1>Kanban Board</h1>
        <p>Görevlerinizi sürükle-bırak ile yönetin</p>
      </div>
      <div class="header-actions">
        <el-select
          v-model="selectedProjectId"
          placeholder="Proje seçin"
          style="width: 300px"
          @change="handleProjectChange"
        >
          <el-option label="Tüm Projeler" value="" />
          <el-option
            v-for="project in availableProjects"
            :key="project.id"
            :label="project.name"
            :value="project.id"
          >
            <div class="project-option">
              <div class="project-color" :style="{ backgroundColor: project.color }"></div>
              <span>{{ project.name }}</span>
            </div>
          </el-option>
        </el-select>
        <el-button
          v-if="canManageProjects"
          type="primary"
          :icon="Plus"
          @click="showCreateDialog = true"
        >
          Yeni Görev
        </el-button>
      </div>
    </div>

    <!-- Project Statistics -->
    <div v-if="selectedProjectId && currentProject" class="project-stats-card">
      <el-card class="stats-card">
        <template #header>
          <div class="stats-header">
            <h3>
              <div class="project-name-header">
                <div class="project-color" :style="{ backgroundColor: currentProject.color }"></div>
                {{ currentProject.name }} - İstatistikler
              </div>
            </h3>
            <el-tag :type="getStatusType(currentProject.status)" size="large">
              {{ getStatusText(currentProject.status) }}
            </el-tag>
          </div>
        </template>

        <el-row :gutter="24" class="stats-grid">
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-item todo">
              <div class="stat-icon">
                <el-icon :size="24"><List /></el-icon>
              </div>
              <div class="stat-content">
                <div class="stat-value">{{ projectStats.todoTasks }}</div>
                <div class="stat-label">Yapılacak</div>
              </div>
            </div>
          </el-col>

          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-item in-progress">
              <div class="stat-icon">
                <el-icon :size="24"><Loading /></el-icon>
              </div>
              <div class="stat-content">
                <div class="stat-value">{{ projectStats.inProgressTasks }}</div>
                <div class="stat-label">Devam Ediyor</div>
              </div>
            </div>
          </el-col>

          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-item review">
              <div class="stat-icon">
                <el-icon :size="24"><View /></el-icon>
              </div>
              <div class="stat-content">
                <div class="stat-value">{{ projectStats.reviewTasks }}</div>
                <div class="stat-label">İncelemede</div>
              </div>
            </div>
          </el-col>

          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-item done">
              <div class="stat-icon">
                <el-icon :size="24"><CircleCheck /></el-icon>
              </div>
              <div class="stat-content">
                <div class="stat-value">{{ projectStats.completedTasks }}</div>
                <div class="stat-label">Tamamlandı</div>
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- Progress Chart -->
        <div class="progress-chart">
          <div class="chart-header">
            <h4>Proje İlerlemesi</h4>
            <span class="progress-percentage" :style="{ color: currentProject.color }">
              {{ currentProject.progress }}%
            </span>
          </div>
          <div class="chart-container">
            <div class="circular-chart">
              <svg width="200" height="200" class="progress-ring-svg">
                <defs>
                  <linearGradient id="progressGradient" x1="0%" y1="0%" x2="100%" y2="100%">
                    <stop
                      offset="0%"
                      :style="{ stopColor: currentProject.color, stopOpacity: 1 }"
                    />
                    <stop
                      offset="100%"
                      :style="{ stopColor: currentProject.color, stopOpacity: 0.6 }"
                    />
                  </linearGradient>
                </defs>
                <circle
                  class="progress-ring-circle-bg"
                  :stroke="currentProject.color + '20'"
                  stroke-width="16"
                  fill="transparent"
                  r="84"
                  cx="100"
                  cy="100"
                />
                <circle
                  class="progress-ring-circle"
                  stroke="url(#progressGradient)"
                  stroke-width="16"
                  fill="transparent"
                  r="84"
                  cx="100"
                  cy="100"
                  :stroke-dasharray="`${(currentProject.progress / 100) * 527.79} 527.79`"
                  stroke-linecap="round"
                />
                <text
                  x="100"
                  y="100"
                  text-anchor="middle"
                  dominant-baseline="central"
                  class="progress-text-large"
                  :fill="currentProject.color"
                >
                  {{ currentProject.progress }}%
                </text>
              </svg>
            </div>
            <div class="chart-details">
              <div class="detail-row">
                <span class="detail-label">Toplam Görev:</span>
                <strong class="detail-value">{{ projectStats.totalTasks }}</strong>
              </div>
              <div class="detail-row">
                <span class="detail-label">Tamamlanan:</span>
                <strong class="detail-value success">{{ projectStats.completedTasks }}</strong>
              </div>
              <div class="detail-row">
                <span class="detail-label">Devam Eden:</span>
                <strong class="detail-value warning">{{ projectStats.inProgressTasks }}</strong>
              </div>
              <div class="detail-row">
                <span class="detail-label">Kalan:</span>
                <strong class="detail-value info">
                  {{ projectStats.totalTasks - projectStats.completedTasks }}
                </strong>
              </div>
              <el-divider />
              <div class="detail-row">
                <span class="detail-label">Başlangıç:</span>
                <span class="detail-value">{{ formatDateShort(currentProject.startDate) }}</span>
              </div>
              <div v-if="currentProject.endDate" class="detail-row">
                <span class="detail-label">Bitiş:</span>
                <span class="detail-value">{{ formatDateShort(currentProject.endDate) }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Tamamlanma Oranı:</span>
                <el-progress
                  :percentage="currentProject.progress"
                  :color="getProgressColor(currentProject.progress)"
                  :stroke-width="10"
                />
              </div>
            </div>
          </div>
        </div>
      </el-card>
    </div>

    <!-- Kanban Board -->
    <div class="kanban-board">
      <div v-for="column in kanbanColumns" :key="column.id" class="kanban-column">
        <div class="column-header" :style="{ borderColor: column.color }">
          <div class="column-title">
            <span>{{ column.title }}</span>
            <el-badge :value="column.tasks.length" :type="getBadgeType(column.id)" />
          </div>
          <el-button
            v-if="canManageProjects"
            class="add-task-btn"
            :icon="Plus"
            circle
            size="small"
            @click="handleAddTask(column.id)"
          />
        </div>

        <div
          class="column-body"
          @drop="handleDrop($event, column.id)"
          @dragover.prevent
          @dragenter.prevent
        >
          <div v-if="column.tasks.length === 0" class="empty-column">
            <el-icon :size="40" :color="column.color + '40'">
              <Document />
            </el-icon>
            <p>Görev yok</p>
          </div>

          <div
            v-for="task in column.tasks"
            :key="task.id"
            class="task-card"
            draggable="true"
            @dragstart="handleDragStart($event, task)"
            @dragend="handleDragEnd"
            @click="handleTaskClick(task)"
          >
            <div class="task-header">
              <el-tag :type="getPriorityType(task.priority)" size="small">
                {{ getPriorityText(task.priority) }}
              </el-tag>
              <el-dropdown v-if="canEdit(task)" trigger="click" @click.stop>
                <el-icon class="task-menu-icon">
                  <MoreFilled />
                </el-icon>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item @click="handleEditTask(task)">
                      <el-icon><Edit /></el-icon>
                      Düzenle
                    </el-dropdown-item>
                    <el-dropdown-item divided @click="handleDeleteTask(task)">
                      <el-icon><Delete /></el-icon>
                      Sil
                    </el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </div>

            <div class="task-title">{{ task.title }}</div>

            <div v-if="task.description" class="task-description">{{ task.description }}</div>

            <div class="task-meta">
              <div v-if="task.dueDate" class="task-due" :class="{ overdue: isOverdue(task) }">
                <el-icon><Calendar /></el-icon>
                <span>{{ formatDate(task.dueDate) }}</span>
              </div>

              <div v-if="task.tags.length > 0" class="task-tags">
                <el-tag v-for="tag in task.tags.slice(0, 2)" :key="tag" size="small" type="info">
                  {{ tag }}
                </el-tag>
              </div>
            </div>

            <div v-if="task.subtasks.length > 0" class="task-progress">
              <el-progress
                :percentage="getTaskProgress(task)"
                :stroke-width="6"
                :show-text="false"
                :color="column.color"
              />
              <span class="progress-text">
                {{ getCompletedSubtasks(task) }}/{{ task.subtasks.length }}
              </span>
            </div>

            <div v-if="task.assigneeIds.length > 0" class="task-assignees">
              <el-avatar-group :max="3" size="small">
                <el-avatar v-for="userId in task.assigneeIds" :key="userId" :size="24">
                  {{ getUserInitial(userId) }}
                </el-avatar>
              </el-avatar-group>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Create/Edit Task Dialog -->
    <el-dialog
      v-model="showCreateDialog"
      :title="editingTask ? 'Görev Düzenle' : 'Yeni Görev'"
      width="700px"
      @close="handleDialogClose"
    >
      <el-form ref="formRef" :model="form" :rules="rules" label-position="top" size="large">
        <el-row :gutter="16">
          <el-col :span="24">
            <el-form-item label="Görev Başlığı" prop="title">
              <el-input v-model="form.title" placeholder="Görev başlığını girin" />
            </el-form-item>
          </el-col>

          <el-col :span="24">
            <el-form-item label="Açıklama" prop="description">
              <el-input
                v-model="form.description"
                type="textarea"
                :rows="3"
                placeholder="Görev açıklaması"
              />
            </el-form-item>
          </el-col>

          <el-col :span="12">
            <el-form-item label="Proje" prop="projectId">
              <el-select
                v-model="form.projectId"
                placeholder="Proje seçin"
                style="width: 100%"
                :disabled="!!selectedProjectId"
              >
                <el-option
                  v-for="project in availableProjects"
                  :key="project.id"
                  :label="project.name"
                  :value="project.id"
                />
              </el-select>
            </el-form-item>
          </el-col>

          <el-col :span="12">
            <el-form-item label="Durum" prop="status">
              <el-select v-model="form.status" placeholder="Durum seçin" style="width: 100%">
                <el-option label="Yapılacak" value="todo" />
                <el-option label="Devam Ediyor" value="in-progress" />
                <el-option label="İncelemede" value="review" />
                <el-option label="Tamamlandı" value="done" />
              </el-select>
            </el-form-item>
          </el-col>

          <el-col :span="12">
            <el-form-item label="Öncelik" prop="priority">
              <el-select v-model="form.priority" placeholder="Öncelik seçin" style="width: 100%">
                <el-option label="Düşük" value="low" />
                <el-option label="Orta" value="medium" />
                <el-option label="Yüksek" value="high" />
                <el-option label="Kritik" value="critical" />
              </el-select>
            </el-form-item>
          </el-col>

          <el-col :span="12">
            <el-form-item label="Son Tarih">
              <el-date-picker
                v-model="form.dueDate"
                type="date"
                placeholder="Son tarih"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>

          <el-col :span="24">
            <el-form-item label="Atanan Kullanıcılar" prop="assigneeIds">
              <el-select
                v-model="form.assigneeIds"
                multiple
                filterable
                placeholder="Kullanıcı seçin"
                style="width: 100%"
              >
                <el-option
                  v-for="user in availableMembers"
                  :key="user.id"
                  :label="user.fullName"
                  :value="user.id"
                >
                  <div class="user-option">
                    <el-avatar :size="24">{{ user.fullName.charAt(0) }}</el-avatar>
                    <span>{{ user.fullName }}</span>
                  </div>
                </el-option>
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>

      <template #footer>
        <el-button @click="showCreateDialog = false">İptal</el-button>
        <el-button type="primary" :loading="loading" @click="handleSubmit">
          {{ editingTask ? 'Güncelle' : 'Oluştur' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, watch } from 'vue'
import { useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import {
  Plus,
  Document,
  Calendar,
  Edit,
  Delete,
  MoreFilled,
  List,
  Loading,
  View,
  CircleCheck,
} from '@element-plus/icons-vue'
import { useProjectsStore } from '@/stores/projects'
import { useTasksStore } from '@/stores/tasks'
import { useUsersStore } from '@/stores/users'
import { useAuthStore } from '@/stores/auth'
import type { Task, TaskForm, TaskStatus } from '@/types'

const route = useRoute()
const projectsStore = useProjectsStore()
const tasksStore = useTasksStore()
const usersStore = useUsersStore()
const authStore = useAuthStore()

// State
const selectedProjectId = ref('')
const showCreateDialog = ref(false)
const loading = ref(false)
const editingTask = ref<Task | null>(null)
const draggedTask = ref<Task | null>(null)
const formRef = ref<FormInstance>()

const form = reactive<TaskForm>({
  title: '',
  description: '',
  projectId: '',
  assigneeIds: [],
  status: 'todo',
  priority: 'medium',
  dueDate: '',
  estimatedHours: 0,
  tags: [],
})

const rules: FormRules = {
  title: [
    { required: true, message: 'Görev başlığı gereklidir', trigger: 'blur' },
    { min: 3, message: 'En az 3 karakter olmalıdır', trigger: 'blur' },
  ],
  description: [{ required: true, message: 'Açıklama gereklidir', trigger: 'blur' }],
  projectId: [{ required: true, message: 'Proje seçimi gereklidir', trigger: 'change' }],
  status: [{ required: true, message: 'Durum seçimi gereklidir', trigger: 'change' }],
  priority: [{ required: true, message: 'Öncelik seçimi gereklidir', trigger: 'change' }],
}

// Computed
const canManageProjects = computed(() => authStore.canManageProjects)
const availableProjects = computed(() => {
  if (authStore.isAdmin) return projectsStore.allProjects
  return projectsStore.myProjects
})

const currentProject = computed(() => {
  if (!selectedProjectId.value) return null
  return projectsStore.getProjectById(selectedProjectId.value)
})

const availableMembers = computed(() => {
  return usersStore.allUsers.filter((u) => u.role === 'member' || u.role === 'workspaceAdmin')
})

const kanbanColumns = computed(() => tasksStore.getKanbanData(selectedProjectId.value || undefined))

const projectStats = computed(() => {
  if (!selectedProjectId.value) {
    return {
      totalTasks: tasksStore.taskCount,
      todoTasks: tasksStore.todoTasks.length,
      inProgressTasks: tasksStore.inProgressTasks.length,
      reviewTasks: tasksStore.reviewTasks.length,
      completedTasks: tasksStore.completedTasks.length,
    }
  }

  const projectTasks = tasksStore.getTasksByProject(selectedProjectId.value)
  return {
    totalTasks: projectTasks.length,
    todoTasks: projectTasks.filter((t) => t.status === 'todo').length,
    inProgressTasks: projectTasks.filter((t) => t.status === 'in-progress').length,
    reviewTasks: projectTasks.filter((t) => t.status === 'review').length,
    completedTasks: projectTasks.filter((t) => t.status === 'done').length,
  }
})

// Methods
const canEdit = (task: Task): boolean => {
  if (authStore.isAdmin) return true
  if (authStore.isWorkspaceAdmin) {
    const project = projectsStore.getProjectById(task.projectId)
    return !!project
  }
  return task.assigneeIds.includes(authStore.user?.id || '')
}

const getBadgeType = (columnId: string) => {
  const types: Record<string, string> = {
    todo: 'info',
    'in-progress': 'warning',
    review: '',
    done: 'success',
  }
  return types[columnId] || 'info'
}

const getStatusType = (status: string): string => {
  const types: Record<string, string> = {
    planning: 'info',
    active: 'success',
    'on-hold': 'warning',
    completed: 'success',
    cancelled: 'danger',
  }
  return types[status] || 'info'
}

const getStatusText = (status: string): string => {
  const texts: Record<string, string> = {
    planning: 'Planlama',
    active: 'Aktif',
    'on-hold': 'Beklemede',
    completed: 'Tamamlandı',
    cancelled: 'İptal',
  }
  return texts[status] || status
}

const getPriorityType = (priority: string) => {
  const types: Record<string, string> = {
    low: 'info',
    medium: 'warning',
    high: 'danger',
    critical: 'danger',
  }
  return types[priority] || 'info'
}

const getPriorityText = (priority: string) => {
  const texts: Record<string, string> = {
    low: 'Düşük',
    medium: 'Orta',
    high: 'Yüksek',
    critical: 'Kritik',
  }
  return texts[priority] || priority
}

const getProgressColor = (progress: number): string => {
  if (progress >= 80) return '#67C23A'
  if (progress >= 60) return '#E6A23C'
  if (progress >= 40) return '#409EFF'
  return '#F56C6C'
}

const formatDate = (dateStr: string) => {
  const date = new Date(dateStr)
  const now = new Date()
  const diffTime = date.getTime() - now.getTime()
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))

  if (diffDays < 0) return `${Math.abs(diffDays)}g gecikti`
  if (diffDays === 0) return 'Bugün'
  if (diffDays === 1) return 'Yarın'
  return `${diffDays}g sonra`
}

const formatDateShort = (dateStr: string) => {
  const date = new Date(dateStr)
  return date.toLocaleDateString('tr-TR', { year: 'numeric', month: 'short', day: 'numeric' })
}

const isOverdue = (task: Task): boolean => {
  if (!task.dueDate || task.status === 'done') return false
  return new Date(task.dueDate) < new Date()
}

const getUserInitial = (userId: string) => {
  const user = usersStore.getUserById(userId)
  return user?.fullName.charAt(0) || 'U'
}

const getTaskProgress = (task: Task) => {
  if (task.subtasks.length === 0) return 0
  const completed = task.subtasks.filter((st) => st.completed).length
  return Math.round((completed / task.subtasks.length) * 100)
}

const getCompletedSubtasks = (task: Task) => {
  return task.subtasks.filter((st) => st.completed).length
}

const handleProjectChange = (projectId: string) => {
  selectedProjectId.value = projectId
}

const handleAddTask = (status: TaskStatus) => {
  form.status = status
  if (selectedProjectId.value) {
    form.projectId = selectedProjectId.value
  }
  showCreateDialog.value = true
}

const handleTaskClick = (task: Task) => {
  // Show task details or open edit dialog
  handleEditTask(task)
}

const handleEditTask = (task: Task) => {
  editingTask.value = task
  form.title = task.title
  form.description = task.description
  form.projectId = task.projectId
  form.assigneeIds = [...task.assigneeIds]
  form.status = task.status
  form.priority = task.priority
  form.dueDate = task.dueDate || ''
  form.estimatedHours = task.estimatedHours || 0
  form.tags = [...task.tags]
  showCreateDialog.value = true
}

const handleDeleteTask = async (task: Task) => {
  try {
    await ElMessageBox.confirm(
      `"${task.title}" görevini silmek istediğinize emin misiniz?`,
      'Görev Sil',
      {
        confirmButtonText: 'Evet, Sil',
        cancelButtonText: 'İptal',
        type: 'warning',
      },
    )

    const success = await tasksStore.deleteTask(task.id)
    if (success) {
      ElMessage.success('Görev başarıyla silindi')
    }
  } catch {
    // User cancelled
  }
}

const handleDragStart = (event: DragEvent, task: Task) => {
  draggedTask.value = task
  if (event.dataTransfer) {
    event.dataTransfer.effectAllowed = 'move'
    event.dataTransfer.dropEffect = 'move'
  }
}

const handleDragEnd = () => {
  draggedTask.value = null
}

const handleDrop = async (event: DragEvent, newStatus: TaskStatus) => {
  event.preventDefault()

  if (!draggedTask.value) return

  const task = draggedTask.value

  // Check if status actually changed
  if (task.status === newStatus) {
    draggedTask.value = null
    return
  }

  // Update task status
  const success = await tasksStore.updateTaskStatus(task.id, newStatus)

  if (success) {
    ElMessage.success(`Görev "${task.title}" durumu güncellendi`)
  }

  draggedTask.value = null
}

const handleSubmit = async () => {
  if (!formRef.value) return

  await formRef.value.validate(async (valid) => {
    if (!valid) return

    loading.value = true

    let success = false
    if (editingTask.value) {
      success = await tasksStore.updateTask(editingTask.value.id, form)
    } else {
      success = await tasksStore.createTask(form)
    }

    if (success) {
      showCreateDialog.value = false
      handleDialogClose()
    }

    loading.value = false
  })
}

const handleDialogClose = () => {
  editingTask.value = null
  form.title = ''
  form.description = ''
  form.projectId = selectedProjectId.value || ''
  form.assigneeIds = []
  form.status = 'todo'
  form.priority = 'medium'
  form.dueDate = ''
  form.estimatedHours = 0
  form.tags = []
  formRef.value?.resetFields()
}

// Initialize from route params
watch(
  () => route.query.project,
  (projectId) => {
    if (projectId && typeof projectId === 'string') {
      selectedProjectId.value = projectId
    }
  },
  { immediate: true },
)

// Set default project if available
watch(
  () => availableProjects.value,
  (newProjects) => {
    if (newProjects.length > 0 && !selectedProjectId.value) {
      selectedProjectId.value = newProjects[0].id
    }
  },
  { immediate: true },
)

// Initialize
projectsStore.fetchProjects()
tasksStore.fetchTasks()
usersStore.fetchUsers()
</script>

<style scoped>
.kanban-view {
  animation: fadeIn 0.3s ease;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  padding-bottom: 20px;
  border-bottom: 1px solid var(--border-color);
}

.header-content h1 {
  font-size: 32px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 8px;
}

.header-content p {
  font-size: 16px;
  color: var(--text-secondary);
  margin: 0;
}

.header-actions {
  display: flex;
  gap: 12px;
}

.project-option {
  display: flex;
  align-items: center;
  gap: 8px;
}

.project-color {
  width: 12px;
  height: 12px;
  border-radius: 50%;
}

.project-stats-card {
  margin-bottom: 24px;
}

.stats-card {
  border-radius: 16px;
  border: 1px solid var(--border-color);
}

.stats-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.stats-header h3 {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}

.project-name-header {
  display: flex;
  align-items: center;
  gap: 12px;
}

.stats-grid {
  margin-bottom: 32px;
}

.stat-item {
  background: var(--bg-tertiary);
  border-radius: 12px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 16px;
  border: 1px solid var(--border-color);
  transition: all 0.3s ease;
}

.stat-item:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
}

.stat-icon {
  width: 56px;
  height: 56px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.stat-item.todo .stat-icon {
  background: linear-gradient(135deg, #909399, #747a82);
}

.stat-item.in-progress .stat-icon {
  background: linear-gradient(135deg, #409eff, #337ecc);
}

.stat-item.review .stat-icon {
  background: linear-gradient(135deg, #e6a23c, #cf9236);
}

.stat-item.done .stat-icon {
  background: linear-gradient(135deg, #67c23a, #5daf34);
}

.stat-content {
  flex: 1;
}

.stat-value {
  font-size: 28px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 4px;
}

.stat-label {
  font-size: 14px;
  color: var(--text-secondary);
}

.progress-chart {
  margin-top: 24px;
  padding-top: 24px;
  border-top: 1px solid var(--border-color);
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.chart-header h4 {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}

.progress-percentage {
  font-size: 24px;
  font-weight: 700;
}

.chart-container {
  display: flex;
  gap: 32px;
  align-items: center;
}

.circular-chart {
  flex-shrink: 0;
}

.progress-ring-svg {
  transform: rotate(-90deg);
}

.progress-ring-circle {
  transition: stroke-dasharray 0.6s ease;
}

.progress-text-large {
  font-size: 32px;
  font-weight: 700;
  transform: rotate(90deg);
  transform-origin: center;
}

.chart-details {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 14px;
}

.detail-label {
  color: var(--text-secondary);
}

.detail-value {
  color: var(--text-primary);
  font-weight: 600;
}

.detail-value.success {
  color: var(--color-success);
}

.detail-value.warning {
  color: var(--color-warning);
}

.detail-value.info {
  color: var(--color-info);
}

.kanban-board {
  display: flex;
  gap: 24px;
  overflow-x: auto;
  padding-bottom: 20px;
  min-height: 500px;
}

.kanban-column {
  flex: 0 0 320px;
  min-width: 320px;
  display: flex;
  flex-direction: column;
  background: var(--bg-secondary);
  border-radius: 16px;
  border: 1px solid var(--border-color);
  overflow: hidden;
  max-height: 800px;
}

.column-header {
  padding: 20px;
  border-bottom: 2px solid currentColor;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: var(--bg-tertiary);
}

.column-title {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
}

.add-task-btn {
  background: transparent;
  border: 1px solid var(--border-color);
  color: var(--text-secondary);
}

.add-task-btn:hover {
  background: var(--bg-hover);
  border-color: var(--color-primary);
  color: var(--color-primary);
}

.column-body {
  flex: 1;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  overflow-y: auto;
}

.empty-column {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
  color: var(--text-tertiary);
  text-align: center;
}

.empty-column p {
  margin: 8px 0 0;
  font-size: 14px;
}

.task-card {
  background: var(--bg-primary);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 16px;
  cursor: grab;
  transition: all 0.3s ease;
}

.task-card:hover {
  border-color: var(--color-primary);
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
}

.task-card:active {
  cursor: grabbing;
  opacity: 0.8;
}

.task-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.task-menu-icon {
  cursor: pointer;
  color: var(--text-secondary);
  transition: color 0.3s ease;
}

.task-menu-icon:hover {
  color: var(--color-primary);
}

.task-title {
  font-size: 15px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 8px;
  line-height: 1.5;
}

.task-description {
  font-size: 13px;
  color: var(--text-secondary);
  margin-bottom: 12px;
  line-height: 1.6;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.task-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  gap: 8px;
}

.task-due {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  color: var(--text-secondary);
}

.task-due.overdue {
  color: var(--color-danger);
  font-weight: 600;
}

.task-tags {
  display: flex;
  gap: 4px;
}

.task-progress {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
}

.task-progress :deep(.el-progress) {
  flex: 1;
}

.progress-text {
  font-size: 12px;
  color: var(--text-secondary);
  min-width: 40px;
  text-align: right;
}

.task-assignees {
  padding-top: 12px;
  border-top: 1px solid var(--border-color);
}

.user-option {
  display: flex;
  align-items: center;
  gap: 8px;
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
  }

  .header-actions {
    flex-direction: column;
    width: 100%;
  }

  .header-actions .el-select {
    width: 100% !important;
  }

  .chart-container {
    flex-direction: column;
  }

  .kanban-board {
    gap: 16px;
  }

  .kanban-column {
    flex: 0 0 280px;
    min-width: 280px;
  }
}
</style>
