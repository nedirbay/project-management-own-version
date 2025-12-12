<template>
  <div class="daily-report-view">
    <div class="page-header">
      <div class="header-content">
        <h1>{{ $t('reports.title') }}</h1>
        <p>
          {{ $t('reports.viewReport') || 'Günlük çalışma raporlarını görüntüleyin ve yönetin' }}
        </p>
      </div>
      <div class="header-actions">
        <el-button type="primary" :icon="Plus" @click="showCreateDialog = true">
          {{ $t('reports.createReport') }}
        </el-button>
      </div>
    </div>

    <!-- Filters -->
    <el-card class="filter-card">
      <el-row :gutter="16">
        <el-col :xs="24" :sm="12" :md="6">
          <el-date-picker
            v-model="filters.dateRange"
            type="daterange"
            range-separator="-"
            :start-placeholder="$t('common.startDate')"
            :end-placeholder="$t('common.endDate')"
            style="width: 100%"
            @change="handleFilterChange"
          />
        </el-col>
        <el-col v-if="canViewAllReports" :xs="24" :sm="12" :md="6">
          <el-select
            v-model="filters.userId"
            :placeholder="$t('users.title') || 'Kullanıcı'"
            clearable
            filterable
            style="width: 100%"
            @change="handleFilterChange"
          >
            <el-option
              v-for="user in availableUsers"
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
        </el-col>
        <el-col :xs="24" :sm="12" :md="6">
          <el-select
            v-model="filters.workspaceId"
            :placeholder="$t('workspaces.title') || 'Çalışma Alanı'"
            clearable
            style="width: 100%"
            @change="handleWorkspaceChange"
          >
            <el-option
              v-for="workspace in availableWorkspaces"
              :key="workspace.id"
              :label="workspace.name"
              :value="workspace.id"
            />
          </el-select>
        </el-col>
        <el-col :xs="24" :sm="12" :md="6">
          <el-select
            v-model="filters.projectId"
            :placeholder="$t('projects.title') || 'Proje'"
            clearable
            style="width: 100%"
            @change="handleFilterChange"
          >
            <el-option
              v-for="project in filteredProjects"
              :key="project.id"
              :label="project.name"
              :value="project.id"
            />
          </el-select>
        </el-col>
      </el-row>
    </el-card>

    <!-- Reports Table -->
    <el-card class="table-card">
      <el-table
        v-loading="loading"
        :data="paginatedReports"
        stripe
        style="width: 100%"
        :default-sort="{ prop: 'date', order: 'descending' }"
        @sort-change="handleSortChange"
      >
        <el-table-column prop="date" :label="$t('common.date')" width="120" sortable="custom">
          <template #default="{ row }">
            <div class="date-cell">
              <el-icon><Calendar /></el-icon>
              <span>{{ formatDate(row.date) }}</span>
            </div>
          </template>
        </el-table-column>

        <el-table-column
          v-if="canViewAllReports"
          prop="userId"
          :label="$t('users.userName')"
          width="150"
        >
          <template #default="{ row }">
            <div class="user-cell">
              <el-avatar :size="32">{{ getUserInitial(row.userId) }}</el-avatar>
              <span>{{ getUserName(row.userId) }}</span>
            </div>
          </template>
        </el-table-column>

        <el-table-column prop="workspaceId" :label="$t('workspaces.title')" width="150">
          <template #default="{ row }">
            <el-tag type="info" effect="plain">
              {{ getWorkspaceName(row.workspaceId) }}
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column prop="projectId" :label="$t('projects.title')" width="150">
          <template #default="{ row }">
            <el-tag v-if="row.projectId" :color="getProjectColor(row.projectId)" effect="dark">
              {{ getProjectName(row.projectId) }}
            </el-tag>
            <span v-else class="text-secondary">-</span>
          </template>
        </el-table-column>

        <el-table-column
          prop="workDescription"
          :label="$t('reports.completedWork')"
          min-width="300"
        >
          <template #default="{ row }">
            <div class="work-description">
              {{ row.workDescription }}
            </div>
          </template>
        </el-table-column>

        <el-table-column
          prop="tasksCompleted"
          :label="$t('projects.completedTasks')"
          width="130"
          align="center"
        >
          <template #default="{ row }">
            <el-tag type="success" effect="dark" round>
              <el-icon><Select /></el-icon>
              {{ row.tasksCompleted.length }}
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column prop="notes" :label="$t('reports.notes')" width="200">
          <template #default="{ row }">
            <div v-if="row.notes" class="notes-cell">
              <el-tooltip :content="row.notes" placement="top">
                <span>{{ truncateText(row.notes, 50) }}</span>
              </el-tooltip>
            </div>
            <span v-else class="text-secondary">-</span>
          </template>
        </el-table-column>

        <el-table-column fixed="right" :label="$t('common.actions')" width="100" align="center">
          <template #default="{ row }">
            <el-button-group>
              <el-button size="small" :icon="View" @click="handleView(row)" />
              <el-button
                v-if="canEditReport(row)"
                size="small"
                :icon="Edit"
                @click="handleEdit(row)"
              />
              <el-button
                v-if="canDeleteReport(row)"
                size="small"
                type="danger"
                :icon="Delete"
                @click="handleDelete(row)"
              />
            </el-button-group>
          </template>
        </el-table-column>

        <template #empty>
          <el-empty :description="$t('reports.noReports')">
            <el-button type="primary" :icon="Plus" @click="showCreateDialog = true">
              {{ $t('reports.createReport') }}
            </el-button>
          </el-empty>
        </template>
      </el-table>

      <!-- Pagination -->
      <div class="pagination-container">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="filteredReports.length"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="handleSizeChange"
          @current-change="handlePageChange"
        />
      </div>
    </el-card>

    <!-- Create/Edit Dialog -->
    <el-dialog
      v-model="showCreateDialog"
      :title="editingReport ? $t('reports.editReport') : $t('reports.createReport')"
      width="600px"
      @close="handleDialogClose"
    >
      <el-form ref="formRef" :model="form" :rules="rules" label-position="top" size="large">
        <el-form-item :label="$t('common.date')" prop="date">
          <el-date-picker
            v-model="form.date"
            type="date"
            :placeholder="$t('common.date')"
            style="width: 100%"
            :disabled-date="disabledDate"
          />
        </el-form-item>

        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item :label="$t('workspaces.title')" prop="workspaceId">
              <el-select
                v-model="form.workspaceId"
                style="width: 100%"
                @change="handleFormWorkspaceChange"
              >
                <el-option
                  v-for="workspace in availableWorkspaces"
                  :key="workspace.id"
                  :label="workspace.name"
                  :value="workspace.id"
                />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item :label="$t('projects.title')">
              <el-select v-model="form.projectId" clearable style="width: 100%">
                <el-option
                  v-for="project in formProjects"
                  :key="project.id"
                  :label="project.name"
                  :value="project.id"
                />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item :label="$t('reports.completedWork')" prop="workDescription">
          <el-input
            v-model="form.workDescription"
            type="textarea"
            :rows="4"
            :placeholder="$t('reports.completedWork')"
          />
        </el-form-item>

        <el-form-item :label="$t('reports.plannedWork') || 'Tamamlanan Görevler'">
          <el-select
            v-model="form.tasksCompleted"
            multiple
            filterable
            style="width: 100%"
            :placeholder="$t('tasks.title')"
          >
            <el-option
              v-for="task in availableTasks"
              :key="task.id"
              :label="task.title"
              :value="task.id"
            />
          </el-select>
        </el-form-item>

        <el-form-item :label="$t('reports.notes')">
          <el-input
            v-model="form.notes"
            type="textarea"
            :rows="3"
            :placeholder="$t('reports.notes')"
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="showCreateDialog = false">{{ $t('common.cancel') }}</el-button>
        <el-button type="primary" :loading="loading" @click="handleSubmit">
          {{ $t('common.save') }}
        </el-button>
      </template>
    </el-dialog>

    <!-- View Dialog -->
    <el-dialog v-model="showViewDialog" :title="$t('reports.viewReport')" width="600px">
      <div v-if="viewingReport" class="view-dialog-content">
        <el-descriptions :column="2" border>
          <el-descriptions-item :label="$t('common.date')">
            <el-icon><Calendar /></el-icon>
            {{ formatDateFull(viewingReport.date) }}
          </el-descriptions-item>
          <el-descriptions-item v-if="canViewAllReports" :label="$t('users.userName')">
            <div class="user-cell">
              <el-avatar :size="24">{{ getUserInitial(viewingReport.userId) }}</el-avatar>
              <span>{{ getUserName(viewingReport.userId) }}</span>
            </div>
          </el-descriptions-item>
          <el-descriptions-item :label="$t('workspaces.title')">
            {{ getWorkspaceName(viewingReport.workspaceId) }}
          </el-descriptions-item>
          <el-descriptions-item :label="$t('projects.title')">
            {{ viewingReport.projectId ? getProjectName(viewingReport.projectId) : '-' }}
          </el-descriptions-item>
          <el-descriptions-item :label="$t('reports.completedWork')" :span="2">
            {{ viewingReport.workDescription }}
          </el-descriptions-item>
          <el-descriptions-item :label="$t('projects.completedTasks')" :span="2">
            <el-space wrap>
              <el-tag v-for="taskId in viewingReport.tasksCompleted" :key="taskId" type="success">
                {{ getTaskName(taskId) }}
              </el-tag>
              <span v-if="viewingReport.tasksCompleted.length === 0" class="text-secondary">-</span>
            </el-space>
          </el-descriptions-item>
          <el-descriptions-item v-if="viewingReport.notes" :label="$t('reports.notes')" :span="2">
            {{ viewingReport.notes }}
          </el-descriptions-item>
        </el-descriptions>
      </div>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox, type FormInstance, type FormRules } from 'element-plus'
