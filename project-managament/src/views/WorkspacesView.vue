<template>
  <div class="workspaces-view">
    <div class="page-header">
      <div class="header-content">
        <h1>Workspace'ler</h1>
        <p>Workspace'lerinizi oluşturun ve yönetin</p>
      </div>
      <div class="header-actions">
        <el-button type="primary" :icon="Plus" @click="showCreateDialog = true">
          Yeni Workspace
        </el-button>
      </div>
    </div>

    <!-- Workspaces Grid -->
    <div v-if="workspaces.length > 0" class="workspaces-grid">
      <div
        v-for="workspace in workspaces"
        :key="workspace.id"
        class="workspace-card"
        :style="{ borderColor: workspace.color }"
        @click="handleWorkspaceClick(workspace)"
      >
        <div class="workspace-header">
          <div class="workspace-icon" :style="{ backgroundColor: workspace.color }">
            <el-icon :size="24"><OfficeBuilding /></el-icon>
          </div>
          <el-dropdown trigger="click" @click.stop>
            <el-button class="action-btn" :icon="More" circle size="small" @click.stop />
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item @click="handleEdit(workspace)">
                  <el-icon><Edit /></el-icon>
                  Düzenle
                </el-dropdown-item>
                <el-dropdown-item @click="handleManageMembers(workspace)">
                  <el-icon><User /></el-icon>
                  Üyeleri Yönet
                </el-dropdown-item>
                <el-dropdown-item divided @click="handleDelete(workspace)">
                  <el-icon><Delete /></el-icon>
                  Sil
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>

        <div class="workspace-body">
          <h3 class="workspace-name">{{ workspace.name }}</h3>
          <p class="workspace-description">{{ workspace.description }}</p>

          <div class="workspace-stats">
            <div class="stat-item">
              <el-icon><Folder /></el-icon>
              <span>{{ getWorkspaceProjectCount(workspace.id) }} Proje</span>
            </div>
            <div class="stat-item">
              <el-icon><User /></el-icon>
              <span>{{ workspace.memberIds.length + 1 }} Üye</span>
            </div>
          </div>
        </div>

        <div class="workspace-footer">
          <div class="owner-info">
            <el-avatar :size="24" :src="getOwnerAvatar(workspace.ownerId)">
              {{ getOwnerName(workspace.ownerId).charAt(0) }}
            </el-avatar>
            <span>{{ getOwnerName(workspace.ownerId) }}</span>
          </div>
          <div class="workspace-date">
            {{ formatDate(workspace.createdAt) }}
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-state">
      <el-empty description="Henüz workspace oluşturulmamış">
        <el-button type="primary" :icon="Plus" @click="showCreateDialog = true">
          İlk Workspace'i Oluştur
        </el-button>
      </el-empty>
    </div>

    <!-- Create/Edit Dialog -->
    <el-dialog
      v-model="showCreateDialog"
      :title="editingWorkspace ? 'Workspace Düzenle' : 'Yeni Workspace'"
      width="600px"
      @close="handleDialogClose"
    >
      <el-form ref="formRef" :model="form" :rules="rules" label-position="top" size="large">
        <el-form-item label="Workspace Adı" prop="name">
          <el-input
            v-model="form.name"
            placeholder="Workspace adını girin"
            :prefix-icon="OfficeBuilding"
          />
        </el-form-item>

        <el-form-item label="Açıklama" prop="description">
          <el-input
            v-model="form.description"
            type="textarea"
            :rows="4"
            placeholder="Workspace açıklaması"
          />
        </el-form-item>

        <el-form-item label="Renk" prop="color">
          <div class="color-picker-wrapper">
            <el-color-picker v-model="form.color" show-alpha :predefine="predefineColors" />
            <span class="color-text">{{ form.color }}</span>
          </div>
        </el-form-item>

        <el-form-item v-if="isAdmin" label="Workspace Yöneticisi" prop="adminId">
          <el-select
            v-model="form.adminId"
            filterable
            placeholder="Workspace yöneticisi seçin"
            style="width: 100%"
          >
            <el-option
              v-for="user in workspaceAdmins"
              :key="user.id"
              :label="user.fullName"
              :value="user.id"
            >
              <div class="user-option">
                <el-avatar :size="24" :src="user.avatar">
                  {{ user.fullName.charAt(0) }}
                </el-avatar>
                <span>{{ user.fullName }}</span>
                <el-tag size="small" type="warning">Workspace Admin</el-tag>
              </div>
            </el-option>
          </el-select>
        </el-form-item>

        <el-form-item label="Üyeler" prop="memberIds">
          <el-select
            v-model="form.memberIds"
            multiple
            filterable
            placeholder="Üye seçin"
            style="width: 100%"
          >
            <el-option
              v-for="user in availableMembers"
              :key="user.id"
              :label="user.fullName"
              :value="user.id"
            >
              <div class="user-option">
                <el-avatar :size="24" :src="user.avatar">
                  {{ user.fullName.charAt(0) }}
                </el-avatar>
                <span>{{ user.fullName }}</span>
                <el-tag v-if="user.role === 'admin'" size="small" type="danger">Admin</el-tag>
                <el-tag v-else-if="user.role === 'workspaceAdmin'" size="small" type="warning"
                  >WS Admin</el-tag
                >
              </div>
            </el-option>
          </el-select>
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="showCreateDialog = false">İptal</el-button>
        <el-button type="primary" :loading="loading" @click="handleSubmit">
          {{ editingWorkspace ? 'Güncelle' : 'Oluştur' }}
        </el-button>
      </template>
    </el-dialog>

    <!-- Manage Members Dialog -->
    <el-dialog v-model="showMembersDialog" title="Üyeleri Yönet" width="700px">
      <div v-if="selectedWorkspace" class="members-management">
        <div class="members-header">
          <h4>Mevcut Üyeler ({{ selectedWorkspace.memberIds.length + 1 }})</h4>
          <el-button type="primary" size="small" :icon="Plus" @click="showAddMemberDialog = true">
            Üye Ekle
          </el-button>
        </div>

        <div class="members-list">
          <!-- Owner -->
          <div class="member-item owner">
            <div class="member-info">
              <el-avatar :size="40" :src="getOwnerAvatar(selectedWorkspace.ownerId)">
                {{ getOwnerName(selectedWorkspace.ownerId).charAt(0) }}
              </el-avatar>
              <div class="member-details">
                <div class="member-name">{{ getOwnerName(selectedWorkspace.ownerId) }}</div>
                <div class="member-email">{{ getOwnerEmail(selectedWorkspace.ownerId) }}</div>
              </div>
            </div>
            <el-tag type="warning">Sahip</el-tag>
          </div>

          <!-- Members -->
          <div v-for="memberId in selectedWorkspace.memberIds" :key="memberId" class="member-item">
            <div class="member-info">
              <el-avatar :size="40" :src="getUserAvatar(memberId)">
                {{ getUserName(memberId).charAt(0) }}
              </el-avatar>
              <div class="member-details">
                <div class="member-name">{{ getUserName(memberId) }}</div>
                <div class="member-email">{{ getUserEmail(memberId) }}</div>
              </div>
            </div>
            <el-button
              type="danger"
              size="small"
              :icon="Delete"
              @click="handleRemoveMember(memberId)"
            >
              Çıkar
            </el-button>
          </div>
        </div>
      </div>
    </el-dialog>

    <!-- Add Member Dialog -->
    <el-dialog v-model="showAddMemberDialog" title="Üye Ekle" width="500px">
      <el-select
        v-model="selectedUserId"
        filterable
        placeholder="Eklenecek kullanıcıyı seçin"
        style="width: 100%"
        size="large"
      >
        <el-option
          v-for="user in nonMemberUsers"
          :key="user.id"
          :label="user.fullName"
          :value="user.id"
        >
          <div class="user-option">
            <el-avatar :size="24" :src="user.avatar">
              {{ user.fullName.charAt(0) }}
            </el-avatar>
            <span>{{ user.fullName }}</span>
          </div>
        </el-option>
      </el-select>

      <template #footer>
        <el-button @click="showAddMemberDialog = false">İptal</el-button>
        <el-button type="primary" :loading="loading" @click="handleAddMember"> Ekle </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { Plus, OfficeBuilding, Folder, User, Edit, Delete, More } from '@element-plus/icons-vue'
