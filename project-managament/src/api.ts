import axios from 'axios'
import { ElMessage } from 'element-plus'

// const API_BASE_URL = (import.meta as any).env?.VITE_API_BASE_URL || 'http://localhost:7001/api'
const API_BASE_URL = import.meta.env.VITE_API_URL+"/api";

export const api = axios.create({
  baseURL: API_BASE_URL,
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  console.log('token:', token)
  if (token) {
    config.headers = config.headers || {}
    config.headers['Authorization'] = `Bearer ${token}`
  }
  return config
})

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status
    if (status === 401) {
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      window.location.href = '/login'
    }

    if (status === 403) {
      ElMessage.error('Yetkiniz yok. Bu işlemi gerçekleştirmek için uygun role sahip olmalısınız.')
    }

    return Promise.reject(error)
  },
)

export default api