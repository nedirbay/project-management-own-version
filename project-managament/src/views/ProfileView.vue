<template>
  <div class="profile-view">
    <div class="page-header">
      <div class="header-content">
        <h1>{{ $t('profile.title') }}</h1>
        <p>{{ $t('profile.personalInformation') }}</p>
      </div>
    </div>

    <el-row :gutter="24">
      <!-- Left Sidebar -->
      <el-col :xs="24" :md="8">
        <!-- Avatar Card -->
        <el-card class="profile-card">
          <div class="profile-avatar">
            <div class="avatar-wrapper">
              <el-avatar :size="140" :src="avatarPreview || currentUser?.avatar">
                <span class="avatar-text">{{ getInitials(currentUser?.fullName) }}</span>
              </el-avatar>
              <div class="avatar-overlay" @click="triggerFileInput">
                <el-icon :size="24"><Camera /></el-icon>
              </div>
            </div>
            <input
              ref="fileInput"
              type="file"
              accept="image/*"
              style="display: none"
              @change="handleAvatarChange"
            />
          </div>

          <div class="avatar-actions">
            <el-button type="primary" size="small" @click="triggerFileInput">
              <el-icon><Upload /></el-icon>
              {{ $t('profile.uploadAvatar') }}
            </el-button>
            <el-button
              v-if="avatarPreview || currentUser?.avatar"
              type="danger"
              size="small"
              plain
              @click="handleRemoveAvatar"
            >
              <el-icon><Delete /></el-icon>
              {{ $t('profile.removeAvatar') }}
            </el-button>
          </div>

          <div class="profile-info">
            <h3>{{ currentUser?.fullName }}</h3>
            <p class="email">{{ currentUser?.email }}</p>
            <el-tag
              :type="getRoleType(currentUser?.role)"
              effect="dark"
              size="large"
              class="role-tag"
            >
              {{ getRoleLabel(currentUser?.role) }}
            </el-tag>
          </div>
        </el-card>

        <!-- Stats Card -->
        <el-card class="stats-card">
          <template #header>
            <h3>{{ $t('dashboard.statistics') }}</h3>
          </template>
          <div class="stats-list">
            <div class="stat-item">
              <div class="stat-icon projects">
                <el-icon><Folder /></el-icon>
              </div>
              <div class="stat-content">
                <span class="stat-label">{{ $t('projects.title') }}</span>
                <span class="stat-value">{{ myProjectsCount }}</span>
              </div>
            </div>
            <div class="stat-item">
              <div class="stat-icon tasks">
                <el-icon><Document /></el-icon>
              </div>
              <div class="stat-content">
                <span class="stat-label">{{ $t('tasks.title') }}</span>
                <span class="stat-value">{{ myTasksCount }}</span>
              </div>
            </div>
            <div class="stat-item">
              <div class="stat-icon completed">
                <el-icon><Check /></el-icon>
              </div>
              <div class="stat-content">
                <span class="stat-label">{{ $t('projects.completedTasks') }}</span>
                <span class="stat-value">{{ completedTasksCount }}</span>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- Right Content -->
      <el-col :xs="24" :md="16">
        <!-- Personal Information Card -->
        <el-card class="details-card">
          <template #header>
            <div class="card-header">
              <h3>{{ $t('profile.personalInformation') }}</h3>
              <el-button
                v-if="!isEditingProfile"
                type="primary"
                size="small"
                @click="startEditProfile"
              >
                <el-icon><Edit /></el-icon>
                {{ $t('common.edit') }}
              </el-button>
            </div>
          </template>

          <el-form
            ref="profileFormRef"
            :model="profileForm"
            :rules="profileRules"
            label-position="top"
            size="large"
          >
            <el-row :gutter="16">
              <el-col :span="12">
                <el-form-item :label="$t('profile.fullName')" prop="fullName">
                  <el-input
                    v-model="profileForm.fullName"
                    :disabled="!isEditingProfile"
                    :prefix-icon="User"
                  />
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item :label="$t('profile.email')" prop="email">
                  <el-input
                    v-model="profileForm.email"
                    :disabled="!isEditingProfile"
                    :prefix-icon="Message"
                  />
                </el-form-item>
              </el-col>
            </el-row>

            <el-row :gutter="16">
              <el-col :span="12">
                <el-form-item :label="$t('profile.phone')" prop="phone">
                  <el-input
                    v-model="profileForm.phone"
                    :disabled="!isEditingProfile"
                    :prefix-icon="Phone"
                  />
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item :label="$t('users.role')">
                  <el-input :value="getRoleLabel(currentUser?.role)" disabled />
                </el-form-item>
              </el-col>
            </el-row>

            <el-form-item :label="$t('profile.bio')" prop="bio">
              <el-input
                v-model="profileForm.bio"
                :disabled="!isEditingProfile"
                type="textarea"
                :rows="4"
                :placeholder="$t('profile.bio')"
              />
            </el-form-item>

            <el-form-item v-if="isEditingProfile">
              <div class="form-actions">
                <el-button @click="cancelEditProfile">
                  {{ $t('common.cancel') }}
                </el-button>
                <el-button type="primary" @click="saveProfile" :loading="isSavingProfile">
                  <el-icon><Check /></el-icon>
                  {{ $t('common.save') }}
                </el-button>
              </div>
            </el-form-item>
          </el-form>
        </el-card>

        <!-- Security Card -->
        <el-card class="password-card">
          <template #header>
            <div class="card-header">
              <h3>{{ $t('profile.security') }}</h3>
              <el-icon><Lock /></el-icon>
            </div>
          </template>

          <el-form
            ref="passwordFormRef"
            :model="passwordForm"
            :rules="passwordRules"
            label-position="top"
            size="large"
          >
            <el-form-item :label="$t('profile.currentPassword')" prop="currentPassword">
              <el-input
                v-model="passwordForm.currentPassword"
                type="password"
                :prefix-icon="Lock"
                show-password
                :placeholder="$t('profile.currentPassword')"
              />
            </el-form-item>

            <el-form-item :label="$t('profile.newPassword')" prop="newPassword">
              <el-input
                v-model="passwordForm.newPassword"
                type="password"
                :prefix-icon="Lock"
                show-password
                :placeholder="$t('profile.newPassword')"
              />
              <div class="password-strength">
                <div
                  class="strength-bar"
                  :class="passwordStrength.class"
                  :style="{ width: passwordStrength.width }"
                ></div>
              </div>
              <p class="password-hint">{{ passwordStrength.label }}</p>
            </el-form-item>

            <el-form-item :label="$t('profile.confirmPassword')" prop="confirmPassword">
              <el-input
                v-model="passwordForm.confirmPassword"
                type="password"
                :prefix-icon="Lock"
                show-password
                :placeholder="$t('profile.confirmPassword')"
              />
            </el-form-item>

            <el-form-item>
              <el-button
                type="primary"
                @click="changePassword"
                :loading="isChangingPassword"
                :disabled="!isPasswordFormValid"
              >
                <el-icon><Key /></el-icon>
                {{ $t('profile.changePassword') }}
              </el-button>
            </el-form-item>
          </el-form>
        </el-card>

        <!-- Notifications Card -->
        <el-card class="notifications-card">
          <template #header>
            <div class="card-header">
              <h3>{{ $t('profile.notifications') }}</h3>
              <el-icon><Bell /></el-icon>
            </div>
          </template>

          <div class="notifications-content">
            <div class="notification-section">
              <h4>{{ $t('profile.emailNotifications') }}</h4>
              <div class="notification-item">
                <div class="notification-info">
                  <span class="notification-label">{{ $t('profile.taskAssigned') }}</span>
                  <span class="notification-desc"
                    >Get notified when someone assigns you a task</span
                  >
                </div>
                <el-switch
                  v-model="notificationSettings.taskAssigned"
                  @change="saveNotificationSettings"
                />
              </div>
              <div class="notification-item">
                <div class="notification-info">
                  <span class="notification-label">{{ $t('profile.taskCompleted') }}</span>
                  <span class="notification-desc">Get notified when a task is completed</span>
                </div>
                <el-switch
                  v-model="notificationSettings.taskCompleted"
                  @change="saveNotificationSettings"
                />
              </div>
              <div class="notification-item">
                <div class="notification-info">
                  <span class="notification-label">{{ $t('profile.projectUpdated') }}</span>
                  <span class="notification-desc">Get notified about project updates</span>
                </div>
                <el-switch
                  v-model="notificationSettings.projectUpdated"
                  @change="saveNotificationSettings"
                />
              </div>
            </div>

            <el-divider />

            <div class="notification-section">
              <h4>{{ $t('profile.pushNotifications') }}</h4>
              <div class="notification-item">
                <div class="notification-info">
                  <span class="notification-label">{{ $t('profile.reportReminder') }}</span>
                  <span class="notification-desc">Daily reminder to submit your report</span>
                </div>
                <el-switch
                  v-model="notificationSettings.reportReminder"
                  @change="saveNotificationSettings"
                />
              </div>
              <div class="notification-item">
                <div class="notification-info">
                  <span class="notification-label">Browser Notifications</span>
                  <span class="notification-desc">Receive browser push notifications</span>
                </div>
                <el-switch
                  v-model="notificationSettings.browserNotifications"
                  @change="saveNotificationSettings"
                />
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive } from 'vue'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import {
  Camera,
  Upload,
  Delete,
  Edit,
  User,
  Message,
  Phone,
  Lock,
  Key,
  Bell,
  Check,
  Folder,
  Document,
} from '@element-plus/icons-vue'
import { useAuthStore } from '@/stores/auth'
import { useProjectsStore } from '@/stores/projects'
import { useTasksStore } from '@/stores/tasks'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const authStore = useAuthStore()
const projectsStore = useProjectsStore()
const tasksStore = useTasksStore()

