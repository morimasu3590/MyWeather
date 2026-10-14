import { useEffect, useRef, useState, type KeyboardEvent } from 'react'
import { TriangleAlert, X } from 'lucide-react'
import { getCitiesByCountry, getCountries } from '../api/client'
import type { City, Country } from '../api/types'
import { MAX_FAVORITES } from '../state/useFavorites'
import { Corners } from './Corners'
import { SearchSelect } from './SearchSelect'

interface AddCityDialogProps {
  /** City ids already on the dashboard; they can't be picked again. */
  existingCityIds: number[]
  limitReached: boolean
  onAdd: (city: City) => void
  onClose: () => void
}

type Load<T> = { status: 'loading' } | { status: 'ready'; data: T } | { status: 'error' }

/** Pick a country, then a city, both searchable. Mirrors AddCityDialog.dc.html. */
export function AddCityDialog({ existingCityIds, limitReached, onAdd, onClose }: AddCityDialogProps) {
  const [countries, setCountries] = useState<Load<Country[]>>({ status: 'loading' })
  const [cities, setCities] = useState<Load<City[]> | null>(null)
  const [countryCode, setCountryCode] = useState<string | null>(null)
  const [cityId, setCityId] = useState<number | null>(null)
  const [openMenu, setOpenMenu] = useState<'country' | 'city' | null>(null)
  const [warn, setWarn] = useState(false)
  const dialogRef = useRef<HTMLDivElement>(null)

  // Focus the dialog on open and give focus back to whatever opened it on close.
  useEffect(() => {
    const opener = document.activeElement as HTMLElement | null
    dialogRef.current?.focus()
    return () => opener?.focus?.()
  }, [])

  useEffect(() => {
    const controller = new AbortController()
    getCountries(controller.signal)
      .then((data) => setCountries({ status: 'ready', data }))
      .catch(() => !controller.signal.aborted && setCountries({ status: 'error' }))
    return () => controller.abort()
  }, [])

  useEffect(() => {
    if (!countryCode) return
    const controller = new AbortController()
    getCitiesByCountry(countryCode, controller.signal)
      .then((data) => setCities({ status: 'ready', data: data ?? [] }))
      .catch(() => !controller.signal.aborted && setCities({ status: 'error' }))
    return () => controller.abort()
  }, [countryCode])

  const countryList = countries.status === 'ready' ? countries.data : []
  const cityList = cities?.status === 'ready' ? cities.data : []
  const country = countryList.find((c) => c.countryCode === countryCode)
  const city = cityList.find((c) => c.cityId === cityId)

  const pickCountry = (code: string) => {
    if (code !== countryCode) setCities({ status: 'loading' })
    setCountryCode(code)
    setCityId(null)
    setWarn(false)
    setOpenMenu('city')
  }

  const pickCity = (id: string) => {
    setCityId(Number(id))
    setWarn(false)
    setOpenMenu(null)
  }

  const add = () => {
    if (!city) return
    if (limitReached) {
      setWarn(true)
      return
    }
    onAdd(city)
  }

  const handleKeyDown = (event: KeyboardEvent<HTMLDivElement>) => {
    if (event.key === 'Escape') {
      event.stopPropagation()
      if (openMenu) setOpenMenu(null)
      else onClose()
    }
  }

  return (
    <div
      className="dialog-overlay"
      onMouseDown={(event) => event.target === event.currentTarget && onClose()}
    >
      <div
        ref={dialogRef}
        className="add-dialog blueprint"
        role="dialog"
        aria-modal="true"
        aria-labelledby="add-city-title"
        tabIndex={-1}
        onKeyDown={handleKeyDown}
      >
        <Corners />
        <div className="add-dialog-head">
          <h2 id="add-city-title" className="dialog-title">
            Add a city
          </h2>
          <button type="button" className="btn btn-ghost btn-icon icon-ghost" title="Close" aria-label="Close" onClick={onClose}>
            <X size={18} strokeWidth={1.5} aria-hidden="true" />
          </button>
        </div>
        <p className="text-muted add-dialog-intro">
          Choose a country, then a city. You can track up to {MAX_FAVORITES} cities.
        </p>

        <SearchSelect
          label="1 · Country"
          placeholder="Select a country"
          searchPlaceholder="Search countries"
          noun="countries"
          options={countryList.map((c) => ({ value: c.countryCode, label: c.countryName, meta: c.countryCode }))}
          value={countryCode}
          open={openMenu === 'country'}
          message={
            countries.status === 'loading'
              ? 'Loading countries…'
              : countries.status === 'error'
                ? "Couldn't load countries. Close and try again."
                : undefined
          }
          onOpenChange={(open) => setOpenMenu(open ? 'country' : null)}
          onSelect={pickCountry}
        />

        <SearchSelect
          label="2 · City"
          placeholder={country ? 'Select a city' : 'Select a country first'}
          searchPlaceholder={`Search cities in ${country?.countryName ?? ''}`.trim()}
          noun="cities"
          options={cityList.map((c) => {
            const added = existingCityIds.includes(c.cityId)
            return { value: String(c.cityId), label: c.cityName, meta: added ? 'Already added' : undefined, disabled: added }
          })}
          value={cityId === null ? null : String(cityId)}
          open={openMenu === 'city'}
          disabled={!country}
          message={
            cities?.status === 'loading'
              ? 'Loading cities…'
              : cities?.status === 'error'
                ? "Couldn't load cities. Pick the country again."
                : undefined
          }
          onOpenChange={(open) => setOpenMenu(open ? 'city' : null)}
          onSelect={pickCity}
        />

        {warn && city && (
          <div role="alert" className="limit-alert">
            <TriangleAlert size={20} strokeWidth={1.5} aria-hidden="true" />
            <div>
              <div className="limit-alert-title">
                City limit reached: {MAX_FAVORITES} of {MAX_FAVORITES}
              </div>
              <div className="limit-alert-body">Remove a city from your dashboard before adding {city.cityName}.</div>
            </div>
          </div>
        )}

        <div className="dialog-actions">
          <button type="button" className="btn btn-secondary" onClick={onClose}>
            Cancel
          </button>
          <button type="button" className="btn btn-primary blueprint" disabled={!city} onClick={add}>
            <Corners />
            Add city
          </button>
        </div>
      </div>
    </div>
  )
}
