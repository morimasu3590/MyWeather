import { describe, expect, it } from 'vitest'
import { celsiusToFahrenheit, compassDirection, formatTemperature, formatUtcTime, formatWindSpeed, humanize } from './units'

describe('units', () => {
  it('converts freezing, boiling and -40 between scales', () => {
    expect(celsiusToFahrenheit(0)).toBe(32)
    expect(celsiusToFahrenheit(100)).toBe(212)
    expect(celsiusToFahrenheit(-40)).toBe(-40)
  })

  it('formats temperatures rounded in the chosen unit', () => {
    expect(formatTemperature(21.4, 'C')).toBe('21°')
    expect(formatTemperature(21.4, 'F')).toBe('71°')
  })

  it('formats wind in km/h or mph', () => {
    expect(formatWindSpeed(14, 'C')).toBe('14 km/h')
    expect(formatWindSpeed(14, 'F')).toBe('9 mph')
  })

  it.each([
    [0, 'N'],
    [22, 'N'],
    [23, 'NE'],
    [180, 'S'],
    [337, 'NW'],
    [338, 'N'],
    [360, 'N'],
    [-90, 'W'],
  ])('maps %s degrees to %s', (degrees, expected) => {
    expect(compassDirection(degrees)).toBe(expected)
  })

  it('formats the UTC time and tolerates bad input', () => {
    expect(formatUtcTime('2026-03-04T07:05:45Z')).toBe('07:05')
    expect(formatUtcTime('not a date')).toBe('--:--')
  })

  it('humanizes enum names', () => {
    expect(humanize('PartlyCloudy')).toBe('Partly cloudy')
    expect(humanize('Clear')).toBe('Clear')
  })
})