const currentUser = computed(() => authStore.currentUser)
const myProjectsCount = computed(() => projectsStore.myProjects.length)
const myTasksCount = computed(() => tasksStore.myTasks.length)
const completedTasksCount = computed(() => {
  return tasksStore.myTasks.filter((t) => t.status === 'done').length
})

// Avatar Upload
const fileInput = ref<HTMLInputElement>()
const avatarPreview = ref<string>('')

const triggerFileInput = () => {
  fileInput.value?.click()
}

const handleAvatarChange = (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]

  if (file) {
    // Validate file size (max 5MB)
    if (file.size > 5 * 1024 * 1024) {
      ElMessage.error('File size must be less than 5MB')
      return
    }

    // Validate file type
    if (!file.type.startsWith('image/')) {
      ElMessage.error('Please upload an image file')
      return
    }

    // Preview image using FileReader
    const reader = new FileReader()
    reader.onload = (e) => {
      avatarPreview.value = e.target?.result as string
      ElMessage.success(t('profile.updated'))
    }
    reader.readAsDataURL(file)
  }
}

const handleRemoveAvatar = () => {
  avatarPreview.value = ''
  if (fileInput.value) {
    fileInput.value.value = ''
  }
  ElMessage.success('Avatar removed')
}

const getInitials = (name?: string) => {
  if (!name) return 'U'
  return name
    .split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2)
}

