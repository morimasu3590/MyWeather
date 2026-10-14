import type { KeyboardEvent, MouseEvent } from 'react'
import { X } from 'lucide-react'
import { statusMessage, type CityView } from '../lib/cityView'
import { SampleTag } from './SampleTag'

interface CityColumnProps {
  view: CityView
  onOpen: () => void
  onRemove: () => void
}

export function CityColumn({ view, onOpen, onRemove }: CityColumnProps) {
  const { favorite, sample, format } = view

  const handleKeyDown = (event: KeyboardEvent<HTMLElement>) => {
    if (event.target === event.currentTarget && (event.key === 'Enter' || event.key === ' ')) {
      event.preventDefault()
      onOpen()
    }
  }

  const handleRemove = (event: MouseEvent) => {
    event.stopPropagation()
    onRemove()
  }

  return (
    <article
      className="city-col"
      role="button"
      tabIndex={0}
      aria-label={`Open forecast for ${favorite.cityName}`}
      onClick={onOpen}
      onKeyDown={handleKeyDown}
    >
      <div className="city-band">
        <div>
          <div className="city-band-name">{favorite.cityName}</div>
          <div className="city-band-country">{favorite.countryName}</div>
        </div>
        <div className="city-band-right">
          {view.updated && (
            <span className="city-band-time" title={`Updated ${view.updated} UTC`}>
              {view.updated}
              <small>UTC</small>
            </span>
          )}
          <button
            type="button"
            className="btn btn-icon band-remove"
            title="Remove"
            aria-label={`Remove ${favorite.cityName}`}
            onClick={handleRemove}
          >
            <X size={16} strokeWidth={1.5} aria-hidden="true" />
          </button>
        </div>
      </div>

      <div className="city-now">
        {view.temperature ? (
          <>
            <div className="big-temp">{view.temperature}</div>
            <div className="city-cond">{view.condition}</div>
          </>
        ) : (
          <div className="no-data text-muted">{statusMessage(view.status)}</div>
        )}
        <div className="city-sub text-muted section-label">
          <span>
            Feels like {format(sample.feelsLikeC)} · H {format(sample.highC)} / L {format(sample.lowC)}
          </span>
          <SampleTag />
        </div>
      </div>

      <div className="hour-bars" aria-hidden="true" title="Sample hourly trend">
        {sample.hours.map((hour) => (
          <span key={hour.label} style={{ height: `${hour.height}%` }} />
        ))}
      </div>

      <div className="stat-list">
        {view.stats.map((stat) => (
          <div key={stat.label} className="stat-row">
            <span className="text-muted">{stat.label}</span>
            <span>{stat.value}</span>
          </div>
        ))}
      </div>

      <div className="col-link">Open forecast →</div>
    </article>
  )
}
