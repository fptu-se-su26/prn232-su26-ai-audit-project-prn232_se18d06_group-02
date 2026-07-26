import { useEffect, useMemo, useState } from 'react';
import { sellerApi } from '../../api/seller';

// ---------- Types (mirror backend DTOs) ----------
interface Metric { current: number; previous: number; changePct: number | null }
interface Period { start: string; end: string; label: string; granularity: string }

interface SalesReport {
  hasStore: boolean; storeName: string; period: Period;
  revenue: Metric; netRevenue: Metric; orders: Metric; units: Metric; averageOrderValue: Metric;
  series: Array<{ label: string; date: string; revenue: number; orders: number; units: number }>;
}
interface ProductReport {
  hasStore: boolean; period: Period; lowStockThreshold: number;
  topProducts: Array<{ productId: string; productName: string; unitsSold: number; revenue: number; orderCount: number }>;
  lowStock: Array<{ variantId: string; productName: string; variantName: string; sku: string; stockQuantity: number }>;
}
interface OperationsReport {
  hasStore: boolean; period: Period; totalOrders: Metric;
  cancellationRate: number; refundRate: number; completionRate: number; avgFulfillmentHours: number | null;
  statusBreakdown: Array<{ status: string; count: number }>;
}
interface CustomerReport {
  hasStore: boolean; period: Period; uniqueBuyers: Metric; newBuyers: number; returningBuyers: number;
  repeatRate: number; newFollowers: Metric; totalFollowers: number;
  followerSeries: Array<{ label: string; date: string; newFollowers: number }>;
}
interface MarketingReport {
  hasStore: boolean; period: Period; voucherUsages: Metric; totalDiscount: Metric;
  vouchers: Array<{ voucherId: string; code: string; name: string; usageCount: number; totalDiscount: number }>;
}
interface ReviewsReport {
  hasStore: boolean; period: Period; avgRating: number; totalReviews: number; repliedCount: number;
  newReviews: Metric; distribution: Array<{ rating: number; count: number }>;
  trend: Array<{ label: string; date: string; avgRating: number; count: number }>;
}

type TabKey = 'sales' | 'products' | 'operations' | 'customers' | 'marketing' | 'reviews';

const TABS: Array<{ key: TabKey; label: string }> = [
  { key: 'sales', label: 'Sales' },
  { key: 'products', label: 'Products' },
  { key: 'operations', label: 'Operations' },
  { key: 'customers', label: 'Customers' },
  { key: 'marketing', label: 'Marketing' },
  { key: 'reviews', label: 'Reviews' },
];

const RANGES: Array<{ key: string; label: string }> = [
  { key: 'today', label: 'Today' },
  { key: '7d', label: '7 days' },
  { key: '30d', label: '30 days' },
  { key: 'thisMonth', label: 'This month' },
  { key: 'lastMonth', label: 'Last month' },
];

// ---------- Helpers ----------
const vnd = (n: number) => `${(n ?? 0).toLocaleString()} ₫`;
const num = (n: number) => (n ?? 0).toLocaleString();

function ChangeBadge({ pct }: { pct: number | null }) {
  if (pct === null || pct === undefined) return <span style={{ color: '#999', fontSize: 12 }}>—</span>;
  const up = pct >= 0;
  return (
    <span style={{ fontSize: 12, fontWeight: 600, color: up ? '#38a169' : '#e53e3e' }}>
      {up ? '▲' : '▼'} {Math.abs(pct).toFixed(1)}%
    </span>
  );
}

function MetricCard({ label, value, metric, color }: { label: string; value: string; metric?: Metric; color: string }) {
  return (
    <div style={{ flex: '1 1 180px', padding: '1.25rem', background: '#fff', borderRadius: 10, border: '1px solid #eee', borderLeft: `4px solid ${color}` }}>
      <p style={{ margin: 0, color: '#888', fontSize: 13 }}>{label}</p>
      <p style={{ margin: '0.4rem 0 0.2rem', fontSize: 22, fontWeight: 700, color }}>{value}</p>
      {metric && <ChangeBadge pct={metric.changePct} />}
      {metric && <span style={{ color: '#aaa', fontSize: 11, marginLeft: 6 }}>vs prev</span>}
    </div>
  );
}

