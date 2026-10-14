import type { Unit } from '../lib/units'

interface UnitToggleProps {
  unit: Unit
  onChange: (unit: Unit) => void
}

export function UnitToggle({ unit, onChange }: UnitToggleProps) {
  return (
    <div className="seg" role="group" aria-label="Temperature unit">
      {(['C', 'F'] as const).map((option) => (
        <button
          key={option}
          type="button"
          className="seg-opt plain-button"
          aria-pressed={unit === option}
          onClick={() => onChange(option)}
        >
          °{option}
        </button>
      ))}
    </div>
  )
}
