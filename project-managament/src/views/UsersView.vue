<template>
  <div class="users-view">
    <div class="page-header">
      <div class="header-content">
        <h1>Ulanyjylary dolandyrmak</h1>
        <p>Sistema ulanyjylaryny dolandyryň (Admin, Workspace Dolandyryjysy, Agza)</p>
      </div>
      <div class="header-actions">
        <el-button type="primary" :icon="Plus" @click="showCreateDialog = true">
          Täze ulanyjy
        </el-button>
      </div>
    </div>

    <!-- Stats Cards -->
    <el-row :gutter="24" class="stats-row">
      <el-col :xs="24" :sm="6">
        <div class="stat-card">
          <div class="stat-icon total">
            <el-icon :size="24"><User /></el-icon>
          </div>
          <div class="stat-content">
            <div class="stat-value">{{ usersStore.userCount }}</div>
            <div class="stat-label">Jemi ulanyjy</div>
          </div>
        </div>
      </el-col>
      <el-col :xs="24" :sm="6">
        <div class="stat-card">
          <div class="stat-icon admins">
            <el-icon :size="24"><UserFilled /></el-icon>
          </div>
          <div class="stat-content">
            <div class="stat-value">{{ adminCount }}</div>
            <div class="stat-label">Admin ulanyjylar</div>
          </div>
        </div>
      </el-col>
      <el-col :xs="24" :sm="6">
        <div class="stat-card">
          <div class="stat-icon workspace-admins">
            <el-icon :size="24"><OfficeBuilding /></el-icon>
          </div>
          <div class="stat-content">
            <div class="stat-value">{{ workspaceAdminCount }}</div>
            <div class="stat-label">Workspace Dolandyryjylary</div>
          </div>
        </div>
      </el-col>
      <el-col :xs="24" :sm="6">
        <div class="stat-card">
          <div class="stat-icon users">
            <el-icon :size="24"><Avatar /></el-icon>
          </div>
          <div class="stat-content">
            <div class="stat-value">{{ memberCount }}</div>
            <div class="stat-label">Agzalar</div>
          </div>
        </div>
      </el-col>
    </el-row>

    <!-- Users Table -->
    <el-card class="table-card">
      <template #header>
        <div class="card-header">
          <h3>Ulanyjylar Sanawy</h3>
          <el-input
            v-model="searchQuery"
            placeholder="Ulanyjy gözleg..."
            :prefix-icon="Search"
            style="width: 300px"
            clearable
          />
        </div>
      </template>

      <el-table
        :data="filteredUsers"
        style="width: 100%"
        :default-sort="{ prop: 'createdAt', order: 'descending' }"
      >
        <el-table-column label="Ulanyjy" min-width="200">
          <template #default="{ row }">
            <div class="user-cell">
              <el-avatar :size="40" :src="row.avatar">
                {{ row.fullName.charAt(0) }}
              </el-avatar>
              <div class="user-info">
                <div class="user-name">{{ row.fullName }}</div>
                <div class="user-username">@{{ row.username }}</div>
              </div>
            </div>
          </template>
        </el-table-column>

        <el-table-column label="E-poçta" prop="email" min-width="200" />

        <el-table-column label="Rol" width="150">
          <template #default="{ row }">
            <el-tag :type="getRoleType(row.role)">
              {{ getRoleText(row.role) }}
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column label="Döredilen" prop="createdAt" width="150">
          <template #default="{ row }">
            {{ formatDate(row.createdAt) }}
          </template>
        </el-table-column>

        <el-table-column label="Işler" width="180" align="center">
          <template #default="{ row }">
            <el-button type="primary" size="small" :icon="Edit" @click="handleEdit(row)" />
            <el-button type="warning" size="small" :icon="Key" @click="handleChangePassword(row)" />
            <el-button
              v-if="row.id !== 'admin-001'"
              type="danger"
              size="small"
              :icon="Delete"
              @click="handleDelete(row)"
            />
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- Create/Edit User Dialog -->
    <el-dialog
      v-model="showCreateDialog"
      :title="editingUser ? 'Ulanyjy Düzenle' : 'Täze Ulanyjy'"
      width="600px"
      @close="handleDialogClose"
    >
      <el-form ref="formRef" :model="form" :rules="rules" label-position="top" size="large">
        <el-form-item label="Ady we familiýasy" prop="fullName">
          <el-input v-model="form.fullName" placeholder="Ady we familiýasy giriziň" :prefix-icon="User" />
        </el-form-item>

        <el-form-item label="Ulanyjy ady" prop="username">
          <el-input
            v-model="form.username"
            placeholder="Ulanyjy ady giriziň"
            :prefix-icon="UserFilled"
          />
        </el-form-item>

        <el-form-item label="E-poçta" prop="email">
          <el-input
            v-model="form.email"
            type="email"
            placeholder="E-poçta adresi giriziň"
            :prefix-icon="Message"
          />
        </el-form-item>

        <el-form-item v-if="!editingUser" label="Açar sözi" prop="password">
          <el-input
            v-model="form.password"
            type="password"
            placeholder="Açar sözi giriziň"
            :prefix-icon="Lock"
            show-password
          />
        </el-form-item>

        <el-form-item label="Rol" prop="role">
          <el-select v-model="form.role" placeholder="Rol saýlaň" style="width: 100%">
            <el-option label="Admin" value="admin">
              <div class="role-option">
                <el-tag type="danger" size="small">Admin</el-tag>
                <span style="margin-left: 8px; font-size: 12px; color: var(--text-tertiary)"
                  >Tam yetki</span
                >
              </div>
            </el-option>
            <el-option label="Workspace dolandyryjy" value="workspaceAdmin">
              <div class="role-option">
                <el-tag type="warning" size="small">Workspace Admin</el-tag>
                <span style="margin-left: 8px; font-size: 12px; color: var(--text-tertiary)"
                  >Workspace dolandyryjylygy</span
                >
              </div>
            </el-option>
            <el-option label="Üye" value="member">
              <div class="role-option">
                <el-tag type="info" size="small">Agza</el-tag>
                <span style="margin-left: 8px; font-size: 12px; color: var(--text-tertiary)"
                  >Proýekt agzasy</span
                >
              </div>
            </el-option>
          </el-select>
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="showCreateDialog = false">Bes et</el-button>
        <el-button type="primary" :loading="loading" @click="handleSubmit">
          {{ editingUser ? 'Üýtget' : 'Döret' }}
        </el-button>
      </template>
    </el-dialog>

    <!-- Change Password Dialog -->
    <el-dialog v-model="showPasswordDialog" title="Açar sözi üýtgetmek" width="500px">
      <el-form ref="passwordFormRef" :model="passwordForm" 
      :rules="passwordRules"
      label-position="left"
      label-width="auto"
      size="large">
        <el-form-item label="Öňki açar sözi" prop="newPassword">
          <el-input
            v-model="passwordForm.newPassword"
            type="password"
            placeholder="Täze açar sözi girin"
            :prefix-icon="Lock"
            show-password
          />
        </el-form-item>

        <el-form-item label="Açar sözi gaýtadan giriň" prop="confirmPassword">
          <el-input
            v-model="passwordForm.confirmPassword"
            type="password"
            placeholder="Açar sözi gaýtadan giriň"
            :prefix-icon="Lock"
            show-password
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="showPasswordDialog = false">Bes et</el-button>
        <el-button type="primary" :loading="loading" @click="handlePasswordSubmit">
          Üýtgetmek
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import {
  Plus,
  User,
  UserFilled,
  Avatar,
  Edit,
  Delete,
  Search,
  Message,
  Lock,
  Key,
  OfficeBuilding,
} from '@element-plus/icons-vue'
import { useUsersStore } from '@/stores/users'
import type { User as UserType, UserForm } from '@/types'

