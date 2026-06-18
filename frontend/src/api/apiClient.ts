import axios, { AxiosHeaders } from 'axios'
import { getAuthToken } from '../auth/tokenStore'

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5087',
  headers: {
    'Content-Type': 'application/json',
  },
})

// Attach JWT token to every request if present
apiClient.interceptors.request.use((config) => {
  const token = getAuthToken()
  const headers = AxiosHeaders.from(config.headers)

  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  } else {
    headers.delete('Authorization')
  }

  config.headers = headers
  return config
})

export default apiClient