// Simple inline SVG bar chart.
function BarChart({ data, valueKey, color = '#3182ce', format = num }:
  { data: Array<Record<string, unknown>>; valueKey: string; color?: string; format?: (n: number) => string }) {
  const values = data.map(d => Number(d[valueKey]) || 0);
  const max = Math.max(1, ...values);
  if (data.length === 0) return <p style={{ color: '#888' }}>No data for this period.</p>;
  return (
    <div style={{ display: 'flex', alignItems: 'flex-end', gap: 6, height: 200, padding: '1rem 0', overflowX: 'auto' }}>
      {data.map((d, i) => {
        const v = values[i];
        const h = Math.round((v / max) * 170);
        return (
          <div key={i} style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', minWidth: 34, flex: '1 0 auto' }}>
            <span style={{ fontSize: 10, color: '#666', marginBottom: 4 }} title={format(v)}>{v > 0 ? format(v) : ''}</span>
            <div style={{ width: '70%', height: Math.max(2, h), background: color, borderRadius: '3px 3px 0 0' }} />
            <span style={{ fontSize: 10, color: '#999', marginTop: 4, whiteSpace: 'nowrap' }}>{String(d.label)}</span>
          </div>
        );
      })}
    </div>
  );
}

const th: React.CSSProperties = { padding: '0.6rem 0.75rem', textAlign: 'left', fontSize: 13, color: '#555' };
const td: React.CSSProperties = { padding: '0.6rem 0.75rem', fontSize: 14 };

