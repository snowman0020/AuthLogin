export interface UserResponse {
  id: string
  username: string
  email: string
  role: string
}

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: UserResponse
}

export interface ApiKeyResponse {
  id: string
  key: string
  name: string
  createdAt: string
  expiresAt: string | null
  isActive: boolean
  lastUsedAt: string | null
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  username: string
  email: string
  password: string
}
