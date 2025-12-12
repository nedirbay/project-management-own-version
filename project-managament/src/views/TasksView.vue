<template>
  <div class="tasks-view">
    <div class="page-header">
      <div class="header-content">
        <h1>{{ $t('tasks.title') }}</h1>
        <p>{{ $t('tasks.taskDescription') || 'Manage all your tasks' }}</p>
      </div>
      <div class="header-actions">
        <el-button-group class="view-mode-toggle">
          <el-button :type="viewMode === 'list' ? 'primary' : ''" @click="viewMode = 'list'">
            <el-icon><List /></el-icon>
            {{ $t('tasks.listView') }}
          </el-button>
          <el-button
            :type="viewMode === 'calendar' ? 'primary' : ''"
            @click="viewMode = 'calendar'"
          >
            <el-icon><Calendar /></el-icon>
            {{ $t('tasks.calendarView') }}
          </el-button>
        </el-button-group>
        <el-button
          v-if="canManageProjects"
          type="primary"
          :icon="Plus"
          @click="showCreateDialog = true"
        >
          {{ $t('tasks.createTask') }}
        </el-button>
      </div>
    </div>

    <!-- Filters -->
    <el-card class="filter-card">
      <el-row :gutter="16">
        <el-col :xs="24" :sm="12" :md="6">
          <el-select
            v-model="filters.workspaceId"
            :placeholder="$t('tasks.filterByProject')"
            clearable
            style="width: 100%"
            @change="handleWorkspaceChange"
          >
            <el-option
              v-for="workspace in workspaces"
              :key="workspace.id"
              :label="workspace.name"
              :value="workspace.id"
            />
          </el-select>
        </el-col>
        <el-col :xs="24" :sm="12" :md="6">
          <el-select
            v-model="filters.projectId"
            :placeholder="$t('projects.title')"
            clearable
            style="width: 100%"
          >
            <el-option
              v-for="project in filteredProjects"
              :key="project.id"
              :label="project.name"
              :value="project.id"
            >
              <div class="project-option">
                <span class="project-color" :style="{ background: project.color }"></span>
                <span>{{ project.name }}</span>
              </div>
            </el-option>
          </el-select>
        </el-col>
        <el-col :xs="24" :sm="12" :md="6">
          <el-select
            v-model="filters.status"
            :placeholder="$t('tasks.filterByStatus')"
            multiple
            clearable
            style="width: 100%"
          >
            <el-option :label="$t('tasks.status.todo')" value="todo" />
            <el-option :label="$t('tasks.status.inProgress')" value="in-progress" />
            <el-option :label="$t('tasks.status.review')" value="review" />
            <el-option :label="$t('tasks.status.done')" value="done" />
          </el-select>
        </el-col>
        <el-col :xs="24" :sm="12" :md="6">
          <el-input
            v-model="filters.search"
            :placeholder="$t('common.search')"
            :prefix-icon="Search"
            clearable
          />
        </el-col>
      </el-row>
    </el-card>

    <!-- List View -->
    <div v-if="viewMode === 'list'" v-loading="loading">
      <div v-if="filteredTasks.length > 0" class="tasks-list">
        <div
          v-for="task in filteredTasks"
          :key="task.id"
          class="task-card"
          @click="handleTaskClick(task)"
        >
          <div class="task-header">
            <div class="task-status">
              <el-tag :type="getStatusType(task.status)" size="small">
                {{ getStatusText(task.status) }}
              </el-tag>
              <el-tag :type="getPriorityType(task.priority)" size="small">
                {{ getPriorityText(task.priority) }}
              </el-tag>
            </div>
            <el-dropdown v-if="canEdit(task)" trigger="click" @click.stop>
              <el-button class="action-btn" :icon="More" circle size="small" />
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item @click="handleEdit(task)">
                    <el-icon><Edit /></el-icon>
                    {{ $t('common.edit') }}
                  </el-dropdown-item>
                  <el-dropdown-item @click="handleChangeStatus(task)">
                    <el-icon><Refresh /></el-icon>
                    Change Status
                  </el-dropdown-item>
                  <el-dropdown-item divided @click="handleDelete(task)">
                    <el-icon><Delete /></el-icon>
                    {{ $t('common.delete') }}
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </div>

          <h3 class="task-title">{{ task.title }}</h3>
          <p v-if="task.description" class="task-description">{{ task.description }}</p>

          <div class="task-meta">
            <div class="meta-item">
              <el-icon><Folder /></el-icon>
              <span>{{ getProjectName(task.projectId) }}</span>
            </div>
            <div v-if="task.dueDate" class="meta-item" :class="{ overdue: isOverdue(task) }">
              <el-icon><Clock /></el-icon>
              <span>{{ formatDate(task.dueDate) }}</span>
            </div>
            <div v-if="task.estimatedHours" class="meta-item">
              <el-icon><Timer /></el-icon>
              <span>{{ task.estimatedHours }}h</span>
            </div>
          </div>

          <div v-if="task.subtasks && task.subtasks.length > 0" class="task-progress">
            <div class="progress-info">
              <span class="progress-text">
                {{ getCompletedSubtasks(task) }} / {{ task.subtasks.length }}
                {{ $t('tasks.title') }}
              </span>
              <span class="progress-text">{{ getTaskProgress(task) }}%</span>
            </div>
            <el-progress :percentage="getTaskProgress(task)" :stroke-width="6" :show-text="false" />
          </div>

          <div v-if="task.assigneeIds && task.assigneeIds.length > 0" class="task-assignees">
            <el-avatar-group :max="3" size="small">
              <el-avatar v-for="userId in task.assigneeIds" :key="userId" size="small">
                {{ getUserInitial(userId) }}
              </el-avatar>
            </el-avatar-group>
          </div>
        </div>
      </div>

      <el-empty v-else :description="$t('tasks.noTasks')" />
    </div>

    <!-- Calendar View -->
    <div v-else-if="viewMode === 'calendar'" v-loading="loading" class="calendar-container">
      <FullCalendar :options="calendarOptions" ref="fullCalendar" />
    </div>

    <!-- Create/Edit Dialog -->
    <el-dialog
      v-model="showCreateDialog"
      :title="editingTask ? $t('tasks.editTask') : $t('tasks.createTask')"
      width="600px"
      @close="handleDialogClose"
    >
      <el-form ref="formRef" :model="form" :rules="rules" label-position="top" size="large">
        <el-form-item :label="$t('tasks.taskName')" prop="title">
          <el-input v-model="form.title" :placeholder="$t('tasks.taskName')" />
        </el-form-item>

        <el-form-item :label="$t('tasks.taskDescription')" prop="description">
          <el-input
            v-model="form.description"
            type="textarea"
            :rows="4"
            :placeholder="$t('tasks.taskDescription')"
          />
        </el-form-item>

        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item :label="$t('tasks.project')" prop="projectId">
              <el-select v-model="form.projectId" style="width: 100%">
                <el-option
                  v-for="project in availableProjects"
                  :key="project.id"
                  :label="project.name"
                  :value="project.id"
                >
                  <div class="project-option">
                    <span class="project-color" :style="{ background: project.color }"></span>
                    <span>{{ project.name }}</span>
                  </div>
                </el-option>
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item :label="$t('tasks.assignedTo')" prop="assigneeIds">
              <el-select v-model="form.assigneeIds" multiple style="width: 100%">
                <el-option
                  v-for="user in availableMembers"
                  :key="user.id"
                  :label="user.fullName"
                  :value="user.id"
                >
                  <div class="user-option">
                    <el-avatar :size="24">{{ getUserInitial(user.id) }}</el-avatar>
                    <span>{{ user.fullName }}</span>
                  </div>
                </el-option>
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="16">
          <el-col :span="8">
            <el-form-item :label="$t('tasks.status.label')" prop="status">
              <el-select v-model="form.status" style="width: 100%">
                <el-option :label="$t('tasks.status.todo')" value="todo" />
                <el-option :label="$t('tasks.status.inProgress')" value="in-progress" />
                <el-option :label="$t('tasks.status.review')" value="review" />
                <el-option :label="$t('tasks.status.done')" value="done" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item :label="$t('tasks.priority.label')" prop="priority">
              <el-select v-model="form.priority" style="width: 100%">
                <el-option :label="$t('tasks.priority.low')" value="low" />
                <el-option :label="$t('tasks.priority.medium')" value="medium" />
                <el-option :label="$t('tasks.priority.high')" value="high" />
                <el-option :label="$t('tasks.priority.critical')" value="critical" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item :label="$t('common.dueDate')" prop="dueDate">
              <el-date-picker
                v-model="form.dueDate"
                type="date"
                style="width: 100%"
                :placeholder="$t('common.dueDate')"
              />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="Estimated Hours">
          <el-input-number v-model="form.estimatedHours" :min="0" :step="0.5" />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="showCreateDialog = false">{{ $t('common.cancel') }}</el-button>
        <el-button type="primary" @click="handleSubmit" :loading="loading">
          {{ $t('common.save') }}
        </el-button>
      </template>
    </el-dialog>

    <!-- Status Change Dialog -->
    <el-dialog v-model="showStatusDialog" title="Change Task Status" width="400px">
      <el-select v-model="selectedStatus" style="width: 100%">
        <el-option :label="$t('tasks.status.todo')" value="todo" />
        <el-option :label="$t('tasks.status.inProgress')" value="in-progress" />
        <el-option :label="$t('tasks.status.review')" value="review" />
        <el-option :label="$t('tasks.status.done')" value="done" />
      </el-select>

      <template #footer>
        <el-button @click="showStatusDialog = false">{{ $t('common.cancel') }}</el-button>
        <el-button type="primary" @click="handleStatusSubmit">
          {{ $t('common.confirm') }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox, type FormInstance, type FormRules } from 'element-plus'
