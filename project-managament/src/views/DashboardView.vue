<template>
  <div class="dashboard-view">
    <div class="dashboard-header">
      <div class="header-title">
        <h1>Dashboard</h1>
        <p>Proje yönetim sistemine hoş geldiniz</p>
      </div>
      <div class="header-actions">
        <el-date-picker
          v-model="dateRange"
          type="daterange"
          range-separator="-"
          start-placeholder="Başlangıç"
          end-placeholder="Bitiş"
          size="default"
        />
      </div>
    </div>

    <!-- Stats Cards -->
    <el-row :gutter="24" class="stats-row">
      <el-col :xs="24" :sm="12" :md="6">
        <div class="stat-card primary">
          <div class="stat-icon">
            <el-icon :size="32"><OfficeBuilding /></el-icon>
          </div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.totalWorkspaces }}</div>
            <div class="stat-label">Workspace</div>
          </div>
          <div class="stat-trend positive">
            <el-icon><CaretTop /></el-icon>
            <span>12%</span>
          </div>
        </div>
      </el-col>

      <el-col :xs="24" :sm="12" :md="6">
        <div class="stat-card success">
          <div class="stat-icon">
            <el-icon :size="32"><Folder /></el-icon>
          </div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.totalProjects }}</div>
            <div class="stat-label">Toplam Proje</div>
          </div>
          <div class="stat-trend positive">
            <el-icon><CaretTop /></el-icon>
            <span>8%</span>
          </div>
        </div>
      </el-col>

      <el-col :xs="24" :sm="12" :md="6">
        <div class="stat-card warning">
          <div class="stat-icon">
            <el-icon :size="32"><Document /></el-icon>
          </div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.totalTasks }}</div>
            <div class="stat-label">Toplam Görev</div>
          </div>
          <div class="stat-trend positive">
            <el-icon><CaretTop /></el-icon>
            <span>15%</span>
          </div>
        </div>
      </el-col>

      <el-col :xs="24" :sm="12" :md="6">
        <div class="stat-card info">
          <div class="stat-icon">
            <el-icon :size="32"><User /></el-icon>
          </div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.totalUsers }}</div>
            <div class="stat-label">Kullanıcı</div>
          </div>
          <div class="stat-trend positive">
            <el-icon><CaretTop /></el-icon>
            <span>5%</span>
          </div>
        </div>
      </el-col>
    </el-row>

    <!-- Main Content -->
    <el-row :gutter="24" class="content-row">
      <!-- Active Projects -->
      <el-col :xs="24" :md="16">
        <el-card class="dashboard-card">
          <template #header>
            <div class="card-header">
              <h3>Aktif Projeler</h3>
              <el-button type="primary" size="small" @click="$router.push('/projects')">
                Tümünü Gör
              </el-button>
            </div>
          </template>

          <div class="projects-list">
            <div v-if="activeProjects.length === 0" class="empty-state">
              <el-empty description="Aktif proje bulunamadı">
                <el-button type="primary" @click="$router.push('/projects')">
                  Yeni Proje Oluştur
                </el-button>
              </el-empty>
            </div>

            <div
              v-for="project in activeProjects.slice(0, 5)"
              :key="project.id"
              class="project-item"
              @click="$router.push(`/projects/${project.id}`)"
            >
              <div class="project-info">
                <div class="project-name">
                  <div class="project-color" :style="{ backgroundColor: project.color }"></div>
                  <span>{{ project.name }}</span>
                  <el-tag :type="getPriorityType(project.priority)" size="small">
                    {{ getPriorityText(project.priority) }}
                  </el-tag>
                </div>
                <div class="project-meta">
                  <span>
                    <el-icon><Document /></el-icon>
                    {{ getProjectTaskCount(project.id) }} görev
                  </span>
                  <span>
                    <el-icon><User /></el-icon>
                    {{ project.memberIds.length }} üye
                  </span>
                </div>
              </div>
              <div class="project-progress">
                <div class="progress-info">
                  <span class="progress-text">{{ project.progress }}%</span>
                </div>
                <el-progress
                  :percentage="project.progress"
                  :color="getProgressColor(project.progress)"
                  :stroke-width="8"
                  :show-text="false"
                />
              </div>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- Quick Stats & Actions -->
      <el-col :xs="24" :md="8">
        <el-card class="dashboard-card">
          <template #header>
            <div class="card-header">
              <h3>Hızlı İstatistikler</h3>
            </div>
          </template>

          <div class="quick-stats">
            <div class="quick-stat-item">
              <div class="quick-stat-label">Aktif Projeler</div>
              <div class="quick-stat-value">{{ stats.activeProjects }}</div>
              <el-progress
                :percentage="getPercentage(stats.activeProjects, stats.totalProjects)"
                :stroke-width="6"
                :show-text="false"
                color="#409EFF"
              />
            </div>

            <div class="quick-stat-item">
              <div class="quick-stat-label">Tamamlanan Görevler</div>
              <div class="quick-stat-value">{{ stats.completedTasks }}</div>
              <el-progress
                :percentage="getPercentage(stats.completedTasks, stats.totalTasks)"
                :stroke-width="6"
                :show-text="false"
                color="#67C23A"
              />
            </div>

            <div class="quick-stat-item">
              <div class="quick-stat-label">Bekleyen Görevler</div>
              <div class="quick-stat-value">{{ stats.myTasks }}</div>
              <el-progress
                :percentage="getPercentage(stats.myTasks, stats.totalTasks)"
                :stroke-width="6"
                :show-text="false"
                color="#E6A23C"
              />
            </div>

            <div class="quick-stat-item">
              <div class="quick-stat-label">Gecikmiş Görevler</div>
              <div class="quick-stat-value">{{ stats.overdueTasksCount }}</div>
              <el-progress
                :percentage="getPercentage(stats.overdueTasksCount, stats.totalTasks)"
                :stroke-width="6"
                :show-text="false"
                color="#F56C6C"
              />
            </div>
          </div>
        </el-card>

        <el-card class="dashboard-card quick-actions-card">
          <template #header>
            <div class="card-header">
              <h3>Hızlı İşlemler</h3>
            </div>
          </template>

          <div class="quick-actions">
            <el-button
              type="primary"
              class="action-btn"
              :icon="Plus"
              @click="$router.push('/projects')"
            >
              Yeni Proje
            </el-button>
            <el-button
              type="success"
              class="action-btn"
              :icon="Document"
              @click="$router.push('/tasks')"
            >
              Yeni Görev
            </el-button>
            <el-button
              type="warning"
              class="action-btn"
              :icon="Notebook"
              @click="$router.push('/reports')"
            >
              Günlük Rapor
            </el-button>
            <el-button
              type="info"
              class="action-btn"
              :icon="Grid"
              @click="$router.push('/kanban')"
            >
              Kanban Board
            </el-button>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- My Tasks & Recent Activity -->
    <el-row :gutter="24" class="content-row">
      <!-- My Tasks -->
      <el-col :xs="24" :md="12">
        <el-card class="dashboard-card">
          <template #header>
            <div class="card-header">
              <h3>Görevlerim</h3>
              <el-button type="primary" size="small" @click="$router.push('/tasks')">
                Tümünü Gör
              </el-button>
            </div>
          </template>

          <div class="tasks-list">
            <div v-if="myTasks.length === 0" class="empty-state">
              <el-empty description="Size atanmış görev yok" />
            </div>

            <div v-for="task in myTasks.slice(0, 5)" :key="task.id" class="task-item">
              <el-checkbox
                :model-value="task.status === 'done'"
                @change="toggleTaskStatus(task)"
              />
              <div class="task-content">
                <div class="task-title">{{ task.title }}</div>
                <div class="task-meta">
                  <el-tag :type="getStatusType(task.status)" size="small">
                    {{ getStatusText(task.status) }}
                  </el-tag>
                  <el-tag :type="getPriorityType(task.priority)" size="small">
                    {{ getPriorityText(task.priority) }}
                  </el-tag>
                  <span v-if="task.dueDate" class="task-due-date">
                    <el-icon><Calendar /></el-icon>
                    {{ formatDate(task.dueDate) }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- Recent Activity -->
      <el-col :xs="24" :md="12">
        <el-card class="dashboard-card">
          <template #header>
            <div class="card-header">
              <h3>Son Aktiviteler</h3>
            </div>
          </template>

          <div class="activity-list">
            <el-timeline>
              <el-timeline-item
                v-for="activity in recentActivities"
                :key="activity.id"
                :timestamp="activity.time"
                :color="activity.color"
                placement="top"
              >
                <div class="activity-item">
                  <el-avatar :size="32" :src="activity.avatar">
                    {{ activity.userName.charAt(0) }}
                  </el-avatar>
                  <div class="activity-content">
                    <div class="activity-text">
                      <strong>{{ activity.userName }}</strong> {{ activity.action }}
                      <span class="activity-target">{{ activity.target }}</span>
                    </div>
                  </div>
                </div>
              </el-timeline-item>
            </el-timeline>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  OfficeBuilding,
  Folder,
  Document,
  User,
  CaretTop,
  Plus,
  Notebook,
  Grid,
  Calendar,
} from '@element-plus/icons-vue'
import { useAuthStore } from '@/stores/auth'
import { useWorkspacesStore } from '@/stores/workspaces'
import { useProjectsStore } from '@/stores/projects'
import { useTasksStore } from '@/stores/tasks'
import { useUsersStore } from '@/stores/users'
import type { DashboardStats, Task } from '@/types'

