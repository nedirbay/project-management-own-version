import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { User, UserForm } from '@/types'
import { ElMessage } from 'element-plus'
import api from '@/api'

export const useUsersStore = defineStore('users', () => {
  // State
  const users = ref<User[]>([])
  const loading = ref(false)

  // Computed
  const allUsers = computed(() => users.value)
  const activeUsers = computed(() => users.value.filter((u) => u.role === 'member'))
  const adminUsers = computed(() => users.value.filter((u) => u.role === 'admin'))
  const userCount = computed(() => users.value.length)

  // Helper function to save to localStorage
  const saveToStorage = () => {
    localStorage.setItem('users', JSON.stringify(users.value))
  }

  // Actions
  const fetchUsers = async () => {
    loading.value = true
    try {
      const response = await api.get('/Users')
      users.value = response.data as User[]
      saveToStorage()
    } catch (error) {
      ElMessage.error('Ulanyjylar getirende säwlik ýüze çykdy')
      console.error('fetchUsers error:', error)
    } finally {
      loading.value = false
    }
  }

  // Return user from local cache. If you need to fetch from backend, use `fetchUsers()` first
  // or implement an async `fetchUserById` that calls the API.
  const getUserById = (id: string): User | undefined => {
    return users.value.find((u) => u.id === id)
  }

  const getUsersByIds = (ids: string[]): User[] => {
    return users.value.filter((u) => ids.includes(u.id))
  }

  const createUser = async (userForm: UserForm): Promise<boolean> => {
    loading.value = true
    try {
      const payload = {
        username: userForm.username,
        email: userForm.email,
        password: userForm.password,
        fullName: userForm.fullName,
        role: userForm.role,
      }

      const response = await api.post('/Users', payload)
      const newUser = response.data as User
      users.value.push(newUser)
      saveToStorage()
      ElMessage.success('Ulanyjy üstünlikli döredildi')
      return true
    } catch (error: any) {
      const message = error.response?.data?.Message || 'Ulanyjy döredilende säwlik ýüze çykdy'
      ElMessage.error(message)
      console.error('createUser error:', error)
      return false
    } finally {
      loading.value = false
    }
  }

  const updateUser = async (id: string, userForm: Partial<UserForm>): Promise<boolean> => {
    loading.value = true
    try {
      const payload = {
        username: userForm.username,
        email: userForm.email,
        fullName: userForm.fullName,
        role: userForm.role,
      }

      const response = await api.put(`/Users/${id}`, payload)
      const updatedUser = response.data as User
      const index = users.value.findIndex((u) => u.id === id)
      if (index !== -1) {
        users.value[index] = updatedUser
      }

      saveToStorage()
      ElMessage.success('Ulanyjy üstünlikli üýtgedildi')
      return true
    } catch (error: any) {
      const message = error.response?.data?.Message || 'Ulanyjy üýtgedilende säwlik ýüze çykdy'
      ElMessage.error(message)
      console.error('updateUser error:', error)
      return false
    } finally {
      loading.value = false
    }
  }

  const deleteUser = async (id: string): Promise<boolean> => {
    loading.value = true
    try {
      await api.delete(`/Users/${id}`)
      const index = users.value.findIndex((u) => u.id === id)
      if (index !== -1) {
        users.value.splice(index, 1)
      }

      saveToStorage()
      ElMessage.success('Ulanyjy üstünlikli pozuldy')
      return true
    } catch (error: any) {
      const message = error.response?.data?.Message || 'Ulanyjy pozulanda säwlik ýüze çykdy'
      ElMessage.error(message)
      console.error('deleteUser error:', error)
      return false
    } finally {
      loading.value = false
    }
  }

  const changePassword = async (id: string, newPassword: string): Promise<boolean> => {
    loading.value = true
    try {
      const payload = {
        currentPassword: '',
        newPassword,
      }

      await api.post(`/Users/${id}/change-password`, payload)
      ElMessage.success('Açar sözi üstünlikli üýtgedildi')
      return true
    } catch (error: any) {
      const message = error.response?.data?.Message || 'Açar sözi üýtgedilende säwlik ýüze çykdy'
      ElMessage.error(message)
      console.error('changePassword error:', error)
      return false
    } finally {
      loading.value = false
    }
  }

  // Initialize on store creation
  // initUsers()

  return {
    // State
    users,
    loading,
    // Computed
    allUsers,
    activeUsers,
    adminUsers,
    userCount,
    // Actions
    fetchUsers,
    getUserById,
    getUsersByIds,
    createUser,
    updateUser,
    deleteUser,
    changePassword,
  }
})
