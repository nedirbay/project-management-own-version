import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { User, AuthState } from '@/types'
import api from '@/api'

export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref<User | null>(null)
  const token = ref<string | null>(null)

  // Computed
  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const isAdmin = computed(() => user.value?.role === 'admin')
  const isWorkspaceAdmin = computed(() => user.value?.role === 'workspaceAdmin')
  const isMember = computed(() => user.value?.role === 'member')
  const canManageUsers = computed(() => user.value?.role === 'admin')
  const canManageWorkspaces = computed(
    () => user.value?.role === 'admin' || user.value?.role === 'workspaceAdmin',
  )
  const canManageProjects = computed(
    () => user.value?.role === 'admin' || user.value?.role === 'workspaceAdmin',
  )
  const canViewReports = computed(
    () => user.value?.role === 'admin' || user.value?.role === 'workspaceAdmin',
  )
  const currentUser = computed(() => user.value)

  // Initialize from localStorage
  const initAuth = () => {
    const storedToken = localStorage.getItem('token')
    const storedUser = localStorage.getItem('user')

    if (storedToken && storedUser) {
      token.value = storedToken
      user.value = JSON.parse(storedUser)
    }
  }

  // Actions
  const login = async (username: string, password: string): Promise<boolean> => {
    try {
      const res = await api.post('/auth/login', { username, password })
      const data = res.data
      const apiUser = data.user

      const mapRole = (r: string): User['role'] => {
        switch (r) {
          case 'Admin':
            return 'admin'
          case 'WorkspaceAdmin':
            return 'workspaceAdmin'
          case 'Member':
            return 'member'
          default:
            return 'member'
        }
      }

      const mappedUser: User = {
        id: apiUser.id,
        username: apiUser.username,
        email: apiUser.email,
        role: mapRole(apiUser.role),
        fullName: apiUser.fullName,
        avatar: apiUser.avatar,
        createdAt: apiUser.createdAt,
        updatedAt: new Date().toISOString(),
      }

      user.value = mappedUser
      token.value = data.token

      localStorage.setItem('token', data.token)
      localStorage.setItem('user', JSON.stringify(mappedUser))

      return true
    } catch (error) {
      console.error('Login error:', error)
      return false
    }
  }

  const register = async (
    username: string,
    email: string,
    password: string,
    fullName: string,
  ): Promise<boolean> => {
    try {
      const res = await api.post('/auth/register', { username, email, password, fullName })
      const data = res.data
      const apiUser = data.user

      const mapRole = (r: string): User['role'] => {
        switch (r) {
          case 'Admin':
            return 'admin'
          case 'WorkspaceAdmin':
            return 'workspaceAdmin'
          case 'Member':
            return 'member'
          default:
            return 'member'
        }
      }

      const mappedUser: User = {
        id: apiUser.id,
        username: apiUser.username,
        email: apiUser.email,
        role: mapRole(apiUser.role),
        fullName: apiUser.fullName,
        avatar: apiUser.avatar,
        createdAt: apiUser.createdAt,
        updatedAt: new Date().toISOString(),
      }

      user.value = mappedUser
      token.value = data.token

      localStorage.setItem('token', data.token)
      localStorage.setItem('user', JSON.stringify(mappedUser))

      return true
    } catch (error) {
      console.error('Register error:', error)
      return false
    }
  }

  const logout = () => {
    user.value = null
    token.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('user')
  }

  const updateUser = (updatedUser: Partial<User>) => {
    if (user.value) {
      user.value = { ...user.value, ...updatedUser }
      localStorage.setItem('user', JSON.stringify(user.value))
    }
  }

  // Initialize on store creation
  initAuth()

  return {
    // State
    user,
    token,
    // Computed
    isAuthenticated,
    isAdmin,
    isWorkspaceAdmin,
    isMember,
    canManageUsers,
    canManageWorkspaces,
    canManageProjects,
    canViewReports,
    currentUser,
    // Actions
    login,
    register,
    logout,
    updateUser,
    initAuth,
  }
})