// Profile Form
const profileFormRef = ref<FormInstance>()
const isEditingProfile = ref(false)
const isSavingProfile = ref(false)

const profileForm = reactive({
  fullName: currentUser.value?.fullName || '',
  email: currentUser.value?.email || '',
  phone: '',
  bio: '',
})

const profileRules: FormRules = {
  fullName: [{ required: true, message: t('validation.required'), trigger: 'blur' }],
  email: [
    { required: true, message: t('validation.required'), trigger: 'blur' },
    { type: 'email', message: t('validation.invalidEmail'), trigger: 'blur' },
  ],
}

const startEditProfile = () => {
  isEditingProfile.value = true
}

const cancelEditProfile = () => {
  isEditingProfile.value = false
  profileForm.fullName = currentUser.value?.fullName || ''
  profileForm.email = currentUser.value?.email || ''
}

const saveProfile = async () => {
  if (!profileFormRef.value) return

  await profileFormRef.value.validate((valid) => {
    if (valid) {
      isSavingProfile.value = true
      setTimeout(() => {
        isSavingProfile.value = false
        isEditingProfile.value = false
        ElMessage.success(t('profile.updated'))
      }, 1000)
    }
  })
}

// Password Change
const passwordFormRef = ref<FormInstance>()
const isChangingPassword = ref(false)