import {
  Plus,
  Search,
  More,
  Edit,
  Delete,
  Refresh,
  Folder,
  Clock,
  Timer,
  Calendar,
  List,
} from '@element-plus/icons-vue'
import { useTasksStore } from '@/stores/tasks'
import { useProjectsStore } from '@/stores/projects'
import { useWorkspacesStore } from '@/stores/workspaces'
import { useUsersStore } from '@/stores/users'
import { useAuthStore } from '@/stores/auth'
import { useI18n } from 'vue-i18n'

// FullCalendar imports
import FullCalendar from '@fullcalendar/vue3'
import dayGridPlugin from '@fullcalendar/daygrid'
import timeGridPlugin from '@fullcalendar/timegrid'
import interactionPlugin from '@fullcalendar/interaction'
import listPlugin from '@fullcalendar/list'
import type { EventInput, DateSelectArg, EventClickArg, EventDropArg } from '@fullcalendar/core'

const { t } = useI18n()
const router = useRouter()
const tasksStore = useTasksStore()
const projectsStore = useProjectsStore()
const workspacesStore = useWorkspacesStore()
const usersStore = useUsersStore()
const authStore = useAuthStore()

// State
const viewMode = ref<'list' | 'calendar'>('list')
const showCreateDialog = ref(false)
const showStatusDialog = ref(false)
const loading = ref(false)
const editingTask = ref<any>(null)
const selectedTask = ref<any>(null)
const selectedStatus = ref('')
const formRef = ref<FormInstance>()
const fullCalendar = ref()

