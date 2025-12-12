<template>
  <div class="settings-view">
    <div class="page-header">
      <div class="header-content">
        <h1>{{ $t('settings.title') }}</h1>
        <p>{{ $t('settings.general') }}</p>
      </div>
    </div>

    <el-row :gutter="24">
      <el-col :xs="24" :md="8">
        <el-card class="settings-menu-card">
          <el-menu :default-active="activeTab" class="settings-menu" @select="handleMenuSelect">
            <el-menu-item index="appearance">
              <el-icon><Sunny /></el-icon>
              <span>{{ $t('settings.appearance') }}</span>
            </el-menu-item>
            <el-menu-item index="preferences">
              <el-icon><Setting /></el-icon>
              <span>{{ $t('settings.preferences') }}</span>
            </el-menu-item>
            <el-menu-item index="notifications">
              <el-icon><Bell /></el-icon>
              <span>{{ $t('settings.notifications') }}</span>
            </el-menu-item>
          </el-menu>
        </el-card>
      </el-col>

      <el-col :xs="24" :md="16">
        <el-card class="settings-card">
          <!-- Appearance Tab -->
          <div v-if="activeTab === 'appearance'" class="settings-content">
            <h3 class="section-title">{{ $t('settings.appearance') }}</h3>
            <el-form label-position="top" size="large">
              <el-form-item :label="$t('settings.theme')">
                <el-radio-group
                  v-model="settingsStore.settings.theme"
                  class="theme-selector"
                  @change="handleThemeChange"
                >
                  <el-radio-button value="light">
                    <el-icon><Sunny /></el-icon>
                    {{ $t('settings.lightTheme') }}
                  </el-radio-button>
                  <el-radio-button value="dark">
                    <el-icon><Moon /></el-icon>
                    {{ $t('settings.darkTheme') }}
                  </el-radio-button>
                  <el-radio-button value="system">
                    <el-icon><Monitor /></el-icon>
                    {{ $t('settings.systemTheme') }}
                  </el-radio-button>
                </el-radio-group>
              </el-form-item>

              <el-divider />

              <div class="theme-preview">
                <h4>{{ $t('common.preview') || 'Preview' }}</h4>
                <div class="preview-card">
                  <div class="preview-header">
                    <div class="preview-title">{{ $t('dashboard.title') }}</div>
                    <div class="preview-actions">
                      <div class="preview-dot"></div>
                      <div class="preview-dot"></div>
                      <div class="preview-dot"></div>
                    </div>
                  </div>
                  <div class="preview-content">
                    <div class="preview-text"></div>
                    <div class="preview-text short"></div>
                  </div>
                </div>
              </div>
            </el-form>
          </div>

          <!-- Preferences Tab -->
          <div v-else-if="activeTab === 'preferences'" class="settings-content">
            <h3 class="section-title">{{ $t('settings.preferences') }}</h3>
            <el-form label-position="top" size="large">
              <el-form-item :label="$t('settings.language')">
                <el-select
                  v-model="settingsStore.settings.language"
                  style="width: 100%"
                  @change="handleLanguageChange"
                >
                  <el-option
                    v-for="locale in availableLocales"
                    :key="locale.code"
                    :label="`${locale.flag} ${locale.name}`"
                    :value="locale.code"
                  >
                    <span style="float: left">{{ locale.flag }}</span>
                    <span
                      style="float: right; color: var(--el-text-color-secondary); font-size: 13px"
                    >
                      {{ locale.name }}
                    </span>
                  </el-option>
                </el-select>
              </el-form-item>

              <el-form-item :label="$t('settings.timezone')">
                <el-select
                  v-model="settingsStore.settings.timezone"
                  filterable
                  style="width: 100%"
                  @change="handleTimezoneChange"
                >
                  <el-option
                    v-for="tz in settingsStore.availableTimezones"
                    :key="tz.value"
                    :label="tz.label"
                    :value="tz.value"
                  />
                </el-select>
              </el-form-item>

              <el-divider />

              <el-form-item :label="$t('settings.dateFormat')">
                <el-select
                  v-model="settingsStore.settings.dateFormat"
                  style="width: 100%"
                  @change="handleSave"
                >
                  <el-option
                    v-for="format in settingsStore.dateFormats"
                    :key="format.value"
                    :label="format.label"
                    :value="format.value"
                  />
                </el-select>
              </el-form-item>

              <el-form-item :label="$t('settings.timeFormat')">
                <el-radio-group v-model="settingsStore.settings.timeFormat" @change="handleSave">
                  <el-radio-button value="12h">12-hour (AM/PM)</el-radio-button>
                  <el-radio-button value="24h">24-hour</el-radio-button>
                </el-radio-group>
              </el-form-item>
            </el-form>
          </div>

          <!-- Notifications Tab -->
          <div v-else-if="activeTab === 'notifications'" class="settings-content">
            <h3 class="section-title">{{ $t('settings.notifications') }}</h3>
            <el-form label-position="top" size="large">
              <div class="notification-section">
                <h4>{{ $t('profile.emailNotifications') }}</h4>
                <el-form-item>
                  <div class="notification-item">
                    <div class="notification-info">
                      <span class="notification-label">{{ $t('profile.emailNotifications') }}</span>
                      <span class="notification-desc">{{
                        $t('common.info') || 'Receive notifications via email'
                      }}</span>
                    </div>
                    <el-switch
                      v-model="settingsStore.settings.notifications.email"
                      @change="handleSave"
                    />
                  </div>
                </el-form-item>

                <el-form-item>
                  <div class="notification-item">
                    <div class="notification-info">
                      <span class="notification-label">{{ $t('profile.pushNotifications') }}</span>
                      <span class="notification-desc">Browser push notifications</span>
                    </div>
                    <el-switch
                      v-model="settingsStore.settings.notifications.push"
                      @change="handleSave"
                    />
                  </div>
                </el-form-item>
              </div>

              <el-divider />

              <div class="notification-section">
                <h4>{{ $t('tasks.title') }}</h4>
                <el-form-item>
                  <div class="notification-item">
                    <div class="notification-info">
                      <span class="notification-label">{{ $t('profile.taskAssigned') }}</span>
                      <span class="notification-desc">When a task is assigned to you</span>
                    </div>
                    <el-switch
                      v-model="settingsStore.settings.notifications.taskAssigned"
                      @change="handleSave"
                    />
                  </div>
                </el-form-item>

                <el-form-item>
                  <div class="notification-item">
                    <div class="notification-info">
                      <span class="notification-label">{{ $t('profile.taskCompleted') }}</span>
                      <span class="notification-desc">When a task is completed</span>
                    </div>
                    <el-switch
                      v-model="settingsStore.settings.notifications.taskCompleted"
                      @change="handleSave"
                    />
                  </div>
                </el-form-item>
              </div>

              <el-divider />

              <div class="notification-section">
                <h4>{{ $t('projects.title') }}</h4>
                <el-form-item>
                  <div class="notification-item">
                    <div class="notification-info">
                      <span class="notification-label">{{ $t('profile.projectUpdated') }}</span>
                      <span class="notification-desc">When a project is updated</span>
                    </div>
                    <el-switch
                      v-model="settingsStore.settings.notifications.projectUpdated"
                      @change="handleSave"
                    />
                  </div>
                </el-form-item>

                <el-form-item>
                  <div class="notification-item">
                    <div class="notification-info">
                      <span class="notification-label">{{ $t('profile.reportReminder') }}</span>
                      <span class="notification-desc">Daily report reminder</span>
                    </div>
                    <el-switch
                      v-model="settingsStore.settings.notifications.reportReminder"
                      @change="handleSave"
                    />
                  </div>
                </el-form-item>
              </div>
            </el-form>
          </div>

          <div class="settings-footer">
            <el-button @click="handleReset">
              <el-icon><RefreshLeft /></el-icon>
              {{ $t('common.cancel') }}
            </el-button>
            <el-button type="primary" @click="handleSave" :loading="settingsStore.isLoading">
              <el-icon><Check /></el-icon>
              {{ $t('common.save') }}
            </el-button>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Setting, Bell, Sunny, Moon, Monitor, Check, RefreshLeft } from '@element-plus/icons-vue'
