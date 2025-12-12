import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'
import type { Theme } from '@/types'

export const useThemeStore = defineStore('theme', () => {
  // State
  const theme = ref<Theme>('dark')
  const systemPreference = ref(false)

  // Computed
  const isDark = computed(() => {
    if (systemPreference.value) {
      return window.matchMedia('(prefers-color-scheme: dark)').matches
    }
    return theme.value === 'dark'
  })

  const currentTheme = computed(() => {
    if (systemPreference.value) {
      return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
    }
    return theme.value
  })

  // Initialize from localStorage
  const initTheme = () => {
    const stored = localStorage.getItem('theme')
    const storedSystemPref = localStorage.getItem('systemPreference')

    if (stored) {
      theme.value = stored as Theme
    }

    if (storedSystemPref) {
      systemPreference.value = storedSystemPref === 'true'
    }

    applyTheme()
  }

  // Apply theme to document
  const applyTheme = () => {
    const root = document.documentElement

    if (isDark.value) {
      root.classList.add('dark')
      root.classList.remove('light')
    } else {
      root.classList.add('light')
      root.classList.remove('dark')
    }

    // Update meta theme color
    const metaThemeColor = document.querySelector('meta[name="theme-color"]')
    if (metaThemeColor) {
      metaThemeColor.setAttribute('content', isDark.value ? '#0f1419' : '#ffffff')
    }
  }

  // Actions
  const setTheme = (newTheme: Theme) => {
    theme.value = newTheme
    systemPreference.value = false
    localStorage.setItem('theme', newTheme)
    localStorage.setItem('systemPreference', 'false')
    applyTheme()
  }

  const toggleTheme = () => {
    const newTheme = theme.value === 'dark' ? 'light' : 'dark'
    setTheme(newTheme)
  }

  const setSystemPreference = (value: boolean) => {
    systemPreference.value = value
    localStorage.setItem('systemPreference', String(value))
    applyTheme()
  }

  // Watch for system preference changes
  if (typeof window !== 'undefined') {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
    mediaQuery.addEventListener('change', () => {
      if (systemPreference.value) {
        applyTheme()
      }
    })
  }

  // Watch theme changes
  watch([theme, systemPreference], () => {
    applyTheme()
  })

  // Initialize on store creation
  initTheme()

  return {
    // State
    theme,
    systemPreference,
    // Computed
    isDark,
    currentTheme,
    // Actions
    setTheme,
    toggleTheme,
    setSystemPreference,
    initTheme,
  }
})
