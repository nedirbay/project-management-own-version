import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import 'element-plus/theme-chalk/dark/css-vars.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'

import App from './App.vue'
import router from './router'
import i18n from './i18n'

const app = createApp(App)

// Register Element Plus Icons
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

const pinia = createPinia()

app.use(pinia)
app.use(router)
app.use(i18n)
app.use(ElementPlus, {
  size: 'default',
  zIndex: 3000,
})

app.mount('#app')

// Initialize theme and settings after app is mounted
import { useThemeStore } from './stores/theme'
import { useSettingsStore } from './stores/settings'

const themeStore = useThemeStore()
const settingsStore = useSettingsStore()

themeStore.initTheme()
settingsStore.initSettings()