const router = useRouter()
const authStore = useAuthStore()
const workspacesStore = useWorkspacesStore()
const projectsStore = useProjectsStore()
const tasksStore = useTasksStore()
const usersStore = useUsersStore()

// State
const dateRange = ref<[Date, Date] | null>(null)

// Computed
const stats = computed<DashboardStats>(() => ({
  totalProjects: projectsStore.projectCount,
  activeProjects: projectsStore.activeProjects.length,
  totalTasks: tasksStore.taskCount,
  completedTasks: tasksStore.completedTasks.length,
  totalUsers: usersStore.userCount,
  totalWorkspaces: workspacesStore.workspaceCount,
  myTasks: tasksStore.myTasks.length,
  overdueTasksCount: tasksStore.overdueTasks.length,
}))

const activeProjects = computed(() => projectsStore.activeProjects)
const myTasks = computed(() => tasksStore.myTasks)

const recentActivities = computed(() => [
  {
    id: 1,
    userName: authStore.user?.fullName || 'Kullanıcı',
    action: 'yeni bir proje oluşturdu:',
    target: 'E-ticaret Platformu',
    time: '2 saat önce',
    color: '#409EFF',
    avatar: '',
  },
  {
    id: 2,
    userName: authStore.user?.fullName || 'Kullanıcı',
    action: 'bir görevi tamamladı:',
    target: 'API Entegrasyonu',
    time: '5 saat önce',
    color: '#67C23A',
    avatar: '',
  },
  {
    id: 3,
    userName: authStore.user?.fullName || 'Kullanıcı',
    action: 'günlük rapor gönderdi',
    target: '',
    time: '1 gün önce',
    color: '#E6A23C',
    avatar: '',
  },
  {
    id: 4,
    userName: authStore.user?.fullName || 'Kullanıcı',
    action: 'yeni bir workspace oluşturdu:',
    target: 'Mobil Uygulama',
    time: '2 gün önce',
    color: '#409EFF',
    avatar: '',
  },
])