const usersStore = useUsersStore()

// State
const showCreateDialog = ref(false)
const showPasswordDialog = ref(false)
const loading = ref(false)
const editingUser = ref<UserType | null>(null)
const selectedUser = ref<UserType | null>(null)
const searchQuery = ref('')
const formRef = ref<FormInstance>()
const passwordFormRef = ref<FormInstance>()

const form = reactive<UserForm>({
  username: '',
  email: '',
  password: '',
  fullName: '',
  role: 'member',
})

const passwordForm = reactive({
  newPassword: '',
  confirmPassword: '',
})

const validatePassword = (rule: any, value: any, callback: any) => {
  if (value === '') {
    callback(new Error('Açar sözi gereklidir'))
  } else if (value.length < 3) {
    callback(new Error('Açar sözi iň azyndan 3 harp bolmaly'))
  } else {
    callback()
  }
}

const validateConfirmPassword = (rule: any, value: any, callback: any) => {
  if (value === '') {
    callback(new Error('Açar sözi tassyklamasy gereklidir'))
  }
  else {
    callback()
  }
}

const rules: FormRules = {
  fullName: [
    { required: true, message: 'Doly ad gerekli', trigger: 'blur' },
    { min: 3, message: 'Iň azyndan 3 harp bolmaly', trigger: 'blur' },
  ],
  username: [
    { required: true, message: 'Ulanyjy ady gerekli', trigger: 'blur' },
    { min: 3, message: 'Iň azyndan 3 harp bolmaly', trigger: 'blur' },
    { max: 20, message: 'Iň köp 20 harp bolmaly', trigger: 'blur' },
  ],
  email: [
    { required: true, message: 'E-poçta gerekli', trigger: 'blur' },
    { type: 'email', message: 'Dogry e-poçta salgysyny giriziň', trigger: 'blur' },
  ],
  password: [{ validator: validatePassword, trigger: 'blur' }],
  role: [{ required: true, message: 'Rol saýlamasy gerekli', trigger: 'change' }],
}

