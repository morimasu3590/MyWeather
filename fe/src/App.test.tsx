import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import App from './App'
import { addFavorite, deleteFavorite, getFavoriteById, getFavorites } from './api/client'
import { citiesByCountry, countries, favorite, osloWeather } from './test/fixtures'

vi.mock('./api/client', () => ({
  getCountries: vi.fn(async () => countries),
  getCitiesByCountry: vi.fn(async (code: string) => citiesByCountry[code] ?? null),
  // Favorite 5 is Oslo below and 42 is the id addFavorite hands out; only those have weather.
  getFavoriteById: vi.fn(async (id: number) => ({ id, notes: '', weather: [5, 42].includes(id) ? osloWeather : null })),
  getFavorites: vi.fn(async () => []),
  addFavorite: vi.fn(async () => 42),
  deleteFavorite: vi.fn(async () => undefined),
}))

const OSLO = favorite(5, citiesByCountry.NO[0])
const JAKARTA = favorite(6, citiesByCountry.ID[0])

function saveFavorites(...favorites: ReturnType<typeof favorite>[]) {
  vi.mocked(getFavorites).mockResolvedValue(favorites)
}

describe('App', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(getFavorites).mockResolvedValue([])
  })

  afterEach(() => vi.restoreAllMocks())

  it('shows the empty state without favorites', async () => {
    render(<App />)

    expect(screen.getByText('Loading favorite cities…')).toBeInTheDocument()
    expect(await screen.findByRole('heading', { name: 'No favorite cities' })).toBeInTheDocument()
    expect(screen.getByText('0 / 3 cities')).toBeInTheDocument()
  })

  it('offers a retry when favorites fail to load', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {})
    vi.mocked(getFavorites).mockRejectedValueOnce(new Error('offline')).mockResolvedValueOnce([OSLO])
    const user = userEvent.setup()
    render(<App />)

    expect(await screen.findByText("Couldn't load your favorite cities.")).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Retry' }))

    expect(await screen.findByText('100°')).toBeInTheDocument()
    expect(getFavorites).toHaveBeenCalledTimes(2)
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

  it('retries only the cities whose weather failed to load', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {})
    vi.mocked(getFavoriteById).mockRejectedValueOnce(new Error('offline'))
    saveFavorites(OSLO, JAKARTA)
    const user = userEvent.setup()
    render(<App />)

    expect(await screen.findByText("Couldn't reach the weather service for some cities.")).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Retry' }))

    expect(await screen.findByText('100°')).toBeInTheDocument()
    expect(screen.queryByText("Couldn't reach the weather service for some cities.")).not.toBeInTheDocument()
    expect(vi.mocked(getFavoriteById).mock.calls.map(([id]) => id)).toEqual([OSLO.id, JAKARTA.id, OSLO.id])
  })

  it('reloads the favorites when one was deleted elsewhere', async () => {
    vi.mocked(getFavoriteById).mockResolvedValueOnce(null)
    vi.mocked(getFavorites).mockResolvedValueOnce([OSLO]).mockResolvedValueOnce([])
    render(<App />)

    expect(await screen.findByRole('heading', { name: 'No favorite cities' })).toBeInTheDocument()
    expect(getFavorites).toHaveBeenCalledTimes(2)
  })

  it('says when a city has no weather data yet', async () => {
    saveFavorites(JAKARTA)
    render(<App />)

    expect(await screen.findByText('No weather data yet')).toBeInTheDocument()
    expect(getFavoriteById).toHaveBeenCalledWith(JAKARTA.id, expect.anything())
  })

  it('removes a city with the × button without opening it', async () => {
    saveFavorites(OSLO, JAKARTA)
    const user = userEvent.setup()
    render(<App />)

    await user.click(await screen.findByRole('button', { name: 'Remove Oslo' }))

    expect(screen.queryByText('Oslo')).not.toBeInTheDocument()
    expect(screen.getByText('1 / 3 cities')).toBeInTheDocument()
    expect(screen.queryByRole('navigation', { name: 'Favorite cities' })).not.toBeInTheDocument()
    expect(deleteFavorite).toHaveBeenCalledWith(OSLO.id)
  })

  it('puts a city back and says so when removing it fails', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {})
    vi.mocked(deleteFavorite).mockRejectedValueOnce(new Error('offline'))
    saveFavorites(OSLO, JAKARTA)
    const user = userEvent.setup()
    render(<App />)

    await user.click(await screen.findByRole('button', { name: 'Remove Oslo' }))

    expect(await screen.findByText("Couldn't remove Oslo from your favorites.")).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Remove Oslo' })).toBeInTheDocument()
    expect(screen.getByText('2 / 3 cities')).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Dismiss' }))
    expect(screen.queryByText("Couldn't remove Oslo from your favorites.")).not.toBeInTheDocument()
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

  async function addOsloThroughDialog(notes = '') {
    const user = userEvent.setup()
    render(<App />)

    await screen.findByRole('heading', { name: 'No favorite cities' })
    await user.click(screen.getAllByRole('button', { name: 'Add city' })[0])
    await user.click(screen.getByRole('button', { name: /1 · Country/ }))
    await user.click(await screen.findByRole('option', { name: /Norway/ }))
    await user.click(await screen.findByRole('option', { name: /Oslo/ }))
    if (notes) await user.type(screen.getByLabelText('3 · Notes (optional)'), notes)
    await user.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Add city' }))
    return user
  }

  it('adds a city through the dialog', async () => {
    await addOsloThroughDialog()

    expect(await screen.findByText('100°')).toBeInTheDocument()
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
    expect(addFavorite).toHaveBeenCalledWith(14, '')
    expect(screen.getByText('1 / 3 cities')).toBeInTheDocument()
  })

  it('saves notes with a new city and shows them in the column and the detail view', async () => {
    const user = await addOsloThroughDialog('Fjords trip')

    const column = await screen.findByRole('button', { name: 'Open forecast for Oslo' })
    expect(addFavorite).toHaveBeenCalledWith(14, 'Fjords trip')
    expect(within(column).getByText('Fjords trip')).toBeInTheDocument()

    await user.click(column)
    expect(screen.getByRole('heading', { name: 'Notes' })).toBeInTheDocument()
    expect(screen.getByText('Fjords trip')).toBeInTheDocument()
  })

  it('shows stored notes and leaves them out when empty', async () => {
    saveFavorites(favorite(5, citiesByCountry.NO[0], 'Fjords'), JAKARTA)
    const user = userEvent.setup()
    render(<App />)

    expect(await screen.findByText('Fjords')).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Open forecast for Jakarta' }))
    expect(screen.queryByRole('heading', { name: 'Notes' })).not.toBeInTheDocument()
  })

  it('says so when adding a city fails', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {})
    vi.mocked(addFavorite).mockRejectedValueOnce(new Error('offline'))

    await addOsloThroughDialog()

    expect(await screen.findByText("Couldn't add Oslo to your favorites.")).toBeInTheDocument()
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'No favorite cities' })).toBeInTheDocument()
  })
})
