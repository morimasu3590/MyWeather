import { act, renderHook, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { addFavorite, deleteFavorite, getFavorites } from '../api/client'
import type { City } from '../api/types'
import { favorite } from '../test/fixtures'
import { MAX_FAVORITES, useFavorites } from './useFavorites'

vi.mock('../api/client', () => ({
  getFavorites: vi.fn(),
  addFavorite: vi.fn(),
  deleteFavorite: vi.fn(),
}))

const city = (cityId: number, cityName = `City ${cityId}`): City => ({
  cityId,
  cityName,
  countryCode: 'NO',
  countryName: 'Norway',
})

async function renderLoaded() {
  const hook = renderHook(() => useFavorites())
  await waitFor(() => expect(hook.result.current.status).toBe('ready'))
  return hook
}

describe('useFavorites', () => {
  beforeEach(() => {
    vi.mocked(getFavorites).mockResolvedValue([])
    vi.mocked(addFavorite).mockImplementation(async (cityId) => cityId + 100)
    vi.mocked(deleteFavorite).mockResolvedValue(undefined)
  })

  afterEach(() => {
    vi.resetAllMocks()
    vi.restoreAllMocks()
  })

  it('loads favorites from the API', async () => {
    vi.mocked(getFavorites).mockResolvedValue([favorite(7, city(1, 'Oslo'), 'Fjords')])
    const { result } = renderHook(() => useFavorites())

    expect(result.current.status).toBe('loading')
    await waitFor(() => expect(result.current.status).toBe('ready'))
    expect(result.current.favorites).toEqual([
      { id: 7, cityId: 1, cityName: 'Oslo', countryCode: 'NO', countryName: 'Norway', notes: 'Fjords' },
    ])
  })

  it('reports a failed load and reloads on request', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {})
    vi.mocked(getFavorites).mockRejectedValueOnce(new Error('offline'))
    const { result } = renderHook(() => useFavorites())

    await waitFor(() => expect(result.current.status).toBe('error'))
    act(() => result.current.reload())

    expect(result.current.status).toBe('loading')
    await waitFor(() => expect(result.current.status).toBe('ready'))
    expect(getFavorites).toHaveBeenCalledTimes(2)
  })

  it('adds a favorite through the API and keeps its id', async () => {
    const { result } = await renderLoaded()

    let added = false
    await act(async () => {
      added = await result.current.add(city(1, 'Oslo'), 'Fjords')
    })

    expect(added).toBe(true)
    expect(addFavorite).toHaveBeenCalledWith(1, 'Fjords')
    expect(result.current.favorites).toEqual([
      { id: 101, cityId: 1, cityName: 'Oslo', countryCode: 'NO', countryName: 'Norway', notes: 'Fjords' },
    ])
  })

  it('ignores a city that is already a favorite without calling the API', async () => {
    vi.mocked(getFavorites).mockResolvedValue([favorite(7, city(1))])
    const { result } = await renderLoaded()

    let added = true
    await act(async () => {
      added = await result.current.add(city(1))
    })

    expect(added).toBe(false)
    expect(addFavorite).not.toHaveBeenCalled()
    expect(result.current.favorites).toHaveLength(1)
  })

  it(`stops at ${MAX_FAVORITES} cities without calling the API`, async () => {
    vi.mocked(getFavorites).mockResolvedValue([1, 2, 3].map((id) => favorite(id, city(id))))
    const { result } = await renderLoaded()

    let added = true
    await act(async () => {
      added = await result.current.add(city(4))
    })

    expect(added).toBe(false)
    expect(result.current.isFull).toBe(true)
    expect(addFavorite).not.toHaveBeenCalled()
  })

  it('reports a failed add and keeps the list unchanged', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {})
    vi.mocked(addFavorite).mockRejectedValueOnce(new Error('offline'))
    const { result } = await renderLoaded()

    let added = true
    await act(async () => {
      added = await result.current.add(city(1, 'Oslo'))
    })

    expect(added).toBe(false)
    expect(result.current.favorites).toEqual([])
    expect(result.current.actionError).toBe("Couldn't add Oslo to your favorites.")

    act(() => result.current.dismissError())
    expect(result.current.actionError).toBeNull()
  })

  it('removes a favorite by its favorite id', async () => {
    vi.mocked(getFavorites).mockResolvedValue([favorite(7, city(1)), favorite(8, city(2))])
    const { result } = await renderLoaded()

    await act(() => result.current.remove(1))

    expect(deleteFavorite).toHaveBeenCalledWith(7)
    expect(result.current.favorites.map((f) => f.cityId)).toEqual([2])
  })

  it('puts a favorite back in place when removing it fails', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {})
    vi.mocked(deleteFavorite).mockRejectedValueOnce(new Error('offline'))
    vi.mocked(getFavorites).mockResolvedValue([favorite(7, city(1)), favorite(8, city(2, 'Bergen'))])
    const { result } = await renderLoaded()

    await act(() => result.current.remove(2))

    expect(result.current.favorites.map((f) => f.cityId)).toEqual([1, 2])
    expect(result.current.actionError).toBe("Couldn't remove Bergen from your favorites.")
  })
})