import { Plus, Calendar, Edit, Delete, View, Select } from '@element-plus/icons-vue'
import { useReportsStore } from '@/stores/reports'
import { useWorkspacesStore } from '@/stores/workspaces'
import { useProjectsStore } from '@/stores/projects'
import { useTasksStore } from '@/stores/tasks'
import { useUsersStore } from '@/stores/users'
import { useAuthStore } from '@/stores/auth'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const reportsStore = useReportsStore()
const workspacesStore = useWorkspacesStore()
const projectsStore = useProjectsStore()
const tasksStore = useTasksStore()
const usersStore = useUsersStore()
const authStore = useAuthStore()

// State
const showCreateDialog = ref(false)
const showViewDialog = ref(false)
const loading = ref(false)
const editingReport = ref<any>(null)
const viewingReport = ref<any>(null)
const formRef = ref<FormInstance>()
const currentPage = ref(1)
const pageSize = ref(20)
const sortProp = ref('date')
const sortOrder = ref('descending')

// Filters
const filters = ref({
  dateRange: [] as any[],
  userId: '',
  workspaceId: '',
  projectId: '',
})

// Form
const form = reactive({
  date: new Date().toISOString().split('T')[0],
  workspaceId: '',
  projectId: '',
  workDescription: '',
  tasksCompleted: [] as string[],
  notes: '',
})