import { useSettingsStore } from '@/stores/settings'
import { availableLocales } from '@/i18n'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const settingsStore = useSettingsStore()
const activeTab = ref('appearance')

onMounted(() => {
  settingsStore.initSettings()
})

const handleMenuSelect = (index: string) => {
  activeTab.value = index
}

const handleThemeChange = (theme: 'light' | 'dark' | 'system') => {
  settingsStore.updateTheme(theme)
  ElMessage.success(t('settings.updated'))
}

const handleLanguageChange = (language: string) => {
  settingsStore.updateLanguage(language)
  ElMessage.success(t('settings.updated'))
}

const handleTimezoneChange = (timezone: string) => {
  settingsStore.updateTimezone(timezone)
  ElMessage.success(t('settings.updated'))
}

const handleSave = () => {
  ElMessage.success(t('settings.updated'))
}

const handleReset = () => {
  settingsStore.resetSettings()
  ElMessage.info(t('common.cancel'))
}
</script>

<style scoped>
.settings-view {
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

.settings-menu-card {
  border-radius: 12px;
  box-shadow: var(--el-box-shadow-light);
}

.settings-menu {
  border: none;
  background: transparent;
}

.settings-menu :deep(.el-menu-item) {
  border-radius: 8px;
  margin-bottom: 4px;
  transition: all 0.3s;
}

.settings-menu :deep(.el-menu-item:hover) {
  background: var(--el-fill-color-light);
}

.settings-menu :deep(.el-menu-item.is-active) {
  background: var(--el-color-primary-light-9);
  color: var(--el-color-primary);
}

.settings-card {
  border-radius: 12px;
  box-shadow: var(--el-box-shadow-light);
  min-height: 500px;
}

.section-title {
  font-size: 20px;
  font-weight: 600;
  color: var(--el-text-color-primary);
  margin-bottom: 24px;
}

.settings-content {
  padding: 24px;
}

.theme-selector {
  display: flex;
  gap: 8px;
  width: 100%;
}

.theme-selector :deep(.el-radio-button) {
  flex: 1;
}

.theme-selector :deep(.el-radio-button__inner) {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 12px 20px;
}

.theme-preview {
  margin-top: 32px;
  padding: 20px;
  background: var(--el-fill-color-lighter);
  border-radius: 12px;
}

.theme-preview h4 {
  font-size: 14px;
  font-weight: 600;
  color: var(--el-text-color-secondary);
  margin-bottom: 16px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.preview-card {
  background: var(--el-bg-color);
  border-radius: 8px;
  padding: 16px;
  border: 1px solid var(--el-border-color);
  box-shadow: var(--el-box-shadow-light);
}

.preview-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.preview-title {
  font-size: 16px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}

.preview-actions {
  display: flex;
  gap: 6px;
}

.preview-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--el-color-primary);
  opacity: 0.5;
}

.preview-content {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.preview-text {
  height: 12px;
  background: var(--el-fill-color);
  border-radius: 4px;
  animation: pulse 2s ease-in-out infinite;
}

.preview-text.short {
  width: 60%;
}

.notification-section {
  margin-bottom: 24px;
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

.settings-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding-top: 24px;
  border-top: 1px solid var(--el-border-color);
  margin-top: 24px;
}

@keyframes pulse {
  0%,
  100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
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
  .settings-view {
    padding: 16px;
  }

  .header-content h1 {
    font-size: 24px;
  }

  .settings-menu-card {
    margin-bottom: 24px;
  }

  .theme-selector {
    flex-direction: column;
  }

  .theme-selector :deep(.el-radio-button__inner) {
    justify-content: flex-start;
  }
}
</style>
