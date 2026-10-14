import { Plus } from 'lucide-react'
import type { Unit } from '../lib/units'
import { MAX_FAVORITES } from '../state/useFavorites'
import { Corners } from './Corners'
import { UnitToggle } from './UnitToggle'

interface AppHeaderProps {
  count: number
  unit: Unit
  onUnitChange: (unit: Unit) => void
  onAddCity: () => void
}

export function AppHeader({ count, unit, onUnitChange, onAddCity }: AppHeaderProps) {
  return (
    <header className="nav app-nav">
      <span className="nav-brand">WEATHERBOARD</span>
      <span className="tag tag-neutral">
        {count} / {MAX_FAVORITES} cities
      </span>
      <UnitToggle unit={unit} onChange={onUnitChange} />
      <button type="button" className="btn btn-primary blueprint" onClick={onAddCity}>
        <Corners />
        <Plus size={16} strokeWidth={1.5} aria-hidden="true" />
        Add city
      </button>
    </header>
  )
}
