import { useEffect, useMemo, useRef, useState } from 'react'
import { Link, useParams, useSearchParams } from 'react-router-dom'
import { getCatalogCategories, getCatalogFilters, getProducts } from '@/api/catalog'
import type {
  CatalogCategory,
  CatalogFilterSidebar,
  CatalogProduct,
  CompareProduct,
  PagedResult,
  ProductBrowseFilter,
} from '@/types/catalog'

const COMPARE_STORAGE_KEY = 'gearzone_compare_list'
const MIN_PRICE = 0
const MAX_PRICE = 100_000_000
const PRICE_STEP = 500_000
const PRICE_MIN_GAP = 5_000_000
const PAGE_SIZE = 12

const metadataCache = new Map<string, { categories: CatalogCategory[]; sidebar: CatalogFilterSidebar }>()
const metadataPromiseCache = new Map<string, Promise<{ categories: CatalogCategory[]; sidebar: CatalogFilterSidebar }>>()
const productCache = new Map<string, PagedResult<CatalogProduct>>()
const productPromiseCache = new Map<string, Promise<PagedResult<CatalogProduct>>>()

function formatPrice(value: number) {
  return new Intl.NumberFormat('vi-VN').format(value)
}

function formatCompactPrice(value: number) {
  if (value >= MAX_PRICE) return '100M+'
  return formatPrice(value)
}

function decodeLabel(slug?: string) {
  if (!slug) return 'All Products'
  return slug.replace(/-/g, ' ')
}

function toNumber(value: string | null, fallback?: number) {
  if (!value) return fallback
  const parsed = Number(value)
  return Number.isFinite(parsed) ? parsed : fallback
}

function readArray(searchParams: URLSearchParams, key: string) {
  return searchParams.getAll(key).filter(Boolean)
}

function buildAttributeMap(searchParams: URLSearchParams) {
  const knownKeys = new Set([
    'search',
    'categorySlug',
    'brand',
    'minPrice',
    'maxPrice',
    'sortBy',
    'viewMode',
    'pageNumber',
    'pageSize',
    'inStockOnly',
  ])

  const attributes: Record<string, string[]> = {}
  for (const [key, value] of searchParams.entries()) {
    if (knownKeys.has(key) || !value) continue
    attributes[key] ??= []
    if (!attributes[key].includes(value)) {
      attributes[key].push(value)
    }
  }

  return attributes
}

function normalizeCompareList(items: CompareProduct[]) {
  if (!Array.isArray(items) || items.length === 0) return []

  const unique: CompareProduct[] = []
  const seen = new Set<string>()
  const lockedCategoryId = String(items[0]?.categoryId ?? '')

  items.forEach((item) => {
    if (!item?.id || seen.has(item.id) || String(item.categoryId ?? '') !== lockedCategoryId) return

    seen.add(item.id)
    unique.push({
      id: item.id,
      name: item.name,
      image: item.image,
      categoryId: String(item.categoryId ?? ''),
    })
  })

  return unique.slice(0, 4)
}

function readCompareList() {
  try {
    return normalizeCompareList(JSON.parse(window.localStorage.getItem(COMPARE_STORAGE_KEY) ?? '[]'))
  } catch {
    return []
  }
}

function findCategoryTrail(categories: CatalogCategory[], slug?: string) {
  if (!slug) return []

  const walk = (nodes: CatalogCategory[], trail: CatalogCategory[]): CatalogCategory[] | null => {
    for (const node of nodes) {
      const nextTrail = [...trail, node]
      if (node.slug === slug) return nextTrail
      const childMatch = walk(node.subCategories, nextTrail)
      if (childMatch) return childMatch
    }

    return null
  }

  return walk(categories, []) ?? []
}

function buildMetadataKey(slug?: string) {
  return slug ?? '__all__'
}

function buildProductKey(slug: string | undefined, searchParams: URLSearchParams) {
  const next = new URLSearchParams(searchParams)
  next.delete('pageNumber')
  return `${slug ?? '__all__'}?${next.toString()}`
}