// Validation Rules
const rules: FormRules = {
  date: [{ required: true, message: t('validation.required'), trigger: 'change' }],
  workspaceId: [{ required: true, message: t('validation.required'), trigger: 'change' }],
  workDescription: [
    { required: true, message: t('validation.required'), trigger: 'blur' },
    { min: 10, message: t('validation.minLength', { min: 10 }), trigger: 'blur' },
  ],
}

// Computed
const canViewAllReports = computed(() => {
  return authStore.currentUser?.role === 'admin' || authStore.currentUser?.role === 'workspaceAdmin'
})

const availableUsers = computed(() => {
  return usersStore.users
})

const availableWorkspaces = computed(() => {
  if (authStore.currentUser?.role === 'admin') {
    return workspacesStore.workspaces
  }
  return workspacesStore.workspaces.filter(
    (ws) =>
      ws.adminId === authStore.currentUser?.id ||
      ws.memberIds.includes(authStore.currentUser?.id || ''),
  )
})

const filteredProjects = computed(() => {
  if (filters.value.workspaceId) {
    return projectsStore.projects.filter((p) => p.workspaceId === filters.value.workspaceId)
  }
  return projectsStore.projects
})

const formProjects = computed(() => {
  if (form.workspaceId) {
    return projectsStore.projects.filter((p) => p.workspaceId === form.workspaceId)
  }
  return []
})

