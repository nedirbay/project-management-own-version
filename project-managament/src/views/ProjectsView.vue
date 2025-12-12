<template>
  <div class="projects-view">
    <div class="page-header">
      <div class="header-content">
        <h1>Projeler</h1>
        <p>Tüm projelerinizi buradan yönetin</p>
      </div>
      <div class="header-actions">
        <el-button
          v-if="canManageProjects"
          type="primary"
          :icon="Plus"
          @click="showCreateDialog = true"
        >
          Yeni Proje
        </el-button>
      </div>
    </div>

    <!-- Progress Charts -->
    <el-card v-if="filteredProjects.length > 0" class="chart-card">
      <template #header>
        <h3>Proje İlerleme Grafikleri</h3>
      </template>
      <div class="charts-container">
        <div v-for="project in filteredProjects.slice(0, 6)" :key="project.id" class="chart-item">
          <div class="chart-header">
            <h4>{{ project.name }}</h4>
            <span class="chart-percentage" :style="{ color: project.color }">
              {{ project.progress }}%
            </span>
          </div>
          <div class="chart-body">
            <div class="progress-ring">
              <svg width="120" height="120">
                <circle
                  class="progress-ring-circle-bg"
                  :stroke="project.color + '20'"
                  stroke-width="10"
                  fill="transparent"
                  r="52"
                  cx="60"
                  cy="60"
                />
                <circle
                  class="progress-ring-circle"
                  :stroke="project.color"
                  stroke-width="10"
                  fill="transparent"
                  r="52"
                  cx="60"
                  cy="60"
                  :stroke-dasharray="`${(project.progress / 100) * 326.73} 326.73`"
                  stroke-linecap="round"
                />
                <text
                  x="60"
                  y="60"
                  text-anchor="middle"
                  dominant-baseline="central"
                  class="progress-text"
                  fill="currentColor"
                >
                  {{ project.progress }}%
                </text>
              </svg>
            </div>
            <div class="chart-info">
              <div class="info-item">
                <span>Toplam Görev:</span>
                <strong>{{ getProjectTaskCount(project.id) }}</strong>
              </div>
              <div class="info-item">
                <span>Tamamlanan:</span>
                <strong>{{ getCompletedTaskCount(project.id) }}</strong>
              </div>
              <div class="info-item">
                <span>Kalan:</span>
                <strong>{{ getRemainingTaskCount(project.id) }}</strong>
              </div>
            </div>
          </div>
        </div>
      </div>
    </el-card>

    <!-- Filters -->
    <el-card class="filter-card">
      <el-row :gutter="16">
        <el-col :xs="24" :sm="12" :md="6">
          <el-select
            v-model="filters.workspaceId"
            placeholder="Workspace"
            clearable
            style="width: 100%"
          >
            <el-option v-for="ws in workspaces" :key="ws.id" :label="ws.name" :value="ws.id" />
          </el-select>
        </el-col>
        <el-col :xs="24" :sm="12" :md="6">
          <el-select
            v-model="filters.status"
            placeholder="Durum"
            multiple
            clearable
            style="width: 100%"
          >
            <el-option label="Planlama" value="planning" />
            <el-option label="Aktif" value="active" />
            <el-option label="Beklemede" value="on-hold" />
            <el-option label="Tamamlandı" value="completed" />
            <el-option label="İptal" value="cancelled" />
          </el-select>
        </el-col>
        <el-col :xs="24" :sm="12" :md="6">
          <el-select
            v-model="filters.priority"
            placeholder="Öncelik"
            multiple
            clearable
            style="width: 100%"
          >
            <el-option label="Düşük" value="low" />
            <el-option label="Orta" value="medium" />
            <el-option label="Yüksek" value="high" />
            <el-option label="Kritik" value="critical" />
          </el-select>
        </el-col>
        <el-col :xs="24" :sm="12" :md="6">
          <el-input
            v-model="filters.search"
            placeholder="Proje ara..."
            :prefix-icon="Search"
            clearable
          />
        </el-col>
      </el-row>
    </el-card>

    <!-- Projects Grid -->
    <div v-if="filteredProjects.length > 0" class="projects-grid">
      <div
        v-for="project in filteredProjects"
        :key="project.id"
        class="project-card"
        :style="{ borderColor: project.color }"
        @click="handleProjectClick(project)"
      >
        <div class="project-header">
          <div class="project-icon" :style="{ backgroundColor: project.color }">
            <el-icon :size="24"><Folder /></el-icon>
          </div>
          <el-dropdown v-if="canEdit(project)" trigger="click" @click.stop>
            <el-button class="action-btn" :icon="More" circle size="small" />
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item @click="handleEdit(project)">
                  <el-icon><Edit /></el-icon>
                  Düzenle
                </el-dropdown-item>
                <el-dropdown-item divided @click="handleDelete(project)">
                  <el-icon><Delete /></el-icon>
                  Sil
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>

        <div class="project-body">
          <h3 class="project-name">{{ project.name }}</h3>
          <p class="project-description">{{ project.description }}</p>

          <div class="project-tags">
            <el-tag :type="getStatusType(project.status)" size="small">
              {{ getStatusText(project.status) }}
            </el-tag>
            <el-tag :type="getPriorityType(project.priority)" size="small">
              {{ getPriorityText(project.priority) }}
            </el-tag>
          </div>

          <div class="project-progress">
            <div class="progress-header">
              <span>İlerleme</span>
              <span class="progress-value">{{ project.progress }}%</span>
            </div>
            <el-progress
              :percentage="project.progress"
              :color="getProgressColor(project.progress)"
              :stroke-width="8"
            />
          </div>

          <div class="project-stats">
            <div class="stat-item">
              <el-icon><Document /></el-icon>
              <span>{{ getProjectTaskCount(project.id) }} Görev</span>
            </div>
            <div class="stat-item">
              <el-icon><User /></el-icon>
              <span>{{ project.memberIds.length }} Üye</span>
            </div>
          </div>
        </div>

        <div class="project-footer">
          <div class="project-date">
            {{ formatDate(project.startDate) }}
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-state">
      <el-empty description="Proje bulunamadı">
        <el-button
          v-if="canManageProjects"
          type="primary"
          :icon="Plus"
          @click="showCreateDialog = true"
        >
          İlk Projeyi Oluştur
        </el-button>
      </el-empty>
    </div>

    <!-- Create/Edit Dialog -->
    <el-dialog
      v-model="showCreateDialog"
      :title="editingProject ? 'Proje Düzenle' : 'Yeni Proje'"
      width="700px"
      @close="handleDialogClose"
    >
      <el-form ref="formRef" :model="form" :rules="rules" label-position="top" size="large">
        <el-row :gutter="16">
          <el-col :span="24">
            <el-form-item label="Proje Adı" prop="name">
              <el-input v-model="form.name" placeholder="Proje adını girin" />
            </el-form-item>
          </el-col>

          <el-col :span="24">
            <el-form-item label="Açıklama" prop="description">
              <el-input
                v-model="form.description"
                type="textarea"
                :rows="3"
                placeholder="Proje açıklaması"
              />
            </el-form-item>
          </el-col>

          <el-col :span="12">
            <el-form-item label="Workspace" prop="workspaceId">
              <el-select
                v-model="form.workspaceId"
                placeholder="Workspace seçin"
                style="width: 100%"
              >
                <el-option
                  v-for="ws in availableWorkspaces"
                  :key="ws.id"
                  :label="ws.name"
                  :value="ws.id"
                />
              </el-select>
            </el-form-item>
          </el-col>

          <el-col :span="12">
            <el-form-item label="Renk" prop="color">
              <el-color-picker v-model="form.color" show-alpha :predefine="predefineColors" />
            </el-form-item>
          </el-col>

          <el-col :span="12">
            <el-form-item label="Durum" prop="status">
              <el-select v-model="form.status" placeholder="Durum seçin" style="width: 100%">
                <el-option label="Planlama" value="planning" />
                <el-option label="Aktif" value="active" />
                <el-option label="Beklemede" value="on-hold" />
                <el-option label="Tamamlandı" value="completed" />
                <el-option label="İptal" value="cancelled" />
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
            <el-form-item label="Başlangıç Tarihi" prop="startDate">
              <el-date-picker
                v-model="form.startDate"
                type="date"
                placeholder="Başlangıç tarihi"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>

          <el-col :span="12">
            <el-form-item label="Bitiş Tarihi">
              <el-date-picker
                v-model="form.endDate"
                type="date"
                placeholder="Bitiş tarihi"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>

          <el-col :span="24">
            <el-form-item label="Proje Üyeleri" prop="memberIds">
              <el-select
                v-model="form.memberIds"
                multiple
                filterable
                placeholder="Üye seçin"
                style="width: 100%"
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
            </el-form-item>
          </el-col>

          <el-col :span="24">
            <el-form-item label="Etiketler">
              <el-select
                v-model="form.tags"
                multiple
                filterable
                allow-create
                placeholder="Etiket ekle"
                style="width: 100%"
              >
                <el-option label="Frontend" value="frontend" />
                <el-option label="Backend" value="backend" />
                <el-option label="Design" value="design" />
                <el-option label="Mobile" value="mobile" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>

      <template #footer>
        <el-button @click="showCreateDialog = false">İptal</el-button>
        <el-button type="primary" :loading="loading" @click="handleSubmit">
          {{ editingProject ? 'Güncelle' : 'Oluştur' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, watch } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { Plus, Folder, Document, User, Edit, Delete, More, Search } from '@element-plus/icons-vue'
import { useProjectsStore } from '@/stores/projects'
import { useWorkspacesStore } from '@/stores/workspaces'
import { useTasksStore } from '@/stores/tasks'
import { useUsersStore } from '@/stores/users'
import { useAuthStore } from '@/stores/auth'
import type { Project, ProjectForm } from '@/types'

const router = useRouter()
const projectsStore = useProjectsStore()
const workspacesStore = useWorkspacesStore()
const tasksStore = useTasksStore()
const usersStore = useUsersStore()
const authStore = useAuthStore()

// State
const showCreateDialog = ref(false)
const loading = ref(false)
const editingProject = ref<Project | null>(null)
const formRef = ref<FormInstance>()

const filters = reactive({
  workspaceId: '',
  status: [] as string[],
  priority: [] as string[],
  search: '',
})

const form = reactive<ProjectForm>({
  name: '',
  description: '',
  workspaceId: '',
  memberIds: [],
  status: 'planning',
  priority: 'medium',
  startDate: new Date().toISOString().split('T')[0],
  endDate: '',
  color: '#409EFF',
  tags: [],
})

const predefineColors = [
  '#409EFF',
  '#67C23A',
  '#E6A23C',
  '#F56C6C',
  '#909399',
  '#8E44AD',
  '#3498DB',
  '#E74C3C',
  '#1ABC9C',
  '#F39C12',
]

const rules: FormRules = {
  name: [
    { required: true, message: 'Proje adı gereklidir', trigger: 'blur' },
    { min: 3, message: 'En az 3 karakter olmalıdır', trigger: 'blur' },
  ],
  description: [
    { required: true, message: 'Açıklama gereklidir', trigger: 'blur' },
    { min: 10, message: 'En az 10 karakter olmalıdır', trigger: 'blur' },
  ],
  workspaceId: [{ required: true, message: 'Workspace seçimi gereklidir', trigger: 'change' }],
  status: [{ required: true, message: 'Durum seçimi gereklidir', trigger: 'change' }],
  priority: [{ required: true, message: 'Öncelik seçimi gereklidir', trigger: 'change' }],
  startDate: [{ required: true, message: 'Başlangıç tarihi gereklidir', trigger: 'change' }],
  color: [{ required: true, message: 'Renk seçimi gereklidir', trigger: 'change' }],
}

// Computed
const canManageProjects = computed(() => authStore.canManageProjects)
const workspaces = computed(() => workspacesStore.myWorkspaces)
const projects = computed(() => projectsStore.myProjects)

const availableWorkspaces = computed(() => {
  if (authStore.isAdmin) {
    return workspacesStore.allWorkspaces
  }
  return workspacesStore.myWorkspaces
})

const availableUsers = computed(() => {
  return usersStore.allUsers.filter((u) => u.id !== authStore.user?.id)
})

const filteredProjects = computed(() => {
  return projectsStore.filterProjects({
    workspaceId: filters.workspaceId,
    status: filters.status.length > 0 ? filters.status : undefined,
    priority: filters.priority.length > 0 ? filters.priority : undefined,
    search: filters.search,
  })
})

// Methods
const canEdit = (project: Project): boolean => {
  if (authStore.isAdmin) return true
  if (authStore.isWorkspaceAdmin) {
    const workspace = workspacesStore.getWorkspaceById(project.workspaceId)
    return workspace?.adminId === authStore.user?.id
  }
  return false
}

const getProjectTaskCount = (projectId: string): number => {
  return tasksStore.getTasksByProject(projectId).length
}

const getCompletedTaskCount = (projectId: string): number => {
  return tasksStore.getTasksByProject(projectId).filter((t) => t.status === 'done').length
}

const getRemainingTaskCount = (projectId: string): number => {
  const tasks = tasksStore.getTasksByProject(projectId)
  return tasks.filter((t) => t.status !== 'done').length
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

const getPriorityType = (priority: string): string => {
  const types: Record<string, string> = {
    low: 'info',
    medium: 'warning',
    high: 'danger',
    critical: 'danger',
  }
  return types[priority] || 'info'
}

const getPriorityText = (priority: string): string => {
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

const formatDate = (dateStr: string): string => {
  const date = new Date(dateStr)
  return date.toLocaleDateString('tr-TR', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

const handleProjectClick = (project: Project) => {
  router.push(`/projects/${project.id}`)
}

const handleEdit = (project: Project) => {
  editingProject.value = project
  form.name = project.name
  form.description = project.description
  form.workspaceId = project.workspaceId
  form.memberIds = [...project.memberIds]
  form.status = project.status
  form.priority = project.priority
  form.startDate = project.startDate
  form.endDate = project.endDate || ''
  form.color = project.color
  form.tags = [...project.tags]
  showCreateDialog.value = true
}

const handleDelete = async (project: Project) => {
  try {
    await ElMessageBox.confirm(
      `"${project.name}" projesini silmek istediğinize emin misiniz?`,
      'Proje Sil',
      {
        confirmButtonText: 'Evet, Sil',
        cancelButtonText: 'İptal',
        type: 'warning',
      },
    )

    loading.value = true
    const success = await projectsStore.deleteProject(project.id)
    if (success) {
      ElMessage.success('Proje başarıyla silindi')
    }
  } catch {
    // User cancelled
  } finally {
    loading.value = false
  }
}

const handleSubmit = async () => {
  if (!formRef.value) return

  await formRef.value.validate(async (valid) => {
    if (!valid) return

    loading.value = true

    let success = false
    if (editingProject.value) {
      success = await projectsStore.updateProject(editingProject.value.id, form)
    } else {
      success = await projectsStore.createProject(form)
    }

    if (success) {
      showCreateDialog.value = false
      handleDialogClose()
    }

    loading.value = false
  })
}

const handleDialogClose = () => {
  editingProject.value = null
  form.name = ''
  form.description = ''
  form.workspaceId = ''
  form.memberIds = []
  form.status = 'planning'
  form.priority = 'medium'
  form.startDate = new Date().toISOString().split('T')[0]
  form.endDate = ''
  form.color = '#409EFF'
  form.tags = []
  formRef.value?.resetFields()
}

// Set default workspace if available
watch(
  () => workspaces.value,
  (newWorkspaces) => {
    if (newWorkspaces.length > 0 && !form.workspaceId) {
      form.workspaceId = newWorkspaces[0].id
    }
  },
  { immediate: true },
)

// Initialize
projectsStore.fetchProjects()
workspacesStore.fetchWorkspaces()
tasksStore.fetchTasks()
usersStore.fetchUsers()
</script>

<style scoped>
.projects-view {
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

.filter-card {
  margin-bottom: 24px;
  border-radius: 16px;
  border: 1px solid var(--border-color);
}

.projects-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 24px;
  margin-bottom: 24px;
}

.project-card {
  background: var(--bg-secondary);
  border-radius: 16px;
  border: 2px solid var(--border-color);
  padding: 24px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.project-card:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-lg);
  border-color: currentColor;
}

.project-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.project-icon {
  width: 56px;
  height: 56px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.action-btn {
  background: var(--bg-tertiary);
  border: 1px solid var(--border-color);
  color: var(--text-secondary);
}

.action-btn:hover {
  background: var(--bg-hover);
  border-color: var(--color-primary);
  color: var(--color-primary);
}

.project-body {
  flex: 1;
}

.project-name {
  font-size: 20px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 8px;
}

.project-description {
  font-size: 14px;
  color: var(--text-secondary);
  margin-bottom: 16px;
  line-height: 1.6;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.project-tags {
  display: flex;
  gap: 8px;
  margin-bottom: 16px;
}

.project-progress {
  margin-bottom: 16px;
}

.progress-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
  font-size: 14px;
  color: var(--text-secondary);
}

.progress-value {
  font-weight: 600;
  color: var(--color-primary);
}

.project-stats {
  display: flex;
  gap: 20px;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 14px;
  color: var(--text-secondary);
}

.project-footer {
  padding-top: 16px;
  border-top: 1px solid var(--border-color);
}

.project-date {
  font-size: 12px;
  color: var(--text-tertiary);
}

.empty-state {
  padding: 80px 20px;
  text-align: center;
}

.chart-card {
  border-radius: 16px;
  border: 1px solid var(--border-color);
  margin-bottom: 24px;
}

.chart-card h3 {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}

.charts-container {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 24px;
}

.chart-item {
  background: var(--bg-tertiary);
  border-radius: 12px;
  padding: 20px;
  border: 1px solid var(--border-color);
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.chart-header h4 {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
  flex: 1;
}

.chart-percentage {
  font-size: 20px;
  font-weight: 700;
}

.chart-body {
  display: flex;
  gap: 16px;
  align-items: center;
}

.progress-ring {
  flex-shrink: 0;
}

.progress-ring-circle {
  transform: rotate(-90deg);
  transform-origin: 50% 50%;
  transition: stroke-dasharray 0.6s ease;
}

.progress-text {
  font-size: 18px;
  font-weight: 700;
}

.chart-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.info-item {
  display: flex;
  justify-content: space-between;
  font-size: 14px;
  color: var(--text-secondary);
}

.info-item strong {
  color: var(--text-primary);
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

  .projects-grid {
    grid-template-columns: 1fr;
  }

  .charts-container {
    grid-template-columns: 1fr;
  }

  .chart-body {
    flex-direction: column;
  }
}
</style>