import { useWorkspacesStore } from '@/stores/workspaces'
import { useProjectsStore } from '@/stores/projects'
import { useUsersStore } from '@/stores/users'
import { useAuthStore } from '@/stores/auth'
import type { Workspace, WorkspaceForm } from '@/types'

const router = useRouter()
const workspacesStore = useWorkspacesStore()
const projectsStore = useProjectsStore()
const usersStore = useUsersStore()
const authStore = useAuthStore()

// State
const showCreateDialog = ref(false)
const showMembersDialog = ref(false)
const showAddMemberDialog = ref(false)
const loading = ref(false)
const editingWorkspace = ref<Workspace | null>(null)
const selectedWorkspace = ref<Workspace | null>(null)
const selectedUserId = ref<string>('')
const formRef = ref<FormInstance>()

const form = reactive<any>({
  name: '',
  description: '',
  color: '#409EFF',
  adminId: '',
  memberIds: [],
})

const predefineColors = [
  '#409EFF',
  '#67C23A',
  '#E6A23C',
  '#F56C6C',
  '#909399',
  '#8E44AD',
  '#3498DB',
  '#E74C3C',
  '#1ABC9C',
  '#F39C12',
]

const rules: FormRules = {
  name: [
    { required: true, message: 'Workspace adı gereklidir', trigger: 'blur' },
    { min: 3, message: 'En az 3 karakter olmalıdır', trigger: 'blur' },
    { max: 50, message: 'En fazla 50 karakter olabilir', trigger: 'blur' },
  ],
  description: [
    { required: true, message: 'Açıklama gereklidir', trigger: 'blur' },
    { min: 10, message: 'En az 10 karakter olmalıdır', trigger: 'blur' },
  ],
  color: [{ required: true, message: 'Renk seçimi gereklidir', trigger: 'change' }],
}