function ProductCard({
  product,
  viewMode,
  selected,
  onCompareToggle,
}: {
  product: CatalogProduct
  viewMode: 'grid' | 'list'
  selected: boolean
  onCompareToggle: (product: CatalogProduct, checked: boolean) => void
}) {
  const isList = viewMode === 'list'
  const imageUrl = product.imageUrl || 'https://placehold.co/640x640/f8fafc/94a3b8?text=GearZone'
  const storeLogoUrl = product.storeLogoUrl || 'https://placehold.co/64x64/e2e8f0/64748b?text=GZ'

  return (
    <article
      className={`group relative overflow-hidden rounded-xl border border-slate-200 bg-white transition-all duration-300 hover:-translate-y-1 hover:shadow-lg ${
        isList ? 'flex h-auto flex-col sm:h-56 sm:flex-row' : 'flex flex-col shadow-sm'
      }`}
    >
      {product.saleBadges[0] ? (
        <span className="absolute left-3 top-3 z-10 rounded bg-rose-500 px-2 py-1 text-[10px] font-bold text-white">
          {product.saleBadges[0]}
        </span>
      ) : null}

      <label className="absolute right-3 top-3 z-10 translate-y-2 cursor-pointer opacity-0 transition-all duration-300 group-hover:translate-y-0 group-hover:opacity-100">
        <input
          checked={selected}
          className="peer sr-only"
          onChange={(event) => onCompareToggle(product, event.target.checked)}
          type="checkbox"
        />
        <span className="relative flex items-center gap-2 rounded-full border border-slate-200 bg-white/85 px-3 py-1.5 text-[10px] font-black uppercase tracking-widest text-slate-600 shadow-lg backdrop-blur-md transition-all peer-checked:border-blue-700 peer-checked:bg-blue-700 peer-checked:text-white">
          <span className="material-symbols-outlined text-[16px]">compare_arrows</span>
          <span>Compare</span>
          <span className="absolute -right-1 -top-1 flex h-4 w-4 scale-0 items-center justify-center rounded-full bg-orange-500 text-white transition-transform peer-checked:scale-100">
            <span className="material-symbols-outlined text-[10px]">check</span>
          </span>
        </span>
      </label>

      <Link
        className={`relative block bg-white ${isList ? 'w-full flex-shrink-0 sm:w-40 lg:w-56' : 'flex h-48 items-center justify-center p-4'}`}
        to={`/product/${product.slug}`}
      >
        <img alt={product.name} className="h-full w-full object-contain p-4 mix-blend-multiply" src={imageUrl} />
      </Link>

      <div className={`flex flex-1 flex-col gap-2 border-slate-200 p-4 ${isList ? '' : 'border-t'}`}>
        <div className="mb-1 flex flex-wrap items-center gap-2">
          <span className="rounded border border-slate-200 px-1.5 py-0.5 text-[10px] font-bold uppercase tracking-wider text-slate-500">
            {product.brandName}
          </span>
          {product.reviewCount > 0 ? (
            <span className="flex items-center gap-0.5 text-amber-400">
              <span className="material-symbols-outlined text-[14px]">star</span>
              <span className="ml-1 text-xs font-medium text-slate-500">
                {product.rating.toFixed(1)} ({product.reviewCount})
              </span>
            </span>
          ) : (
            <span className="text-xs font-medium text-slate-400">New listing</span>
          )}
        </div>

        <Link
          className="min-h-[2.5em] text-sm font-bold leading-snug text-slate-900 transition-colors group-hover:text-blue-700 sm:text-base"
          to={`/product/${product.slug}`}
        >
          {product.name}
        </Link>

        <div className="my-1 hidden flex-wrap gap-1 sm:flex">
          {product.highlightTags.slice(0, isList ? 5 : 3).map((tag) => (
            <span key={tag} className="rounded bg-slate-100 px-1.5 py-0.5 text-[10px] text-slate-500">
              {tag}
            </span>
          ))}
        </div>

        <div className={isList ? 'mt-auto flex flex-row justify-between gap-4 sm:items-center' : 'mt-auto flex flex-col gap-1 pt-2'}>
          <div className={`flex flex-col gap-1 ${isList ? 'sm:flex-row sm:items-baseline sm:gap-2' : ''}`}>
            <div className="flex flex-wrap items-baseline gap-2">
              <span className="text-lg font-bold text-blue-700 sm:text-xl">{formatPrice(product.basePrice)} VND</span>
              {product.originalPrice ? (
                <span className="text-xs text-slate-400 line-through">{formatPrice(product.originalPrice)} VND</span>
              ) : null}
            </div>
          </div>

          <div className={isList ? 'flex items-center gap-4' : 'flex flex-col gap-1'}>
            <div className={`${isList ? 'hidden sm:flex' : 'flex'} items-center gap-2`}>
              <div className="h-5 w-5 overflow-hidden rounded-full bg-slate-200">
                <img alt={product.storeName} className="h-full w-full object-cover" src={storeLogoUrl} />
              </div>
              <span className="truncate text-xs text-slate-500">{product.storeName}</span>
            </div>

            <button
              className={`flex items-center justify-center gap-2 rounded-lg py-2 text-sm font-semibold transition-all ${
                product.isInStock
                  ? `${isList ? 'w-auto px-4 sm:px-6' : 'mt-2 w-full'} bg-orange-500/10 text-orange-500 hover:bg-orange-500 hover:text-white`
                  : `${isList ? 'w-auto px-4 sm:px-6' : 'mt-2 w-full'} cursor-not-allowed bg-slate-200 text-slate-500`
              }`}
              disabled={!product.isInStock}
              type="button"
            >
              <span className="material-symbols-outlined text-[18px]">shopping_cart</span>
              Add to Cart
            </button>
          </div>
        </div>
      </div>
    </article>
  )
}

