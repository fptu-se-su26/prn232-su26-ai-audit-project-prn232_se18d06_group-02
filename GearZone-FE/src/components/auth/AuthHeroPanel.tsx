import { Link } from 'react-router-dom'

interface AuthHeroPanelProps {
  title: string
  description: React.ReactNode
  buttonLabel: string
  onClick: () => void
  className: string
  style: React.CSSProperties
}

export function AuthHeroPanel({ title, description, buttonLabel, onClick, className, style }: AuthHeroPanelProps) {
  return (
    <div className={className} style={style}>
      <h1 className="mb-4 text-4xl font-extrabold tracking-tight">{title}</h1>
      <p className="mb-8 text-sm leading-relaxed font-medium text-blue-100 opacity-90">{description}</p>
      <button
        type="button"
        onClick={onClick}
        className="rounded-xl border-2 border-white/30 px-10 py-3 text-[10px] font-bold tracking-widest text-white uppercase transition-all duration-300 hover:bg-white hover:text-primary"
      >
        {buttonLabel}
      </button>
      <div className="absolute bottom-12">
        <Link to="/" className="text-2xl font-black tracking-tighter uppercase">
          GearZone
        </Link>
      </div>
    </div>
  )
}
