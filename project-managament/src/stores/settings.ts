import { defineStore } from 'pinia'
import { ref, watch } from 'vue'
import { setLocale } from '@/i18n'

export interface UserSettings {
  theme: 'light' | 'dark' | 'system'
  language: string
  timezone: string
  dateFormat: string
  timeFormat: '12h' | '24h'
  notifications: {
    email: boolean
    push: boolean
    taskAssigned: boolean
    taskCompleted: boolean
    projectUpdated: boolean
    reportReminder: boolean
  }
}

export const useSettingsStore = defineStore('settings', () => {
  // State
  const settings = ref<UserSettings>({
    theme: 'dark',
    language: 'tr',
    timezone: 'Europe/Istanbul',
    dateFormat: 'DD/MM/YYYY',
    timeFormat: '24h',
    notifications: {
      email: true,
      push: true,
      taskAssigned: true,
      taskCompleted: true,
      projectUpdated: true,
      reportReminder: true,
    },
  })

  const isLoading = ref(false)

  // Initialize settings from localStorage
  const initSettings = () => {
    const savedSettings = localStorage.getItem('userSettings')
    if (savedSettings) {
      try {
        const parsed = JSON.parse(savedSettings)
        settings.value = { ...settings.value, ...parsed }

        // Apply language
        if (parsed.language) {
          setLocale(parsed.language)
        }
      } catch (error) {
        console.error('Failed to parse saved settings:', error)
      }
    }
  }

  // Watch for settings changes and save to localStorage
  watch(
    settings,
    (newSettings) => {
      localStorage.setItem('userSettings', JSON.stringify(newSettings))
    },
    { deep: true }
  )

  // Actions
  const updateTheme = (theme: 'light' | 'dark' | 'system') => {
    settings.value.theme = theme
    applyTheme(theme)
  }

  const updateLanguage = (language: string) => {
    settings.value.language = language
    setLocale(language)
  }

  const updateTimezone = (timezone: string) => {
    settings.value.timezone = timezone
  }

  const updateDateFormat = (format: string) => {
    settings.value.dateFormat = format
  }

  const updateTimeFormat = (format: '12h' | '24h') => {
    settings.value.timeFormat = format
  }

  const updateNotifications = (notifications: Partial<UserSettings['notifications']>) => {
    settings.value.notifications = {
      ...settings.value.notifications,
      ...notifications,
    }
  }

  const updateSettings = (newSettings: Partial<UserSettings>) => {
    isLoading.value = true
    try {
      if (newSettings.theme) {
        updateTheme(newSettings.theme)
      }
      if (newSettings.language) {
        updateLanguage(newSettings.language)
      }
      if (newSettings.timezone) {
        updateTimezone(newSettings.timezone)
      }
      if (newSettings.dateFormat) {
        updateDateFormat(newSettings.dateFormat)
      }
      if (newSettings.timeFormat) {
        updateTimeFormat(newSettings.timeFormat)
      }
      if (newSettings.notifications) {
        updateNotifications(newSettings.notifications)
      }
    } finally {
      isLoading.value = false
    }
  }

  const resetSettings = () => {
    settings.value = {
      theme: 'dark',
      language: 'tr',
      timezone: 'Europe/Istanbul',
      dateFormat: 'DD/MM/YYYY',
      timeFormat: '24h',
      notifications: {
        email: true,
        push: true,
        taskAssigned: true,
        taskCompleted: true,
        projectUpdated: true,
        reportReminder: true,
      },
    }
    localStorage.removeItem('userSettings')
  }

  // Helper to apply theme
  const applyTheme = (theme: 'light' | 'dark' | 'system') => {
    const html = document.documentElement

    if (theme === 'system') {
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches
      html.classList.toggle('dark', prefersDark)
    } else {
      html.classList.toggle('dark', theme === 'dark')
    }
  }

  // Timezones list
  const availableTimezones = [
    { value: 'Europe/Istanbul', label: 'Istanbul (GMT+3)' },
    { value: 'Europe/London', label: 'London (GMT+0)' },
    { value: 'Europe/Paris', label: 'Paris (GMT+1)' },
    { value: 'Europe/Berlin', label: 'Berlin (GMT+1)' },
    { value: 'America/New_York', label: 'New York (GMT-5)' },
    { value: 'America/Los_Angeles', label: 'Los Angeles (GMT-8)' },
    { value: 'America/Chicago', label: 'Chicago (GMT-6)' },
    { value: 'Asia/Tokyo', label: 'Tokyo (GMT+9)' },
    { value: 'Asia/Shanghai', label: 'Shanghai (GMT+8)' },
    { value: 'Asia/Dubai', label: 'Dubai (GMT+4)' },
    { value: 'Australia/Sydney', label: 'Sydney (GMT+10)' },
  ]

  const dateFormats = [
    { value: 'DD/MM/YYYY', label: 'DD/MM/YYYY (31/12/2023)' },
    { value: 'MM/DD/YYYY', label: 'MM/DD/YYYY (12/31/2023)' },
    { value: 'YYYY-MM-DD', label: 'YYYY-MM-DD (2023-12-31)' },
    { value: 'DD.MM.YYYY', label: 'DD.MM.YYYY (31.12.2023)' },
  ]

  return {
    // State
    settings,
    isLoading,
    availableTimezones,
    dateFormats,

    // Actions
    initSettings,
    updateSettings,
    updateTheme,
    updateLanguage,
    updateTimezone,
    updateDateFormat,
    updateTimeFormat,
    updateNotifications,
    resetSettings,
  }
})
