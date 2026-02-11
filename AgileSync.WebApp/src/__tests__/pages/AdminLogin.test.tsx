import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@solidjs/testing-library'
import AdminLogin from '../../pages/AdminLogin'

describe('AdminLogin', () => {
  const mockOnLogin = vi.fn()

  beforeEach(() => {
    vi.restoreAllMocks()
    mockOnLogin.mockClear()
  })

  it('renders the login form with username and password fields', () => {
    render(() => <AdminLogin onLogin={mockOnLogin} />)

    expect(screen.getByLabelText('Username')).toBeInTheDocument()
    expect(screen.getByLabelText('Password')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument()
  })

  it('renders the Super Admin Portal heading', () => {
    render(() => <AdminLogin onLogin={mockOnLogin} />)

    expect(screen.getByText('Super Admin Portal')).toBeInTheDocument()
    expect(screen.getByText('AgileSync')).toBeInTheDocument()
  })

  it('shows error message on 401 response', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      status: 401,
      ok: false,
    }))

    render(() => <AdminLogin onLogin={mockOnLogin} />)

    const usernameInput = screen.getByLabelText('Username')
    const passwordInput = screen.getByLabelText('Password')
    const submitButton = screen.getByRole('button', { name: /sign in/i })

    fireEvent.input(usernameInput, { target: { value: 'wrong' } })
    fireEvent.input(passwordInput, { target: { value: 'wrong' } })
    fireEvent.click(submitButton)

    await waitFor(() => {
      expect(screen.getByText('Invalid credentials')).toBeInTheDocument()
    })

    expect(mockOnLogin).not.toHaveBeenCalled()
  })

  it('calls onLogin with token on successful login', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      status: 200,
      ok: true,
      json: () => Promise.resolve({ data: { token: 'test-token-123' } }),
    }))

    render(() => <AdminLogin onLogin={mockOnLogin} />)

    const usernameInput = screen.getByLabelText('Username')
    const passwordInput = screen.getByLabelText('Password')
    const submitButton = screen.getByRole('button', { name: /sign in/i })

    fireEvent.input(usernameInput, { target: { value: 'admin' } })
    fireEvent.input(passwordInput, { target: { value: 'password' } })
    fireEvent.click(submitButton)

    await waitFor(() => {
      expect(mockOnLogin).toHaveBeenCalledWith('test-token-123')
    })
  })

  it('disables button while loading', async () => {
    let resolvePromise: (value: any) => void
    const fetchPromise = new Promise(resolve => { resolvePromise = resolve })

    vi.stubGlobal('fetch', vi.fn().mockReturnValue(fetchPromise))

    render(() => <AdminLogin onLogin={mockOnLogin} />)

    const usernameInput = screen.getByLabelText('Username')
    const passwordInput = screen.getByLabelText('Password')
    const submitButton = screen.getByRole('button', { name: /sign in/i })

    fireEvent.input(usernameInput, { target: { value: 'admin' } })
    fireEvent.input(passwordInput, { target: { value: 'password' } })
    fireEvent.click(submitButton)

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /signing in/i })).toBeDisabled()
    })

    // Resolve to unblock
    resolvePromise!({
      status: 200,
      ok: true,
      json: () => Promise.resolve({ data: { token: 'tok' } }),
    })
  })
})