export default function ProductBrowsePage() {
  const { slug } = useParams()
  const [searchParams, setSearchParams] = useSearchParams()
  const [categories, setCategories] = useState<CatalogCategory[]>([])
  const [sidebar, setSidebar] = useState<CatalogFilterSidebar>({ brands: [], attributes: [] })
  const [products, setProducts] = useState<CatalogProduct[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [pageNumber, setPageNumber] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [initialLoading, setInitialLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [loadingMore, setLoadingMore] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [collapsedSections, setCollapsedSections] = useState<Record<string, boolean>>({})
  const [compareList, setCompareList] = useState<CompareProduct[]>([])
  const [draftMinPrice, setDraftMinPrice] = useState(MIN_PRICE)
  const [draftMaxPrice, setDraftMaxPrice] = useState(MAX_PRICE)
  const [activePriceThumb, setActivePriceThumb] = useState<'min' | 'max'>('max')
  const hasLoadedOnceRef = useRef(false)
  const loadingMoreRef = useRef(false)
  const pageNumberRef = useRef(1)
  const totalPagesRef = useRef(1)
  const refreshingRef = useRef(false)
  const initialLoadingRef = useRef(true)
  const filterVersionRef = useRef(0)
  const currentFilterRef = useRef<ProductBrowseFilter | null>(null)
  const loadMoreFnRef = useRef<() => Promise<void>>(async () => {})

  const searchParamsKey = searchParams.toString()
  const search = searchParams.get('search') ?? ''
  const viewMode = searchParams.get('viewMode') === 'list' ? 'list' : 'grid'
  const sortBy = searchParams.get('sortBy') ?? 'popular'
  const selectedBrands = useMemo(() => readArray(searchParams, 'brand'), [searchParamsKey])
  const minPrice = toNumber(searchParams.get('minPrice'), MIN_PRICE) ?? MIN_PRICE
  const maxPrice = toNumber(searchParams.get('maxPrice'), MAX_PRICE) ?? MAX_PRICE
  const inStockOnly = searchParams.get('inStockOnly') === 'true'
  const attributes = useMemo(() => buildAttributeMap(searchParams), [searchParamsKey])
  const metadataKey = useMemo(() => buildMetadataKey(slug), [slug])
  const productKey = useMemo(() => buildProductKey(slug, new URLSearchParams(searchParamsKey)), [searchParamsKey, slug])

  const currentFilter = useMemo<ProductBrowseFilter>(
    () => ({
      search: search || undefined,
      categorySlug: slug,
      brandSlugs: selectedBrands.length > 0 ? selectedBrands : undefined,
      minPrice,
      maxPrice,
      inStockOnly,
      attributes: Object.keys(attributes).length > 0 ? attributes : undefined,
      sortBy,
      viewMode,
      pageSize: PAGE_SIZE,
    }),
    [attributes, inStockOnly, maxPrice, minPrice, search, selectedBrands, slug, sortBy, viewMode],
  )

  const categoryTrail = useMemo(() => findCategoryTrail(categories, slug), [categories, slug])

  useEffect(() => {
    currentFilterRef.current = currentFilter
  }, [currentFilter])

  useEffect(() => {
    setCompareList(readCompareList())
  }, [])

  useEffect(() => {
    setDraftMinPrice(minPrice)
    setDraftMaxPrice(maxPrice)
  }, [minPrice, maxPrice])

  useEffect(() => {
    pageNumberRef.current = pageNumber
  }, [pageNumber])

  useEffect(() => {
    totalPagesRef.current = totalPages
  }, [totalPages])

  useEffect(() => {
    refreshingRef.current = refreshing
  }, [refreshing])

  useEffect(() => {
    initialLoadingRef.current = initialLoading
  }, [initialLoading])

  useEffect(() => {
    let cancelled = false

    async function loadMetadata() {
      try {
        const cached = metadataCache.get(metadataKey)
        if (cached) {
          setCategories(cached.categories)
          setSidebar(cached.sidebar)
          return
        }

        let request = metadataPromiseCache.get(metadataKey)
        if (!request) {
          request = Promise.all([getCatalogCategories(), getCatalogFilters(slug)]).then(([categoryData, filterData]) => ({
            categories: categoryData,
            sidebar: filterData,
          }))
          metadataPromiseCache.set(metadataKey, request)
        }

        const result = await request
        if (cancelled) return

        metadataCache.set(metadataKey, result)
        setCategories(result.categories)
        setSidebar(result.sidebar)
      } catch (loadError) {
        if (cancelled) return
        setError(loadError instanceof Error ? loadError.message : 'Could not load filters.')
      }
    }

    loadMetadata()

    return () => {
      cancelled = true
    }
  }, [metadataKey, slug])

  useEffect(() => {
    let cancelled = false

    async function loadFirstPage() {
      filterVersionRef.current += 1
      loadingMoreRef.current = false
      setLoadingMore(false)
      setError(null)
      const cached = productCache.get(productKey)
      const activeVersion = filterVersionRef.current

      if (cached) {
        setProducts(cached.items)
        setTotalCount(cached.totalCount)
        setPageNumber(cached.pageNumber)
        setTotalPages(cached.totalPages)
        hasLoadedOnceRef.current = true
        setInitialLoading(false)
        setRefreshing(true)
      } else if (hasLoadedOnceRef.current) {
        setRefreshing(true)
      } else {
        setInitialLoading(true)
      }

      try {
        let request = productPromiseCache.get(productKey)
        if (!request) {
          request = getProducts({
            ...currentFilter,
            pageNumber: 1,
          })
          productPromiseCache.set(productKey, request)
        }

        const result = await request
        if (cancelled || activeVersion !== filterVersionRef.current) return

        productCache.set(productKey, result)
        setProducts(result.items)
        setTotalCount(result.totalCount)
        setPageNumber(result.pageNumber)
        setTotalPages(result.totalPages)
        hasLoadedOnceRef.current = true
      } catch (loadError) {
        if (cancelled) return
        setError(loadError instanceof Error ? loadError.message : 'Could not load products.')
      } finally {
        if (!cancelled) {
          setInitialLoading(false)
          setRefreshing(false)
        }
      }
    }

    loadFirstPage()

    return () => {
      cancelled = true
    }
  }, [currentFilter, productKey])

  useEffect(() => {
    setCollapsedSections({})
  }, [slug])

  function replaceSearchParams(mutator: (params: URLSearchParams) => void) {
    const next = new URLSearchParams(searchParams)
    mutator(next)
    next.delete('pageNumber')
    setSearchParams(next)
  }

  function setSingleValue(key: string, value?: string) {
    replaceSearchParams((params) => {
      if (value && value.trim()) params.set(key, value)
      else params.delete(key)
    })
  }

  function toggleArrayValue(key: string, value: string, checked: boolean) {
    replaceSearchParams((params) => {
      const values = params.getAll(key).filter((item) => item !== value)
      params.delete(key)
      const nextValues = checked ? [...values, value] : values
      nextValues.forEach((item) => params.append(key, item))
    })
  }

  function toggleAttributeValue(key: string, value: string, checked: boolean) {
    toggleArrayValue(key, value, checked)
  }

  function resetFilters() {
    const next = new URLSearchParams()
    if (search.trim()) next.set('search', search.trim())
    setSearchParams(next)
  }

  function handleMinPriceInput(value: number) {
    const bounded = Math.min(value, draftMaxPrice - PRICE_MIN_GAP)
    setDraftMinPrice(Math.max(MIN_PRICE, bounded))
  }

  function handleMaxPriceInput(value: number) {
    const bounded = Math.max(value, draftMinPrice + PRICE_MIN_GAP)
    setDraftMaxPrice(Math.min(MAX_PRICE, bounded))
  }

  function commitPriceRange(nextMin: number, nextMax: number) {
    const safeMin = Math.max(MIN_PRICE, Math.min(nextMin, nextMax - PRICE_MIN_GAP))
    const safeMax = Math.min(MAX_PRICE, Math.max(nextMax, safeMin + PRICE_MIN_GAP))

    replaceSearchParams((params) => {
      if (safeMin > MIN_PRICE) params.set('minPrice', String(safeMin))
      else params.delete('minPrice')

      if (safeMax < MAX_PRICE) params.set('maxPrice', String(safeMax))
      else params.delete('maxPrice')
    })
  }

  async function loadMore() {
    if (
      loadingMoreRef.current ||
      initialLoadingRef.current ||
      refreshingRef.current ||
      pageNumberRef.current >= totalPagesRef.current
    ) {
      return
    }

    loadingMoreRef.current = true
    setLoadingMore(true)

    try {
      const nextPage = pageNumberRef.current + 1
      const activeVersion = filterVersionRef.current
      const result = await getProducts({
        ...(currentFilterRef.current ?? currentFilter),
        pageNumber: nextPage,
      })

      if (activeVersion !== filterVersionRef.current) {
        return
      }

      if (result.items.length === 0) {
        setTotalPages(pageNumberRef.current)
        return
      }

      setProducts((current) => {
        const existingIds = new Set(current.map((item) => item.id))
        const nextItems = result.items.filter((item) => !existingIds.has(item.id))
        return nextItems.length > 0 ? [...current, ...nextItems] : current
      })
      setPageNumber(nextPage)
      pageNumberRef.current = nextPage
      setTotalPages(result.totalPages)
      totalPagesRef.current = result.totalPages
    } catch (loadError) {
      setError(loadError instanceof Error ? loadError.message : 'Could not load more products.')
    } finally {
      loadingMoreRef.current = false
      setLoadingMore(false)
    }
  }

  useEffect(() => {
    loadMoreFnRef.current = loadMore
  })

  useEffect(() => {
    let ticking = false

    const tryLoadMoreOnScroll = () => {
      if (ticking) return

      ticking = true
      window.requestAnimationFrame(() => {
        const scrollPosition = window.innerHeight + window.scrollY
        const threshold = document.documentElement.scrollHeight - 320

        if (scrollPosition >= threshold) {
          void loadMoreFnRef.current()
        }

        ticking = false
      })
    }

    window.addEventListener('scroll', tryLoadMoreOnScroll, { passive: true })
    window.addEventListener('resize', tryLoadMoreOnScroll)
    tryLoadMoreOnScroll()

    return () => {
      window.removeEventListener('scroll', tryLoadMoreOnScroll)
      window.removeEventListener('resize', tryLoadMoreOnScroll)
    }
  }, [])

  function toggleCompare(product: CatalogProduct, checked: boolean) {
    const current = [...compareList]
    const categoryId = String(product.categoryId)
    const lockedCategoryId = current[0]?.categoryId

    if (checked && lockedCategoryId && lockedCategoryId !== categoryId) {
      const shouldReplace = window.confirm('Comparison only works for products in the same category. Clear the current comparison list and start a new one?')
      if (!shouldReplace) return
      current.length = 0
    }

    if (checked) {
      if (current.some((item) => item.id === product.id)) return
      if (current.length >= 4) {
        window.alert('You can compare up to 4 products at a time.')
        return
      }

      current.push({
        id: product.id,
        name: product.name,
        image: product.imageUrl,
        categoryId,
      })
    } else {
      const next = current.filter((item) => item.id !== product.id)
      window.localStorage.setItem(COMPARE_STORAGE_KEY, JSON.stringify(next))
      setCompareList(next)
      return
    }

    const normalized = normalizeCompareList(current)
    window.localStorage.setItem(COMPARE_STORAGE_KEY, JSON.stringify(normalized))
    setCompareList(normalized)
  }

  function removeCompareItem(id: string) {
    const next = compareList.filter((item) => item.id !== id)
    window.localStorage.setItem(COMPARE_STORAGE_KEY, JSON.stringify(next))
    setCompareList(next)
  }

  function clearCompare() {
    window.localStorage.setItem(COMPARE_STORAGE_KEY, JSON.stringify([]))
    setCompareList([])
  }

  const selectedCompareIds = new Set(compareList.map((item) => item.id))
  const minPercent = (draftMinPrice / MAX_PRICE) * 100
  const maxPercent = (draftMaxPrice / MAX_PRICE) * 100

  return (
    <main className="flex flex-1 justify-center px-4 py-6 md:px-10">
      <div className="flex w-full max-w-[1440px] flex-col gap-6 text-slate-900">
        <div className="flex flex-col gap-2">
          <nav className="flex flex-wrap items-center gap-2 text-sm text-slate-500">
            <Link className="transition hover:text-blue-700" to="/">
              Home
            </Link>
            <span className="material-symbols-outlined text-[16px]">chevron_right</span>
            {categoryTrail.length > 0 ? (
              categoryTrail.map((category, index) => (
                <span className="flex items-center gap-2" key={category.id}>
                  <Link className="capitalize transition hover:text-blue-700" to={`/products/${category.slug}`}>
                    {category.name}
                  </Link>
                  {index < categoryTrail.length - 1 ? <span className="material-symbols-outlined text-[16px]">chevron_right</span> : null}
                </span>
              ))
            ) : search ? (
              <>
                <Link className="transition hover:text-blue-700" to="/products">
                  All Products
                </Link>
                <span className="material-symbols-outlined text-[16px]">chevron_right</span>
                <span className="font-medium text-slate-900">Search: {search}</span>
              </>
            ) : (
              <span className="font-medium capitalize text-slate-900">{decodeLabel(slug)}</span>
            )}
          </nav>

          <div className="flex flex-wrap items-baseline justify-between gap-4">
            <h1 className="text-3xl font-bold tracking-tight text-slate-900">
              <span className="capitalize">{search ? `Results for "${search}"` : decodeLabel(slug)}</span>
              <span className="ml-2 text-lg font-normal text-slate-500">({totalCount} products)</span>
            </h1>
          </div>
        </div>

        <div className="flex flex-col gap-8 lg:flex-row">
          <aside className="w-full flex-shrink-0 lg:sticky lg:top-24 lg:h-[calc(100vh-7rem)] lg:w-[260px] lg:self-start">
            <div className="h-full overflow-y-auto rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
              <div className="mb-6 flex items-center justify-between">
                <h2 className="text-lg font-bold">Filter by</h2>
                <button className="text-xs font-medium text-slate-500 transition hover:text-blue-700" onClick={resetFilters} type="button">
                  Reset
                </button>
              </div>

              <div className="space-y-5">
                {sidebar.brands.length > 0 ? (
                  <section className="mb-3 border-b border-slate-200 pb-3">
                    <button
                      className="group mb-2 flex w-full items-center justify-between"
                      onClick={() => setCollapsedSections((current) => ({ ...current, brands: !current.brands }))}
                      type="button"
                    >
                      <span className="text-sm font-semibold">Brand</span>
                      <span className={`material-symbols-outlined text-[20px] text-slate-500 transition-transform duration-200 ${collapsedSections.brands ? 'rotate-180' : ''}`}>
                        expand_less
                      </span>
                    </button>
                    {!collapsedSections.brands ? (
                      <div className="space-y-2.5 px-1">
                        {sidebar.brands.map((brand) => (
                          <label className="group flex cursor-pointer items-center gap-3" key={brand.slug}>
                            <input
                              checked={selectedBrands.includes(brand.slug)}
                              className="h-4 w-4 rounded border-slate-300 text-blue-700 focus:ring-blue-700"
                              onChange={(event) => toggleArrayValue('brand', brand.slug, event.target.checked)}
                              type="checkbox"
                            />
                            <span className="text-sm text-slate-500 transition-colors group-hover:text-slate-900">{brand.name}</span>
                            <span className="ml-auto text-xs text-slate-500">{brand.productCount}</span>
                          </label>
                        ))}
                      </div>
                    ) : null}
                  </section>
                ) : null}

                <section className="mb-3 border-b border-slate-200 pb-3">
                  <div className="mb-2 flex items-center justify-between">
                    <span className="text-sm font-semibold">Price (VND)</span>
                  </div>
                  <div className="mt-2">
                    <div className="relative mx-3 mb-8 mt-2 h-2 rounded-full bg-slate-200">
                      <div
                        className="absolute z-10 h-full rounded-full bg-blue-700"
                        style={{
                          left: `${minPercent}%`,
                          right: `${100 - maxPercent}%`,
                        }}
                      />
                      <input
                        className={`dual-slider-input absolute m-0 h-full w-full bg-transparent ${activePriceThumb === 'min' ? 'z-40' : 'z-30'}`}
                        max={MAX_PRICE}
                        min={MIN_PRICE}
                        onChange={(event) => handleMinPriceInput(Number(event.target.value))}
                        onMouseDown={() => setActivePriceThumb('min')}
                        onKeyUp={() => commitPriceRange(draftMinPrice, draftMaxPrice)}
                        onPointerUp={() => commitPriceRange(draftMinPrice, draftMaxPrice)}
                        onTouchEnd={() => commitPriceRange(draftMinPrice, draftMaxPrice)}
                        step={PRICE_STEP}
                        type="range"
                        value={draftMinPrice}
                      />
                      <input
                        className={`dual-slider-input absolute m-0 h-full w-full bg-transparent ${activePriceThumb === 'max' ? 'z-40' : 'z-30'}`}
                        max={MAX_PRICE}
                        min={MIN_PRICE}
                        onChange={(event) => handleMaxPriceInput(Number(event.target.value))}
                        onMouseDown={() => setActivePriceThumb('max')}
                        onKeyUp={() => commitPriceRange(draftMinPrice, draftMaxPrice)}
                        onPointerUp={() => commitPriceRange(draftMinPrice, draftMaxPrice)}
                        onTouchEnd={() => commitPriceRange(draftMinPrice, draftMaxPrice)}
                        step={PRICE_STEP}
                        type="range"
                        value={draftMaxPrice}
                      />
                      <div
                        className="pointer-events-none absolute top-1/2 z-20 h-5 w-5 -translate-y-1/2 rounded-full border-2 border-blue-700 bg-white shadow-md"
                        style={{ left: `calc(${minPercent}% - 10px)` }}
                      />
                      <div
                        className="pointer-events-none absolute top-1/2 z-20 h-5 w-5 -translate-y-1/2 rounded-full border-2 border-blue-700 bg-white shadow-md"
                        style={{ left: `calc(${maxPercent}% - 10px)` }}
                      />
                    </div>
                    <input
                      readOnly
                      type="hidden"
                      value={draftMinPrice}
                    />
                    <input
                      readOnly
                      type="hidden"
                      value={draftMaxPrice}
                    />
                  </div>
                  <div className="mt-4 flex items-center justify-between gap-3">
                    <div className="flex w-full flex-col gap-1 text-center">
                      <p className="text-[10px] font-medium text-slate-500">Min</p>
                      <div className="rounded border border-slate-200 bg-slate-50 px-1 py-1.5 text-[11px] font-bold">
                        {formatCompactPrice(draftMinPrice)} VND
                      </div>
                    </div>
                    <span className="mt-4 font-bold text-slate-500">-</span>
                    <div className="flex w-full flex-col gap-1 text-center">
                      <p className="text-[10px] font-medium text-slate-500">Max</p>
                      <div className="rounded border border-slate-200 bg-slate-50 px-1 py-1.5 text-[11px] font-bold">
                        {draftMaxPrice >= MAX_PRICE ? '100M+ VND' : `${formatCompactPrice(draftMaxPrice)} VND`}
                      </div>
                    </div>
                  </div>
                </section>

                {sidebar.attributes.map((attribute) => {
                  const collapsed = collapsedSections[attribute.name]
                  const selectedValues = attributes[attribute.name] ?? []

                  return (
                    <section className="mb-3 border-b border-slate-200 pb-3 last:border-b-0 last:pb-0" key={attribute.name}>
                      <button
                        className="group mb-2 flex w-full items-center justify-between"
                        onClick={() =>
                          setCollapsedSections((current) => ({
                            ...current,
                            [attribute.name]: !current[attribute.name],
                          }))
                        }
                        type="button"
                      >
                        <span className="text-sm font-semibold">{attribute.name}</span>
                        <span className={`material-symbols-outlined text-[20px] text-slate-500 transition-transform duration-200 ${collapsed ? 'rotate-180' : ''}`}>
                          expand_less
                        </span>
                      </button>
                      {!collapsed ? (
                        <div className="flex flex-wrap gap-2">
                          {attribute.values.map((value) => (
                            <label className="cursor-pointer" key={`${attribute.name}-${value.value}`}>
                              <input
                                checked={selectedValues.includes(value.value)}
                                className="peer sr-only"
                                onChange={(event) => toggleAttributeValue(attribute.name, value.value, event.target.checked)}
                                type="checkbox"
                              />
                              <span className="inline-flex rounded-full border border-slate-200 bg-white px-3 py-1.5 text-xs font-medium text-slate-500 transition-all peer-checked:border-blue-700 peer-checked:bg-blue-50 peer-checked:text-blue-700">
                                {value.value}
                              </span>
                            </label>
                          ))}
                        </div>
                      ) : null}
                    </section>
                  )
                })}

                <section className="flex items-center justify-between py-2">
                  <span className="text-sm font-semibold">In Stock Only</span>
                  <label className="relative inline-flex cursor-pointer items-center">
                    <input
                      checked={inStockOnly}
                      className="peer sr-only"
                      onChange={(event) => setSingleValue('inStockOnly', event.target.checked ? 'true' : undefined)}
                      type="checkbox"
                    />
                    <span className="h-5 w-9 rounded-full bg-slate-200 after:absolute after:start-[2px] after:top-[2px] after:h-4 after:w-4 after:rounded-full after:border after:border-slate-300 after:bg-white after:transition-all peer-checked:bg-blue-700 peer-checked:after:translate-x-full peer-checked:after:border-white" />
                  </label>
                </section>
              </div>
            </div>
          </aside>

          <section className="flex min-w-0 flex-1 flex-col gap-6">
            <div className="flex flex-col justify-between gap-4 rounded-xl border border-slate-200 bg-white p-2 shadow-sm sm:flex-row sm:items-center">
              <div className="flex items-center gap-2 overflow-x-auto px-2 pb-1 sm:pb-0">
                <span className="mr-2 whitespace-nowrap text-sm font-medium text-slate-500">Sort by:</span>
                {[
                  ['popular', 'Popular'],
                  ['newest', 'Newest'],
                  ['price_asc', 'Price: Low-High'],
                  ['price_desc', 'Price: High-Low'],
                ].map(([value, label]) => (
                  <button
                    className={`rounded-full px-4 py-1.5 text-sm font-medium whitespace-nowrap transition-colors ${
                      sortBy === value ? 'bg-blue-50 text-blue-700' : 'text-slate-500 hover:bg-slate-100'
                    }`}
                    key={value}
                    onClick={() => setSingleValue('sortBy', value === 'popular' ? undefined : value)}
                    type="button"
                  >
                    {label}
                  </button>
                ))}
              </div>

              <div className="flex items-center gap-1 border-l border-slate-200 pl-2 sm:pl-4">
                {[
                  ['grid', 'grid_view'],
                  ['list', 'view_list'],
                ].map(([mode, icon]) => (
                  <button
                    className={`rounded p-1.5 transition-all ${viewMode === mode ? 'bg-blue-50 text-blue-700' : 'text-slate-500 hover:bg-slate-100'}`}
                    key={mode}
                    onClick={() => setSingleValue('viewMode', mode === 'grid' ? undefined : mode)}
                    title={mode}
                    type="button"
                  >
                    <span className="material-symbols-outlined text-[20px]">{icon}</span>
                  </button>
                ))}
              </div>
            </div>

            {error ? (
              <div className="rounded-xl border border-rose-200 bg-rose-50 px-5 py-4 text-sm font-medium text-rose-700">{error}</div>
            ) : null}

            {initialLoading ? (
              <div className="grid grid-cols-1 gap-4 md:gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                {Array.from({ length: 8 }).map((_, index) => (
                  <div className="h-[340px] animate-pulse rounded-xl border border-slate-200 bg-white shadow-sm" key={index} />
                ))}
              </div>
            ) : products.length > 0 ? (
              <>
                <div className="relative">
                  {refreshing ? <div className="pointer-events-none absolute inset-0 z-10 rounded-xl bg-white/45" /> : null}
                  <div
                    className={`${viewMode === 'list' ? 'flex flex-col gap-4' : 'grid grid-cols-1 gap-4 md:gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4'} transition ${
                      refreshing ? 'opacity-70' : 'opacity-100'
                    }`}
                  >
                    {products.map((product) => (
                      <ProductCard
                        key={product.id}
                        onCompareToggle={toggleCompare}
                        product={product}
                        selected={selectedCompareIds.has(product.id)}
                        viewMode={viewMode}
                      />
                    ))}
                  </div>
                </div>

                <div className="mt-8 flex flex-col items-center gap-4">
                  {loadingMore ? (
                    <div className="mt-2 flex justify-center py-4">
                      <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-700 border-r-transparent" />
                    </div>
                  ) : null}
                  {pageNumber >= totalPages ? <p className="py-4 text-center text-sm text-slate-500">You've reached the end of the list.</p> : null}
                </div>
              </>
            ) : (
              <div className="rounded-xl border border-dashed border-slate-300 bg-white px-6 py-14 text-center">
                <p className="text-xs font-black uppercase tracking-[0.24em] text-slate-400">No Match</p>
                <h3 className="mt-2 text-2xl font-bold text-slate-950">No products found for this setup.</h3>
                <p className="mt-3 text-sm text-slate-500">Try a wider price range, fewer filters, or switch to another category.</p>
                <button className="mt-5 rounded-lg bg-slate-950 px-5 py-3 text-sm font-bold text-white transition hover:bg-blue-700" onClick={resetFilters} type="button">
                  Clear filters
                </button>
              </div>
            )}
          </section>
        </div>

        <div
          className={`fixed bottom-10 left-1/2 z-[60] -translate-x-1/2 transition-all duration-700 ${
            compareList.length > 0 ? 'translate-y-0 scale-100 opacity-100' : 'pointer-events-none translate-y-40 scale-95 opacity-0'
          }`}
        >
          <div className="relative">
            <div className="absolute inset-0 rounded-3xl bg-blue-700/20 blur-2xl" />
            <div className="relative flex min-w-[300px] items-center gap-4 rounded-[2rem] border border-white/20 bg-white/80 p-3 shadow-[0_20px_50px_rgba(0,0,0,0.15)] backdrop-blur-2xl sm:min-w-[450px] sm:gap-8 sm:p-4">
              <div className="flex items-center gap-4 pl-2">
                <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-blue-700 text-white shadow-lg shadow-blue-700/30">
                  <span className="material-symbols-outlined text-[24px]">compare_arrows</span>
                </div>
                <div className="hidden flex-col sm:flex">
                  <span className="mb-1 text-[10px] font-black uppercase tracking-[0.2em] text-blue-700">Vault</span>
                  <span className="whitespace-nowrap text-sm font-bold text-slate-900">{compareList.length}/4 Selected</span>
                </div>
              </div>

              <div className="flex max-w-[120px] items-center gap-3 overflow-x-auto border-x border-slate-200/60 px-2 sm:max-w-[280px]">
                {compareList.map((item) => (
                  <div className="relative flex-shrink-0" key={item.id}>
                    <img
                      alt={item.name}
                      className="h-12 w-12 rounded-xl border border-slate-200 bg-white object-cover"
                      src={item.image || 'https://placehold.co/56x56/f8fafc/94a3b8?text=GZ'}
                      title={item.name}
                    />
                    <button
                      className="absolute -right-1.5 -top-1.5 flex h-5 w-5 items-center justify-center rounded-full bg-rose-500 text-white shadow-lg"
                      onClick={() => removeCompareItem(item.id)}
                      type="button"
                    >
                      <span className="material-symbols-outlined text-[12px]">close</span>
                    </button>
                  </div>
                ))}
                {Array.from({ length: Math.max(0, 4 - compareList.length) }).map((_, index) => (
                  <div className="flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-xl border-2 border-dashed border-slate-200 text-slate-300" key={`placeholder-${index}`}>
                    <span className="material-symbols-outlined text-[18px]">add</span>
                  </div>
                ))}
              </div>

              <div className="flex items-center gap-2 pr-1 sm:gap-4">
                <button className="rounded-full p-2 text-slate-400 transition-all hover:bg-rose-50 hover:text-rose-500" onClick={clearCompare} type="button">
                  <span className="material-symbols-outlined text-[20px]">delete_sweep</span>
                </button>
                <button
                  className="rounded-2xl bg-slate-900 px-6 py-3 text-xs font-black uppercase tracking-widest text-white transition-all hover:-translate-y-0.5 hover:shadow-xl disabled:pointer-events-none disabled:opacity-30"
                  disabled={compareList.length < 2}
                  onClick={() => {
                    const ids = compareList.map((item) => item.id).join(',')
                    const categoryId = compareList[0]?.categoryId
                    window.location.href = `/compare?categoryId=${categoryId}&ids=${ids}`
                  }}
                  type="button"
                >
                  <span className="flex items-center gap-2">
                    Compare
                    <span className="material-symbols-outlined text-[18px]">bolt</span>
                  </span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>
  )
}