// Filters
const filters = ref({
  workspaceId: '',
  projectId: '',
  status: [] as string[],
  priority: [] as string[],
  search: '',
})

// Form
const form = ref({
  title: '',
  description: '',
  projectId: '',
  assigneeIds: [] as string[],
  status: 'todo',
  priority: 'medium',
  dueDate: '',
  estimatedHours: 0,
  tags: [] as string[],
})

// Validation Rules
const rules: FormRules = {
  title: [
    { required: true, message: t('validation.required'), trigger: 'blur' },
    { min: 3, message: t('validation.minLength', { min: 3 }), trigger: 'blur' },
  ],
  description: [
    { required: true, message: t('validation.required'), trigger: 'blur' },
    { min: 10, message: t('validation.minLength', { min: 10 }), trigger: 'blur' },
  ],
  projectId: [{ required: true, message: t('validation.required'), trigger: 'change' }],
  status: [{ required: true, message: t('validation.required'), trigger: 'change' }],
  priority: [{ required: true, message: t('validation.required'), trigger: 'change' }],
}

// Computed
const canManageProjects = computed(() => {
  return authStore.currentUser?.role === 'admin' || authStore.currentUser?.role === 'workspaceAdmin'
})

const workspaces = computed(() => workspacesStore.workspaces)

