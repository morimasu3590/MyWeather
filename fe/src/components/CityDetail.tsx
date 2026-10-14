import { statusMessage, sampleStats, type CityView } from '../lib/cityView'
import { SampleTag } from './SampleTag'

interface CityDetailProps {
  views: CityView[]
  selected: CityView
  onSelect: (cityId: number) => void
  onBack: () => void
}

/** Detail view: a rail of all favorites for switching, then the selected city's spec sheet. */
export function CityDetail({ views, selected, onSelect, onBack }: CityDetailProps) {
  const { favorite, sample, format } = selected

  return (
    <div className="detail">
      <nav className="rail" aria-label="Favorite cities">
        <button type="button" className="rail-back plain-button" onClick={onBack}>
          ← Overview
        </button>
        {views.map((view) => {
          const isSelected = view.favorite.cityId === favorite.cityId
          return (
            <button
              key={view.favorite.cityId}
              type="button"
              className="rail-item plain-button"
              aria-current={isSelected}
              onClick={() => onSelect(view.favorite.cityId)}
            >
              <div>
                <div className="rail-item-name">{view.favorite.cityName}</div>
                <div className="rail-item-meta">
                  {view.updated ? `${view.updated} UTC · ${view.condition}` : statusMessage(view.status)}
                </div>
              </div>
              <span className="rail-item-temp">{view.temperature ?? '—'}</span>
            </button>
          )
        })}
      </nav>

      <section className="detail-main" aria-labelledby="detail-title">
        <div className="detail-head">
          <div>
            <div className="card-kicker">
              {favorite.countryName}
              {selected.updated && ` · Updated ${selected.updated} UTC`}
            </div>
            <h1 id="detail-title">{favorite.cityName}</h1>
            <div className="detail-summary section-label">
              <span>
                {selected.condition ?? statusMessage(selected.status)} · Feels like {format(sample.feelsLikeC)} · H{' '}
                {format(sample.highC)} / L {format(sample.lowC)}
              </span>
              <SampleTag />
            </div>
          </div>
          <div className="detail-temp">{selected.temperature ?? '—'}</div>
        </div>

        <div className="spec-grid">
          {selected.stats.map((stat) => (
            <div key={stat.label} className="spec-cell">
              <div className="spec-label">{stat.label}</div>
              <div className="spec-value">{stat.value}</div>
            </div>
          ))}
        </div>

        <div className="detail-section">
          <h6 className="section-label">
            <span>Air & sun</span>
            <SampleTag />
          </h6>
          <div className="spec-grid spec-grid-3">
            {sampleStats(selected).map((stat) => (
              <div key={stat.label} className="spec-cell">
                <div className="spec-label">{stat.label}</div>
                <div className="spec-value">{stat.value}</div>
              </div>
            ))}
          </div>
        </div>

        <div className="detail-cols">
          <div>
            <h6 className="section-label">
              <span>Hourly</span>
              <SampleTag />
            </h6>
            <div className="hourly">
              {sample.hours.map((hour) => (
                <div key={hour.label} className="hourly-cell">
                  <span className="text-muted hourly-label">{hour.label}</span>
                  <div className="hourly-plot">
                    <div className="hourly-dot" style={{ bottom: `${hour.height}%` }} />
                  </div>
                  <span className="hourly-temp">{format(hour.tempC)}</span>
                  <span className="hourly-pop">{hour.precipitation}%</span>
                </div>
              ))}
            </div>
          </div>
          <div>
            <h6 className="section-label">
              <span>7 days</span>
              <SampleTag />
            </h6>
            {sample.week.map((day) => (
              <div key={day.label} className="week-row">
                <span>{day.label}</span>
                <span>{day.condition}</span>
                <span>{format(day.highC)}</span>
                <span className="text-muted">{format(day.lowC)}</span>
              </div>
            ))}
          </div>
        </div>
      </section>
    </div>
  )
}