export default function SellerReportsPage() {
  const [tab, setTab] = useState<TabKey>('sales');
  const [range, setRange] = useState('30d');
  const [loading, setLoading] = useState(true);
  const [data, setData] = useState<unknown>(null);
  const [error, setError] = useState<string | null>(null);
  const [exporting, setExporting] = useState(false);

  const params = useMemo(() => ({ range }), [range]);

  useEffect(() => {
    setLoading(true);
    setError(null);
    const fetcher = sellerApi.reports[tab] as (p?: Record<string, unknown>) => Promise<unknown>;
    fetcher(params)
      .then(setData)
      .catch(e => setError(e instanceof Error ? e.message : 'Failed to load report'))
      .finally(() => setLoading(false));
  }, [tab, params]);

  const handleExport = async () => {
    setExporting(true);
    try { await sellerApi.reports.exportSalesCsv(params); }
    catch (e) { alert(e instanceof Error ? e.message : 'Export failed'); }
    finally { setExporting(false); }
  };

  return (
    <div style={{ padding: '2rem', background: '#fafafa', minHeight: '100%' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 12 }}>
        <h1 style={{ margin: 0 }}>Reports</h1>
        {tab === 'sales' && (
          <button onClick={handleExport} disabled={exporting}
            style={{ padding: '0.55rem 1.1rem', background: '#2d3748', color: '#fff', border: 'none', borderRadius: 6, fontWeight: 600, cursor: 'pointer', opacity: exporting ? 0.6 : 1 }}>
            {exporting ? 'Exporting…' : '⬇ Export CSV'}
          </button>
        )}
      </div>

      {/* Range selector */}
      <div style={{ display: 'flex', gap: 8, margin: '1.25rem 0', flexWrap: 'wrap' }}>
        {RANGES.map(r => (
          <button key={r.key} onClick={() => setRange(r.key)}
            style={{ padding: '0.4rem 0.9rem', borderRadius: 20, border: '1px solid #ddd', cursor: 'pointer',
              background: range === r.key ? '#3182ce' : '#fff', color: range === r.key ? '#fff' : '#444', fontWeight: 600, fontSize: 13 }}>
            {r.label}
          </button>
        ))}
      </div>

      {/* Tabs */}
      <div style={{ display: 'flex', gap: 4, borderBottom: '2px solid #eee', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        {TABS.map(t => (
          <button key={t.key} onClick={() => setTab(t.key)}
            style={{ padding: '0.6rem 1.1rem', border: 'none', background: 'none', cursor: 'pointer', fontSize: 14, fontWeight: 600,
              color: tab === t.key ? '#3182ce' : '#777', borderBottom: tab === t.key ? '2px solid #3182ce' : '2px solid transparent', marginBottom: -2 }}>
            {t.label}
          </button>
        ))}
      </div>

      {loading && <div style={{ padding: '2rem', color: '#888' }}>Loading…</div>}
      {error && <div style={{ padding: '2rem', color: '#e53e3e' }}>{error}</div>}
      {!loading && !error && data != null && (data as { hasStore?: boolean }).hasStore === false && (
        <div style={{ padding: '2rem', color: '#888' }}>You don't have a store yet.</div>
      )}

      {!loading && !error && data != null && (data as { hasStore?: boolean }).hasStore !== false && (
        <>
          {tab === 'sales' && <SalesTab d={data as SalesReport} />}
          {tab === 'products' && <ProductsTab d={data as ProductReport} />}
          {tab === 'operations' && <OperationsTab d={data as OperationsReport} />}
          {tab === 'customers' && <CustomersTab d={data as CustomerReport} />}
          {tab === 'marketing' && <MarketingTab d={data as MarketingReport} />}
          {tab === 'reviews' && <ReviewsTab d={data as ReviewsReport} />}
        </>
      )}
    </div>
  );
}

function Cards({ children }: { children: React.ReactNode }) {
  return <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', marginBottom: '1.5rem' }}>{children}</div>;
}
function Panel({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div style={{ background: '#fff', border: '1px solid #eee', borderRadius: 10, padding: '1.25rem', marginBottom: '1.5rem' }}>
      <h3 style={{ margin: '0 0 0.75rem', fontSize: 16 }}>{title}</h3>
      {children}
    </div>
  );
}

// ---------- Tabs ----------
function SalesTab({ d }: { d: SalesReport }) {
  return (
    <>
      <Cards>
        <MetricCard label="Revenue" value={vnd(d.revenue.current)} metric={d.revenue} color="#38a169" />
        <MetricCard label="Net Revenue" value={vnd(d.netRevenue.current)} metric={d.netRevenue} color="#319795" />
        <MetricCard label="Orders" value={num(d.orders.current)} metric={d.orders} color="#3182ce" />
        <MetricCard label="Units Sold" value={num(d.units.current)} metric={d.units} color="#805ad5" />
        <MetricCard label="Avg Order Value" value={vnd(d.averageOrderValue.current)} metric={d.averageOrderValue} color="#dd6b20" />
      </Cards>
      <Panel title={`Revenue trend (${d.period.granularity})`}>
        <BarChart data={d.series as unknown as Array<Record<string, unknown>>} valueKey="revenue" color="#38a169" format={vnd} />
      </Panel>
      <Panel title="Orders trend">
        <BarChart data={d.series as unknown as Array<Record<string, unknown>>} valueKey="orders" color="#3182ce" />
      </Panel>
    </>
  );
}

function ProductsTab({ d }: { d: ProductReport }) {
  return (
    <>
      <Panel title="Top products by revenue">
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead><tr style={{ background: '#f7f7f7' }}>
            <th style={th}>Product</th><th style={{ ...th, textAlign: 'right' }}>Units</th>
            <th style={{ ...th, textAlign: 'right' }}>Orders</th><th style={{ ...th, textAlign: 'right' }}>Revenue</th>
          </tr></thead>
          <tbody>
            {d.topProducts.map(p => (
              <tr key={p.productId} style={{ borderBottom: '1px solid #f0f0f0' }}>
                <td style={td}>{p.productName}</td>
                <td style={{ ...td, textAlign: 'right' }}>{num(p.unitsSold)}</td>
                <td style={{ ...td, textAlign: 'right' }}>{num(p.orderCount)}</td>
                <td style={{ ...td, textAlign: 'right', fontWeight: 600 }}>{vnd(p.revenue)}</td>
              </tr>
            ))}
            {d.topProducts.length === 0 && <tr><td style={{ ...td, color: '#888', textAlign: 'center' }} colSpan={4}>No sales in this period.</td></tr>}
          </tbody>
        </table>
      </Panel>
      <Panel title={`Low stock (≤ ${d.lowStockThreshold})`}>
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead><tr style={{ background: '#f7f7f7' }}>
            <th style={th}>Product</th><th style={th}>Variant</th><th style={th}>SKU</th><th style={{ ...th, textAlign: 'right' }}>Stock</th>
          </tr></thead>
          <tbody>
            {d.lowStock.map(v => (
              <tr key={v.variantId} style={{ borderBottom: '1px solid #f0f0f0' }}>
                <td style={td}>{v.productName}</td>
                <td style={td}>{v.variantName}</td>
                <td style={{ ...td, color: '#777' }}>{v.sku}</td>
                <td style={{ ...td, textAlign: 'right', fontWeight: 700, color: v.stockQuantity === 0 ? '#e53e3e' : '#dd6b20' }}>{v.stockQuantity}</td>
              </tr>
            ))}
            {d.lowStock.length === 0 && <tr><td style={{ ...td, color: '#888', textAlign: 'center' }} colSpan={4}>All variants are well stocked. 🎉</td></tr>}
          </tbody>
        </table>
      </Panel>
    </>
  );
}

function OperationsTab({ d }: { d: OperationsReport }) {
  return (
    <>
      <Cards>
        <MetricCard label="Total Orders" value={num(d.totalOrders.current)} metric={d.totalOrders} color="#3182ce" />
        <MetricCard label="Cancellation Rate" value={`${d.cancellationRate}%`} color="#e53e3e" />
        <MetricCard label="Refund Rate" value={`${d.refundRate}%`} color="#dd6b20" />
        <MetricCard label="Completion Rate" value={`${d.completionRate}%`} color="#38a169" />
        <MetricCard label="Avg Fulfillment" value={d.avgFulfillmentHours == null ? '—' : `${d.avgFulfillmentHours.toFixed(1)}h`} color="#805ad5" />
      </Cards>
      <Panel title="Order status breakdown">
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead><tr style={{ background: '#f7f7f7' }}><th style={th}>Status</th><th style={{ ...th, textAlign: 'right' }}>Count</th></tr></thead>
          <tbody>
            {d.statusBreakdown.map(s => (
              <tr key={s.status} style={{ borderBottom: '1px solid #f0f0f0' }}>
                <td style={td}>{s.status}</td><td style={{ ...td, textAlign: 'right' }}>{num(s.count)}</td>
              </tr>
            ))}
            {d.statusBreakdown.length === 0 && <tr><td style={{ ...td, color: '#888', textAlign: 'center' }} colSpan={2}>No orders in this period.</td></tr>}
          </tbody>
        </table>
      </Panel>
    </>
  );
}

function CustomersTab({ d }: { d: CustomerReport }) {
  return (
    <>
      <Cards>
        <MetricCard label="Unique Buyers" value={num(d.uniqueBuyers.current)} metric={d.uniqueBuyers} color="#3182ce" />
        <MetricCard label="New Buyers" value={num(d.newBuyers)} color="#38a169" />
        <MetricCard label="Returning Buyers" value={num(d.returningBuyers)} color="#805ad5" />
        <MetricCard label="Repeat Rate" value={`${d.repeatRate}%`} color="#dd6b20" />
        <MetricCard label="New Followers" value={num(d.newFollowers.current)} metric={d.newFollowers} color="#d53f8c" />
        <MetricCard label="Total Followers" value={num(d.totalFollowers)} color="#319795" />
      </Cards>
      <Panel title="New followers trend">
        <BarChart data={d.followerSeries as unknown as Array<Record<string, unknown>>} valueKey="newFollowers" color="#d53f8c" />
      </Panel>
    </>
  );
}

function MarketingTab({ d }: { d: MarketingReport }) {
  return (
    <>
      <Cards>
        <MetricCard label="Voucher Uses" value={num(d.voucherUsages.current)} metric={d.voucherUsages} color="#3182ce" />
        <MetricCard label="Total Discount Given" value={vnd(d.totalDiscount.current)} metric={d.totalDiscount} color="#dd6b20" />
      </Cards>
      <Panel title="Voucher performance">
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead><tr style={{ background: '#f7f7f7' }}>
            <th style={th}>Code</th><th style={th}>Name</th>
            <th style={{ ...th, textAlign: 'right' }}>Uses</th><th style={{ ...th, textAlign: 'right' }}>Discount</th>
          </tr></thead>
          <tbody>
            {d.vouchers.map(v => (
              <tr key={v.voucherId} style={{ borderBottom: '1px solid #f0f0f0' }}>
                <td style={{ ...td, fontFamily: 'monospace' }}>{v.code}</td>
                <td style={td}>{v.name}</td>
                <td style={{ ...td, textAlign: 'right' }}>{num(v.usageCount)}</td>
                <td style={{ ...td, textAlign: 'right', fontWeight: 600 }}>{vnd(v.totalDiscount)}</td>
              </tr>
            ))}
            {d.vouchers.length === 0 && <tr><td style={{ ...td, color: '#888', textAlign: 'center' }} colSpan={4}>No voucher usage in this period.</td></tr>}
          </tbody>
        </table>
      </Panel>
    </>
  );
}

function ReviewsTab({ d }: { d: ReviewsReport }) {
  const maxCount = Math.max(1, ...d.distribution.map(x => x.count));
  return (
    <>
      <Cards>
        <MetricCard label="Avg Rating" value={`${d.avgRating.toFixed(2)} ★`} color="#dd6b20" />
        <MetricCard label="Total Reviews" value={num(d.totalReviews)} metric={d.newReviews} color="#3182ce" />
        <MetricCard label="Replied" value={`${d.repliedCount}/${d.totalReviews}`} color="#38a169" />
      </Cards>
      <Panel title="Rating distribution">
        {[...d.distribution].sort((a, b) => b.rating - a.rating).map(x => (
          <div key={x.rating} style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 8 }}>
            <span style={{ width: 40, fontSize: 13, color: '#555' }}>{x.rating} ★</span>
            <div style={{ flex: 1, background: '#f0f0f0', borderRadius: 4, height: 16 }}>
              <div style={{ width: `${(x.count / maxCount) * 100}%`, background: '#dd6b20', height: '100%', borderRadius: 4 }} />
            </div>
            <span style={{ width: 40, textAlign: 'right', fontSize: 13, color: '#555' }}>{x.count}</span>
          </div>
        ))}
      </Panel>
      <Panel title="Reviews trend">
        <BarChart data={d.trend as unknown as Array<Record<string, unknown>>} valueKey="count" color="#3182ce" />
      </Panel>
    </>
  );
}
