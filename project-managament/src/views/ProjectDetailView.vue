<template>
  <div class="project-detail-view">
    <div class="page-header">
      <div class="header-content">
        <h1>Proje Detayı</h1>
        <p>Proje detay sayfası geliştiriliyor...</p>
      </div>
      <div class="header-actions">
        <el-button @click="$router.back()">Geri</el-button>
      </div>
    </div>

    <el-card class="content-card">
      <el-empty description="Proje detay sayfası geliştiriliyor">
        <el-icon :size="80" color="#409EFF">
          <Folder />
        </el-icon>
      </el-empty>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { Folder } from '@element-plus/icons-vue'
</script>

<style scoped>
.project-detail-view {
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

.content-card {
  border-radius: 16px;
  border: 1px solid rgba(64, 158, 255, 0.1);
  min-height: 400px;
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>