// Methods
const getProjectTaskCount = (projectId: string): number => {
  return tasksStore.getTasksByProject(projectId).length
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

const getStatusType = (status: string): string => {
  const types: Record<string, string> = {
    todo: 'info',
    'in-progress': 'warning',
    review: 'warning',
    done: 'success',
  }
  return types[status] || 'info'
}

const getStatusText = (status: string): string => {
  const texts: Record<string, string> = {
    todo: 'Yapılacak',
    'in-progress': 'Devam Ediyor',
    review: 'İncelemede',
    done: 'Tamamlandı',
  }
  return texts[status] || status
}

const getProgressColor = (progress: number): string => {
  if (progress >= 80) return '#67C23A'
  if (progress >= 60) return '#E6A23C'
  if (progress >= 40) return '#409EFF'
  return '#F56C6C'
}

const getPercentage = (value: number, total: number): number => {
  if (total === 0) return 0
  return Math.round((value / total) * 100)
}

const formatDate = (dateStr: string): string => {
  const date = new Date(dateStr)
  const now = new Date()
  const diffTime = date.getTime() - now.getTime()
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))

  if (diffDays < 0) return `${Math.abs(diffDays)} gün gecikti`
  if (diffDays === 0) return 'Bugün'
  if (diffDays === 1) return 'Yarın'
  return `${diffDays} gün sonra`
}

const toggleTaskStatus = async (task: Task) => {
  const newStatus = task.status === 'done' ? 'todo' : 'done'
  await tasksStore.updateTaskStatus(task.id, newStatus)
}

// Initialize
onMounted(() => {
  projectsStore.fetchProjects()
  tasksStore.fetchTasks()
  usersStore.fetchUsers()
})
</script>

<style scoped>
.dashboard-view {
  animation: fadeIn 0.3s ease;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 32px;
  flex-wrap: wrap;
  gap: 16px;
}

.header-title h1 {
  font-size: 32px;
  font-weight: 700;
  color: #ffffff;
  margin-bottom: 8px;
}

.header-title p {
  font-size: 16px;
  color: rgba(255, 255, 255, 0.6);
  margin: 0;
}

.stats-row {
  margin-bottom: 24px;
}

.stat-card {
  background: linear-gradient(135deg, #1e232d 0%, #2a3142 100%);
  border-radius: 16px;
  padding: 24px;
  display: flex;
  align-items: center;
  gap: 16px;
  border: 1px solid rgba(64, 158, 255, 0.1);
  transition: all 0.3s ease;
  cursor: pointer;
  position: relative;
  overflow: hidden;
}

.stat-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 4px;
  background: linear-gradient(90deg, transparent, currentColor);
  opacity: 0.5;
}