const filteredProjects = computed(() => {
  if (filters.value.workspaceId) {
    return projectsStore.projects.filter((p) => p.workspaceId === filters.value.workspaceId)
  }
  return projectsStore.projects
})

const availableProjects = computed(() => {
  if (authStore.currentUser?.role === 'admin') {
    return projectsStore.projects
  }
  return projectsStore.myProjects
})

const availableMembers = computed(() => {
  return usersStore.users
})

const filteredTasks = computed(() => {
  let tasks = tasksStore.tasks

  if (filters.value.projectId) {
    tasks = tasks.filter((t) => t.projectId === filters.value.projectId)
  }
  if (filters.value.status.length > 0) {
    tasks = tasks.filter((t) => filters.value.status.includes(t.status))
  }
  if (filters.value.priority.length > 0) {
    tasks = tasks.filter((t) => filters.value.priority.includes(t.priority))
  }
  if (filters.value.search) {
    tasks = tasks.filter((t) => t.title.toLowerCase().includes(filters.value.search.toLowerCase()))
  }

  return tasks
})

// FullCalendar Configuration
const calendarEvents = computed<EventInput[]>(() => {
  return filteredTasks.value.map((task) => {
    const project = projectsStore.projects.find((p) => p.id === task.projectId)

    return {
      id: task.id,
      title: task.title,
      start: task.dueDate || task.createdAt,
      allDay: true,
      backgroundColor: getEventColor(task.priority),
      borderColor: project?.color || '#409eff',
      extendedProps: {
        task,
        project,
      },
    }
  })
})

const calendarOptions = computed(() => ({
  plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin, listPlugin],
  initialView: 'dayGridMonth',
  headerToolbar: {
    left: 'prev,next today',
    center: 'title',
    right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek',
  },
  buttonText: {
    today: t('calendar.today'),
    month: t('calendar.month'),
    week: t('calendar.week'),
    day: t('calendar.day'),
    list: t('calendar.list'),
  },
  events: calendarEvents.value,
  editable: true,
  droppable: true,
  selectable: true,
  selectMirror: true,
  dayMaxEvents: true,
  weekends: true,
  height: 'auto',

  // Event handlers
  eventClick: handleCalendarEventClick,
  eventDrop: handleCalendarEventDrop,
  select: handleCalendarSelect,

  // Styling
  eventClassNames: (arg: any) => {
    const task = arg.event.extendedProps.task
    return [`priority-${task.priority}`, `status-${task.status}`]
  },
}))

// Calendar Event Handlers
const handleCalendarEventClick = (info: EventClickArg) => {
  const task = info.event.extendedProps.task
  handleTaskClick(task)
}

