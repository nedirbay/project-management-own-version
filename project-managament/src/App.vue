<template>
  <router-view />
</template>

<script setup lang="ts">
// App.vue serves as the root component
// All layout logic is handled by individual layout components
</script>

<style>
/* CSS Reset and Base Styles */
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

html,
body {
  height: 100%;
  font-family:
    -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  line-height: 1.6;
  color: #ffffff;
  background: #0f1419;
  overflow-x: hidden;
}

#app {
  min-height: 100vh;
  background: #0f1419;
}

/* Color Variables */
:root {
  --color-primary: #409eff;
  --color-success: #67c23a;
  --color-warning: #e6a23c;
  --color-danger: #f56c6c;
  --color-info: #909399;
}

/* Dark Theme */
:root.dark,
:root:not(.light) {
  --bg-primary: #0f1419;
  --bg-secondary: #1e232d;
  --bg-tertiary: #2a3142;
  --bg-hover: rgba(64, 158, 255, 0.1);

  --text-primary: #ffffff;
  --text-secondary: rgba(255, 255, 255, 0.7);
  --text-tertiary: rgba(255, 255, 255, 0.5);

  --border-color: rgba(64, 158, 255, 0.1);
  --border-color-hover: rgba(64, 158, 255, 0.3);

  --shadow-sm: 0 2px 12px rgba(0, 0, 0, 0.3);
  --shadow-md: 0 4px 20px rgba(0, 0, 0, 0.4);
  --shadow-lg: 0 8px 30px rgba(0, 0, 0, 0.5);
}

/* Light Theme */
:root.light {
  --bg-primary: #ffffff;
  --bg-secondary: #f5f7fa;
  --bg-tertiary: #ebeef5;
  --bg-hover: rgba(64, 158, 255, 0.05);

  --text-primary: #303133;
  --text-secondary: #606266;
  --text-tertiary: #909399;

  --border-color: #dcdfe6;
  --border-color-hover: #c0c4cc;

  --shadow-sm: 0 2px 12px rgba(0, 0, 0, 0.1);
  --shadow-md: 0 4px 20px rgba(0, 0, 0, 0.12);
  --shadow-lg: 0 8px 30px rgba(0, 0, 0, 0.15);
}

/* Global Element Plus Theme Overrides */
.el-message {
  min-width: 300px;
}

.el-notification {
  min-width: 330px;
}

/* Dark Theme Overrides */
.dark .el-message,
:root:not(.light) .el-message {
  background-color: #2a3142 !important;
  border: 1px solid rgba(64, 158, 255, 0.2) !important;
}

.dark .el-message__content,
:root:not(.light) .el-message__content {
  color: #ffffff !important;
}

.dark .el-notification,
:root:not(.light) .el-notification {
  background-color: #2a3142 !important;
  border: 1px solid rgba(64, 158, 255, 0.2) !important;
}

.dark .el-notification__title,
:root:not(.light) .el-notification__title {
  color: #ffffff !important;
}

.dark .el-notification__content,
:root:not(.light) .el-notification__content {
  color: rgba(255, 255, 255, 0.8) !important;
}

.dark .el-dialog,
:root:not(.light) .el-dialog {
  background-color: #1e232d !important;
  border: 1px solid rgba(64, 158, 255, 0.2) !important;
}

.dark .el-dialog__header,
:root:not(.light) .el-dialog__header {
  border-bottom: 1px solid rgba(64, 158, 255, 0.1) !important;
}

.dark .el-dialog__title,
:root:not(.light) .el-dialog__title {
  color: #ffffff !important;
}

.dark .el-dialog__close,
:root:not(.light) .el-dialog__close {
  color: rgba(255, 255, 255, 0.7) !important;
}

.dark .el-dialog__close:hover,
:root:not(.light) .el-dialog__close:hover {
  color: #409eff !important;
}

.dark .el-drawer,
:root:not(.light) .el-drawer {
  background-color: #1e232d !important;
}

