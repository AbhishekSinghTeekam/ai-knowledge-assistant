import { AxiosError } from 'axios'
import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

interface ApiErrorResponse {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

export default function LoginPage() {
  const navigate = useNavigate()
  const { signIn } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const onSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError(null)
    setLoading(true)

    try {
      await signIn({ email, password })
      navigate('/', { replace: true })
    } catch (requestError) {
      const apiError = requestError as AxiosError<ApiErrorResponse>
      setError(
        apiError.response?.data?.detail ??
          apiError.response?.data?.title ??
          'Login failed. Please check your credentials.',
      )
    } finally {
      setLoading(false)
    }
  }

  return (
    <main className="mx-auto flex min-h-screen w-full max-w-6xl items-center justify-center px-4 py-8 md:px-8">
      <div className="grid w-full overflow-hidden rounded-3xl border border-amber-950/15 bg-amber-50/70 shadow-[0_20px_70px_rgba(60,40,20,0.15)] backdrop-blur md:grid-cols-2">
        <section className="relative hidden p-10 md:block">
          <div className="absolute -left-14 top-10 h-56 w-56 rounded-full bg-teal-700/25 blur-3xl" />
          <div className="absolute bottom-4 right-0 h-52 w-52 rounded-full bg-orange-500/25 blur-3xl" />
          <div className="relative space-y-4">
            <p className="inline-flex rounded-full border border-amber-900/20 bg-white/50 px-3 py-1 text-sm font-medium uppercase tracking-wide text-amber-900">
              AI Knowledge Assistant
            </p>
            <h1 className="text-5xl leading-tight text-stone-900">
              Ask your private knowledge base with confidence.
            </h1>
            <p className="max-w-md text-lg text-stone-700">
              Sign in to continue using the offline RAG workspace with secure JWT sessions and protected APIs.
            </p>
          </div>
        </section>

        <section className="p-6 sm:p-8 md:p-10">
          <h2 className="text-3xl text-stone-900">Login</h2>
          <p className="mt-2 text-stone-700">Use your account credentials to get a new JWT session.</p>

          <form onSubmit={onSubmit} className="mt-8 space-y-4">
            <label className="block space-y-2">
              <span className="text-sm font-medium text-stone-800">Email</span>
              <input
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                required
                className="w-full rounded-xl border border-stone-300 bg-white px-4 py-3 text-stone-900 outline-none ring-orange-300 transition focus:border-orange-500 focus:ring"
                placeholder="you@company.com"
              />
            </label>

            <label className="block space-y-2">
              <span className="text-sm font-medium text-stone-800">Password</span>
              <input
                type="password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                required
                className="w-full rounded-xl border border-stone-300 bg-white px-4 py-3 text-stone-900 outline-none ring-orange-300 transition focus:border-orange-500 focus:ring"
                placeholder="Enter your password"
              />
            </label>

            {error && (
              <p className="rounded-xl border border-red-300 bg-red-50 px-3 py-2 text-sm text-red-700">
                {error}
              </p>
            )}

            <button
              type="submit"
              disabled={loading}
              className="mt-2 w-full rounded-xl bg-stone-900 px-4 py-3 text-sm font-semibold tracking-wide text-amber-50 transition hover:bg-stone-800 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {loading ? 'Signing in...' : 'Sign in'}
            </button>
          </form>

          <p className="mt-6 text-sm text-stone-700">
            No account yet?{' '}
            <Link to="/register" className="font-semibold text-teal-800 hover:text-teal-700">
              Create one
            </Link>
          </p>
        </section>
      </div>
    </main>
  )
}