// Computed
const isAdmin = computed(() => authStore.isAdmin)
const workspaces = computed(() => workspacesStore.myWorkspaces)
const workspaceAdmins = computed(() => {
  return usersStore.allUsers.filter((u) => u.role === 'workspaceAdmin')
})
const availableMembers = computed(() => {
  return usersStore.allUsers.filter((u) => u.id !== authStore.user?.id && u.role === 'member')
})
const nonMemberUsers = computed(() => {
  if (!selectedWorkspace.value) return []
  return usersStore.allUsers.filter(
    (u) =>
      u.id !== selectedWorkspace.value!.ownerId &&
      !selectedWorkspace.value!.memberIds.includes(u.id),
  )
})

// Methods
const getWorkspaceProjectCount = (workspaceId: string): number => {
  return projectsStore.getProjectsByWorkspace(workspaceId).length
}

const getOwnerName = (userId: string): string => {
  const user = usersStore.getUserById(userId)
  return user?.fullName || 'Bilinmeyen'
}

const getOwnerEmail = (userId: string): string => {
  const user = usersStore.getUserById(userId)
  return user?.email || ''
}

const getOwnerAvatar = (userId: string): string => {
  const user = usersStore.getUserById(userId)
  return user?.avatar || ''
}

const getUserName = (userId: string): string => {
  const user = usersStore.getUserById(userId)
  return user?.fullName || 'Bilinmeyen'
}

const getUserEmail = (userId: string): string => {
  const user = usersStore.getUserById(userId)
  return user?.email || ''
}

const getUserAvatar = (userId: string): string => {
  const user = usersStore.getUserById(userId)
  return user?.avatar || ''
}

