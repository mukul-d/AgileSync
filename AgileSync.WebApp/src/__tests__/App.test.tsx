import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@solidjs/testing-library'
import App from '../App'

describe('App', () => {
  const defaultUser = {
    id: 'user1',
    email: 'user@test.com',
    displayName: 'Test User',
    role: 'Member',
  }

  const mockLogout = vi.fn()

  beforeEach(() => {
    vi.restoreAllMocks()
    mockLogout.mockClear()
    localStorage.clear()
  })

  it('renders sidebar with navigation links', () => {
    const { container } = render(() => <App user={defaultUser} token="test-token" onLogout={mockLogout} />)

    // Query within the sidebar nav to avoid collisions with dashboard stats and topbar
    const navLinks = container.querySelector('.nav-links')!
    expect(navLinks).toBeTruthy()
    expect(navLinks.textContent).toContain('Dashboard')
    expect(navLinks.textContent).toContain('Projects')
    expect(navLinks.textContent).toContain('Board')
  })

  it('displays user display name in sidebar', () => {
    render(() => <App user={defaultUser} token="test-token" onLogout={mockLogout} />)

    // The user name appears in both sidebar and topbar
    const names = screen.getAllByText('Test User')
    expect(names.length).toBeGreaterThanOrEqual(1)
  })

  it('renders the AgileSync logo', () => {
    render(() => <App user={defaultUser} token="test-token" onLogout={mockLogout} />)

    expect(screen.getByText('AgileSync')).toBeInTheDocument()
  })

  it('does NOT show View Admin link for regular users', () => {
    render(() => <App user={defaultUser} token="test-token" onLogout={mockLogout} />)

    expect(screen.queryByText('View Admin')).not.toBeInTheDocument()
  })

  it('shows View Admin link for SuperAdmin role', () => {
    const superAdmin = { ...defaultUser, role: 'SuperAdmin' }
    render(() => <App user={superAdmin} token="test-token" onLogout={mockLogout} />)

    expect(screen.getByText('View Admin')).toBeInTheDocument()
  })

  it('has a theme toggle button', () => {
    render(() => <App user={defaultUser} token="test-token" onLogout={mockLogout} />)

    const toggleButton = screen.getByRole('button', { name: /toggle theme/i })
    expect(toggleButton).toBeInTheDocument()
  })

  it('has a sign out button', () => {
    render(() => <App user={defaultUser} token="test-token" onLogout={mockLogout} />)

    expect(screen.getByText('Sign Out')).toBeInTheDocument()
  })

  it('defaults to the Dashboard page', () => {
    render(() => <App user={defaultUser} token="test-token" onLogout={mockLogout} />)

    expect(screen.getByText('Welcome to AgileSync')).toBeInTheDocument()
  })
})
