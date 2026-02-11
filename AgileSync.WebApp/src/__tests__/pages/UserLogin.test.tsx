import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@solidjs/testing-library'
import UserLogin from '../../pages/UserLogin'

describe('UserLogin', () => {
  const mockOnLogin = vi.fn()

  beforeEach(() => {
    vi.restoreAllMocks()
    mockOnLogin.mockClear()
    localStorage.clear()
  })

  it('renders the login form with email and password fields', () => {
    render(() => <UserLogin onLogin={mockOnLogin} />)

    expect(screen.getByLabelText('Email')).toBeInTheDocument()
    expect(screen.getByLabelText('Password')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument()
  })

  it('renders the AgileSync heading and subtitle', () => {
    render(() => <UserLogin onLogin={mockOnLogin} />)

    expect(screen.getByText('AgileSync')).toBeInTheDocument()
    expect(screen.getByText('Sign in to your account')).toBeInTheDocument()
  })

  it('shows error message on 401 response', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      status: 401,
      ok: false,
    }))

    render(() => <UserLogin onLogin={mockOnLogin} />)

    fireEvent.input(screen.getByLabelText('Email'), { target: { value: 'user@test.com' } })
    fireEvent.input(screen.getByLabelText('Password'), { target: { value: 'wrong' } })
    fireEvent.click(screen.getByRole('button', { name: /sign in/i }))

    await waitFor(() => {
      expect(screen.getByText('Invalid email or password')).toBeInTheDocument()
    })

    expect(mockOnLogin).not.toHaveBeenCalled()
  })

  it('calls onLogin with token and user info on success', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      status: 200,
      ok: true,
      json: () => Promise.resolve({
        data: {
          token: 'user-token-123',
          id: 'user1',
          email: 'user@test.com',
          displayName: 'Test User',
          role: 'Member',
        },
      }),
    }))

    render(() => <UserLogin onLogin={mockOnLogin} />)

    fireEvent.input(screen.getByLabelText('Email'), { target: { value: 'user@test.com' } })
    fireEvent.input(screen.getByLabelText('Password'), { target: { value: 'password' } })
    fireEvent.click(screen.getByRole('button', { name: /sign in/i }))

    await waitFor(() => {
      expect(mockOnLogin).toHaveBeenCalledWith('user-token-123', {
        id: 'user1',
        email: 'user@test.com',
        displayName: 'Test User',
        role: 'Member',
      })
    })
  })

  it('has a link to the super admin portal', () => {
    render(() => <UserLogin onLogin={mockOnLogin} />)

    const adminLink = screen.getByText('Super Admin')
    expect(adminLink).toBeInTheDocument()
    expect(adminLink.getAttribute('href')).toBe('/admin')
  })

  it('has a theme toggle button', () => {
    render(() => <UserLogin onLogin={mockOnLogin} />)

    const toggleButton = screen.getByRole('button', { name: /toggle theme/i })
    expect(toggleButton).toBeInTheDocument()
  })
})