.stat-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.3);
  border-color: rgba(64, 158, 255, 0.3);
}

.stat-card.primary {
  color: #409eff;
}

.stat-card.success {
  color: #67c23a;
}

.stat-card.warning {
  color: #e6a23c;
}

.stat-card.info {
  color: #909399;
}

.stat-icon {
  width: 64px;
  height: 64px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(64, 158, 255, 0.1);
  border-radius: 12px;
}

.stat-content {
  flex: 1;
}

.stat-value {
  font-size: 32px;
  font-weight: 700;
  color: #ffffff;
  line-height: 1;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 14px;
  color: rgba(255, 255, 255, 0.6);
}

.stat-trend {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 14px;
  font-weight: 600;
  padding: 4px 8px;
  border-radius: 6px;
  background: rgba(103, 194, 58, 0.1);
  color: #67c23a;
}

.content-row {
  margin-bottom: 24px;
}

.dashboard-card {
  margin-bottom: 24px;
  border-radius: 16px;
  border: 1px solid rgba(64, 158, 255, 0.1);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-header h3 {
  font-size: 18px;
  font-weight: 600;
  color: #ffffff;
  margin: 0;
}

.projects-list,
.tasks-list,
.activity-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.project-item {
  padding: 16px;
  background: rgba(45, 55, 72, 0.5);
  border-radius: 12px;
  border: 1px solid rgba(64, 158, 255, 0.1);
  cursor: pointer;
  transition: all 0.3s ease;
}

.project-item:hover {
  background: rgba(45, 55, 72, 0.8);
  border-color: rgba(64, 158, 255, 0.3);
  transform: translateX(4px);
}

.project-info {
  margin-bottom: 16px;
}

.project-name {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 8px;
  font-size: 16px;
  font-weight: 600;
  color: #ffffff;
}

.project-color {
  width: 12px;
  height: 12px;
  border-radius: 50%;
}

.project-meta {
  display: flex;
  gap: 16px;
  font-size: 13px;
  color: rgba(255, 255, 255, 0.6);
}

.project-meta span {
  display: flex;
  align-items: center;
  gap: 4px;
}

.project-progress {
  display: flex;
  align-items: center;
  gap: 12px;
}

.progress-info {
  min-width: 50px;
}

.progress-text {
  font-size: 14px;
  font-weight: 600;
  color: #409eff;
}

.quick-stats {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.quick-stat-item {
  padding: 16px;
  background: rgba(45, 55, 72, 0.5);
  border-radius: 12px;
  border: 1px solid rgba(64, 158, 255, 0.1);
}

.quick-stat-label {
  font-size: 13px;
  color: rgba(255, 255, 255, 0.6);
  margin-bottom: 8px;
}

.quick-stat-value {
  font-size: 24px;
  font-weight: 700;
  color: #ffffff;
  margin-bottom: 12px;
}

.quick-actions-card {
  margin-top: 24px;
}

.quick-actions {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.action-btn {
  width: 100%;
  height: 48px;
  font-size: 15px;
  font-weight: 600;
  border-radius: 12px;
}

.task-item {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 12px;
  background: rgba(45, 55, 72, 0.5);
  border-radius: 12px;
  border: 1px solid rgba(64, 158, 255, 0.1);
  transition: all 0.3s ease;
}

.task-item:hover {
  background: rgba(45, 55, 72, 0.8);
  border-color: rgba(64, 158, 255, 0.3);
}

.task-content {
  flex: 1;
}

.task-title {
  font-size: 15px;
  font-weight: 500;
  color: #ffffff;
  margin-bottom: 8px;
}

.task-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.task-due-date {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  color: rgba(255, 255, 255, 0.6);
}

.activity-list :deep(.el-timeline) {
  padding-left: 0;
}

.activity-item {
  display: flex;
  align-items: flex-start;
  gap: 12px;
}

.activity-content {
  flex: 1;
}

.activity-text {
  font-size: 14px;
  color: rgba(255, 255, 255, 0.8);
  line-height: 1.6;
}

.activity-target {
  color: #409eff;
  font-weight: 600;
}

.empty-state {
  padding: 40px 20px;
  text-align: center;
}

@media (max-width: 768px) {
  .dashboard-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .header-title h1 {
    font-size: 24px;
  }

  .stat-card {
    padding: 20px;
  }

  .stat-icon {
    width: 48px;
    height: 48px;
  }

  .stat-value {
    font-size: 24px;
  }
}
</style>
