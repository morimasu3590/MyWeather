import { useMemo, useState } from 'react'
import { RotateCw } from 'lucide-react'
import type { City } from './api/types'
import { AddCityDialog } from './components/AddCityDialog'
import { AppHeader } from './components/AppHeader'
import { CityColumn } from './components/CityColumn'
import { CityDetail } from './components/CityDetail'
import { Corners } from './components/Corners'
import { EmptySlot } from './components/EmptySlot'
import { buildCityView } from './lib/cityView'
import type { Unit } from './lib/units'
import { MAX_FAVORITES, useFavorites } from './state/useFavorites'
import { useWeather } from './state/useWeather'

export default function App() {
  const { favorites, status, reload, add, remove, isFull, actionError, dismissError } = useFavorites()
  const { states, retry } = useWeather(favorites.map((f) => f.id), reload)
  const [unit, setUnit] = useState<Unit>('C')
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const [dialogOpen, setDialogOpen] = useState(false)

  const views = useMemo(
    () => favorites.map((f) => buildCityView(f, states[f.id], unit)),
    [favorites, states, unit],
  )
  const selected = views.find((v) => v.favorite.cityId === selectedId)
  const hasError = views.some((v) => v.status === 'error')

  const openDialog = () => setDialogOpen(true)

  const handleRemove = (cityId: number) => {
    void remove(cityId)
    if (selectedId === cityId) setSelectedId(null)
  }

  // A failed add closes the dialog too; the error banner says what happened.
  const handleAdd = async (city: City, notes: string) => {
    await add(city, notes)
    setDialogOpen(false)
  }

  return (
    <div className="app">
      <AppHeader count={favorites.length} unit={unit} onUnitChange={setUnit} onAddCity={openDialog} />

      <main>
        {actionError && (
          <div className="notice">
            <div className="banner" role="alert">
              <span>{actionError}</span>
              <button type="button" className="btn btn-secondary" onClick={dismissError}>
                Dismiss
              </button>
            </div>
          </div>
        )}
        {status === 'loading' ? (
          <div className="empty-state">
            <p className="text-muted">Loading favorite cities…</p>
          </div>
        ) : status === 'error' ? (
          <div className="page">
            <div className="banner" role="alert">
              <span>Couldn't load your favorite cities.</span>
              <button type="button" className="btn btn-secondary" onClick={reload}>
                <RotateCw size={16} strokeWidth={1.5} aria-hidden="true" />
                Retry
              </button>
            </div>
          </div>
        ) : selected ? (
          <CityDetail views={views} selected={selected} onSelect={setSelectedId} onBack={() => setSelectedId(null)} />
        ) : favorites.length === 0 ? (
          <div className="empty-state">
            <h2>No favorite cities</h2>
            <p className="text-muted">Add a city to start tracking its weather.</p>
            <button type="button" className="btn btn-primary blueprint" onClick={openDialog}>
              <Corners />
              Add city
            </button>
          </div>
        ) : (
          <div className="page">
            {hasError && (
              <div className="banner" role="alert">
                <span>Couldn't reach the weather service for some cities.</span>
                <button type="button" className="btn btn-secondary" onClick={retry}>
                  <RotateCw size={16} strokeWidth={1.5} aria-hidden="true" />
                  Retry
                </button>
              </div>
            )}
            <div className="city-grid">
              {views.map((view) => (
                <CityColumn
                  key={view.favorite.cityId}
                  view={view}
                  onOpen={() => setSelectedId(view.favorite.cityId)}
                  onRemove={() => handleRemove(view.favorite.cityId)}
                />
              ))}
              {Array.from({ length: Math.max(0, MAX_FAVORITES - favorites.length) }, (_, i) => (
                <EmptySlot key={`slot-${i}`} onClick={openDialog} />
              ))}
            </div>
          </div>
        )}
      </main>

      {dialogOpen && (
        <AddCityDialog
          existingCityIds={favorites.map((f) => f.cityId)}
          limitReached={isFull}
          onAdd={handleAdd}
          onClose={() => setDialogOpen(false)}
        />
      )}
    </div>
  )
}
