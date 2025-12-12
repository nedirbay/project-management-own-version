<template>
  <div class="main-layout">
    <!-- Sidebar -->
    <aside class="sidebar" :class="{ collapsed: sidebarCollapsed, light: !isDark }">
      <div class="sidebar-header">
        <div class="logo">
          <el-icon :size="32" color="#409EFF">
            <Management />
          </el-icon>
          <transition name="fade">
            <span v-show="!sidebarCollapsed" class="logo-text">Project Manager</span>
          </transition>
        </div>
        <el-button
          class="collapse-btn"
          :icon="sidebarCollapsed ? Expand : Fold"
          circle
          @click="toggleSidebar"
        />
      </div>

      <div class="sidebar-content">
        <!-- Workspace Selector -->
        <div v-show="!sidebarCollapsed" class="workspace-selector">
          <el-select
            v-model="currentWorkspaceId"
            placeholder="Workspace seçin"
            size="large"
            @change="handleWorkspaceChange"
          >
            <el-option v-for="ws in workspaces" :key="ws.id" :label="ws.name" :value="ws.id">
              <div class="workspace-option">
                <div class="workspace-color" :style="{ backgroundColor: ws.color }"></div>
                <span>{{ ws.name }}</span>
              </div>
            </el-option>
          </el-select>
        </div>

        <!-- Navigation Menu -->
        <el-menu
          :default-active="activeMenu"
          :collapse="sidebarCollapsed"
          class="sidebar-menu"
          @select="handleMenuSelect"
        >
          <el-menu-item index="/">
            <el-icon><HomeFilled /></el-icon>
            <template #title>Dashboard</template>
          </el-menu-item>

          <el-menu-item index="/workspaces">
            <el-icon><OfficeBuilding /></el-icon>
            <template #title>Workspace'ler</template>
          </el-menu-item>

          <el-menu-item index="/projects">
            <el-icon><Folder /></el-icon>
            <template #title>Projeler</template>
          </el-menu-item>

          <el-menu-item index="/tasks">
            <el-icon><Document /></el-icon>
            <template #title>Görevler</template>
          </el-menu-item>

          <el-menu-item index="/kanban">
            <el-icon><Grid /></el-icon>
            <template #title>Kanban Board</template>
          </el-menu-item>

          <el-menu-item index="/reports">
            <el-icon><Notebook /></el-icon>
            <template #title>Günlük Rapor</template>
          </el-menu-item>

          <el-divider v-show="!sidebarCollapsed" class="menu-divider" />

          <el-menu-item v-if="isAdmin" index="/users">
            <el-icon><User /></el-icon>
            <template #title>Kullanıcılar</template>
          </el-menu-item>
        </el-menu>
      </div>

      <div class="sidebar-footer">
        <el-dropdown trigger="click" placement="top-start">
          <div class="user-profile">
            <el-avatar :size="sidebarCollapsed ? 32 : 40" :src="currentUser?.avatar">
              {{ currentUser?.fullName?.charAt(0) }}
            </el-avatar>
            <transition name="fade">
              <div v-show="!sidebarCollapsed" class="user-info">
                <div class="user-name">{{ currentUser?.fullName }}</div>
                <div class="user-role">{{ userRoleText }}</div>
              </div>
            </transition>
          </div>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item @click="$router.push('/profile')">
                <el-icon><User /></el-icon>
                Profil
              </el-dropdown-item>
              <el-dropdown-item @click="$router.push('/settings')">
                <el-icon><Setting /></el-icon>
                Ayarlar
              </el-dropdown-item>
              <el-dropdown-item divided @click="handleLogout">
                <el-icon><SwitchButton /></el-icon>
                Çıkış Yap
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </aside>

    <!-- Main Content -->
    <main class="main-content">
      <!-- Header -->
      <header class="main-header">
        <div class="header-left">
          <el-breadcrumb separator="/">
            <el-breadcrumb-item v-for="(item, index) in breadcrumbs" :key="index">
              {{ item }}
            </el-breadcrumb-item>
          </el-breadcrumb>
        </div>

        <div class="header-right">
          <!-- Notifications -->
          <el-badge
            :value="notificationCount"
            :hidden="notificationCount === 0"
            class="notification-badge"
          >
            <el-button class="header-btn" :icon="Bell" circle />
          </el-badge>

          <!-- Theme Toggle -->
          <el-button
            class="header-btn"
            :icon="isDark ? Sunny : Moon"
            circle
            @click="themeStore.toggleTheme"
          />

          <!-- Quick Actions -->
          <el-dropdown trigger="click">
            <el-button type="primary" :icon="Plus" circle />
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item @click="showCreateWorkspace = true">
                  <el-icon><OfficeBuilding /></el-icon>
                  Yeni Workspace
                </el-dropdown-item>
                <el-dropdown-item @click="showCreateProject = true">
                  <el-icon><Folder /></el-icon>
                  Yeni Proje
                </el-dropdown-item>
                <el-dropdown-item @click="showCreateTask = true">
                  <el-icon><Document /></el-icon>
                  Yeni Görev
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </header>

      <!-- Page Content -->
      <div class="page-content">
        <router-view v-slot="{ Component }">
          <transition name="fade-slide" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </div>
    </main>

    <!-- Quick Create Dialogs -->
    <el-dialog v-model="showCreateWorkspace" title="Yeni Workspace" width="500px">
      <p>Workspace oluşturma formu buraya gelecek</p>
    </el-dialog>

    <el-dialog v-model="showCreateProject" title="Yeni Proje" width="600px">
      <p>Proje oluşturma formu buraya gelecek</p>
    </el-dialog>

    <el-dialog v-model="showCreateTask" title="Yeni Görev" width="600px">
      <p>Görev oluşturma formu buraya gelecek</p>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  Management,
  Expand,
  Fold,
  HomeFilled,
  OfficeBuilding,
  Folder,
  Document,
  Grid,
  Notebook,
  User,
  Setting,
  SwitchButton,
  Bell,
  Moon,
  Sunny,
  Plus,
} from '@element-plus/icons-vue'
import { useAuthStore } from '@/stores/auth'
import { useWorkspacesStore } from '@/stores/workspaces'
import { useThemeStore } from '@/stores/theme'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const workspacesStore = useWorkspacesStore()
const themeStore = useThemeStore()