const passwordRules: FormRules = {
  newPassword: [{ validator: validatePassword, trigger: 'blur' }],
  confirmPassword: [{ validator: validateConfirmPassword, trigger: 'blur' }],
}

// Computed
const filteredUsers = computed(() => {
  if (!searchQuery.value) return usersStore.allUsers

  const query = searchQuery.value.toLowerCase()
  return usersStore.allUsers.filter(
    (user) =>
      user.fullName.toLowerCase().includes(query) ||
      user.username.toLowerCase().includes(query) ||
      user.email.toLowerCase().includes(query),
  )
})

const adminCount = computed(() => usersStore.allUsers.filter((u) => u.role === 'admin').length)
const workspaceAdminCount = computed(
  () => usersStore.allUsers.filter((u) => u.role === 'workspaceAdmin').length,
)
const memberCount = computed(() => usersStore.allUsers.filter((u) => u.role === 'member').length)

// Methods
const formatDate = (dateStr: string): string => {
  const date = new Date(dateStr)
  return date.toLocaleDateString('tm-TM', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  })
}

const getRoleType = (role: string): string => {
  const types: Record<string, string> = {
    admin: 'danger',
    workspaceAdmin: 'warning',
    member: 'info',
  }
  return types[role] || 'info'
}

const getRoleText = (role: string): string => {
  const texts: Record<string, string> = {
    admin: 'Admin',
    workspaceAdmin: 'Workspace Admin',
    member: 'Üye',
  }
  return texts[role] || role
}

const handleEdit = (user: UserType) => {
  editingUser.value = user
  form.username = user.username
  form.email = user.email
  form.fullName = user.fullName
  form.role = user.role
  form.password = ''
  showCreateDialog.value = true
}

const handleDelete = async (user: UserType) => {
  try {
    await ElMessageBox.confirm(
      `"${user.fullName}" ulanyjysyny pozmak isleýändigiňize ynanýarsyňyzmy?`,
      'Ulanyjyny Pozmak',
      {
        confirmButtonText: 'Hawa, Poz',
        cancelButtonText: 'Ýatyr',
        type: 'warning',
      },
    )

    loading.value = true
    await usersStore.deleteUser(user.id)
    loading.value = false
  } catch {
    // User cancelled
    loading.value = false
  }
}