const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
})

const validatePasswordConfirm = (rule: any, value: string, callback: any) => {
  if (value === '') {
    callback(new Error(t('validation.required')))
  } else if (value !== passwordForm.newPassword) {
    callback(new Error(t('profile.passwordMismatch')))
  } else {
    callback()
  }
}

const passwordRules: FormRules = {
  currentPassword: [{ required: true, message: t('validation.required'), trigger: 'blur' }],
  newPassword: [
    { required: true, message: t('validation.required'), trigger: 'blur' },
    { min: 8, message: t('validation.minLength', { min: 8 }), trigger: 'blur' },
  ],
  confirmPassword: [
    { required: true, message: t('validation.required'), trigger: 'blur' },
    { validator: validatePasswordConfirm, trigger: 'blur' },
  ],
}

const passwordStrength = computed(() => {
  const password = passwordForm.newPassword
  if (!password) return { width: '0%', class: '', label: '' }

  let strength = 0
  if (password.length >= 8) strength++
  if (/[a-z]/.test(password)) strength++
  if (/[A-Z]/.test(password)) strength++
  if (/[0-9]/.test(password)) strength++
  if (/[^a-zA-Z0-9]/.test(password)) strength++

  if (strength <= 2) return { width: '33%', class: 'weak', label: 'Weak' }
  if (strength <= 3) return { width: '66%', class: 'medium', label: 'Medium' }
  return { width: '100%', class: 'strong', label: 'Strong' }
})

const isPasswordFormValid = computed(() => {
  return (
    passwordForm.currentPassword &&
    passwordForm.newPassword &&
    passwordForm.confirmPassword &&
    passwordForm.newPassword === passwordForm.confirmPassword
  )
})

const changePassword = async () => {
  if (!passwordFormRef.value) return

  await passwordFormRef.value.validate((valid) => {
    if (valid) {
      isChangingPassword.value = true
      setTimeout(() => {
        isChangingPassword.value = false
        passwordForm.currentPassword = ''
        passwordForm.newPassword = ''
        passwordForm.confirmPassword = ''
        ElMessage.success(t('profile.passwordChanged'))
      }, 1000)
    }
  })
}

// Notification Settings
const notificationSettings = reactive({
  taskAssigned: true,
  taskCompleted: true,
  projectUpdated: true,
  reportReminder: true,
  browserNotifications: false,
})

const saveNotificationSettings = () => {
  localStorage.setItem('notificationSettings', JSON.stringify(notificationSettings))
  ElMessage.success(t('profile.updated'))
}

// Helper Functions
const getRoleType = (role?: string) => {
  const types: Record<string, any> = {
    admin: 'danger',
    workspaceAdmin: 'warning',
    member: 'info',
  }
  return types[role || 'member']
}

const getRoleLabel = (role?: string) => {
  if (!role) return t('users.roles.member')
  return t(`users.roles.${role}`)
}

// Initialize
projectsStore.fetchProjects()
tasksStore.fetchTasks()

// Load notification settings
const savedSettings = localStorage.getItem('notificationSettings')
if (savedSettings) {
  Object.assign(notificationSettings, JSON.parse(savedSettings))
}
</script>