// State
const sidebarCollapsed = ref(false)
const showCreateWorkspace = ref(false)
const showCreateProject = ref(false)
const showCreateTask = ref(false)
const notificationCount = ref(0)

// Computed
const currentUser = computed(() => authStore.currentUser)
const isAdmin = computed(() => authStore.isAdmin)
const workspaces = computed(() => workspacesStore.myWorkspaces)
const currentWorkspaceId = computed({
  get: () => workspacesStore.currentWorkspaceId,
  set: (value) => workspacesStore.setCurrentWorkspace(value),
})
const isDark = computed(() => themeStore.isDark)

const activeMenu = computed(() => {
  return route.path
})

const userRoleText = computed(() => {
  return currentUser.value?.role === 'admin' ? 'Yönetici' : 'Kullanıcı'
})

const breadcrumbs = computed(() => {
  const paths = route.path.split('/').filter((p) => p)
  if (paths.length === 0) return ['Dashboard']

  const breadcrumbMap: Record<string, string> = {
    workspaces: "Workspace'ler",
    projects: 'Projeler',
    tasks: 'Görevler',
    kanban: 'Kanban Board',
    reports: 'Günlük Rapor',
    users: 'Kullanıcılar',
    settings: 'Ayarlar',
    profile: 'Profil',
  }

  return paths.map((path) => breadcrumbMap[path] || path)
})

// Methods
const toggleSidebar = () => {
  sidebarCollapsed.value = !sidebarCollapsed.value
  localStorage.setItem('sidebarCollapsed', String(sidebarCollapsed.value))
}

const handleMenuSelect = (index: string) => {
  router.push(index)
}

const handleWorkspaceChange = (workspaceId: string) => {
  workspacesStore.setCurrentWorkspace(workspaceId)
  ElMessage.success('Workspace değiştirildi')
}

const handleLogout = async () => {
  try {
    await ElMessageBox.confirm('Çıkış yapmak istediğinize emin misiniz?', 'Çıkış', {
      confirmButtonText: 'Evet',
      cancelButtonText: 'Hayır',
      type: 'warning',
    })

    authStore.logout()
    router.push('/login')
    ElMessage.success('Başarıyla çıkış yapıldı')
  } catch {
    // User cancelled
  }
}

// Initialize
const initSidebar = () => {
  const stored = localStorage.getItem('sidebarCollapsed')
  if (stored !== null) {
    sidebarCollapsed.value = stored === 'true'
  }
}

// Fetch workspaces
workspacesStore.fetchWorkspaces()

// Initialize sidebar state
initSidebar()
</script>

<style scoped>
.main-layout {
  display: flex;
  min-height: 100vh;
  background: #0f1419;
}

/* Sidebar Styles */
.sidebar {
  width: 280px;
  background: var(--bg-secondary);
  border-right: 1px solid var(--border-color);
  display: flex;
  flex-direction: column;
  transition: width 0.3s ease;
  position: fixed;
  left: 0;
  top: 0;
  height: 100vh;
  z-index: 1000;
}

.sidebar.collapsed {
  width: 80px;
}

.sidebar-header {
  padding: 24px 20px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid var(--border-color);
}

.logo {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
}