const handleCalendarEventDrop = async (info: EventDropArg) => {
  const task = info.event.extendedProps.task
  const newDate = info.event.start

  if (newDate) {
    try {
      const success = await tasksStore.updateTask(task.id, {
        ...task,
        dueDate: newDate.toISOString(),
      })

      if (success) {
        ElMessage.success(t('calendar.taskRescheduled'))
      } else {
        info.revert()
        ElMessage.error(t('errors.somethingWentWrong'))
      }
    } catch (error) {
      info.revert()
      ElMessage.error(t('errors.somethingWentWrong'))
    }
  }
}

const handleCalendarSelect = (selectInfo: DateSelectArg) => {
  form.value.dueDate = selectInfo.startStr
  showCreateDialog.value = true
}

// Helper Functions
const getEventColor = (priority: string) => {
  const colors: Record<string, string> = {
    low: '#67c23a',
    medium: '#409eff',
    high: '#e6a23c',
    critical: '#f56c6c',
  }
  return colors[priority] || '#409eff'
}

const canEdit = (task: any) => {
  if (authStore.currentUser?.role === 'admin') return true

  const project = projectsStore.projects.find((p) => p.id === task.projectId)
  if (!project) return false

  const workspace = workspacesStore.workspaces.find((w) => w.id === project.workspaceId)
  return workspace?.adminId === authStore.currentUser?.id
}

const getProjectName = (projectId: string) => {
  const project = projectsStore.projects.find((p) => p.id === projectId)
  return project?.name || 'Unknown'
}

const getUserInitial = (userId: string) => {
  const user = usersStore.users.find((u) => u.id === userId)
  return user?.fullName?.charAt(0) || 'U'
}

const getStatusType = (status: string) => {
  const types: Record<string, any> = {
    todo: '',
    'in-progress': 'primary',
    review: 'warning',
    done: 'success',
  }
  return types[status]
}

const getStatusText = (status: string) => {
  return t(`tasks.status.${status.replace('-', '')}`) || status
}

const getPriorityType = (priority: string) => {
  const types: Record<string, any> = {
    low: 'info',
    medium: '',
    high: 'warning',
    critical: 'danger',
  }
  return types[priority]
}

const getPriorityText = (priority: string) => {
  return t(`tasks.priority.${priority}`)
}

const formatDate = (dateStr: string) => {
  const date = new Date(dateStr)
  const now = new Date()
  const diffTime = date.getTime() - now.getTime()
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))

  if (diffDays < 0) return t('common.overdue') || 'Overdue'
  if (diffDays === 0) return t('calendar.today')
  if (diffDays === 1) return 'Tomorrow'
  return date.toLocaleDateString()
}

const isOverdue = (task: any) => {
  if (!task.dueDate) return false
  return new Date(task.dueDate) < new Date() && task.status !== 'done'
}

const getCompletedSubtasks = (task: any) => {
  return task.subtasks?.filter((s: any) => s.completed).length || 0
}

const getTaskProgress = (task: any) => {
  if (!task.subtasks || task.subtasks.length === 0) return 0
  const completed = getCompletedSubtasks(task)
  return Math.round((completed / task.subtasks.length) * 100)
}

// Event Handlers
const handleWorkspaceChange = () => {
  filters.value.projectId = ''
}

const handleTaskClick = (task: any) => {
  router.push(`/projects/${task.projectId}`)
}

const handleEdit = (task: any) => {
  editingTask.value = task
  form.value = {
    title: task.title,
    description: task.description,
    projectId: task.projectId,
    assigneeIds: task.assigneeIds || [],
    status: task.status,
    priority: task.priority,
    dueDate: task.dueDate,
    estimatedHours: task.estimatedHours || 0,
    tags: task.tags || [],
  }
  showCreateDialog.value = true
}

const handleChangeStatus = (task: any) => {
  selectedTask.value = task
  selectedStatus.value = task.status
  showStatusDialog.value = true
}

