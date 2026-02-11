import { createSignal } from 'solid-js'
import { Show } from 'solid-js/web'
import { getStoredTheme, applyTheme, type Theme } from './lib/theme'
import type { UserInfo } from './pages/UserLogin'
import './styles/app.scss'

/** Available pages in the main app navigation. */
type Page = 'dashboard' | 'projects' | 'board'

/** Props passed to the main App component. */
interface AppProps {
  /** The authenticated user's profile data. */
  user: UserInfo
  /** The current bearer token for API calls. */
  token: string
  /** Callback to sign the user out. */
  onLogout: () => void
}

/**
 * Main application component.
 * Renders the sidebar navigation, topbar, and the currently selected page.
 * Includes theme toggling and user profile display.
 */
function App(props: AppProps) {
  const [currentPage, setCurrentPage] = createSignal<Page>('dashboard')
  const [sidebarOpen, setSidebarOpen] = createSignal(false)
  const [theme, setTheme] = createSignal<Theme>(getStoredTheme())

  /** Toggles between dark and light themes and persists the choice. */
  function toggleTheme() {
    const next: Theme = theme() === 'dark' ? 'light' : 'dark'
    setTheme(next)
    applyTheme(next)
  }

  /**
   * Navigates to a page and closes the mobile sidebar.
   * @param page - The page to navigate to.
   */
  function navigateTo(page: Page) {
    setCurrentPage(page)
    setSidebarOpen(false)
  }

  return (
    <div class="app">
      {/* Mobile overlay */}
      <div
        class="sidebar-overlay"
        classList={{ visible: sidebarOpen() }}
        onClick={() => setSidebarOpen(false)}
      />

      <nav class="sidebar" classList={{ open: sidebarOpen() }}>
        <div class="logo">
          <h2>AgileSync</h2>
        </div>
        <ul class="nav-links">
          <li
            classList={{ active: currentPage() === 'dashboard' }}
            onClick={() => navigateTo('dashboard')}
          >
            Dashboard
          </li>
          <li
            classList={{ active: currentPage() === 'projects' }}
            onClick={() => navigateTo('projects')}
          >
            Projects
          </li>
          <li
            classList={{ active: currentPage() === 'board' }}
            onClick={() => navigateTo('board')}
          >
            Board
          </li>
          <Show when={props.user.role === 'SuperAdmin'}>
            <li onClick={() => { window.location.href = '/admin' }}>View Admin</li>
          </Show>
        </ul>
        <div class="sidebar-footer">
          <button class="btn-ghost theme-toggle" onClick={toggleTheme} aria-label="Toggle theme">
            {theme() === 'dark' ? '☀️' : '🌙'}
          </button>
          <div class="sidebar-user">
            <span class="sidebar-user-name">{props.user.displayName}</span>
            <button class="btn-ghost btn-sm" onClick={props.onLogout}>Sign Out</button>
          </div>
        </div>
      </nav>

      <main class="content">
        <header class="topbar">
          <button class="hamburger" onClick={() => setSidebarOpen(true)} aria-label="Open menu">
            &#9776;
          </button>
          <h1>{currentPage().charAt(0).toUpperCase() + currentPage().slice(1)}</h1>
          <span class="topbar-user">{props.user.displayName}</span>
        </header>

        <div class="page">
          <Show when={currentPage() === 'dashboard'}>
            <DashboardPage />
          </Show>
          <Show when={currentPage() === 'projects'}>
            <PlaceholderPage name="Projects" />
          </Show>
          <Show when={currentPage() === 'board'}>
            <PlaceholderPage name="Board" />
          </Show>
        </div>
      </main>
    </div>
  )
}

/**
 * Dashboard page displaying welcome info, API connectivity test, and stats overview.
 */
function DashboardPage() {
  const [sample, setSample] = createSignal<{ message?: string; timestamp?: string } | null>(null)
  const [loading, setLoading] = createSignal(false)
  const [error, setError] = createSignal<string | null>(null)

  /** Fetches the sample API endpoint to verify backend connectivity. */
  async function fetchSample() {
    setLoading(true)
    setError(null)
    try {
      const res = await fetch('/api/sample')
      if (!res.ok) throw new Error(res.statusText)
      const data = await res.json()
      setSample(data)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to fetch')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div class="dashboard">
      <h2>Welcome to AgileSync</h2>
      <p>Your agile project management platform.</p>

      <div class="sample-section">
        <button onClick={fetchSample} disabled={loading()}>
          {loading() ? 'Loading...' : 'Test API connection'}
        </button>
        <Show when={sample()}>
          <pre class="sample-response">{JSON.stringify(sample(), null, 2)}</pre>
        </Show>
        <Show when={error()}>
          <p class="sample-error">Error: {error()}</p>
        </Show>
      </div>

      <div class="stats-grid">
        <div class="stat-card">
          <span class="stat-value">0</span>
          <span class="stat-label">Projects</span>
        </div>
        <div class="stat-card">
          <span class="stat-value">0</span>
          <span class="stat-label">Active Sprints</span>
        </div>
        <div class="stat-card">
          <span class="stat-value">0</span>
          <span class="stat-label">Open Tasks</span>
        </div>
        <div class="stat-card">
          <span class="stat-value">0</span>
          <span class="stat-label">Team Members</span>
        </div>
      </div>
    </div>
  )
}

/**
 * Generic placeholder page for sections that are under construction.
 * @param props.name - The name to display as the page heading.
 */
function PlaceholderPage(props: { name: string }) {
  return (
    <div class="placeholder">
      <h2>{props.name}</h2>
      <p>This page is under construction.</p>
    </div>
  )
}

export default App
