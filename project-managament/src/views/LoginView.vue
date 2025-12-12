<template>
  <div class="login-view">
    <el-form
      ref="loginFormRef"
      :model="loginForm"
      :rules="loginRules"
      class="login-form"
      size="large"
      @keyup.enter="handleLogin"
    >
      <div class="form-title">
        <h2>Giriş Yap</h2>
        <p>Hesabınıza giriş yapmak için bilgilerinizi girin</p>
      </div>

      <el-form-item prop="username">
        <el-input
          v-model="loginForm.username"
          placeholder="Kullanıcı adı"
          :prefix-icon="User"
          clearable
        />
      </el-form-item>

      <el-form-item prop="password">
        <el-input
          v-model="loginForm.password"
          type="password"
          placeholder="Şifre"
          :prefix-icon="Lock"
          show-password
          clearable
        />
      </el-form-item>

      <el-form-item>
        <div class="form-options">
          <el-checkbox v-model="loginForm.rememberMe" label="Beni hatırla" />
        </div>
      </el-form-item>

      <el-form-item>
        <el-button type="primary" class="login-button" :loading="loading" @click="handleLogin">
          {{ loading ? 'Giriş yapılıyor...' : 'Giriş Yap' }}
        </el-button>
      </el-form-item>

    </el-form>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElNotification } from 'element-plus'
import { User, Lock } from '@element-plus/icons-vue'
import type { FormInstance, FormRules } from 'element-plus'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()
const loginFormRef = ref<FormInstance>()
const loading = ref(false)

const loginForm = reactive({
  username: '',
  password: '',
  rememberMe: false,
})

const loginRules: FormRules = {
  username: [
    { required: true, message: 'Lütfen kullanıcı adınızı girin', trigger: 'blur' },
    { min: 3, message: 'Kullanıcı adı en az 3 karakter olmalıdır', trigger: 'blur' },
  ],
  password: [
    { required: true, message: 'Lütfen şifrenizi girin', trigger: 'blur' },
    { min: 3, message: 'Şifre en az 3 karakter olmalıdır', trigger: 'blur' },
  ],
}

const handleLogin = async () => {
  if (!loginFormRef.value) return

  await loginFormRef.value.validate(async (valid) => {
    if (valid) {
      loading.value = true

      try {
        // Simulate network delay
        await new Promise((resolve) => setTimeout(resolve, 800))

        const success = await authStore.login(loginForm.username, loginForm.password)

        if (success) {
          ElNotification({
            title: 'Giriş Başarılı',
            message: `Hoş geldiniz, ${authStore.user?.fullName || loginForm.username}!`,
            type: 'success',
            duration: 3000,
          })

          // Redirect to dashboard
          setTimeout(() => {
            router.push('/')
          }, 500)
        } else {
          ElMessage.error('Kullanıcı adı veya şifre hatalı!')
        }
      } catch (error) {
        ElMessage.error('Giriş yapılırken bir hata oluştu. Lütfen tekrar deneyin.')
        console.error('Login error:', error)
      } finally {
        loading.value = false
      }
    }
  })
}
</script>

<style scoped>
.login-view {
  width: 100%;
}

.login-form {
  width: 100%;
  max-width: 520px; /* увеличить форму, но сохранить отзывчивость */
  margin: 0 auto; /* отцентрировать форму */
}

.form-title {
  text-align: center;
  margin-bottom: 32px;
}

.form-title h2 {
  margin: 0 0 8px;
  font-size: 28px;
  font-weight: 700;
  color: #ffffff;
}

.form-title p {
  margin: 0;
  font-size: 15px;
  color: rgba(255, 255, 255, 0.6);
}

.login-form :deep(.el-form-item) {
  margin-bottom: 24px;
}

.login-form :deep(.el-input__wrapper) {
  background: rgba(45, 55, 72, 0.8);
  border: 1px solid rgba(64, 158, 255, 0.2);
  box-shadow: none;
  transition: all 0.3s ease;
  border-radius: 12px;
  padding: 12px 16px;
  height: 48px;
}

.login-form :deep(.el-input__wrapper:hover) {
  border-color: rgba(64, 158, 255, 0.5);
  background: rgba(45, 55, 72, 1);
}

.login-form :deep(.el-input__wrapper.is-focus) {
  border-color: #409eff;
  background: rgba(45, 55, 72, 1);
  box-shadow: 0 0 0 2px rgba(64, 158, 255, 0.2);
}

.login-form :deep(.el-input__inner) {
  color: #ffffff;
  font-size: 15px;
}

.login-form :deep(.el-input__inner::placeholder) {
  color: rgba(255, 255, 255, 0.4);
}

.login-form :deep(.el-input__prefix) {
  color: rgba(255, 255, 255, 0.5);
}

.login-form :deep(.el-input__suffix) {
  color: rgba(255, 255, 255, 0.5);
}

.login-form :deep(.el-checkbox) {
  color: rgba(255, 255, 255, 0.8);
}

.login-form :deep(.el-checkbox__input.is-checked .el-checkbox__inner) {
  background-color: #409eff;
  border-color: #409eff;
}

.login-form :deep(.el-checkbox__label) {
  color: rgba(255, 255, 255, 0.8);
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
  font-size: 14px;
}

.login-button {
  width: 100%;
  height: 52px;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 600;
  background: linear-gradient(135deg, #409eff 0%, #3a8ee6 100%);
  border: none;
  transition: all 0.3s ease;
  margin-top: 8px;
}

.login-button:hover {
  background: linear-gradient(135deg, #3a8ee6 0%, #337ecc 100%);
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(64, 158, 255, 0.4);
}

.login-button:active {
  transform: translateY(0);
}

.login-hint {
  margin-top: 24px;
}

.login-hint :deep(.el-alert) {
  background: rgba(45, 55, 72, 0.6);
  border: 1px solid rgba(64, 158, 255, 0.2);
  border-radius: 12px;
}

.login-hint :deep(.el-alert__title) {
  color: #66b1ff;
  font-size: 14px;
  font-weight: 600;
  margin-bottom: 8px;
}

.login-hint :deep(.el-alert__content) {
  color: rgba(255, 255, 255, 0.7);
}

.login-hint :deep(.el-alert__icon) {
  color: #66b1ff;
}

.login-hint p {
  margin: 4px 0;
  font-size: 13px;
  color: rgba(255, 255, 255, 0.7);
}

.login-hint strong {
  color: rgba(255, 255, 255, 0.9);
  font-weight: 600;
}

@media (max-width: 640px) {
  .form-title h2 {
    font-size: 24px;
  }

  .form-title p {
    font-size: 14px;
  }

  .login-form :deep(.el-input__wrapper) {
    height: 44px;
  }

  .login-button {
    height: 48px;
  }
}
</style>
