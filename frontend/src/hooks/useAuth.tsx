import {
  createContext,
  type ReactNode,
  useContext,
  useMemo,
  useState,
} from 'react'
import {
  type LoginPayload,
  login,
  type RegisterPayload,
  register,
} from '../api/authApi'
import { clearAuthToken, setAuthToken } from '../auth/tokenStore'

interface AuthSession {
  token: string
  email: string
  name: string
  expiresAt: string
}

interface AuthContextValue {
  session: AuthSession | null
  isAuthenticated: boolean
  signIn: (payload: LoginPayload) => Promise<void>
  signUp: (payload: RegisterPayload) => Promise<void>
  signOut: () => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthSession | null>(null)

  async function signIn(payload: LoginPayload): Promise<void> {
    const result = await login(payload)
    setSession(result)
    setAuthToken(result.token)
  }

  async function signUp(payload: RegisterPayload): Promise<void> {
    const result = await register(payload)
    setSession(result)
    setAuthToken(result.token)
  }

  function signOut() {
    setSession(null)
    clearAuthToken()
  }

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      isAuthenticated: Boolean(session?.token),
      signIn,
      signUp,
      signOut,
    }),
    [session],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)

  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider')
  }

  return context
}