.dark .el-drawer__header,
:root:not(.light) .el-drawer__header {
  color: #ffffff !important;
  border-bottom: 1px solid rgba(64, 158, 255, 0.1) !important;
}

.dark .el-card,
:root:not(.light) .el-card {
  background-color: #1e232d !important;
  border: 1px solid rgba(64, 158, 255, 0.1) !important;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.3) !important;
}

.dark .el-card__header,
:root:not(.light) .el-card__header {
  background-color: rgba(30, 35, 45, 0.5) !important;
  border-bottom: 1px solid rgba(64, 158, 255, 0.1) !important;
  color: #ffffff !important;
}

.dark .el-table,
:root:not(.light) .el-table {
  background-color: #1e232d !important;
  color: #ffffff !important;
}

.dark .el-table th.el-table__cell,
:root:not(.light) .el-table th.el-table__cell {
  background-color: #2a3142 !important;
  color: #ffffff !important;
  border-bottom: 1px solid rgba(64, 158, 255, 0.1) !important;
}

.dark .el-table tr,
:root:not(.light) .el-table tr {
  background-color: #1e232d !important;
}

.dark .el-table td.el-table__cell,
:root:not(.light) .el-table td.el-table__cell {
  border-bottom: 1px solid rgba(64, 158, 255, 0.05) !important;
  color: rgba(255, 255, 255, 0.8) !important;
}

.dark .el-table__row:hover,
:root:not(.light) .el-table__row:hover {
  background-color: rgba(64, 158, 255, 0.05) !important;
}

.dark .el-table--striped .el-table__body tr.el-table__row--striped td.el-table__cell,
:root:not(.light) .el-table--striped .el-table__body tr.el-table__row--striped td.el-table__cell {
  background-color: rgba(42, 49, 66, 0.5) !important;
}

.dark .el-input__wrapper,
:root:not(.light) .el-input__wrapper {
  background-color: rgba(45, 55, 72, 0.8) !important;
  border: 1px solid rgba(64, 158, 255, 0.2) !important;
  box-shadow: none !important;
}

.dark .el-input__wrapper:hover,
:root:not(.light) .el-input__wrapper:hover {
  border-color: rgba(64, 158, 255, 0.5) !important;
}

.dark .el-input__wrapper.is-focus,
:root:not(.light) .el-input__wrapper.is-focus {
  border-color: #409eff !important;
  box-shadow: 0 0 0 2px rgba(64, 158, 255, 0.2) !important;
}

.dark .el-input__inner,
:root:not(.light) .el-input__inner {
  color: #ffffff !important;
}

.dark .el-input__inner::placeholder,
:root:not(.light) .el-input__inner::placeholder {
  color: rgba(255, 255, 255, 0.4) !important;
}

.dark .el-input__prefix,
.dark .el-input__suffix,
:root:not(.light) .el-input__prefix,
:root:not(.light) .el-input__suffix {
  color: rgba(255, 255, 255, 0.5) !important;
}

.dark .el-textarea__inner,
:root:not(.light) .el-textarea__inner {
  background-color: rgba(45, 55, 72, 0.8) !important;
  border: 1px solid rgba(64, 158, 255, 0.2) !important;
  color: #ffffff !important;
}

.dark .el-textarea__inner:hover,
:root:not(.light) .el-textarea__inner:hover {
  border-color: rgba(64, 158, 255, 0.5) !important;
}

.dark .el-textarea__inner:focus,
:root:not(.light) .el-textarea__inner:focus {
  border-color: #409eff !important;
}

.dark .el-select-dropdown,
:root:not(.light) .el-select-dropdown {
  background-color: #2a3142 !important;
  border: 1px solid rgba(64, 158, 255, 0.2) !important;
}

.dark .el-select-dropdown__item,
:root:not(.light) .el-select-dropdown__item {
  color: rgba(255, 255, 255, 0.8) !important;
}

.dark .el-select-dropdown__item:hover,
:root:not(.light) .el-select-dropdown__item:hover {
  background-color: rgba(64, 158, 255, 0.1) !important;
  color: #409eff !important;
}