const handleStatusSubmit = async () => {
  if (!selectedTask.value) return

  loading.value = true
  try {
    const success = await tasksStore.updateTask(selectedTask.value.id, {
      ...selectedTask.value,
      status: selectedStatus.value,
    })

    if (success) {
      ElMessage.success(t('tasks.updated'))
      showStatusDialog.value = false
    }
  } finally {
    loading.value = false
  }
}

const handleDelete = (task: any) => {
  ElMessageBox.confirm(t('tasks.confirmDelete'), t('common.warning'), {
    confirmButtonText: t('common.yes'),
    cancelButtonText: t('common.no'),
    type: 'warning',
  }).then(async () => {
    loading.value = true
    try {
      const success = await tasksStore.deleteTask(task.id)

      if (success) {
        ElMessage.success(t('tasks.deleted'))
      }
    } finally {
      loading.value = false
    }
  })
}

const handleSubmit = async () => {
  if (!formRef.value) return

  await formRef.value.validate(async (valid) => {
    if (valid) {
      loading.value = true
      try {
        let success = false

        if (editingTask.value) {
          success = await tasksStore.updateTask(editingTask.value.id, form.value as any)
        } else {
          success = await tasksStore.createTask(form.value as any)
        }

        if (success) {
          ElMessage.success(editingTask.value ? t('tasks.updated') : t('tasks.created'))
          showCreateDialog.value = false
          handleDialogClose()
        }
      } finally {
        loading.value = false
      }
    }
  })
}

const handleDialogClose = () => {
  editingTask.value = null
  form.value = {
    title: '',
    description: '',
    projectId: '',
    assigneeIds: [],
    status: 'todo',
    priority: 'medium',
    dueDate: '',
    estimatedHours: 0,
    tags: [],
  }
  formRef.value?.resetFields()
}

// Initialize
onMounted(() => {
  tasksStore.fetchTasks()
  projectsStore.fetchProjects()
  workspacesStore.fetchWorkspaces()
  usersStore.fetchUsers()
})

// Watch for calendar view changes
watch(calendarEvents, () => {
  if (fullCalendar.value) {
    const calendarApi = fullCalendar.value.getApi()
    calendarApi.refetchEvents()
  }
})
</script>

<style scoped>
.tasks-view {
  animation: fadeIn 0.3s ease;
  padding: 24px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 32px;
  padding-bottom: 20px;
  border-bottom: 1px solid var(--el-border-color);
}

.header-content h1 {
  font-size: 32px;
  font-weight: 700;
  color: var(--el-text-color-primary);
  margin-bottom: 8px;
}

.header-content p {
  font-size: 16px;
  color: var(--el-text-color-secondary);
  margin: 0;
}

.header-actions {
  display: flex;
  gap: 12px;
  align-items: center;
}

.view-mode-toggle {
  margin-right: 12px;
}

.filter-card {
  margin-bottom: 24px;
  border-radius: 12px;
  box-shadow: var(--el-box-shadow-light);
}

/* List View */
.tasks-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 20px;
}

.task-card {
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color);
  border-radius: 12px;
  padding: 20px;
  cursor: pointer;
  transition: all 0.3s;
  box-shadow: var(--el-box-shadow-light);
}

.task-card:hover {
  transform: translateY(-4px);
  box-shadow: var(--el-box-shadow);
  border-color: var(--el-color-primary);
}

.task-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.task-status {
  display: flex;
  gap: 8px;
}

.action-btn {
  opacity: 0;
  transition: opacity 0.3s;
}

.task-card:hover .action-btn {
  opacity: 1;
}

.task-title {
  font-size: 18px;
  font-weight: 600;
  color: var(--el-text-color-primary);
  margin: 0 0 8px;
  line-height: 1.4;
}

.task-description {
  font-size: 14px;
  color: var(--el-text-color-secondary);
  margin: 0 0 16px;
  line-height: 1.6;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.task-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin-bottom: 12px;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: var(--el-text-color-secondary);
}

