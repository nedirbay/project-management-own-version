import { createI18n } from 'vue-i18n'
import en from './locales/en'
import tr from './locales/tr'

// Get saved locale from localStorage or default to Turkish
const savedLocale = localStorage.getItem('locale') || 'tr'

const i18n = createI18n({
  legacy: false, // Use Composition API mode
  locale: savedLocale,
  fallbackLocale: 'en',
  messages: {
    en,
    tr,
  },
  globalInjection: true, // Enable global $t
})

export default i18n

// Helper function to change locale
export function setLocale(locale: string) {
  i18n.global.locale.value = locale as any
  localStorage.setItem('locale', locale)
  document.documentElement.setAttribute('lang', locale)
}

// Helper function to get current locale
export function getLocale() {
  return i18n.global.locale.value
}

// Available locales
export const availableLocales = [
  { code: 'tr', name: 'Türkçe', flag: '🇹🇷' },
  { code: 'en', name: 'English', flag: '🇬🇧' },
]
