/* @refresh reload */
import { render } from 'solid-js/web'
import { createSignal, Show } from 'solid-js'
import './styles/global.scss'
import './styles/admin.scss'
import App from './App.tsx'
import UserLogin from './pages/UserLogin.tsx'
import type { UserInfo } from './pages/UserLogin.tsx'
import AdminLogin from './pages/AdminLogin.tsx'
import AdminTenants from './pages/AdminTenants.tsx'
import { initTheme } from './lib/theme.ts'

// Apply theme before first render to avoid flash of unstyled content.
initTheme()

/**
 * Root component that routes between the admin panel and the user-facing app
 * based on the current URL pathname.
 */
function Root() {
  const isAdminRoute = window.location.pathname.startsWith('/admin')

  if (isAdminRoute) {
    return <AdminApp />
  }

  return <UserApp />
}

/**
 * User-facing application shell.
 * Manages authentication state via sessionStorage and conditionally renders
 * the login page or the main app.
 */
function UserApp() {
  const stored = sessionStorage.getItem('user_session')
  const parsed = stored ? JSON.parse(stored) : null

  const [token, setToken] = createSignal<string | null>(parsed?.token ?? null)
  const [user, setUser] = createSignal<UserInfo | null>(parsed?.user ?? null)

  /**
   * Handles a successful login by persisting the session and updating signals.
   * @param newToken - The bearer token from the API.
   * @param userInfo - The authenticated user's profile data.
   */
  function handleLogin(newToken: string, userInfo: UserInfo) {
    sessionStorage.setItem('user_session', JSON.stringify({ token: newToken, user: userInfo }))
    setToken(newToken)
    setUser(userInfo)
  }

  /** Signs the user out by clearing the session and calling the logout API. */
  function handleLogout() {
    const t = token()
    if (t) {
      fetch('/api/identity/logout', {
        method: 'POST',
        headers: { Authorization: `Bearer ${t}` },
      }).catch(() => {})
    }
    sessionStorage.removeItem('user_session')
    setToken(null)
    setUser(null)
  }

  return (
    <>
      <Show when={!token() || !user()}>
        <UserLogin onLogin={handleLogin} />
      </Show>
      <Show when={token() && user()}>
        <App user={user()!} token={token()!} onLogout={handleLogout} />
      </Show>
    </>
  )
}

/**
 * Admin application shell.
 * Manages superadmin authentication via sessionStorage and conditionally
 * renders the admin login or the tenant management panel.
 */
function AdminApp() {
  const [token, setToken] = createSignal<string | null>(
    sessionStorage.getItem('admin_token')
  )

  /** Stores the admin token in sessionStorage after successful login. */
  function handleLogin(newToken: string) {
    sessionStorage.setItem('admin_token', newToken)
    setToken(newToken)
  }

  /** Clears the admin session. */
  function handleLogout() {
    sessionStorage.removeItem('admin_token')
    setToken(null)
  }

  return (
    <>
      <Show when={!token()}>
        <AdminLogin onLogin={handleLogin} />
      </Show>
      <Show when={token()}>
        <AdminTenants token={token()!} onLogout={handleLogout} />
      </Show>
    </>
  )
}

const root = document.getElementById('root')
render(() => <Root />, root!)