const handleChangePassword = (user: UserType) => {
  selectedUser.value = user
  passwordForm.newPassword = ''
  passwordForm.confirmPassword = ''
  showPasswordDialog.value = true
}

const handlePasswordSubmit = async () => {
  if (!passwordFormRef.value || !selectedUser.value) return

  await passwordFormRef.value.validate(async (valid) => {
    if (!valid) return

    loading.value = true
    const success = await usersStore.changePassword(
      selectedUser.value!.id,
      passwordForm.newPassword,
    )

    if (success) {
      showPasswordDialog.value = false
      passwordForm.newPassword = ''
      passwordForm.confirmPassword = ''
    }

    loading.value = false
  })
}

const handleSubmit = async () => {
  if (!formRef.value) return

  await formRef.value.validate(async (valid) => {
    if (!valid) return

    loading.value = true

    let success = false
    if (editingUser.value) {
      const updateData: Partial<UserForm> = {
        username: form.username,
        email: form.email,
        fullName: form.fullName,
        role: form.role,
      }
      success = await usersStore.updateUser(editingUser.value.id, updateData)
    } else {
      success = await usersStore.createUser(form)
    }

    if (success) {
      showCreateDialog.value = false
      handleDialogClose()
    }

    loading.value = false
  })
}

const handleDialogClose = () => {
  editingUser.value = null
  form.username = ''
  form.email = ''
  form.password = ''
  form.fullName = ''
  form.role = 'member'
  formRef.value?.resetFields()
}

// Initialize
usersStore.fetchUsers()
</script>

<style scoped>
.users-view {
  animation: fadeIn 0.3s ease;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 32px;
  padding-bottom: 20px;
  border-bottom: 1px solid rgba(64, 158, 255, 0.1);
}

.header-content h1 {
  font-size: 32px;
  font-weight: 700;
  color: #ffffff;
  margin-bottom: 8px;
}

.header-content p {
  font-size: 16px;
  color: rgba(255, 255, 255, 0.6);
  margin: 0;
}

.stats-row {
  margin-bottom: 32px;
}

.stat-card {
  background: linear-gradient(135deg, #1e232d 0%, #2a3142 100%);
  border-radius: 16px;
  padding: 24px;
  display: flex;
  align-items: center;
  gap: 16px;
  border: 1px solid rgba(64, 158, 255, 0.1);
  transition: all 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.3);
}

.stat-icon {
  width: 56px;
  height: 56px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 12px;
  color: white;
}

.stat-icon.total {
  background: linear-gradient(135deg, #409eff, #3a8ee6);
}

.stat-icon.admins {
  background: linear-gradient(135deg, #e6a23c, #cf9236);
}

.stat-icon.workspace-admins {
  background: linear-gradient(135deg, #e6a23c, #cf9236);
}

.stat-icon.users {
  background: linear-gradient(135deg, #67c23a, #5daf34);
}

.role-option {
  display: flex;
  align-items: center;
}

.stat-content {
  flex: 1;
}

.stat-value {
  font-size: 28px;
  font-weight: 700;
  color: #ffffff;
  line-height: 1;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 14px;
  color: rgba(255, 255, 255, 0.6);
}

.table-card {
  border-radius: 16px;
  border: 1px solid rgba(64, 158, 255, 0.1);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-header h3 {
  font-size: 18px;
  font-weight: 600;
  color: #ffffff;
  margin: 0;
}

.user-cell {
  display: flex;
  align-items: center;
  gap: 12px;
}

.user-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.user-name {
  font-size: 15px;
  font-weight: 600;
  color: #ffffff;
}

.user-username {
  font-size: 13px;
  color: rgba(255, 255, 255, 0.6);
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
  }

  .card-header {
    flex-direction: column;
    gap: 16px;
    align-items: flex-start;
  }

  .card-header .el-input {
    width: 100% !important;
  }
}
</style>
