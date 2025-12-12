<template>
  <div class="login-layout">
    <div class="login-container">
      <div class="login-card">
        <div class="login-header">
          <div class="logo">
            <el-icon :size="48" color="#409EFF">
              <Management />
            </el-icon>
            <h1>Project Manager</h1>
            <p>Proýektleri aňsatlyk bilen dolandyryň</p>
          </div>
        </div>
        <div class="login-content">
          <router-view />
        </div>
        <div class="login-footer">
          <p>&copy; 2024 Project Manager. Tüm hakları saklıdır.</p>
        </div>
      </div>
    </div>
    <div class="login-background">
      <div class="bg-shape shape-1"></div>
      <div class="bg-shape shape-2"></div>
      <div class="bg-shape shape-3"></div>
      <div class="bg-gradient"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Management } from '@element-plus/icons-vue'
</script>

<style scoped>
.login-layout {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #0f1419;
  position: relative;
  overflow: hidden;
}

.login-container {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  padding: 20px;
  position: relative;
  z-index: 10;
}

.login-card {
  background: rgba(30, 35, 45, 0.95);
  backdrop-filter: blur(20px);
  border-radius: 24px;
  box-shadow: 0 25px 50px rgba(0, 0, 0, 0.5);
  padding: 0;
  width: 100%;
  max-width: 480px;
  overflow: hidden;
  border: 1px solid rgba(64, 158, 255, 0.2);
}

.login-header {
  background: linear-gradient(135deg, #1e232d 0%, #2a3142 100%);
  color: white;
  padding: 48px 40px 32px;
  text-align: center;
  border-bottom: 1px solid rgba(64, 158, 255, 0.1);
}

.logo {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}

.logo h1 {
  margin: 0;
  font-size: 32px;
  font-weight: 700;
  background: linear-gradient(135deg, #409eff 0%, #66b1ff 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.logo p {
  margin: 0;
  font-size: 15px;
  color: rgba(255, 255, 255, 0.7);
  font-weight: 400;
}

.login-content {
  padding: 48px 40px;
  background: rgba(30, 35, 45, 0.95);
}

.login-footer {
  padding: 24px 40px;
  text-align: center;
  background: rgba(20, 25, 35, 0.95);
  border-top: 1px solid rgba(64, 158, 255, 0.1);
}

.login-footer p {
  margin: 0;
  font-size: 13px;
  color: rgba(255, 255, 255, 0.5);
}

.login-background {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  z-index: 1;
}

.bg-gradient {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background:
    radial-gradient(circle at 20% 50%, rgba(64, 158, 255, 0.15) 0%, transparent 50%),
    radial-gradient(circle at 80% 80%, rgba(103, 194, 58, 0.1) 0%, transparent 50%);
}

.bg-shape {
  position: absolute;
  border-radius: 50%;
  background: rgba(64, 158, 255, 0.08);
  animation: float 8s ease-in-out infinite;
  filter: blur(40px);
}

.shape-1 {
  width: 300px;
  height: 300px;
  top: -100px;
  left: -100px;
  animation-delay: 0s;
}

.shape-2 {
  width: 400px;
  height: 400px;
  bottom: -150px;
  right: -150px;
  animation-delay: 2s;
  background: rgba(103, 194, 58, 0.08);
}

.shape-3 {
  width: 250px;
  height: 250px;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  animation-delay: 4s;
  background: rgba(230, 162, 60, 0.06);
}

@keyframes float {
  0%,
  100% {
    transform: translate(0, 0) rotate(0deg);
    opacity: 0.6;
  }
  25% {
    transform: translate(30px, -30px) rotate(90deg);
    opacity: 0.8;
  }
  50% {
    transform: translate(0, -50px) rotate(180deg);
    opacity: 0.6;
  }
  75% {
    transform: translate(-30px, -30px) rotate(270deg);
    opacity: 0.8;
  }
}

@media (max-width: 640px) {
  .login-container {
    padding: 16px;
  }

  .login-card {
    max-width: 100%;
    border-radius: 20px;
  }

  .login-header {
    padding: 36px 28px 24px;
  }

  .logo h1 {
    font-size: 28px;
  }

  .logo p {
    font-size: 14px;
  }

  .login-content {
    padding: 36px 28px;
  }

  .login-footer {
    padding: 20px 28px;
  }
}

@media (max-width: 360px) {
  .login-header {
    padding: 28px 20px 20px;
  }

  .login-content {
    padding: 28px 20px;
  }

  .login-footer {
    padding: 16px 20px;
  }
}
</style>
