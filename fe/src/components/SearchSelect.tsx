import { useEffect, useId, useMemo, useRef, useState, type KeyboardEvent } from 'react'
import { ChevronDown, Search } from 'lucide-react'

export interface SelectOption {
  value: string
  label: string
  meta?: string
  disabled?: boolean
}

interface SearchSelectProps {
  label: string
  placeholder: string
  searchPlaceholder: string
  options: SelectOption[]
  value: string | null
  open: boolean
  disabled?: boolean
  /** Shown in the menu instead of options, e.g. while loading. */
  message?: string
  noun: string
  onOpenChange: (open: boolean) => void
  onSelect: (value: string) => void
}

/** A field whose dropdown has its own search box, as drawn in AddCityDialog.dc.html. */
export function SearchSelect({
  label,
  placeholder,
  searchPlaceholder,
  options,
  value,
  open,
  disabled = false,
  message,
  noun,
  onOpenChange,
  onSelect,
}: SearchSelectProps) {
  const id = useId()
  const triggerRef = useRef<HTMLButtonElement>(null)
  const selected = options.find((o) => o.value === value)
  const labelId = `${id}-label`

  const closeAndRefocus = () => {
    onOpenChange(false)
    triggerRef.current?.focus()
  }

  return (
    <div className={`field select${open ? ' is-open' : ''}${disabled ? ' is-disabled' : ''}`}>
      <label id={labelId} htmlFor={`${id}-trigger`}>
        {label}
      </label>
      <button
        ref={triggerRef}
        id={`${id}-trigger`}
        type="button"
        className="input select-trigger"
        aria-haspopup="listbox"
        aria-expanded={open}
        disabled={disabled}
        onClick={() => onOpenChange(!open)}
      >
        <span className={selected ? undefined : 'select-placeholder'}>{selected?.label ?? placeholder}</span>
        <ChevronDown size={16} strokeWidth={1.5} aria-hidden="true" />
      </button>

      {/* Mounted only while open, so the search box starts empty every time. */}
      {open && (
        <SelectMenu
          id={id}
          labelId={labelId}
          options={options}
          value={value}
          message={message}
          noun={noun}
          searchPlaceholder={searchPlaceholder}
          onSelect={onSelect}
          onClose={closeAndRefocus}
        />
      )}
    </div>
  )
}

interface SelectMenuProps {
  id: string
  labelId: string
  options: SelectOption[]
  value: string | null
  message?: string
  noun: string
  searchPlaceholder: string
  onSelect: (value: string) => void
  onClose: () => void
}

function SelectMenu({ id, labelId, options, value, message, noun, searchPlaceholder, onSelect, onClose }: SelectMenuProps) {
  const [query, setQuery] = useState('')
  const [activeIndex, setActiveIndex] = useState(0)
  const searchRef = useRef<HTMLInputElement>(null)
  const listId = `${id}-list`

  useEffect(() => {
    searchRef.current?.focus()
  }, [])

  const filtered = useMemo(
    () => options.filter((o) => o.label.toLowerCase().includes(query.trim().toLowerCase())),
    [options, query],
  )

  const choose = (option: SelectOption | undefined) => {
    if (!option || option.disabled) return
    onSelect(option.value)
  }

  const handleKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault()
        setActiveIndex((i) => Math.min(i + 1, filtered.length - 1))
        break
      case 'ArrowUp':
        event.preventDefault()
        setActiveIndex((i) => Math.max(i - 1, 0))
        break
      case 'Enter':
        event.preventDefault()
        choose(filtered[activeIndex])
        break
      case 'Escape':
        // Close only the menu, not the dialog around it.
        event.preventDefault()
        event.stopPropagation()
        onClose()
        break
    }
  }

  return (
    <div className="select-menu">
      <div className="select-search">
        <Search size={16} strokeWidth={1.5} aria-hidden="true" />
        <input
          ref={searchRef}
          className="input"
          value={query}
          placeholder={searchPlaceholder}
          aria-label={searchPlaceholder}
          role="combobox"
          aria-expanded="true"
          aria-controls={listId}
          aria-activedescendant={filtered[activeIndex] ? `${id}-opt-${activeIndex}` : undefined}
          onChange={(e) => {
            setQuery(e.target.value)
            setActiveIndex(0)
          }}
          onKeyDown={handleKeyDown}
        />
      </div>
      {message ? (
        <div className="select-message text-muted">{message}</div>
      ) : (
        <ul id={listId} className="select-options" role="listbox" aria-labelledby={labelId}>
          {filtered.map((option, index) => (
            <li
              key={option.value}
              id={`${id}-opt-${index}`}
              role="option"
              aria-selected={option.value === value}
              aria-disabled={option.disabled || undefined}
              className={`select-option${index === activeIndex ? ' is-active' : ''}`}
              onMouseEnter={() => setActiveIndex(index)}
              onClick={() => choose(option)}
            >
              <span>{option.label}</span>
              {option.meta && <span className="text-muted select-option-meta">{option.meta}</span>}
            </li>
          ))}
          {filtered.length === 0 && (
            <li className="select-message text-muted" role="presentation">
              No {noun} match "{query}".
            </li>
          )}
        </ul>
      )}
    </div>
  )
}
