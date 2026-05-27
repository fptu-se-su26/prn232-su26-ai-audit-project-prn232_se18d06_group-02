export default function App() {
  return (
    <main className="min-h-screen bg-slate-50 text-slate-900">
      <section className="mx-auto flex min-h-screen max-w-3xl flex-col items-center justify-center px-6 text-center">
        <div className="mb-6 inline-flex h-16 w-16 items-center justify-center rounded-2xl bg-primary text-white shadow-lg shadow-primary/20">
          <span className="material-symbols-outlined text-3xl">memory</span>
        </div>

        <h1 className="text-4xl font-extrabold tracking-tight sm:text-5xl">GearZone FE</h1>
        <p className="mt-4 max-w-xl text-base text-slate-600 sm:text-lg">
          Vite + React + TypeScript scaffold is ready. Next step is cloning the login and register
          experience from the React source app into this frontend.
        </p>

        <div className="mt-8 rounded-2xl border border-slate-200 bg-white px-6 py-4 text-sm text-slate-600 shadow-sm">
          Branch and tooling scaffold are in place for upcoming auth UI work.
        </div>
      </section>
    </main>
  )
}
