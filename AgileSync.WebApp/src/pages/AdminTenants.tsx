import { createSignal, onMount, For, Show } from 'solid-js'
import { getStoredTheme, applyTheme, type Theme } from '../lib/theme'

/** Organization data returned by the admin API. */
interface Organization {
  id: string
  name: string
  slug: string
  description: string
  isActive: boolean
  createdAt: string
}

/** Tenant admin data returned by the admin API. */
interface TenantAdmin {
  id: string
  email: string
  displayName: string
  role: string
  joinedAt: string
}

/** Props for the AdminTenants component. */
interface Props {
  /** Bearer token for authenticating admin API calls. */
  token: string
  /** Callback to sign the superadmin out. */
  onLogout: () => void
}

/**
 * Admin panel for managing tenant organizations and their administrators.
 * Provides CRUD operations for organizations and tenant admin management
 * via modals. All API responses are wrapped in BaseResponse.
 */
export default function AdminTenants(props: Props) {
  const [orgs, setOrgs] = createSignal<Organization[]>([])
  const [loading, setLoading] = createSignal(true)
  const [error, setError] = createSignal<string | null>(null)
  const [showForm, setShowForm] = createSignal(false)
  const [editingOrg, setEditingOrg] = createSignal<Organization | null>(null)
  const [sidebarOpen, setSidebarOpen] = createSignal(false)
  const [theme, setTheme] = createSignal<Theme>(getStoredTheme())

  /** Toggles between dark and light themes. */
  function toggleTheme() {
    const next: Theme = theme() === 'dark' ? 'light' : 'dark'
    setTheme(next)
    applyTheme(next)
  }

  // ── Admins modal state ────────────────────────────────
  const [adminsOrg, setAdminsOrg] = createSignal<Organization | null>(null)
  const [admins, setAdmins] = createSignal<TenantAdmin[]>([])
  const [adminsLoading, setAdminsLoading] = createSignal(false)
  const [showAddAdmin, setShowAddAdmin] = createSignal(false)
  const [adminEmail, setAdminEmail] = createSignal('')
  const [adminName, setAdminName] = createSignal('')
  const [adminPassword, setAdminPassword] = createSignal('')
  const [adminError, setAdminError] = createSignal<string | null>(null)
  const [adminSaving, setAdminSaving] = createSignal(false)

  // ── Organization form state ───────────────────────────
  const [formName, setFormName] = createSignal('')
  const [formSlug, setFormSlug] = createSignal('')
  const [formDescription, setFormDescription] = createSignal('')
  const [formIsActive, setFormIsActive] = createSignal(true)
  const [formError, setFormError] = createSignal<string | null>(null)
  const [saving, setSaving] = createSignal(false)

  /** Builds standard headers with auth token for API calls. */
  const headers = () => ({
    'Content-Type': 'application/json',
    Authorization: `Bearer ${props.token}`,
  })

  /** Fetches all organizations from the admin API. */
  async function fetchOrgs() {
    setLoading(true)
    setError(null)
    try {
      const res = await fetch('/api/identity/admin/organizations', { headers: headers() })
      if (res.status === 401) { props.onLogout(); return }
      if (!res.ok) throw new Error('Failed to fetch organizations')
      const json = await res.json()
      setOrgs(json.data ?? [])
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load')
    } finally {
      setLoading(false)
    }
  }

  onMount(fetchOrgs)

  /** Opens the create organization form with empty fields. */
  function openCreateForm() {
    setEditingOrg(null)
    setFormName('')
    setFormSlug('')
    setFormDescription('')
    setFormIsActive(true)
    setFormError(null)
    setShowForm(true)
  }

  /** Opens the edit form pre-populated with the given organization data. */
  function openEditForm(org: Organization) {
    setEditingOrg(org)
    setFormName(org.name)
    setFormSlug(org.slug)
    setFormDescription(org.description)
    setFormIsActive(org.isActive)
    setFormError(null)
    setShowForm(true)
  }

  /** Closes the create/edit form modal. */
  function closeForm() {
    setShowForm(false)
    setEditingOrg(null)
  }

  /**
   * Generates a URL-friendly slug from a display name.
   * @param name - The organization name to slugify.
   */
  function generateSlug(name: string) {
    return name
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-|-$/g, '')
  }

  /** Auto-generates slug when typing the name (create mode only). */
  function handleNameInput(value: string) {
    setFormName(value)
    if (!editingOrg()) {
      setFormSlug(generateSlug(value))
    }
  }

  /** Submits the create or update organization form. */
  async function handleSubmit(e: Event) {
    e.preventDefault()
    setSaving(true)
    setFormError(null)

    try {
      const editing = editingOrg()

      if (editing) {
        const res = await fetch(`/api/identity/admin/organizations/${editing.id}`, {
          method: 'PUT',
          headers: headers(),
          body: JSON.stringify({
            name: formName(),
            description: formDescription(),
            isActive: formIsActive(),
          }),
        })
        if (res.status === 401) { props.onLogout(); return }
        if (!res.ok) throw new Error('Failed to update organization')
      } else {
        const res = await fetch('/api/identity/admin/organizations', {
          method: 'POST',
          headers: headers(),
          body: JSON.stringify({
            name: formName(),
            slug: formSlug(),
            description: formDescription(),
          }),
        })
        if (res.status === 401) { props.onLogout(); return }
        if (res.status === 409) {
          setFormError('An organization with this slug already exists.')
          return
        }
        if (!res.ok) throw new Error('Failed to create organization')
      }

      closeForm()
      await fetchOrgs()
    } catch (e) {
      setFormError(e instanceof Error ? e.message : 'Save failed')
    } finally {
      setSaving(false)
    }
  }

  /** Opens the admins modal for a specific organization. */
  async function openAdminsModal(org: Organization) {
    setAdminsOrg(org)
    setShowAddAdmin(false)
    setAdminError(null)
    await fetchAdmins(org.id)
  }

  /** Closes the admins modal and resets state. */
  function closeAdminsModal() {
    setAdminsOrg(null)
    setAdmins([])
    setShowAddAdmin(false)
  }

  /**
   * Fetches the list of admins for a given organization.
   * @param orgId - The organization ID to fetch admins for.
   */
  async function fetchAdmins(orgId: string) {
    setAdminsLoading(true)
    try {
      const res = await fetch(`/api/identity/admin/organizations/${orgId}/admins`, { headers: headers() })
      if (res.status === 401) { props.onLogout(); return }
      if (!res.ok) throw new Error('Failed to fetch admins')
      const json = await res.json()
      setAdmins(json.data ?? [])
    } catch (e) {
      setAdminError(e instanceof Error ? e.message : 'Failed to load admins')
    } finally {
      setAdminsLoading(false)
    }
  }

  /** Submits the add-admin form for the current organization. */
  async function handleAddAdmin(e: Event) {
    e.preventDefault()
    setAdminSaving(true)
    setAdminError(null)

    const org = adminsOrg()
    if (!org) return

    try {
      const res = await fetch(`/api/identity/admin/organizations/${org.id}/admins`, {
        method: 'POST',
        headers: headers(),
        body: JSON.stringify({
          email: adminEmail(),
          displayName: adminName(),
          password: adminPassword(),
        }),
      })
      if (res.status === 401) { props.onLogout(); return }
      if (res.status === 409) {
        setAdminError('User is already a member of this organization.')
        return
      }
      if (!res.ok) throw new Error('Failed to add admin')

      setAdminEmail('')
      setAdminName('')
      setAdminPassword('')
      setShowAddAdmin(false)
      await fetchAdmins(org.id)
    } catch (e) {
      setAdminError(e instanceof Error ? e.message : 'Failed to add admin')
    } finally {
      setAdminSaving(false)
    }
  }

  /**
   * Removes an admin from the current organization after confirmation.
   * @param userId - The user ID of the admin to remove.
   */
  async function handleRemoveAdmin(userId: string) {
    const org = adminsOrg()
    if (!org || !confirm('Remove this admin from the organization?')) return

    try {
      const res = await fetch(`/api/identity/admin/organizations/${org.id}/admins/${userId}`, {
        method: 'DELETE',
        headers: headers(),
      })
      if (res.status === 401) { props.onLogout(); return }
      if (!res.ok) throw new Error('Failed to remove admin')
      await fetchAdmins(org.id)
    } catch (e) {
      setAdminError(e instanceof Error ? e.message : 'Failed to remove admin')
    }
  }

  /**
   * Gets a user-compatible token from the admin API, stores it as a user session,
   * and navigates to the main app.
   */
  async function handleViewApp() {
    try {
      const res = await fetch('/api/identity/admin/app-token', {
        method: 'POST',
        headers: headers(),
      })
      if (res.status === 401) { props.onLogout(); return }
      if (!res.ok) throw new Error('Failed to get app token')
      const json = await res.json()
      const data = json.data
      sessionStorage.setItem('user_session', JSON.stringify({
        token: data.token,
        user: {
          id: data.id,
          email: data.email,
          displayName: data.displayName,
          role: data.role,
        },
      }))
      window.location.href = '/'
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to open app')
    }
  }

  /** Deactivates an organization after confirmation. */
  async function handleDeactivate(org: Organization) {
    if (!confirm(`Deactivate "${org.name}"?`)) return

    try {
      const res = await fetch(`/api/identity/admin/organizations/${org.id}`, {
        method: 'DELETE',
        headers: headers(),
      })
      if (res.status === 401) { props.onLogout(); return }
      if (!res.ok) throw new Error('Failed to deactivate')
      await fetchOrgs()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Action failed')
    }
  }

  return (
    <div class="admin-layout">
      {/* Mobile overlay */}
      <div
        class="admin-sidebar-overlay"
        classList={{ visible: sidebarOpen() }}
        onClick={() => setSidebarOpen(false)}
      />

      <nav class="admin-sidebar" classList={{ open: sidebarOpen() }}>
        <div class="admin-logo">
          <h2>AgileSync</h2>
          <span class="admin-badge">Admin</span>
        </div>
        <ul class="nav-links">
          <li class="active" onClick={() => setSidebarOpen(false)}>Tenants</li>
          <li onClick={handleViewApp}>View App</li>
        </ul>
        <div class="admin-sidebar-footer">
          <button class="btn-ghost theme-toggle" onClick={toggleTheme} aria-label="Toggle theme">
            {theme() === 'dark' ? '☀️' : '🌙'}
          </button>
          <button class="btn-ghost" onClick={props.onLogout}>Sign Out</button>
        </div>
      </nav>

      <main class="admin-content">
        <header class="admin-topbar">
          <button class="admin-hamburger" onClick={() => setSidebarOpen(true)} aria-label="Open menu">
            &#9776;
          </button>
          <h1>Tenants</h1>
          <button class="btn-primary" onClick={openCreateForm}>+ New Tenant</button>
        </header>

        <div class="admin-page">
          <Show when={error()}>
            <div class="form-error" style="margin-bottom: 16px">{error()}</div>
          </Show>

          <Show when={loading()}>
            <p class="text-muted">Loading...</p>
          </Show>

          <Show when={!loading() && orgs().length === 0}>
            <div class="empty-state">
              <p>No tenants yet. Create your first organization.</p>
              <button class="btn-primary" onClick={openCreateForm}>Create Tenant</button>
            </div>
          </Show>

          <Show when={!loading() && orgs().length > 0}>
            {/* Desktop table */}
            <table class="admin-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Slug</th>
                  <th>Description</th>
                  <th>Status</th>
                  <th>Created</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <For each={orgs()}>
                  {(org) => (
                    <tr>
                      <td>{org.name}</td>
                      <td><code>{org.slug}</code></td>
                      <td class="text-muted">{org.description || '—'}</td>
                      <td>
                        <span class={`status-badge ${org.isActive ? 'active' : 'inactive'}`}>
                          {org.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                      <td class="text-muted">{new Date(org.createdAt).toLocaleDateString()}</td>
                      <td class="actions-cell">
                        <button class="btn-sm" onClick={() => openEditForm(org)}>Edit</button>
                        <button class="btn-sm" onClick={() => openAdminsModal(org)}>Admins</button>
                        <Show when={org.isActive}>
                          <button class="btn-sm btn-danger" onClick={() => handleDeactivate(org)}>Deactivate</button>
                        </Show>
                      </td>
                    </tr>
                  )}
                </For>
              </tbody>
            </table>

            {/* Mobile card view */}
            <div class="admin-cards">
              <For each={orgs()}>
                {(org) => (
                  <div class="admin-card">
                    <div class="admin-card-header">
                      <span class="admin-card-name">{org.name}</span>
                      <span class={`status-badge ${org.isActive ? 'active' : 'inactive'}`}>
                        {org.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </div>
                    <div class="admin-card-slug"><code>{org.slug}</code></div>
                    <Show when={org.description}>
                      <div class="admin-card-desc">{org.description}</div>
                    </Show>
                    <div class="admin-card-footer">
                      <span class="admin-card-meta">{new Date(org.createdAt).toLocaleDateString()}</span>
                      <div class="actions-cell">
                        <button class="btn-sm" onClick={() => openEditForm(org)}>Edit</button>
                        <button class="btn-sm" onClick={() => openAdminsModal(org)}>Admins</button>
                        <Show when={org.isActive}>
                          <button class="btn-sm btn-danger" onClick={() => handleDeactivate(org)}>Deactivate</button>
                        </Show>
                      </div>
                    </div>
                  </div>
                )}
              </For>
            </div>
          </Show>
        </div>
      </main>

      {/* Create/Edit Organization Modal */}
      <Show when={showForm()}>
        <div class="modal-overlay" onClick={closeForm}>
          <div class="modal" onClick={(e) => e.stopPropagation()}>
            <h2>{editingOrg() ? 'Edit Tenant' : 'New Tenant'}</h2>
            <form onSubmit={handleSubmit}>
              <div class="form-group">
                <label for="org-name">Name</label>
                <input
                  id="org-name"
                  type="text"
                  value={formName()}
                  onInput={(e) => handleNameInput(e.currentTarget.value)}
                  placeholder="Acme Corporation"
                  required
                />
              </div>

              <Show when={!editingOrg()}>
                <div class="form-group">
                  <label for="org-slug">Slug</label>
                  <input
                    id="org-slug"
                    type="text"
                    value={formSlug()}
                    onInput={(e) => setFormSlug(e.currentTarget.value)}
                    placeholder="acme-corp"
                    required
                  />
                  <span class="form-hint">URL-friendly identifier (cannot be changed later)</span>
                </div>
              </Show>

              <div class="form-group">
                <label for="org-desc">Description</label>
                <textarea
                  id="org-desc"
                  value={formDescription()}
                  onInput={(e) => setFormDescription(e.currentTarget.value)}
                  placeholder="Optional description"
                  rows={3}
                />
              </div>

              <Show when={editingOrg()}>
                <div class="form-group">
                  <label class="checkbox-label">
                    <input
                      type="checkbox"
                      checked={formIsActive()}
                      onChange={(e) => setFormIsActive(e.currentTarget.checked)}
                    />
                    Active
                  </label>
                </div>
              </Show>

              {formError() && <div class="form-error">{formError()}</div>}

              <div class="form-actions">
                <button type="button" class="btn-ghost" onClick={closeForm}>Cancel</button>
                <button type="submit" class="btn-primary" disabled={saving()}>
                  {saving() ? 'Saving...' : editingOrg() ? 'Update' : 'Create'}
                </button>
              </div>
            </form>
          </div>
        </div>
      </Show>

      {/* Admins Modal */}
      <Show when={adminsOrg()}>
        <div class="modal-overlay" onClick={closeAdminsModal}>
          <div class="modal modal-wide" onClick={(e) => e.stopPropagation()}>
            <div class="modal-header">
              <h2>Admins — {adminsOrg()!.name}</h2>
              <button class="btn-ghost" onClick={closeAdminsModal} aria-label="Close">&times;</button>
            </div>

            <Show when={adminError()}>
              <div class="form-error" style="margin-bottom: 12px">{adminError()}</div>
            </Show>

            <Show when={adminsLoading()}>
              <p class="text-muted">Loading admins...</p>
            </Show>

            <Show when={!adminsLoading()}>
              <Show when={admins().length === 0}>
                <p class="text-muted" style="margin-bottom: 16px">No admins assigned yet.</p>
              </Show>

              <Show when={admins().length > 0}>
                <table class="admin-table admins-table">
                  <thead>
                    <tr>
                      <th>Name</th>
                      <th>Email</th>
                      <th>Role</th>
                      <th>Joined</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    <For each={admins()}>
                      {(admin) => (
                        <tr>
                          <td>{admin.displayName}</td>
                          <td><code>{admin.email}</code></td>
                          <td><span class="status-badge active">{admin.role}</span></td>
                          <td class="text-muted">{new Date(admin.joinedAt).toLocaleDateString()}</td>
                          <td>
                            <button class="btn-sm btn-danger" onClick={() => handleRemoveAdmin(admin.id)}>Remove</button>
                          </td>
                        </tr>
                      )}
                    </For>
                  </tbody>
                </table>
              </Show>

              <Show when={!showAddAdmin()}>
                <button class="btn-primary" style="margin-top: 16px" onClick={() => setShowAddAdmin(true)}>+ Add Admin</button>
              </Show>

              <Show when={showAddAdmin()}>
                <form onSubmit={handleAddAdmin} class="add-admin-form">
                  <h3>Add Admin</h3>
                  <div class="form-row">
                    <div class="form-group">
                      <label for="admin-email">Email</label>
                      <input
                        id="admin-email"
                        type="email"
                        value={adminEmail()}
                        onInput={(e) => setAdminEmail(e.currentTarget.value)}
                        placeholder="admin@example.com"
                        required
                      />
                    </div>
                    <div class="form-group">
                      <label for="admin-name">Display Name</label>
                      <input
                        id="admin-name"
                        type="text"
                        value={adminName()}
                        onInput={(e) => setAdminName(e.currentTarget.value)}
                        placeholder="John Doe"
                        required
                      />
                    </div>
                    <div class="form-group">
                      <label for="admin-pass">Password</label>
                      <input
                        id="admin-pass"
                        type="password"
                        value={adminPassword()}
                        onInput={(e) => setAdminPassword(e.currentTarget.value)}
                        placeholder="Initial password"
                        required
                      />
                    </div>
                  </div>
                  <div class="form-actions">
                    <button type="button" class="btn-ghost" onClick={() => setShowAddAdmin(false)}>Cancel</button>
                    <button type="submit" class="btn-primary" disabled={adminSaving()}>
                      {adminSaving() ? 'Adding...' : 'Add Admin'}
                    </button>
                  </div>
                </form>
              </Show>
            </Show>
          </div>
        </div>
      </Show>
    </div>
  )
}