.dark .el-select-dropdown__item.selected,
:root:not(.light) .el-select-dropdown__item.selected {
  color: #409eff !important;
  font-weight: 600;
}

.dark .el-popper,
:root:not(.light) .el-popper {
  background-color: #2a3142 !important;
  border: 1px solid rgba(64, 158, 255, 0.2) !important;
  color: #ffffff !important;
}

.dark .el-dropdown-menu,
:root:not(.light) .el-dropdown-menu {
  background-color: #2a3142 !important;
  border: 1px solid rgba(64, 158, 255, 0.2) !important;
}

.dark .el-dropdown-menu__item,
:root:not(.light) .el-dropdown-menu__item {
  color: rgba(255, 255, 255, 0.8) !important;
}

.dark .el-dropdown-menu__item:hover,
:root:not(.light) .el-dropdown-menu__item:hover {
  background-color: rgba(64, 158, 255, 0.1) !important;
  color: #409eff !important;
}

.dark .el-form-item__label,
:root:not(.light) .el-form-item__label {
  color: rgba(255, 255, 255, 0.8) !important;
}

.dark .el-checkbox__label,
:root:not(.light) .el-checkbox__label {
  color: rgba(255, 255, 255, 0.8) !important;
}

.dark .el-radio__label,
:root:not(.light) .el-radio__label {
  color: rgba(255, 255, 255, 0.8) !important;
}

.dark .el-breadcrumb__item,
:root:not(.light) .el-breadcrumb__item {
  color: rgba(255, 255, 255, 0.6) !important;
}

.dark .el-breadcrumb__item:last-child .el-breadcrumb__inner,
:root:not(.light) .el-breadcrumb__item:last-child .el-breadcrumb__inner {
  color: #ffffff !important;
}

.dark .el-empty__description p,
:root:not(.light) .el-empty__description p {
  color: rgba(255, 255, 255, 0.6) !important;
}

.dark .el-divider,
:root:not(.light) .el-divider {
  border-color: rgba(64, 158, 255, 0.1) !important;
}

.dark .el-divider__text,
:root:not(.light) .el-divider__text {
  background-color: #0f1419 !important;
  color: rgba(255, 255, 255, 0.6) !important;
}

.dark .el-loading-mask,
:root:not(.light) .el-loading-mask {
  background-color: rgba(15, 20, 25, 0.8) !important;
}

/* Light Theme Overrides */
.light .el-message {
  background-color: #ffffff !important;
  border: 1px solid #e4e7ed !important;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1) !important;
}

.light .el-message__content {
  color: #303133 !important;
}

.light .el-notification {
  background-color: #ffffff !important;
  border: 1px solid #e4e7ed !important;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1) !important;
}

.light .el-notification__title {
  color: #303133 !important;
}

.light .el-notification__content {
  color: #606266 !important;
}

.light .el-dialog {
  background-color: #ffffff !important;
  border: 1px solid #e4e7ed !important;
}

.light .el-dialog__header {
  border-bottom: 1px solid #e4e7ed !important;
}

.light .el-dialog__title {
  color: #303133 !important;
}

.light .el-dialog__close {
  color: #909399 !important;
}

.light .el-dialog__close:hover {
  color: #409eff !important;
}

.light .el-card {
  background-color: #ffffff !important;
  border: 1px solid #e4e7ed !important;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1) !important;
}

.light .el-card__header {
  background-color: #f5f7fa !important;
  border-bottom: 1px solid #e4e7ed !important;
  color: #303133 !important;
}

.light .el-table {
  background-color: #ffffff !important;
  color: #303133 !important;
}

.light .el-table th.el-table__cell {
  background-color: #f5f7fa !important;
  color: #303133 !important;
  border-bottom: 1px solid #e4e7ed !important;
}

.light .el-table tr {
  background-color: #ffffff !important;
}

.light .el-table td.el-table__cell {
  border-bottom: 1px solid #ebeef5 !important;
  color: #606266 !important;
}

.light .el-table__row:hover {
  background-color: #f5f7fa !important;
}

