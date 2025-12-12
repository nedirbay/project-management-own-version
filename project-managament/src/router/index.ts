import { createRouter, createWebHistory } from 'vue-router'
import MainLayout from '../layouts/MainLayout.vue'
import LoginLayout from '../layouts/LoginLayout.vue'
import { useAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      component: LoginLayout,
      children: [
        {
          path: '',
          name: 'login',
          component: () => import('../views/LoginView.vue'),
          meta: { requiresGuest: true },
        },
      ],
    },
    {
      path: '/',
      component: MainLayout,
      meta: { requiresAuth: true },
      children: [
        {
          path: '',
          name: 'dashboard',
          component: () => import('../views/DashboardView.vue'),
        },
        {
          path: '/workspaces',
          name: 'workspaces',
          component: () => import('../views/WorkspacesView.vue'),
        },
        {
          path: '/projects',
          name: 'projects',
          component: () => import('../views/ProjectsView.vue'),
        },
        {
          path: '/projects/:id',
          name: 'project-detail',
          component: () => import('../views/ProjectDetailView.vue'),
        },
        {
          path: '/tasks',
          name: 'tasks',
          component: () => import('../views/TasksView.vue'),
        },
        {
          path: '/kanban',
          name: 'kanban',
          component: () => import('../views/KanbanView.vue'),
        },
        {
          path: '/reports',
          name: 'reports',
          component: () => import('../views/DailyReportView.vue'),
        },
        {
          path: '/users',
          name: 'users',
          component: () => import('../views/UsersView.vue'),
          meta: { requiresAdmin: true },
        },
        {
          path: '/profile',
          name: 'profile',
          component: () => import('../views/ProfileView.vue'),
        },
        {
          path: '/settings',
          name: 'settings',
          component: () => import('../views/SettingsView.vue'),
        },
      ],
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/',
    },
  ],
})

// Navigation guards
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()

  // Check if route requires authentication
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
    return
  }

  // Check if route requires guest (not authenticated)
  if (to.meta.requiresGuest && authStore.isAuthenticated) {
    next('/')
    return
  }

  // Check if route requires admin
  if (to.meta.requiresAdmin && !authStore.isAdmin) {
    next('/')
    return
  }

  next()
})

export default router
