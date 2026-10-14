import { act, renderHook } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { loadFavorites, MAX_FAVORITES, useFavorites, type Favorite } from './useFavorites'

const city = (cityId: number, cityName = `City ${cityId}`): Favorite => ({
  cityId,
  cityName,
  countryCode: 'NO',
  countryName: 'Norway',
})

describe('useFavorites', () => {
  afterEach(() => vi.restoreAllMocks())

  it('adds and removes favorites and persists them', () => {
    const { result } = renderHook(() => useFavorites())

    act(() => {
      result.current.add(city(1, 'Oslo'))
    })
    expect(result.current.favorites.map((f) => f.cityName)).toEqual(['Oslo'])
    expect(loadFavorites()).toEqual([city(1, 'Oslo')])

    act(() => result.current.remove(1))
    expect(result.current.favorites).toEqual([])
    expect(loadFavorites()).toEqual([])
  })

  it('ignores a city that is already a favorite', () => {
    const { result } = renderHook(() => useFavorites())

    act(() => {
      result.current.add(city(1))
    })
    let added = true
    act(() => {
      added = result.current.add(city(1))
    })

    expect(added).toBe(false)
    expect(result.current.favorites).toHaveLength(1)
  })

  it(`stops at ${MAX_FAVORITES} cities`, () => {
    const { result } = renderHook(() => useFavorites())

    for (const id of [1, 2, 3]) {
      act(() => {
        result.current.add(city(id))
      })
    }
    let added = true
    act(() => {
      added = result.current.add(city(4))
    })

    expect(added).toBe(false)
    expect(result.current.isFull).toBe(true)
    expect(result.current.favorites.map((f) => f.cityId)).toEqual([1, 2, 3])
  })

  it('keeps working in memory when storage throws', () => {
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
      throw new Error('blocked')
    })
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
      throw new Error('blocked')
    })
    const { result } = renderHook(() => useFavorites())

    act(() => {
      result.current.add(city(1, 'Oslo'))
    })

    expect(result.current.favorites.map((f) => f.cityName)).toEqual(['Oslo'])
  })

  it('ignores corrupt stored data', () => {
    localStorage.setItem('myweather.favorites', '{not json')
    expect(loadFavorites()).toEqual([])
  })
})
