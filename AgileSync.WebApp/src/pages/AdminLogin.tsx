import { createSignal } from 'solid-js'

/** Props for the AdminLogin component. */
interface Props {
  /** Callback invoked with the bearer token after successful superadmin login. */
  onLogin: (token: string) => void
}

/**
 * Superadmin login page.
 * Authenticates against the `/api/identity/admin/login` endpoint using
 * username/password credentials and returns a session token.
 */
export default function AdminLogin(props: Props) {
  const [username, setUsername] = createSignal('')
  const [password, setPassword] = createSignal('')
  const [error, setError] = createSignal<string | null>(null)
  const [loading, setLoading] = createSignal(false)

  /**
   * Submits the login form, sending credentials to the admin auth endpoint.
   * On success, calls `props.onLogin` with the returned token.
   */
  async function handleSubmit(e: Event) {
    e.preventDefault()
    setLoading(true)
    setError(null)

    try {
      const res = await fetch('/api/identity/admin/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username: username(), password: password() }),
      })

      if (res.status === 401) {
        setError('Invalid credentials')
        return
      }

      if (!res.ok) throw new Error('Login failed')

      const data = await res.json()
      props.onLogin(data.data.token)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Login failed')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div class="admin-login-page">
      <div class="admin-login-card">
        <div class="admin-login-header">
          <h1>AgileSync</h1>
          <p>Super Admin Portal</p>
        </div>

        <form onSubmit={handleSubmit}>
          <div class="form-group">
            <label for="username">Username</label>
            <input
              id="username"
              type="text"
              value={username()}
              onInput={(e) => setUsername(e.currentTarget.value)}
              placeholder="Enter username"
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
      </div>
    </div>
  )
}