.meta-item.overdue {
  color: var(--el-color-danger);
  font-weight: 600;
}

.task-progress {
  margin-bottom: 12px;
}

.progress-info {
  display: flex;
  justify-content: space-between;
  margin-bottom: 6px;
  font-size: 13px;
}

.progress-text {
  font-size: 12px;
  color: var(--el-text-color-secondary);
}

.task-assignees {
  padding-top: 12px;
  border-top: 1px solid var(--el-border-color);
}

/* Calendar View */
.calendar-container {
  background: var(--el-bg-color);
  border-radius: 12px;
  padding: 24px;
  box-shadow: var(--el-box-shadow-light);
}

.calendar-container :deep(.fc) {
  font-family: inherit;
}

.calendar-container :deep(.fc-toolbar-title) {
  font-size: 24px;
  font-weight: 700;
  color: var(--el-text-color-primary);
}

.calendar-container :deep(.fc-button) {
  background: var(--el-color-primary);
  border-color: var(--el-color-primary);
  text-transform: capitalize;
}

.calendar-container :deep(.fc-button:hover) {
  background: var(--el-color-primary-light-3);
  border-color: var(--el-color-primary-light-3);
}

.calendar-container :deep(.fc-button-active) {
  background: var(--el-color-primary-dark-2);
  border-color: var(--el-color-primary-dark-2);
}

.calendar-container :deep(.fc-daygrid-day-number) {
  color: var(--el-text-color-primary);
  font-weight: 500;
}

.calendar-container :deep(.fc-daygrid-day:hover) {
  background: var(--el-fill-color-light);
}

.calendar-container :deep(.fc-event) {
  border-radius: 4px;
  padding: 2px 4px;
  margin-bottom: 2px;
  cursor: pointer;
  transition: all 0.3s;
}

.calendar-container :deep(.fc-event:hover) {
  opacity: 0.8;
  transform: scale(1.02);
}

.calendar-container :deep(.fc-event-title) {
  font-weight: 500;
  font-size: 13px;
}

.calendar-container :deep(.fc-daygrid-event-dot) {
  display: none;
}

.calendar-container :deep(.priority-low) {
  background: var(--el-color-success) !important;
  border-color: var(--el-color-success) !important;
}

.calendar-container :deep(.priority-medium) {
  background: var(--el-color-primary) !important;
  border-color: var(--el-color-primary) !important;
}

.calendar-container :deep(.priority-high) {
  background: var(--el-color-warning) !important;
  border-color: var(--el-color-warning) !important;
}

.calendar-container :deep(.priority-critical) {
  background: var(--el-color-danger) !important;
  border-color: var(--el-color-danger) !important;
}

.calendar-container :deep(.status-done) {
  opacity: 0.6;
  text-decoration: line-through;
}

.calendar-container :deep(.fc-day-today) {
  background: var(--el-color-primary-light-9) !important;
}

/* Dialog Styles */
.project-option {
  display: flex;
  align-items: center;
  gap: 8px;
}

.project-color {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  flex-shrink: 0;
}

.user-option {
  display: flex;
  align-items: center;
  gap: 8px;
}

/* Animations */
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Responsive */
@media (max-width: 768px) {
  .tasks-view {
    padding: 16px;
  }

  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
  }

  .header-actions {
    width: 100%;
    flex-direction: column;
  }

  .view-mode-toggle {
    width: 100%;
    margin-right: 0;
    margin-bottom: 8px;
  }

  .view-mode-toggle button {
    flex: 1;
  }

  .tasks-list {
    grid-template-columns: 1fr;
  }

  .calendar-container {
    padding: 16px;
  }

  .calendar-container :deep(.fc-toolbar) {
    flex-direction: column;
    gap: 12px;
  }

  .calendar-container :deep(.fc-toolbar-chunk) {
    display: flex;
    justify-content: center;
  }
}
</style>
