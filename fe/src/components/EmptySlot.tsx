import { Plus } from 'lucide-react'

export function EmptySlot({ onClick }: { onClick: () => void }) {
  return (
    <button type="button" className="empty-slot plain-button" onClick={onClick}>
      <Plus size={28} strokeWidth={1.5} aria-hidden="true" />
      <span className="empty-slot-title">Empty slot</span>
      <span className="text-muted empty-slot-hint">
        Add a city to fill it
      </span>
    </button>
  )
}
