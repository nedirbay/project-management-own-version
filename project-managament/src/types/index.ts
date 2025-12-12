// User Types
export interface User {
  id: string
  username: string
  email: string
  password?: string
  role: 'admin' | 'workspaceAdmin' | 'member'
  fullName: string
  avatar?: string
  createdAt: string
  updatedAt: string
}

export interface AuthState {
  user: User | null
  token: string | null
  isAuthenticated: boolean
}

// Workspace Types
export interface Workspace {
  id: string
  name: string
  description: string
  ownerId: string
  adminId: string
  memberIds: string[]
  color: string
  icon?: string
  createdAt: string
  updatedAt: string
}

// Project Types
export interface Project {
  id: string
  name: string
  description: string
  workspaceId: string
  ownerId: string
  memberIds: string[]
  status: ProjectStatus
  priority: Priority
  startDate: string
  endDate?: string
  progress: number
  color: string
  tags: string[]
  createdAt: string
  updatedAt: string
}

export type ProjectStatus = 'planning' | 'active' | 'on-hold' | 'completed' | 'cancelled'
export type Priority = 'low' | 'medium' | 'high' | 'critical'

// Task Types
export interface Task {
  id: string
  title: string
  description: string
  projectId: string
  assigneeIds: string[]
  status: TaskStatus
  priority: Priority
  dueDate?: string
  estimatedHours?: number
  actualHours?: number
  tags: string[]
  subtasks: SubTask[]
  attachments: Attachment[]
  comments: Comment[]
  createdBy: string
  createdAt: string
  updatedAt: string
}

export type TaskStatus = 'todo' | 'in-progress' | 'review' | 'done'

export interface SubTask {
  id: string
  title: string
  completed: boolean
  createdAt: string
}

export interface Attachment {
  id: string
  name: string
  url: string
  size: number
  type: string
  uploadedBy: string
  uploadedAt: string
}

export interface Comment {
  id: string
  text: string
  userId: string
  createdAt: string
  updatedAt: string
}

// Daily Report Types
export interface DailyReport {
  id: string
  userId: string
  date: string
  workspaceId: string
  projectId?: string
  tasksCompleted: string[]
  workDescription: string
  hoursWorked: number
  blockers?: string
  notes?: string
  createdAt: string
  updatedAt: string
}

// Kanban Types
export interface KanbanColumn {
  id: TaskStatus
  title: string
  tasks: Task[]
  color: string
}

// Form Types
export interface LoginForm {
  username: string
  password: string
  rememberMe: boolean
}

export interface UserForm {
  username: string
  email: string
  password?: string
  fullName: string
  role: 'admin' | 'workspaceAdmin' | 'member'
}

export interface WorkspaceForm {
  name: string
  description: string
  color: string
  memberIds: string[]
}

export interface ProjectForm {
  name: string
  description: string
  workspaceId: string
  memberIds: string[]
  status: ProjectStatus
  priority: Priority
  startDate: string
  endDate?: string
  color: string
  tags: string[]
}

export interface TaskForm {
  title: string
  description: string
  projectId: string
  assigneeIds: string[]
  status: TaskStatus
  priority: Priority
  dueDate?: string
  estimatedHours?: number
  tags: string[]
}

export interface DailyReportForm {
  workspaceId: string
  projectId?: string
  tasksCompleted: string[]
  workDescription: string
  hoursWorked: number
  blockers?: string
  notes?: string
}

// Statistics Types
export interface DashboardStats {
  totalProjects: number
  activeProjects: number
  totalTasks: number
  completedTasks: number
  totalUsers: number
  totalWorkspaces: number
  myTasks: number
  overdueTasksCount: number
}

export interface ProjectStats {
  totalTasks: number
  completedTasks: number
  inProgressTasks: number
  todoTasks: number
  progress: number
  totalHours: number
}

// Filter & Sort Types
export interface ProjectFilter {
  workspaceId?: string
  status?: ProjectStatus[]
  priority?: Priority[]
  memberIds?: string[]
  search?: string
}

export interface TaskFilter {
  projectId?: string
  status?: TaskStatus[]
  priority?: Priority[]
  assigneeIds?: string[]
  search?: string
  overdue?: boolean
}

// API Response Types
export interface ApiResponse<T> {
  success: boolean
  data?: T
  message?: string
  error?: string
}

export interface PaginatedResponse<T> {
  data: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

// Theme Types
export type Theme = 'light' | 'dark' | 'auto'

export interface ThemeSettings {
  theme: Theme
  systemPreference: boolean
}

// Chart Data Types
export interface ProjectChartData {
  projectId: string
  projectName: string
  progress: number
  totalTasks: number
  completedTasks: number
  color: string
}
