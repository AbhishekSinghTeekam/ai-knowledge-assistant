import { AxiosError } from 'axios'
import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

interface ApiErrorResponse {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

function flattenValidationErrors(errors?: Record<string, string[]>): string | null {
  if (!errors) {
    return null
  }

  const messages = Object.values(errors).flat()
  if (!messages.length) {
    return null
  }

  return messages.join(' ')
}

export default function RegisterPage() {
  const navigate = useNavigate()
  const { signUp } = useAuth()
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const onSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError(null)

    if (password !== confirmPassword) {
      setError('Passwords do not match.')
      return
    }

    setLoading(true)

    try {
      await signUp({ name, email, password })
      navigate('/', { replace: true })
    } catch (requestError) {
      const apiError = requestError as AxiosError<ApiErrorResponse>
      setError(
        flattenValidationErrors(apiError.response?.data?.errors) ??
          apiError.response?.data?.detail ??
          apiError.response?.data?.title ??
          'Registration failed. Please check your details.',
      )
    } finally {
      setLoading(false)
    }
  }

  return (
    <main className="mx-auto flex min-h-screen w-full max-w-6xl items-center justify-center px-4 py-8 md:px-8">
      <div className="grid w-full overflow-hidden rounded-3xl border border-teal-950/20 bg-orange-50/70 shadow-[0_20px_70px_rgba(30,60,55,0.15)] backdrop-blur md:grid-cols-2">
        <section className="relative hidden p-10 md:block">
          <div className="absolute -right-16 top-8 h-56 w-56 rounded-full bg-teal-600/25 blur-3xl" />
          <div className="absolute bottom-8 left-2 h-56 w-56 rounded-full bg-orange-500/20 blur-3xl" />
          <div className="relative space-y-4">
            <p className="inline-flex rounded-full border border-teal-950/20 bg-white/60 px-3 py-1 text-sm font-medium uppercase tracking-wide text-teal-900">
              Private by design
            </p>
            <h1 className="text-5xl leading-tight text-stone-900">
              Create your account and start ingesting documents.
            </h1>
            <p className="max-w-md text-lg text-stone-700">
              Your JWT stays in-memory for the active session. A refresh clears the token automatically.
            </p>
          </div>
        </section>

        <section className="p-6 sm:p-8 md:p-10">
          <h2 className="text-3xl text-stone-900">Register</h2>
          <p className="mt-2 text-stone-700">Create a user and receive your JWT access token.</p>

          <form onSubmit={onSubmit} className="mt-8 space-y-4">
            <label className="block space-y-2">
              <span className="text-sm font-medium text-stone-800">Name</span>
              <input
                type="text"
                value={name}
                onChange={(event) => setName(event.target.value)}
                required
                className="w-full rounded-xl border border-stone-300 bg-white px-4 py-3 text-stone-900 outline-none ring-teal-300 transition focus:border-teal-500 focus:ring"
                placeholder="Jane Doe"
              />
            </label>

            <label className="block space-y-2">
              <span className="text-sm font-medium text-stone-800">Email</span>
              <input
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                required
                className="w-full rounded-xl border border-stone-300 bg-white px-4 py-3 text-stone-900 outline-none ring-teal-300 transition focus:border-teal-500 focus:ring"
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
                className="w-full rounded-xl border border-stone-300 bg-white px-4 py-3 text-stone-900 outline-none ring-teal-300 transition focus:border-teal-500 focus:ring"
                placeholder="At least 8 chars, with upper/lower/digit"
              />
            </label>

            <label className="block space-y-2">
              <span className="text-sm font-medium text-stone-800">Confirm password</span>
              <input
                type="password"
                value={confirmPassword}
                onChange={(event) => setConfirmPassword(event.target.value)}
                required
                className="w-full rounded-xl border border-stone-300 bg-white px-4 py-3 text-stone-900 outline-none ring-teal-300 transition focus:border-teal-500 focus:ring"
                placeholder="Repeat password"
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
              className="mt-2 w-full rounded-xl bg-teal-800 px-4 py-3 text-sm font-semibold tracking-wide text-orange-50 transition hover:bg-teal-700 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {loading ? 'Creating account...' : 'Create account'}
            </button>
          </form>

          <p className="mt-6 text-sm text-stone-700">
            Already have an account?{' '}
            <Link to="/login" className="font-semibold text-stone-900 hover:text-stone-700">
              Sign in
            </Link>
          </p>
        </section>
      </div>
    </main>
  )
}
