/** Supported theme values. */
export type Theme = 'dark' | 'light'

/** Supported platform types for theme preferences. */
export type Platform = 'web' | 'pwa'

/** Key used to persist the theme in localStorage. */
const THEME_STORAGE_KEY = 'agilesync_theme'

/**
 * Detects whether the app is running as an installed PWA.
 * Checks the display-mode media query and the iOS standalone flag.
 */
export function isPwa(): boolean {
  return (
    window.matchMedia('(display-mode: standalone)').matches ||
    (navigator as any).standalone === true
  )
}

/**
 * Returns the current platform type based on PWA detection.
 * @returns `'pwa'` if running as a PWA, otherwise `'web'`.
 */
export function getPlatform(): Platform {
  return isPwa() ? 'pwa' : 'web'
}

/**
 * Applies the given theme to the document and persists it in localStorage.
 * Also updates the PWA status bar meta theme-color.
 * @param theme - The theme to apply ('dark' or 'light').
 */
export function applyTheme(theme: Theme): void {
  document.documentElement.setAttribute('data-theme', theme)
  localStorage.setItem(THEME_STORAGE_KEY, theme)

  const metaThemeColor = document.querySelector('meta[name="theme-color"]')
  if (metaThemeColor) {
    metaThemeColor.setAttribute('content', theme === 'dark' ? '#0f1117' : '#ffffff')
  }
}

/**
 * Retrieves the stored theme from localStorage, falling back to system preference.
 * @returns The stored or system-preferred theme.
 */
export function getStoredTheme(): Theme {
  const stored = localStorage.getItem(THEME_STORAGE_KEY)
  if (stored === 'dark' || stored === 'light') return stored
  if (window.matchMedia('(prefers-color-scheme: light)').matches) return 'light'
  return 'dark'
}

/**
 * Initializes the theme on app startup. Should be called before first render
 * to prevent a flash of unstyled content.
 */
export function initTheme(): void {
  applyTheme(getStoredTheme())
}