<style scoped>
.profile-view {
  animation: fadeIn 0.3s ease;
  padding: 24px;
}

.page-header {
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

/* Profile Card */
.profile-card,
.stats-card,
.details-card,
.password-card,
.notifications-card {
  border-radius: 12px;
  box-shadow: var(--el-box-shadow-light);
  margin-bottom: 24px;
}

.profile-avatar {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 24px 0;
}

.avatar-wrapper {
  position: relative;
  cursor: pointer;
  transition: transform 0.3s;
}

.avatar-wrapper:hover {
  transform: scale(1.05);
}

.avatar-wrapper :deep(.el-avatar) {
  border: 4px solid var(--el-color-primary);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.avatar-text {
  font-size: 48px;
  font-weight: 700;
}

.avatar-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: opacity 0.3s;
  color: white;
}

.avatar-wrapper:hover .avatar-overlay {
  opacity: 1;
}

.avatar-actions {
  display: flex;
  gap: 8px;
  margin-top: 16px;
}

.profile-info {
  text-align: center;
  padding: 24px;
  width: 100%;
}

.profile-info h3 {
  font-size: 24px;
  font-weight: 600;
  color: var(--el-text-color-primary);
  margin: 0 0 8px;
}

.email {
  font-size: 14px;
  color: var(--el-text-color-secondary);
  margin: 0 0 12px;
}

.role-tag {
  font-size: 13px;
  padding: 8px 16px;
}

/* Stats Card */
.stats-card h3 {
  font-size: 18px;
  font-weight: 600;
  color: var(--el-text-color-primary);
  margin: 0;
}

.stats-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px;
  background: var(--el-fill-color-lighter);
  border-radius: 8px;
  transition: all 0.3s;
}

.stat-item:hover {
  background: var(--el-fill-color-light);
  transform: translateX(4px);
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  color: white;
}

.stat-icon.projects {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.stat-icon.tasks {
  background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
}

.stat-icon.completed {
  background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
}

.stat-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.stat-label {
  font-size: 14px;
  color: var(--el-text-color-secondary);
}

.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: var(--el-text-color-primary);
}

/* Cards */
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-header h3 {
  font-size: 18px;
  font-weight: 600;
  color: var(--el-text-color-primary);
  margin: 0;
}

.form-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  width: 100%;
}

/* Password Strength */
.password-strength {
  margin-top: 8px;
  height: 4px;
  background: var(--el-fill-color);
  border-radius: 2px;
  overflow: hidden;
}

.strength-bar {
  height: 100%;
  transition: all 0.3s;
  border-radius: 2px;
}

.strength-bar.weak {
  background: var(--el-color-danger);
}

.strength-bar.medium {
  background: var(--el-color-warning);
}

.strength-bar.strong {
  background: var(--el-color-success);
}

.password-hint {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  margin-top: 4px;
}

/* Notifications */
.notifications-content {
  padding: 8px 0;
}

.notification-section {
  margin-bottom: 16px;
}

.notification-section h4 {
  font-size: 16px;
  font-weight: 600;
  color: var(--el-text-color-primary);
  margin-bottom: 16px;
}

.notification-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  background: var(--el-fill-color-lighter);
  border-radius: 8px;
  margin-bottom: 8px;
  transition: all 0.3s;
}

.notification-item:hover {
  background: var(--el-fill-color-light);
}

.notification-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.notification-label {
  font-size: 15px;
  font-weight: 500;
  color: var(--el-text-color-primary);
}

.notification-desc {
  font-size: 13px;
  color: var(--el-text-color-secondary);
}

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

@media (max-width: 768px) {
  .profile-view {
    padding: 16px;
  }

  .header-content h1 {
    font-size: 24px;
  }

  .avatar-actions {
    flex-direction: column;
    width: 100%;
  }

  .avatar-actions button {
    width: 100%;
  }
}
</style>