.logo-text {
  font-size: 20px;
  font-weight: 700;
  background: linear-gradient(135deg, #409eff 0%, #66b1ff 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.collapse-btn {
  background: transparent;
  border: 1px solid var(--border-color);
  color: var(--color-primary);
}

.collapse-btn:hover {
  background: var(--bg-hover);
  border-color: var(--color-primary);
}

.sidebar-content {
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 20px 0;
}

.workspace-selector {
  padding: 0 20px 20px;
}

.workspace-selector :deep(.el-select) {
  width: 100%;
}

.workspace-selector :deep(.el-input__wrapper) {
  background: var(--bg-tertiary);
  border: 1px solid var(--border-color);
  box-shadow: none;
}

.workspace-selector :deep(.el-input__wrapper:hover) {
  border-color: var(--border-color-hover);
}

.workspace-selector :deep(.el-input__inner) {
  color: var(--text-primary);
}

.workspace-option {
  display: flex;
  align-items: center;
  gap: 8px;
}

.workspace-color {
  width: 12px;
  height: 12px;
  border-radius: 50%;
}

.sidebar-menu {
  background: transparent;
  border: none;
}

.sidebar-menu :deep(.el-menu-item) {
  color: var(--text-secondary);
  margin: 4px 12px;
  border-radius: 8px;
  transition: all 0.3s ease;
}

.sidebar-menu :deep(.el-menu-item:hover) {
  background: var(--bg-hover);
  color: var(--color-primary);
}

.sidebar-menu :deep(.el-menu-item.is-active) {
  background: var(--bg-hover);
  color: var(--color-primary);
  font-weight: 600;
}

.sidebar-menu :deep(.el-icon) {
  color: inherit;
}

.menu-divider {
  margin: 16px 20px;
  border-color: var(--border-color);
}

.sidebar-footer {
  padding: 20px;
  border-top: 1px solid var(--border-color);
}

.user-profile {
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
  padding: 8px;
  border-radius: 12px;
  transition: background 0.3s ease;
}

.user-profile:hover {
  background: var(--bg-hover);
}

.user-info {
  flex: 1;
  min-width: 0;
}

.user-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.user-role {
  font-size: 12px;
  color: var(--text-tertiary);
}

/* Main Content Styles */
.main-content {
  flex: 1;
  margin-left: 280px;
  transition: margin-left 0.3s ease;
  display: flex;
  flex-direction: column;
}

.sidebar.collapsed ~ .main-content {
  margin-left: 80px;
}

.main-header {
  height: 64px;
  background: var(--bg-secondary);
  border-bottom: 1px solid var(--border-color);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 32px;
  position: sticky;
  top: 0;
  z-index: 999;
}

.header-left {
  flex: 1;
}

.header-left :deep(.el-breadcrumb) {
  font-size: 14px;
}

.header-left :deep(.el-breadcrumb__item) {
  color: var(--text-secondary);
}

.header-left :deep(.el-breadcrumb__item:last-child .el-breadcrumb__inner) {
  color: var(--text-primary);
  font-weight: 600;
}

.header-left :deep(.el-breadcrumb__separator) {
  color: var(--text-tertiary);
}

.header-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.header-btn {
  background: transparent;
  border: 1px solid var(--border-color);
  color: var(--text-secondary);
}

.header-btn:hover {
  background: var(--bg-hover);
  border-color: var(--color-primary);
  color: var(--color-primary);
}

.notification-badge :deep(.el-badge__content) {
  background-color: #f56c6c;
  border: none;
}

.page-content {
  flex: 1;
  padding: 32px;
  overflow-y: auto;
  background: var(--bg-primary);
}

/* Transitions */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: all 0.3s ease;
}

.fade-slide-enter-from {
  opacity: 0;
  transform: translateX(20px);
}

.fade-slide-leave-to {
  opacity: 0;
  transform: translateX(-20px);
}

/* Scrollbar */
.sidebar-content::-webkit-scrollbar {
  width: 6px;
}

.sidebar-content::-webkit-scrollbar-track {
  background: var(--bg-tertiary);
}

.sidebar-content::-webkit-scrollbar-thumb {
  background: var(--border-color);
  border-radius: 3px;
}

.sidebar-content::-webkit-scrollbar-thumb:hover {
  background: var(--border-color-hover);
}

.page-content::-webkit-scrollbar {
  width: 8px;
}

.page-content::-webkit-scrollbar-track {
  background: var(--bg-secondary);
}

.page-content::-webkit-scrollbar-thumb {
  background: var(--border-color);
  border-radius: 4px;
}

.page-content::-webkit-scrollbar-thumb:hover {
  background: var(--border-color-hover);
}

/* Responsive */
@media (max-width: 768px) {
  .sidebar {
    transform: translateX(-100%);
    transition: transform 0.3s ease;
  }

  .sidebar.mobile-open {
    transform: translateX(0);
  }

  .main-content {
    margin-left: 0;
  }

  .sidebar.collapsed ~ .main-content {
    margin-left: 0;
  }

  .main-header {
    padding: 0 16px;
  }

  .page-content {
    padding: 16px;
  }
}
</style>