const availableTasks = computed(() => {
  if (form.projectId) {
    return tasksStore.tasks.filter((t) => t.projectId === form.projectId)
  }
  return tasksStore.tasks
})

const filteredReports = computed(() => {
  let reports = reportsStore.reports

  // Role-based filtering
  if (!canViewAllReports.value) {
    reports = reports.filter((r) => r.userId === authStore.currentUser?.id)
  }

  // Date range filter
  if (filters.value.dateRange && filters.value.dateRange.length === 2) {
    const startDate = filters.value.dateRange[0]
      ? new Date(filters.value.dateRange[0]).toISOString().split('T')[0]
      : ''
    const endDate = filters.value.dateRange[1]
      ? new Date(filters.value.dateRange[1]).toISOString().split('T')[0]
      : ''
    if (startDate && endDate) {
      reports = reports.filter((r) => r.date >= startDate && r.date <= endDate)
    }
  }

  // User filter
  if (filters.value.userId) {
    reports = reports.filter((r) => r.userId === filters.value.userId)
  }

  // Workspace filter
  if (filters.value.workspaceId) {
    reports = reports.filter((r) => r.workspaceId === filters.value.workspaceId)
  }

  // Project filter
  if (filters.value.projectId) {
    reports = reports.filter((r) => r.projectId === filters.value.projectId)
  }

  // Sort
  return reports.sort((a, b) => {
    if (sortOrder.value === 'ascending') {
      return a.date > b.date ? 1 : -1
    }
    return a.date < b.date ? 1 : -1
  })
})

const paginatedReports = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredReports.value.slice(start, end)
})

// Helper Functions
const getUserName = (userId: string) => {
  const user = usersStore.users.find((u) => u.id === userId)
  return user?.fullName || 'Unknown'
}

const getUserInitial = (userId: string) => {
  const user = usersStore.users.find((u) => u.id === userId)
  return user?.fullName?.charAt(0) || 'U'
}

const getWorkspaceName = (workspaceId: string) => {
  const workspace = workspacesStore.workspaces.find((w) => w.id === workspaceId)
  return workspace?.name || 'Unknown'
}

const getProjectName = (projectId: string) => {
  const project = projectsStore.projects.find((p) => p.id === projectId)
  return project?.name || 'Unknown'
}

const getProjectColor = (projectId: string) => {
  const project = projectsStore.projects.find((p) => p.id === projectId)
  return project?.color || '#409eff'
}

const getTaskName = (taskId: string) => {
  const task = tasksStore.tasks.find((t) => t.id === taskId)
  return task?.title || 'Unknown Task'
}

const formatDate = (dateStr: string) => {
  const date = new Date(dateStr)
  const day = String(date.getDate()).padStart(2, '0')
  const month = String(date.getMonth() + 1).padStart(2, '0')
  return `${day}/${month}`
}

const formatDateFull = (dateStr: string) => {
  const date = new Date(dateStr)
  const options: Intl.DateTimeFormatOptions = {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    weekday: 'long',
  }
  return date.toLocaleDateString('tr-TR', options)
}

const truncateText = (text: string, length: number) => {
  if (text.length <= length) return text
  return text.substring(0, length) + '...'
}

const disabledDate = (date: Date) => {
  return date > new Date()
}

const canEditReport = (report: any) => {
  if (authStore.currentUser?.role === 'admin') return true
  return report.userId === authStore.currentUser?.id
}