.light .el-input__wrapper {
  background-color: #ffffff !important;
  border: 1px solid #dcdfe6 !important;
  box-shadow: none !important;
}

.light .el-input__wrapper:hover {
  border-color: #c0c4cc !important;
}

.light .el-input__wrapper.is-focus {
  border-color: #409eff !important;
  box-shadow: 0 0 0 2px rgba(64, 158, 255, 0.1) !important;
}

.light .el-input__inner {
  color: #303133 !important;
}

.light .el-input__inner::placeholder {
  color: #c0c4cc !important;
}

.light .el-input__prefix,
.light .el-input__suffix {
  color: #909399 !important;
}

.light .el-textarea__inner {
  background-color: #ffffff !important;
  border: 1px solid #dcdfe6 !important;
  color: #303133 !important;
}

.light .el-select-dropdown {
  background-color: #ffffff !important;
  border: 1px solid #e4e7ed !important;
}

.light .el-select-dropdown__item {
  color: #606266 !important;
}

.light .el-select-dropdown__item:hover {
  background-color: #f5f7fa !important;
  color: #409eff !important;
}

.light .el-dropdown-menu {
  background-color: #ffffff !important;
  border: 1px solid #e4e7ed !important;
}

.light .el-dropdown-menu__item {
  color: #606266 !important;
}

.light .el-dropdown-menu__item:hover {
  background-color: #f5f7fa !important;
  color: #409eff !important;
}

.light .el-form-item__label {
  color: #606266 !important;
}

.light .el-checkbox__label {
  color: #606266 !important;
}

.light .el-radio__label {
  color: #606266 !important;
}

.light .el-breadcrumb__item {
  color: #909399 !important;
}

.light .el-breadcrumb__item:last-child .el-breadcrumb__inner {
  color: #303133 !important;
}

.light .el-empty__description p {
  color: #909399 !important;
}

.light .el-divider {
  border-color: #e4e7ed !important;
}

.light .el-divider__text {
  background-color: #ffffff !important;
  color: #909399 !important;
}

.light .el-loading-mask {
  background-color: rgba(255, 255, 255, 0.9) !important;
}

/* Scrollbar Customization */
::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}

::-webkit-scrollbar-track {
  background: rgba(30, 35, 45, 0.5);
  border-radius: 4px;
}

::-webkit-scrollbar-thumb {
  background: rgba(64, 158, 255, 0.2);
  border-radius: 4px;
  transition: background 0.3s ease;
}

::-webkit-scrollbar-thumb:hover {
  background: rgba(64, 158, 255, 0.3);
}

/* Firefox Scrollbar */
* {
  scrollbar-width: thin;
  scrollbar-color: rgba(64, 158, 255, 0.2) rgba(30, 35, 45, 0.5);
}

/* Utility Classes */
.text-primary {
  color: var(--text-primary);
}

.text-secondary {
  color: var(--text-secondary);
}

.text-tertiary {
  color: var(--text-tertiary);
}

.bg-primary {
  background-color: var(--bg-primary);
}

.bg-secondary {
  background-color: var(--bg-secondary);
}

.bg-tertiary {
  background-color: var(--bg-tertiary);
}

/* Animations */
@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

@keyframes slideInRight {
  from {
    transform: translateX(20px);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

@keyframes slideInLeft {
  from {
    transform: translateX(-20px);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

@keyframes slideInUp {
  from {
    transform: translateY(20px);
    opacity: 0;
  }
  to {
    transform: translateY(0);
    opacity: 1;
  }
}

.fade-in {
  animation: fadeIn 0.3s ease;
}

.slide-in-right {
  animation: slideInRight 0.3s ease;
}

.slide-in-left {
  animation: slideInLeft 0.3s ease;
}

.slide-in-up {
  animation: slideInUp 0.3s ease;
}

/* Print Styles */
@media print {
  .sidebar,
  .main-header,
  .no-print {
    display: none !important;
  }

  .main-content {
    margin-left: 0 !important;
  }

  .page-content {
    padding: 0 !important;
  }
}
</style>
