import apiClient from './apiClient'

export interface AuthResponse {
  token: string
  email: string
  name: string
  expiresAt: string
}

export interface LoginPayload {
  email: string
  password: string
}

export interface RegisterPayload {
  name: string
  email: string
  password: string
}

export async function login(payload: LoginPayload): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>('/api/auth/login', payload)
  return response.data
}

export async function register(payload: RegisterPayload): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>('/api/auth/register', payload)
  return response.data
}
