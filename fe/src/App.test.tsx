import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import App from './App'
import { getWeatherByCity } from './api/client'
import { citiesByCountry, countries, osloWeather } from './test/fixtures'

vi.mock('./api/client', () => ({
  getCountries: vi.fn(async () => countries),
  getCitiesByCountry: vi.fn(async (code: string) => citiesByCountry[code] ?? null),
  getWeatherByCity: vi.fn(async (name: string) => (name === 'Oslo' ? osloWeather : null)),
}))

function saveFavorites(...cities: { cityId: number; cityName: string; countryCode: string; countryName: string }[]) {
  localStorage.setItem('myweather.favorites', JSON.stringify(cities))
}

const OSLO = { cityId: 14, cityName: 'Oslo', countryCode: 'NO', countryName: 'Norway' }
const JAKARTA = { cityId: 1, cityName: 'Jakarta', countryCode: 'ID', countryName: 'Indonesia' }

describe('App', () => {
  beforeEach(() => vi.clearAllMocks())

  it('shows the empty state without favorites', () => {
    render(<App />)

    expect(screen.getByRole('heading', { name: 'No favorite cities' })).toBeInTheDocument()
    expect(screen.getByText('0 / 3 cities')).toBeInTheDocument()
  })

  it('fills the grid with empty slots up to 3', async () => {
    saveFavorites(OSLO)
    render(<App />)

    expect(await screen.findByText('100°')).toBeInTheDocument()
    expect(screen.getAllByRole('button', { name: /Empty slot/ })).toHaveLength(2)
    expect(screen.getByText('1 / 3 cities')).toBeInTheDocument()
  })

  it('shows real stats from the API', async () => {
    saveFavorites(OSLO)
    render(<App />)

    const column = await screen.findByRole('button', { name: 'Open forecast for Oslo' })
    expect(await within(column).findByText('14 km/h NW')).toBeInTheDocument()
    expect(within(column).getByText('62%')).toBeInTheDocument()
    expect(within(column).getByText('1013 hPa')).toBeInTheDocument()
    expect(within(column).getByText('12:30')).toBeInTheDocument()
  })

  it('switches temperatures between °C and °F', async () => {
    saveFavorites(OSLO)
    const user = userEvent.setup()
    render(<App />)

    expect(await screen.findByText('100°')).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: '°F' }))

    expect(screen.getByText('212°')).toBeInTheDocument()
    expect(screen.getByText('9 mph NW')).toBeInTheDocument()
  })

  it('says when a city has no weather data yet', async () => {
    saveFavorites(JAKARTA)
    render(<App />)

    expect(await screen.findByText('No weather data yet')).toBeInTheDocument()
    expect(getWeatherByCity).toHaveBeenCalledWith('Jakarta', expect.anything())
  })

  it('removes a city with the × button without opening it', async () => {
    saveFavorites(OSLO, JAKARTA)
    const user = userEvent.setup()
    render(<App />)

    await user.click(await screen.findByRole('button', { name: 'Remove Oslo' }))

    expect(screen.queryByText('Oslo')).not.toBeInTheDocument()
    expect(screen.getByText('1 / 3 cities')).toBeInTheDocument()
    expect(screen.queryByRole('navigation', { name: 'Favorite cities' })).not.toBeInTheDocument()
  })

  it('opens the detail view with a rail and goes back', async () => {
    saveFavorites(OSLO, JAKARTA)
    const user = userEvent.setup()
    render(<App />)

    await user.click(await screen.findByRole('button', { name: 'Open forecast for Oslo' }))
    expect(screen.getByRole('heading', { level: 1, name: 'Oslo' })).toBeInTheDocument()
    const rail = screen.getByRole('navigation', { name: 'Favorite cities' })
    expect(within(rail).getByRole('button', { name: /Oslo/ })).toHaveAttribute('aria-current', 'true')

    await user.click(within(rail).getByRole('button', { name: /Jakarta/ }))
    expect(screen.getByRole('heading', { level: 1, name: 'Jakarta' })).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: '← Overview' }))
    expect(screen.queryByRole('navigation', { name: 'Favorite cities' })).not.toBeInTheDocument()
  })

  it('adds a city through the dialog', async () => {
    const user = userEvent.setup()
    render(<App />)

    await user.click(screen.getAllByRole('button', { name: 'Add city' })[0])
    await user.click(screen.getByRole('button', { name: /1 · Country/ }))
    await user.click(await screen.findByRole('option', { name: /Norway/ }))
    await user.click(await screen.findByRole('option', { name: /Oslo/ }))
    await user.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Add city' }))

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
    expect(await screen.findByText('100°')).toBeInTheDocument()
    expect(JSON.parse(localStorage.getItem('myweather.favorites')!)).toEqual([OSLO])
  })
})
