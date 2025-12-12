<template>
  <div class="team-view">
    <div class="page-header">
      <h1>Team</h1>
      <p>Collaborate with your team members and manage permissions.</p>
    </div>

    <div class="page-content">
      <el-card class="content-card">
        <template #header>
          <div class="card-header">
            <span>Team Members</span>
            <el-button type="primary" :icon="Plus">
              Invite Member
            </el-button>
          </div>
        </template>

        <div class="placeholder-content">
          <el-empty description="Team management functionality coming soon">
            <template #image>
              <el-icon size="80" color="#409EFF">
                <User />
              </el-icon>
            </template>
            <el-button type="primary" plain>
              Invite Team Members
            </el-button>
          </el-empty>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Plus, User } from '@element-plus/icons-vue'
</script>

<style scoped>
.team-view {
  padding: 0;
}

.page-header {
  margin-bottom: 24px;
  padding-bottom: 16px;
  border-bottom: 1px solid #ebeef5;
}

.page-header h1 {
  font-size: 28px;
  font-weight: 600;
  color: #303133;
  margin-bottom: 8px;
}

.page-header p {
  font-size: 16px;
  color: #606266;
  margin: 0;
}

.page-content {
  min-height: 400px;
}

.content-card {
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-weight: 600;
  font-size: 16px;
}

.placeholder-content {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 300px;
}
</style>
