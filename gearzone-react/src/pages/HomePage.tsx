import { useEffect, useRef, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { catalogApi } from '../api/catalog';
import ProductCard, { ProductCardData } from '../components/ProductCard';

interface HeroSlide {
  imageUrl?: string;
  imageAlt?: string;
  eyebrow?: string;
  title?: string;
  description?: string;
  tags?: string[];
  primaryHref?: string;
  primaryLabel?: string;
  secondaryHref?: string;
  secondaryLabel?: string;
}

interface ServiceItem {
  icon?: string;
  title?: string;
  subtitle?: string;
  href?: string;
  tone?: string;
}

interface HomeCategory { name: string; href: string; icon?: string; tone?: string; productCount?: number; }
interface HomeStore { storeName: string; logoUrl?: string; href: string; description?: string; productCount?: number; totalSold?: number; }
interface HomeData {
  heroSlides?: HeroSlide[];
  serviceItems?: ServiceItem[];
  categories?: HomeCategory[];
  flashRail?: { eyebrow?: string; title?: string; description?: string; products?: ProductCardData[] };
  recommendedRail?: { eyebrow?: string; title?: string; description?: string; products?: ProductCardData[]; viewAllHref?: string; viewAllLabel?: string };
  stores?: HomeStore[];
}

const TONE_COLORS: Record<string, string> = {
  orange: 'bg-orange-50 text-orange-600',
  violet: 'bg-violet-50 text-violet-600',
  sky: 'bg-sky-50 text-sky-600',
  emerald: 'bg-emerald-50 text-emerald-600',
  blue: 'bg-blue-50 text-blue-600',
  slate: 'bg-slate-100 text-slate-700',
};

function toneClass(tone?: string) { return TONE_COLORS[tone ?? 'blue'] ?? TONE_COLORS.blue; }

function HeroCarousel({ slides }: { slides: HeroSlide[] }) {
  const [active, setActive] = useState(0);
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const scheduleNext = () => {
    if (timerRef.current) clearTimeout(timerRef.current);
    if (slides.length > 1) {
      timerRef.current = setTimeout(() => {
        setActive(i => (i + 1) % slides.length);
      }, 3000);
    }
  };

  useEffect(() => {
    scheduleNext();
    return () => { if (timerRef.current) clearTimeout(timerRef.current); };
  }, [active, slides.length]);

  const goTo = (idx: number) => {
    setActive((idx + slides.length) % slides.length);
  };

  const slide = slides[active];

  return (
    <div
      className="relative rounded-[1.25rem] overflow-hidden text-white min-h-[420px] flex items-center"
      style={{ background: 'linear-gradient(135deg, #003a8c 0%, #0056b3 45%, #0095ff 100%)' }}
      onMouseEnter={() => { if (timerRef.current) clearTimeout(timerRef.current); }}
      onMouseLeave={() => scheduleNext()}
    >
      {/* Background image */}
      {slide.imageUrl && (
        <div className="absolute inset-0">
          <img src={slide.imageUrl} alt={slide.imageAlt ?? ''} className="w-full h-full object-cover" />
          <div className="absolute inset-0"
            style={{ background: 'linear-gradient(100deg,rgba(2,6,23,.70) 12%,rgba(2,6,23,.50) 42%,rgba(2,6,23,.22) 68%,rgba(2,6,23,.12) 100%)' }} />
        </div>
      )}

      {!slide.imageUrl && (
        <>
          <div className="absolute top-0 right-0 w-80 h-80 rounded-full opacity-10" style={{ background: 'radial-gradient(circle, #fff 0%, transparent 70%)', transform: 'translate(30%, -30%)' }} />
          <div className="absolute bottom-0 left-0 w-64 h-64 rounded-full opacity-5" style={{ background: 'radial-gradient(circle, #fff 0%, transparent 70%)', transform: 'translate(-30%, 30%)' }} />
        </>
      )}

      {/* Copy */}
      <div className="relative z-10 px-10 py-12 md:px-16 max-w-2xl pb-16">
        {slide.eyebrow && (
          <div className="inline-flex items-center gap-2 rounded-full border border-white/20 bg-white/10 px-4 py-2 text-[11px] font-extrabold uppercase tracking-[0.2em] text-white/90 backdrop-blur-xl mb-6">
            <span className="w-2 h-2 rounded-full bg-orange-400" />
            {slide.eyebrow}
          </div>
        )}
        <h1 className="text-[clamp(2rem,4.5vw,3.2rem)] font-extrabold leading-[0.95] tracking-[-0.04em] text-white mb-4"
          style={{ textShadow: '0 14px 30px rgba(2,6,23,0.38)' }}
          dangerouslySetInnerHTML={{ __html: (slide.title ?? 'Your PC Gaming<br/>Gear Destination').replace(/\n/g, '<br/>') }} />
        {slide.description && (
          <p className="text-sm md:text-base leading-7 text-white/80 mb-6 max-w-lg">{slide.description}</p>
        )}
        {slide.tags && slide.tags.length > 0 && (
          <div className="flex flex-wrap gap-2 mb-6">
            {slide.tags.map(tag => (
              <span key={tag} className="rounded-full border border-white/20 bg-white/10 px-3 py-1.5 text-[11px] font-extrabold text-white/90 backdrop-blur-xl">{tag}</span>
            ))}
          </div>
        )}
        <div className="flex flex-wrap gap-3">
          <a href={slide.primaryHref ?? '/products'}
            className="inline-flex items-center gap-2 rounded-xl bg-secondary px-5 py-3 text-sm font-extrabold text-white shadow-[0_18px_28px_-18px_rgba(255,107,0,0.78)] hover:-translate-y-0.5 transition-transform">
            {slide.primaryLabel ?? 'Shop Now'}
            <span className="material-symbols-outlined text-[18px]">arrow_forward</span>
          </a>
          {slide.secondaryHref && (
            <a href={slide.secondaryHref}
              className="inline-flex items-center rounded-xl border border-white/30 bg-white/20 px-5 py-3 text-sm font-extrabold text-white backdrop-blur-xl hover:-translate-y-0.5 hover:bg-white/30 transition-all">
              {slide.secondaryLabel ?? 'View More'}
            </a>
          )}
        </div>
      </div>

      {/* Carousel controls */}
      {slides.length > 1 && (
        <div className="absolute inset-x-4 bottom-4 z-10 flex items-center justify-between gap-3 md:inset-x-8">
          <div className="flex items-center gap-2">
            {slides.map((_, i) => (
              <button key={i} onClick={() => goTo(i)}
                className="border-none rounded-full transition-all"
                style={{ width: i === active ? '1.45rem' : '0.5rem', height: '0.5rem', background: i === active ? 'white' : 'rgba(255,255,255,0.42)' }}
                aria-label={`Go to slide ${i + 1}`} />
            ))}
          </div>
          <div className="flex items-center gap-2">
            <button onClick={() => goTo(active - 1)}
              className="inline-flex h-9 w-9 items-center justify-center rounded-full border border-white/16 bg-white/12 text-white backdrop-blur-lg hover:bg-white/20 transition-colors"
              aria-label="Previous">
              <span className="material-symbols-outlined text-[18px]">west</span>
            </button>
            <button onClick={() => goTo(active + 1)}
              className="inline-flex h-9 w-9 items-center justify-center rounded-full border border-white/16 bg-white/12 text-white backdrop-blur-lg hover:bg-white/20 transition-colors"
              aria-label="Next">
              <span className="material-symbols-outlined text-[18px]">east</span>
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

function StaticHero() {
  const navigate = useNavigate();
  return (
    <div className="relative rounded-[1.25rem] overflow-hidden text-white flex items-center min-h-[380px]"
      style={{ background: 'linear-gradient(135deg, #003a8c 0%, #0056b3 45%, #0095ff 100%)' }}>
      <div className="absolute top-0 right-0 w-80 h-80 rounded-full opacity-10" style={{ background: 'radial-gradient(circle, #fff 0%, transparent 70%)', transform: 'translate(30%, -30%)' }} />
      <div className="absolute bottom-0 left-0 w-64 h-64 rounded-full opacity-5" style={{ background: 'radial-gradient(circle, #fff 0%, transparent 70%)', transform: 'translate(-30%, 30%)' }} />
      <div className="relative z-10 px-10 py-12 md:px-16 max-w-2xl">
        <div className="inline-flex items-center gap-2 rounded-full border border-white/20 bg-white/10 px-4 py-2 text-[11px] font-extrabold uppercase tracking-[0.2em] text-white/90 backdrop-blur-xl mb-6">
          <span className="w-2 h-2 rounded-full bg-orange-400" />
          GearZone Marketplace
        </div>
        <h1 className="text-[clamp(2rem,4.5vw,3.2rem)] font-extrabold leading-[0.95] tracking-[-0.04em] text-white mb-4"
          style={{ textShadow: '0 14px 30px rgba(2,6,23,0.38)' }}>
          Your PC Gaming<br />Gear Destination
        </h1>
        <p className="text-sm md:text-base leading-7 text-white/80 mb-8 max-w-lg">
          Shop the latest CPUs, GPUs, peripherals, and more from verified stores across Vietnam.
        </p>
        <div className="flex flex-wrap gap-3">
          <button onClick={() => navigate('/products')}
            className="inline-flex items-center gap-2 rounded-xl bg-secondary px-5 py-3 text-sm font-extrabold text-white shadow-[0_18px_28px_-18px_rgba(255,107,0,0.78)] hover:-translate-y-0.5 transition-transform">
            Shop Now
            <span className="material-symbols-outlined text-[18px]">arrow_forward</span>
          </button>
          <button onClick={() => navigate('/products?sort=newest')}
            className="inline-flex items-center rounded-xl border border-white/30 bg-white/20 px-5 py-3 text-sm font-extrabold text-white backdrop-blur-xl hover:-translate-y-0.5 hover:bg-white/30 transition-all">
            New Arrivals
          </button>
        </div>
      </div>
    </div>
  );
}

export default function HomePage() {
  const [data, setData] = useState<HomeData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    catalogApi.home()
      .then(d => setData(d as HomeData))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return (
    <div className="flex items-center justify-center py-24">
      <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full" />
    </div>
  );

  const categories = data?.categories ?? [];
  const flashProducts = data?.flashRail?.products ?? [];
  const recommendedProducts = data?.recommendedRail?.products ?? [];
  const stores = data?.stores ?? [];
  const heroSlides = data?.heroSlides ?? [];
  const serviceItems = data?.serviceItems ?? [];

  return (
    <div style={{ background: 'linear-gradient(180deg, #eef5ff 0%, #f7fbff 32%, #f8fafc 100%)' }} className="pb-16 overflow-x-clip">

      {/* Hero */}
      <section className="pt-5 pb-0">
        <div className="max-w-[1200px] mx-auto px-4 md:px-6 lg:px-8">
          {heroSlides.length > 0
            ? <HeroCarousel slides={heroSlides} />
            : <StaticHero />
          }
        </div>
      </section>

      {/* Service strip */}
      {serviceItems.length > 0 && (
        <section className="pt-4">
          <div className="max-w-[1200px] mx-auto px-4 md:px-6 lg:px-8">
            <div className="bg-white rounded-[1.2rem] border border-gray-200/90 shadow-[0_18px_32px_-26px_rgba(15,23,42,0.22)] grid grid-cols-2 gap-3 p-3 md:grid-cols-3 xl:grid-cols-6">
              {serviceItems.slice(0, 6).map((item, i) => (
                <a key={i} href={item.href ?? '#'}
                  className="flex items-center gap-3 rounded-2xl border border-slate-200/90 px-3 py-3 transition hover:border-blue-200 hover:bg-slate-50">
                  <span className={`inline-flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-2xl ${toneClass(item.tone)}`}>
                    <span className="material-symbols-outlined text-[20px]">{item.icon ?? 'check'}</span>
                  </span>
                  <span className="min-w-0">
                    <span className="block text-sm font-extrabold text-slate-900">{item.title}</span>
                    {item.subtitle && <span className="mt-0.5 block text-[11px] leading-4 text-slate-500">{item.subtitle}</span>}
                  </span>
                </a>
              ))}
            </div>
          </div>
        </section>
      )}

      {/* Categories */}
      {categories.length > 0 && (
        <section className="pt-4">
          <div className="max-w-[1200px] mx-auto px-4 md:px-6 lg:px-8">
            <div className="bg-white rounded-[1.2rem] border border-gray-200/90 shadow-[0_18px_32px_-26px_rgba(15,23,42,0.22)] p-5">
              <div className="flex items-center justify-between gap-3 mb-5">
                <div>
                  <p className="text-[11px] font-extrabold uppercase tracking-[0.2em] text-slate-500">Popular categories</p>
                  <h2 className="mt-1 text-xl font-black tracking-[-0.03em] text-slate-950">Browse the catalog</h2>
                </div>
                <Link to="/products" className="text-xs font-extrabold uppercase tracking-[0.18em] text-primary hover:underline">View all</Link>
              </div>
              <div className="grid grid-cols-2 gap-3 md:grid-cols-4 xl:grid-cols-6">
                {categories.slice(0, 12).map(c => (
                  <Link key={c.href} to={`/products?categorySlug=${c.href}`}
                    className="rounded-2xl border border-slate-200/90 bg-white p-4 text-center hover:-translate-y-0.5 hover:border-blue-200 hover:shadow-[0_18px_28px_-24px_rgba(37,99,235,.18)] transition-all">
                    <span className={`mx-auto inline-flex h-12 w-12 items-center justify-center rounded-2xl ${toneClass(c.tone)}`}>
                      <span className="material-symbols-outlined text-[22px]">{c.icon ?? 'category'}</span>
                    </span>
                    <p className="mt-3 text-sm font-extrabold leading-5 text-slate-900">{c.name}</p>
                    {c.productCount != null && (
                      <p className="mt-1 text-[11px] uppercase tracking-[0.16em] text-slate-500">{c.productCount} items</p>
                    )}
                  </Link>
                ))}
              </div>
            </div>
          </div>
        </section>
      )}

      {/* Flash / Featured Products */}
      <section className="pt-4">
        <div className="max-w-[1200px] mx-auto px-4 md:px-6 lg:px-8">
          <div className="bg-white rounded-[1.2rem] border border-gray-200/90 shadow-[0_18px_32px_-26px_rgba(15,23,42,0.22)] overflow-hidden">
            <div className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-200/80 px-5 py-4 text-white"
              style={{ background: 'linear-gradient(to right, #0b58c4, #0f78e4)' }}>
              <div className="flex items-center gap-3">
                <span className="rounded-full bg-white px-3 py-1 text-xs font-extrabold uppercase tracking-[0.2em] text-primary">
                  {data?.flashRail?.eyebrow ?? 'Featured'}
                </span>
                <h2 className="text-xl font-black tracking-[-0.03em]">
                  {data?.flashRail?.title ?? 'Top Picks'}
                </h2>
              </div>
              <span className="rounded-full border border-white/20 bg-white/10 px-3 py-1 text-[11px] font-extrabold uppercase tracking-[0.18em] text-white/85">
                Sorted by latest updates
              </span>
            </div>
            <div className="p-5">
              {data?.flashRail?.description && (
                <p className="mb-4 text-sm leading-7 text-slate-500">{data.flashRail.description}</p>
              )}
              {flashProducts.length > 0 ? (
                <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-5">
                  {flashProducts.slice(0, 10).map(p => (
                    <ProductCard key={p.slug} product={p} />
                  ))}
                </div>
              ) : (
                <div className="rounded-2xl border border-dashed border-slate-300/80 bg-white/70 px-4 py-8 text-center text-sm text-slate-500">
                  No featured products available yet.
                </div>
              )}
            </div>
          </div>
        </div>
      </section>

      {/* Store showcase */}
      {stores.length > 0 && (
        <section className="pt-4">
          <div className="max-w-[1200px] mx-auto px-4 md:px-6 lg:px-8">
            <div className="bg-white rounded-[1.2rem] border border-gray-200/90 shadow-[0_18px_32px_-26px_rgba(15,23,42,0.22)] p-5">
              <div className="flex items-center justify-between gap-3 mb-5">
                <div>
                  <p className="text-[11px] font-extrabold uppercase tracking-[0.2em] text-slate-500">Official store zone</p>
                  <h2 className="mt-1 text-xl font-black tracking-[-0.03em] text-slate-950">Verified Stores</h2>
                </div>
                <Link to="/products" className="text-xs font-extrabold uppercase tracking-[0.18em] text-primary hover:underline">See more</Link>
              </div>
              <div className="grid gap-4 xl:grid-cols-[1.15fr_0.85fr]">
                <Link to={stores[0]?.href ?? '/products'}
                  className="rounded-[1.35rem] p-6 block"
                  style={{ background: 'linear-gradient(135deg, #eef5ff, #fff, #fff1e8)' }}>
                  <p className="text-[11px] font-extrabold uppercase tracking-[0.22em] text-primary">Official Hub</p>
                  <h3 className="mt-2 text-[1.8rem] font-black leading-none tracking-[-0.05em] text-slate-950">{stores[0]?.storeName ?? 'GearZone Official'}</h3>
                  <p className="mt-3 text-sm leading-7 text-slate-600 max-w-lg">{stores[0]?.description ?? 'Official routes, seeded catalog, and support entry points.'}</p>
                  <div className="mt-5 flex flex-wrap gap-2">
                    {stores.slice(0, 4).map(s => (
                      <span key={s.href} className="rounded-full bg-white px-3 py-2 text-[11px] font-extrabold text-slate-700 shadow-sm">{s.storeName}</span>
                    ))}
                  </div>
                </Link>
                <div className="grid gap-3 md:grid-cols-2">
                  {stores.map(s => (
                    <Link key={s.href} to={s.href}
                      className="rounded-2xl border border-slate-200/90 bg-white p-4 hover:-translate-y-0.5 hover:border-blue-200 hover:shadow-[0_18px_28px_-24px_rgba(37,99,235,.18)] transition-all">
                      <div className="flex items-start gap-3">
                        <span className="inline-flex h-12 w-12 flex-shrink-0 items-center justify-center overflow-hidden rounded-2xl bg-blue-50 text-base font-black text-primary">
                          {s.logoUrl ? <img src={s.logoUrl} alt={s.storeName} className="h-full w-full object-cover" /> : (s.storeName[0] ?? 'G')}
                        </span>
                        <span>
                          <span className="block truncate text-sm font-extrabold text-slate-900">{s.storeName}</span>
                          <span className="mt-1 block text-xs leading-5 text-slate-500">{s.description ?? 'Verified marketplace seller.'}</span>
                        </span>
                      </div>
                      <div className="mt-3 flex flex-wrap gap-2">
                        {s.productCount != null && <span className="rounded-full bg-slate-50 px-2.5 py-1.5 text-[11px] font-extrabold text-slate-700">{s.productCount} products</span>}
                        {s.totalSold != null && <span className="rounded-full bg-slate-50 px-2.5 py-1.5 text-[11px] font-extrabold text-slate-700">{s.totalSold} sold</span>}
                      </div>
                    </Link>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </section>
      )}

      {/* Recommended */}
      <section className="pt-4">
        <div className="max-w-[1200px] mx-auto px-4 md:px-6 lg:px-8">
          <div className="bg-white rounded-[1.2rem] border border-gray-200/90 shadow-[0_18px_32px_-26px_rgba(15,23,42,0.22)] p-5">
            <div className="flex items-center justify-between gap-3 mb-5">
              <div>
                <p className="text-[11px] font-extrabold uppercase tracking-[0.2em] text-slate-500">
                  {data?.recommendedRail?.eyebrow ?? 'Recommended for You'}
                </p>
                <h2 className="mt-1 text-xl font-black tracking-[-0.03em] text-slate-950">
                  {data?.recommendedRail?.title ?? 'Top Products'}
                </h2>
                {data?.recommendedRail?.description && (
                  <p className="mt-1 text-sm leading-7 text-slate-500">{data.recommendedRail.description}</p>
                )}
              </div>
              <Link to={data?.recommendedRail?.viewAllHref ?? '/products'} className="text-xs font-extrabold uppercase tracking-[0.18em] text-primary hover:underline whitespace-nowrap">
                {data?.recommendedRail?.viewAllLabel ?? 'View All'}
              </Link>
            </div>
            {recommendedProducts.length > 0 ? (
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-5">
                {recommendedProducts.slice(0, 10).map(p => (
                  <ProductCard key={p.slug} product={p} />
                ))}
              </div>
            ) : (
              <div className="rounded-2xl border border-dashed border-slate-300/80 bg-white/70 px-4 py-8 text-center text-sm text-slate-500">
                No recommended products available yet.
              </div>
            )}
          </div>
        </div>
      </section>
    </div>
  );
}
