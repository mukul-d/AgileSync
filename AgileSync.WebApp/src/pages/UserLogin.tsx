import { createSignal } from 'solid-js'
import { getStoredTheme, applyTheme, type Theme } from '../lib/theme'

/** Props for the UserLogin component. */
interface Props {
  /** Callback invoked after successful login with token and user data. */
  onLogin: (token: string, user: UserInfo) => void
}

/** Authenticated user profile returned by the login API. */
export interface UserInfo {
  /** Unique user identifier. */
  id: string
  /** User's email address. */
  email: string
  /** User's display name shown in the UI. */
  displayName: string
  /** User's role (e.g., Admin, Member, Viewer). */
  role: string
}

/**
 * User login page with email/password authentication.
 * Includes a theme toggle and a link to the superadmin portal.
 */
export default function UserLogin(props: Props) {
  const [email, setEmail] = createSignal('')
  const [password, setPassword] = createSignal('')
  const [error, setError] = createSignal<string | null>(null)
  const [loading, setLoading] = createSignal(false)
  const [theme, setTheme] = createSignal<Theme>(getStoredTheme())

  /** Toggles between dark and light themes. */
  function toggleTheme() {
    const next: Theme = theme() === 'dark' ? 'light' : 'dark'
    setTheme(next)
    applyTheme(next)
  }

  /**
   * Submits the login form to the identity API.
   * Extracts token and user info from the BaseResponse wrapper on success.
   */
  async function handleSubmit(e: Event) {
    e.preventDefault()
    setLoading(true)
    setError(null)

    try {
      const res = await fetch('/api/identity/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: email(), password: password() }),
      })

      if (res.status === 401) {
        setError('Invalid email or password')
        return
      }

      if (!res.ok) throw new Error('Login failed')

      const json = await res.json()
      const data = json.data
      props.onLogin(data.token, {
        id: data.id,
        email: data.email,
        displayName: data.displayName,
        role: data.role,
      })
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Login failed')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div class="login-page">
      <div class="login-card">
        <div class="login-header">
          <h1>AgileSync</h1>
          <p>Sign in to your account</p>
        </div>

        <form onSubmit={handleSubmit}>
          <div class="form-group">
            <label for="email">Email</label>
            <input
              id="email"
              type="email"
              value={email()}
              onInput={(e) => setEmail(e.currentTarget.value)}
              placeholder="you@example.com"
              required
            />
          </div>

          <div class="form-group">
            <label for="password">Password</label>
            <input
              id="password"
              type="password"
              value={password()}
              onInput={(e) => setPassword(e.currentTarget.value)}
              placeholder="Enter password"
              required
            />
          </div>

          {error() && <div class="form-error">{error()}</div>}

          <button type="submit" class="btn-primary" disabled={loading()}>
            {loading() ? 'Signing in...' : 'Sign In'}
          </button>
        </form>

        <div class="login-footer">
          <button class="btn-ghost theme-toggle" onClick={toggleTheme} aria-label="Toggle theme">
            {theme() === 'dark' ? '☀️' : '🌙'}
          </button>
          <a href="/admin" class="admin-link">Super Admin</a>
        </div>
      </div>
    </div>
  )
}
