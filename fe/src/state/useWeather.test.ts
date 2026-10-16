import { act, renderHook, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { getFavoriteById } from '../api/client'
import type { FavoriteCityDetail } from '../api/types'
import { osloWeather } from '../test/fixtures'
import { useWeather } from './useWeather'

vi.mock('../api/client', () => ({ getFavoriteById: vi.fn() }))

const detail = (id: number, withWeather = true): FavoriteCityDetail => ({
  id,
  notes: '',
  weather: withWeather ? osloWeather : null,
})

/** Favorite ids each call was made for, in order. */
const requestedIds = () => vi.mocked(getFavoriteById).mock.calls.map(([id]) => id)

function renderWeather(initialIds: number[], onGone?: (id: number) => void) {
  return renderHook(({ ids }) => useWeather(ids, onGone), { initialProps: { ids: initialIds } })
}

describe('useWeather', () => {
  beforeEach(() => {
    vi.mocked(getFavoriteById).mockImplementation(async (id) => detail(id))
  })

  afterEach(() => {
    vi.resetAllMocks()
    vi.restoreAllMocks()
  })

  it('loads each favorite once and marks one without weather as missing', async () => {
    vi.mocked(getFavoriteById).mockImplementation(async (id) => detail(id, id === 1))
    const { result } = renderWeather([1, 2])

    await waitFor(() => expect(result.current.states[2]).toEqual({ status: 'missing' }))
    expect(result.current.states[1]).toEqual({ status: 'ready', weather: osloWeather })
    expect(requestedIds()).toEqual([1, 2])
  })

  it('fetches only a newly added favorite', async () => {
    const { result, rerender } = renderWeather([1, 2])
    await waitFor(() => expect(result.current.states[2]?.status).toBe('ready'))

    rerender({ ids: [1, 2, 3] })

    await waitFor(() => expect(result.current.states[3]?.status).toBe('ready'))
    expect(requestedIds()).toEqual([1, 2, 3])
  })

  it('cancels only a removed favorite and keeps the others loading', async () => {
    const signals = new Map<number, AbortSignal>()
    const pending = new Map<number, () => void>()
    vi.mocked(getFavoriteById).mockImplementation(
      (id, signal) =>
        new Promise((resolve) => {
          signals.set(id, signal!)
          pending.set(id, () => resolve(detail(id)))
        }),
    )
    const { result, rerender } = renderWeather([1, 2])

    rerender({ ids: [1] })

    expect(signals.get(2)?.aborted).toBe(true)
    expect(signals.get(1)?.aborted).toBe(false)
    await act(async () => pending.get(1)!())
    expect(result.current.states[1]?.status).toBe('ready')
    expect(result.current.states[2]).toBeUndefined()
    expect(requestedIds()).toEqual([1, 2])
  })

  it('does not refetch the others when a favorite is removed', async () => {
    const { result, rerender } = renderWeather([1, 2])
    await waitFor(() => expect(result.current.states[2]?.status).toBe('ready'))

    rerender({ ids: [2] })

    expect(result.current.states[1]).toBeUndefined()
    expect(requestedIds()).toEqual([1, 2])
  })

  it('fetches again when a removed id comes back for a new favorite', async () => {
    const { result, rerender } = renderWeather([1])
    await waitFor(() => expect(result.current.states[1]?.status).toBe('ready'))

    rerender({ ids: [] })
    rerender({ ids: [1] })

    await waitFor(() => expect(requestedIds()).toEqual([1, 1]))
  })

  it('retries only the favorites that failed', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {})
    vi.mocked(getFavoriteById).mockImplementation(async (id) => {
      if (id === 2 && requestedIds().filter((x) => x === 2).length === 1) throw new Error('offline')
      return detail(id)
    })
    const { result } = renderWeather([1, 2])
    await waitFor(() => expect(result.current.states[2]).toEqual({ status: 'error' }))

    act(() => result.current.retry())

    expect(result.current.states[2]).toEqual({ status: 'loading' })
    await waitFor(() => expect(result.current.states[2]?.status).toBe('ready'))
    expect(requestedIds()).toEqual([1, 2, 2])
  })

  it('reports a favorite that no longer exists', async () => {
    vi.mocked(getFavoriteById).mockResolvedValue(null)
    const onGone = vi.fn()
    const { result } = renderWeather([9], onGone)

    await waitFor(() => expect(result.current.states[9]).toEqual({ status: 'missing' }))
    expect(onGone).toHaveBeenCalledWith(9)
  })

  it('cancels requests on unmount', () => {
    let signal: AbortSignal | undefined
    vi.mocked(getFavoriteById).mockImplementation((_id, s) => {
      signal = s
      return new Promise(() => {})
    })
    const { unmount } = renderWeather([1])

    unmount()

    expect(signal?.aborted).toBe(true)
  })
})
