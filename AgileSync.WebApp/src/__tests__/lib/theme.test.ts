import { describe, it, expect, beforeEach, vi } from 'vitest'
import { getStoredTheme, applyTheme, isPwa, getPlatform } from '../../lib/theme'

describe('theme utilities', () => {
  beforeEach(() => {
    localStorage.clear()
    document.documentElement.removeAttribute('data-theme')
    // Remove any meta theme-color tags
    document.querySelectorAll('meta[name="theme-color"]').forEach(el => el.remove())
  })

  describe('getStoredTheme', () => {
    it('returns stored dark theme', () => {
      localStorage.setItem('agilesync_theme', 'dark')
      expect(getStoredTheme()).toBe('dark')
    })

    it('returns stored light theme', () => {
      localStorage.setItem('agilesync_theme', 'light')
      expect(getStoredTheme()).toBe('light')
    })

    it('ignores invalid stored values and falls back to system preference', () => {
      localStorage.setItem('agilesync_theme', 'invalid')
      // jsdom defaults to no color-scheme preference, so fallback should be 'dark'
      expect(getStoredTheme()).toBe('dark')
    })

    it('returns dark when no stored value and system prefers dark', () => {
      // jsdom default: prefers-color-scheme: light returns false
      expect(getStoredTheme()).toBe('dark')
    })
  })

  describe('applyTheme', () => {
    it('sets data-theme attribute on document element', () => {
      applyTheme('dark')
      expect(document.documentElement.getAttribute('data-theme')).toBe('dark')
    })

    it('persists theme to localStorage', () => {
      applyTheme('light')
      expect(localStorage.getItem('agilesync_theme')).toBe('light')
    })

    it('updates meta theme-color for dark theme', () => {
      const meta = document.createElement('meta')
      meta.setAttribute('name', 'theme-color')
      document.head.appendChild(meta)

      applyTheme('dark')
      expect(meta.getAttribute('content')).toBe('#0f1117')
    })

    it('updates meta theme-color for light theme', () => {
      const meta = document.createElement('meta')
      meta.setAttribute('name', 'theme-color')
      document.head.appendChild(meta)

      applyTheme('light')
      expect(meta.getAttribute('content')).toBe('#ffffff')
    })
  })

  describe('isPwa', () => {
    it('returns false in standard browser (jsdom default)', () => {
      expect(isPwa()).toBe(false)
    })
  })

  describe('getPlatform', () => {
    it('returns web in standard browser (jsdom default)', () => {
      expect(getPlatform()).toBe('web')
    })
  })
})