const canDeleteReport = (report: any) => {
  if (authStore.currentUser?.role === 'admin') return true
  return report.userId === authStore.currentUser?.id
}

// Event Handlers
const handleFilterChange = () => {
  currentPage.value = 1
}

const handleWorkspaceChange = () => {
  filters.value.projectId = ''
  handleFilterChange()
}

const handleFormWorkspaceChange = () => {
  form.projectId = ''
}

const handleSortChange = ({ prop, order }: any) => {
  sortProp.value = prop
  sortOrder.value = order
}

const handlePageChange = (page: number) => {
  currentPage.value = page
}

const handleSizeChange = (size: number) => {
  pageSize.value = size
  currentPage.value = 1
}

const handleView = (report: any) => {
  viewingReport.value = report
  showViewDialog.value = true
}

const handleEdit = (report: any) => {
  editingReport.value = report
  form.date = report.date
  form.workspaceId = report.workspaceId
  form.projectId = report.projectId || ''
  form.workDescription = report.workDescription
  form.tasksCompleted = [...report.tasksCompleted]
  form.notes = report.notes || ''
  showCreateDialog.value = true
}

const handleDelete = (report: any) => {
  ElMessageBox.confirm(t('reports.confirmDelete'), t('common.warning'), {
    confirmButtonText: t('common.yes'),
    cancelButtonText: t('common.no'),
    type: 'warning',
  }).then(async () => {
    loading.value = true
    try {
      const success = await reportsStore.deleteReport(report.id)
      if (success) {
        ElMessage.success(t('reports.deleted'))
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

        if (editingReport.value) {
          success = await reportsStore.updateReport(editingReport.value.id, form as any)
        } else {
          success = await reportsStore.createReport({
            ...form,
            userId: authStore.currentUser?.id || '',
          } as any)
        }

        if (success) {
          ElMessage.success(editingReport.value ? t('reports.updated') : t('reports.created'))
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
  editingReport.value = null
  form.date = new Date().toISOString().split('T')[0]
  form.workspaceId = ''
  form.projectId = ''
  form.workDescription = ''
  form.tasksCompleted = []
  form.notes = ''
  formRef.value?.resetFields()
}

// Initialize
onMounted(() => {
  reportsStore.fetchReports()
  workspacesStore.fetchWorkspaces()
  projectsStore.fetchProjects()
  tasksStore.fetchTasks()
  usersStore.fetchUsers()
})
</script>

<style scoped>
.daily-report-view {
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

.filter-card {
  margin-bottom: 24px;
  border-radius: 12px;
  box-shadow: var(--el-box-shadow-light);
}

.table-card {
  border-radius: 12px;
  box-shadow: var(--el-box-shadow-light);
}

/* Table Cell Styles */
.date-cell {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 500;
}

.user-cell {
  display: flex;
  align-items: center;
  gap: 8px;
}

.work-description {
  line-height: 1.6;
  color: var(--el-text-color-primary);
}

.notes-cell {
  color: var(--el-text-color-secondary);
  font-size: 13px;
  font-style: italic;
}

.text-secondary {
  color: var(--el-text-color-secondary);
}

/* User Option */
.user-option {
  display: flex;
  align-items: center;
  gap: 8px;
}

/* Pagination */
.pagination-container {
  margin-top: 24px;
  display: flex;
  justify-content: flex-end;
}

/* View Dialog */
.view-dialog-content {
  padding: 8px 0;
}

/* Table Hover Effects */
:deep(.el-table__row) {
  cursor: pointer;
  transition: all 0.3s;
}

:deep(.el-table__row:hover) {
  background-color: var(--el-fill-color-light);
}

/* Custom Tag Colors */
:deep(.el-tag) {
  font-weight: 500;
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
  .daily-report-view {
    padding: 16px;
  }

  .page-header {
    flex-direction: column;
    gap: 16px;
  }

  .header-actions {
    width: 100%;
  }

  .header-actions button {
    width: 100%;
  }

  .pagination-container {
    justify-content: center;
  }

  :deep(.el-pagination) {
    flex-wrap: wrap;
  }
}
</style>
