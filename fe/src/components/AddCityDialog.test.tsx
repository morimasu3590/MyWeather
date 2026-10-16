import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { citiesByCountry, countries } from '../test/fixtures'
import { AddCityDialog } from './AddCityDialog'

vi.mock('../api/client', () => ({
  getCountries: vi.fn(async () => countries),
  getCitiesByCountry: vi.fn(async (code: string) => citiesByCountry[code] ?? null),
  getFavoriteById: vi.fn(),
}))

function renderDialog(props: Partial<Parameters<typeof AddCityDialog>[0]> = {}) {
  const onAdd = vi.fn()
  const onClose = vi.fn()
  render(<AddCityDialog existingCityIds={[]} limitReached={false} onAdd={onAdd} onClose={onClose} {...props} />)
  return { onAdd, onClose, user: userEvent.setup() }
}

const countryTrigger = () => screen.getByRole('button', { name: /1 · Country/ })
const cityTrigger = () => screen.getByRole('button', { name: /2 · City/ })
const addButton = () => screen.getByRole('button', { name: 'Add city' })

async function pick(user: ReturnType<typeof userEvent.setup>, option: string) {
  await user.click(await screen.findByRole('option', { name: new RegExp(option) }))
}

describe('AddCityDialog', () => {
  beforeEach(() => vi.clearAllMocks())

  it('filters countries by the search text', async () => {
    const { user } = renderDialog()

    await user.click(countryTrigger())
    await screen.findByRole('option', { name: /Indonesia/ })
    await user.type(screen.getByRole('combobox', { name: 'Search countries' }), 'in')

    const names = screen.getAllByRole('option').map((o) => within(o).getAllByText(/./)[0].textContent)
    expect(names).toEqual(['Indonesia'])
  })

  it('says when no country matches', async () => {
    const { user } = renderDialog()

    await user.click(countryTrigger())
    await screen.findByRole('option', { name: /Indonesia/ })
    await user.type(screen.getByRole('combobox', { name: 'Search countries' }), 'zz')

    expect(screen.getByText('No countries match "zz".')).toBeInTheDocument()
  })

  it('keeps the city field disabled until a country is chosen', async () => {
    const { user } = renderDialog()

    expect(cityTrigger()).toBeDisabled()
    expect(cityTrigger()).toHaveTextContent('Select a country first')

    await user.click(countryTrigger())
    await pick(user, 'Indonesia')

    expect(cityTrigger()).toBeEnabled()
    expect(await screen.findByRole('option', { name: /Jakarta/ })).toBeInTheDocument()
  })

  it('cannot pick a city that is already a favorite', async () => {
    const { user } = renderDialog({ existingCityIds: [1] })

    await user.click(countryTrigger())
    await pick(user, 'Indonesia')
    const jakarta = await screen.findByRole('option', { name: /Jakarta/ })

    expect(jakarta).toHaveAttribute('aria-disabled', 'true')
    expect(jakarta).toHaveTextContent('Already added')
    await user.click(jakarta)
    expect(addButton()).toBeDisabled()
  })

  it('disables Add until a city is picked, then adds it', async () => {
    const { user, onAdd } = renderDialog()

    expect(addButton()).toBeDisabled()
    await user.click(countryTrigger())
    await pick(user, 'Indonesia')
    await pick(user, 'Jakarta')

    expect(cityTrigger()).toHaveTextContent('Jakarta')
    await user.click(addButton())
    expect(onAdd).toHaveBeenCalledWith(citiesByCountry.ID[0], '')
  })

  it('adds the city with its notes, trimmed', async () => {
    const { user, onAdd } = renderDialog()

    await user.click(countryTrigger())
    await pick(user, 'Indonesia')
    await pick(user, 'Jakarta')
    await user.type(screen.getByLabelText('3 · Notes (optional)'), '  Visiting in May  ')

    expect(screen.getByText('19 / 500')).toBeInTheDocument()
    await user.click(addButton())
    expect(onAdd).toHaveBeenCalledWith(citiesByCountry.ID[0], 'Visiting in May')
  })

  it('limits notes to 500 characters', () => {
    renderDialog()

    expect(screen.getByLabelText('3 · Notes (optional)')).toHaveAttribute('maxLength', '500')
  })

  it('warns instead of adding when the limit is reached', async () => {
    const { user, onAdd } = renderDialog({ limitReached: true })

    await user.click(countryTrigger())
    await pick(user, 'Indonesia')
    await pick(user, 'Jakarta')
    await user.click(addButton())

    expect(screen.getByRole('alert')).toHaveTextContent('City limit reached: 3 of 3')
    expect(screen.getByRole('alert')).toHaveTextContent('before adding Jakarta')
    expect(onAdd).not.toHaveBeenCalled()
  })

  it('closes on Cancel, on the close button and on Escape', async () => {
    const { user, onClose } = renderDialog()

    await user.click(screen.getByRole('button', { name: 'Cancel' }))
    await user.click(screen.getByRole('button', { name: 'Close' }))
    screen.getByRole('dialog').focus()
    await user.keyboard('{Escape}')

    expect(onClose).toHaveBeenCalledTimes(3)
  })

  it('closes only the open menu on the first Escape', async () => {
    const { user, onClose } = renderDialog()

    await user.click(countryTrigger())
    await screen.findByRole('option', { name: /Indonesia/ })
    await user.keyboard('{Escape}')

    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
    expect(onClose).not.toHaveBeenCalled()
  })

  it('picks a country with the keyboard', async () => {
    const { user } = renderDialog()

    await user.click(countryTrigger())
    await screen.findByRole('option', { name: /Indonesia/ })
    await user.keyboard('{ArrowDown}{Enter}')

    expect(countryTrigger()).toHaveTextContent('Canada')
  })
})