const formatDate = (dateStr: string): string => {
  const date = new Date(dateStr)
  return date.toLocaleDateString('tr-TR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })
}

const handleWorkspaceClick = (workspace: Workspace) => {
  workspacesStore.setCurrentWorkspace(workspace.id)
  router.push('/projects')
}

const handleEdit = (workspace: Workspace) => {
  editingWorkspace.value = workspace
  form.name = workspace.name
  form.description = workspace.description
  form.color = workspace.color
  form.adminId = workspace.adminId || ''
  form.memberIds = [...workspace.memberIds]
  showCreateDialog.value = true
}

const handleDelete = async (workspace: Workspace) => {
  try {
    await ElMessageBox.confirm(
      `"${workspace.name}" workspace'ini silmek istediğinize emin misiniz? Bu işlem geri alınamaz.`,
      'Workspace Sil',
      {
        confirmButtonText: 'Evet, Sil',
        cancelButtonText: 'İptal',
        type: 'warning',
      },
    )

    loading.value = true
    const success = await workspacesStore.deleteWorkspace(workspace.id)
    if (success) {
      ElMessage.success('Workspace başarıyla silindi')
    }
  } catch {
    // User cancelled
  } finally {
    loading.value = false
  }
}

const handleManageMembers = async (workspace: Workspace) => {
  // Fetch latest workspace details (members) from backend before showing dialog
  const detailed = await workspacesStore.fetchWorkspaceDetail(workspace.id)
  selectedWorkspace.value = detailed || workspace
  showMembersDialog.value = true
}

const handleRemoveMember = async (userId: string) => {
  if (!selectedWorkspace.value) return

  try {
    await ElMessageBox.confirm(
      "Bu üyeyi workspace'ten çıkarmak istediğinize emin misiniz?",
      'Üyeyi Çıkar',
      {
        confirmButtonText: 'Evet',
        cancelButtonText: 'İptal',
        type: 'warning',
      },
    )

    loading.value = true
    const success = await workspacesStore.removeMember(selectedWorkspace.value.id, userId)
    if (success) {
      // Refresh selected workspace details from backend
      const detailed = await workspacesStore.fetchWorkspaceDetail(selectedWorkspace.value.id)
      selectedWorkspace.value = detailed || workspacesStore.getWorkspaceById(selectedWorkspace.value.id) || selectedWorkspace.value
    }
  } catch {
    // User cancelled
  } finally {
    loading.value = false
  }
}

const handleAddMember = async () => {
  if (!selectedUserId.value || !selectedWorkspace.value) {
    ElMessage.warning('Lütfen bir kullanıcı seçin')
    return
  }

  loading.value = true
  const success = await workspacesStore.addMember(selectedWorkspace.value.id, selectedUserId.value)

  if (success) {
    const detailed = await workspacesStore.fetchWorkspaceDetail(selectedWorkspace.value.id)
    selectedWorkspace.value = detailed || workspacesStore.getWorkspaceById(selectedWorkspace.value.id) || selectedWorkspace.value
    selectedUserId.value = ''
    showAddMemberDialog.value = false
  }
  loading.value = false
}

const handleSubmit = async () => {
  if (!formRef.value) return

  await formRef.value.validate(async (valid) => {
    if (!valid) return

    loading.value = true

    let success = false
    if (editingWorkspace.value) {
      success = await workspacesStore.updateWorkspace(editingWorkspace.value.id, form)
    } else {
      success = await workspacesStore.createWorkspace(form)
    }

    if (success) {
      showCreateDialog.value = false
      handleDialogClose()
    }

    loading.value = false
  })
}

const handleDialogClose = () => {
  editingWorkspace.value = null
  form.name = ''
  form.description = ''
  form.color = '#409EFF'
  form.adminId = ''
  form.memberIds = []
  formRef.value?.resetFields()
}

// Initialize
workspacesStore.fetchWorkspaces()
projectsStore.fetchProjects()
usersStore.fetchUsers()
</script>

<style scoped>
.workspaces-view {
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

.workspaces-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 24px;
}

.workspace-card {
  background: linear-gradient(135deg, #1e232d 0%, #2a3142 100%);
  border-radius: 16px;
  border: 2px solid rgba(64, 158, 255, 0.1);
  padding: 24px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.workspace-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 35px rgba(0, 0, 0, 0.3);
  border-color: currentColor;
}

.workspace-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.workspace-icon {
  width: 56px;
  height: 56px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.action-btn {
  background: rgba(45, 55, 72, 0.8);
  border: 1px solid rgba(64, 158, 255, 0.2);
  color: rgba(255, 255, 255, 0.8);
}

.action-btn:hover {
  background: rgba(64, 158, 255, 0.2);
  border-color: #409eff;
  color: #409eff;
}

.workspace-body {
  flex: 1;
}

.workspace-name {
  font-size: 20px;
  font-weight: 600;
  color: #ffffff;
  margin-bottom: 8px;
}

.workspace-description {
  font-size: 14px;
  color: rgba(255, 255, 255, 0.6);
  margin-bottom: 16px;
  line-height: 1.6;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.workspace-stats {
  display: flex;
  gap: 20px;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 14px;
  color: rgba(255, 255, 255, 0.7);
}

.workspace-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 16px;
  border-top: 1px solid rgba(64, 158, 255, 0.1);
}

.owner-info {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  color: rgba(255, 255, 255, 0.7);
}

.workspace-date {
  font-size: 12px;
  color: rgba(255, 255, 255, 0.5);
}

.empty-state {
  padding: 80px 20px;
  text-align: center;
}

.color-picker-wrapper {
  display: flex;
  align-items: center;
  gap: 12px;
}

.color-text {
  font-size: 14px;
  color: rgba(255, 255, 255, 0.8);
  font-family: monospace;
}

.user-option {
  display: flex;
  align-items: center;
  gap: 8px;
}

.members-management {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.members-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: 16px;
  border-bottom: 1px solid rgba(64, 158, 255, 0.1);
}

.members-header h4 {
  font-size: 16px;
  font-weight: 600;
  color: #ffffff;
  margin: 0;
}

.members-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-height: 400px;
  overflow-y: auto;
}

.member-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  background: rgba(45, 55, 72, 0.5);
  border-radius: 12px;
  border: 1px solid rgba(64, 158, 255, 0.1);
}

.member-item.owner {
  background: rgba(64, 158, 255, 0.1);
  border-color: rgba(64, 158, 255, 0.2);
}

.member-info {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
}

.member-details {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.member-name {
  font-size: 15px;
  font-weight: 600;
  color: #ffffff;
}

.member-email {
  font-size: 13px;
  color: rgba(255, 255, 255, 0.6);
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
  }

  .workspaces-grid {
    grid-template-columns: 1fr;
  }

  .workspace-stats {
    flex-direction: column;
    gap: 8px;
  }
}
</style>
