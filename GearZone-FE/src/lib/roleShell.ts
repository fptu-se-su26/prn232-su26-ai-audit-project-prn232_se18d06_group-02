export type ShellRoleKey = 'customer' | 'store-owner' | 'admin' | 'staff'

export interface ShellMetric {
  label: string
  value: string
  detail: string
}

export interface ShellWorkflowItem {
  title: string
  description: string
  icon: string
}

export interface ShellBackendCheck {
  label: string
  status: 'ready' | 'partial' | 'pending'
  detail: string
}

export interface ShellAction {
  label: string
  description: string
  icon: string
  targetId: string
}

export interface ShellConfig {
  key: ShellRoleKey
  label: string
  badge: string
  title: string
  subtitle: string
  accent: string
  heroIcon: string
  routeHint: string
  summary: string
  metrics: ShellMetric[]
  workflow: ShellWorkflowItem[]
  backendChecks: ShellBackendCheck[]
  actions: ShellAction[]
}

export const shellConfigMap: Record<ShellRoleKey, ShellConfig> = {
  customer: {
    key: 'customer',
    label: 'Customer',
    badge: 'Customer workspace',
    title: 'Customer Shell',
    subtitle: 'Browse, buy, track, and support flows share one consistent customer frame.',
    accent: 'from-cyan-500 to-blue-600',
    heroIcon: 'shopping_bag',
    routeHint: '/customer',
    summary:
      'This shell is ready for customer-facing screens, with backend coverage for catalog, cart, checkout, orders, reviews, and chat endpoints.',
    metrics: [
      { label: 'Primary flows', value: '6', detail: 'Catalog, cart, checkout, orders, reviews, chat' },
      { label: 'API readiness', value: 'Ready', detail: 'Public and customer APIs are available' },
      { label: 'UX scope', value: 'Unified', detail: 'Single frame for all customer journeys' },
    ],
    workflow: [
      { title: 'Explore the catalog', description: 'Surface products, filters, and store detail entry points.', icon: 'search' },
      { title: 'Complete the basket', description: 'Keep cart, checkout, and payment steps inside the same shell.', icon: 'shopping_cart' },
      { title: 'Stay informed', description: 'Expose order tracking, reviews, and support touchpoints from one place.', icon: 'notifications' },
    ],
    backendChecks: [
      { label: 'Auth', status: 'ready', detail: 'Login, register, email verification, and profile lookup exist.' },
      { label: 'Catalog', status: 'ready', detail: 'Products, store profile, and browse APIs are present.' },
      { label: 'Cart & checkout', status: 'ready', detail: 'Cart and checkout controllers already exist.' },
      { label: 'Orders & reviews', status: 'ready', detail: 'Order, tracking, and review APIs are present.' },
    ],
    actions: [
      { label: 'Browse catalog', description: 'Jump into product discovery.', icon: 'inventory_2', targetId: 'workflow' },
      { label: 'Track orders', description: 'Review delivery and fulfillment progress.', icon: 'local_shipping', targetId: 'backend' },
      { label: 'Manage profile', description: 'Keep account details current.', icon: 'person', targetId: 'summary' },
    ],
  },
  'store-owner': {
    key: 'store-owner',
    label: 'Store Owner',
    badge: 'Store operations',
    title: 'Store Owner Shell',
    subtitle: 'A compact operating room for products, orders, revenue, vouchers, and store settings.',
    accent: 'from-amber-500 to-orange-600',
    heroIcon: 'storefront',
    routeHint: '/store-owner',
    summary:
      'This shell matches the backend seller scope. Dedicated Seller controllers already exist for dashboard, products, orders, revenue, vouchers, and store settings.',
    metrics: [
      { label: 'Operational modules', value: '6', detail: 'Dashboard, products, orders, revenue, vouchers, settings' },
      { label: 'API readiness', value: 'Ready', detail: 'Seller APIs are implemented and role-protected' },
      { label: 'Focus', value: 'Operations', detail: 'Optimized for store management workflows' },
    ],
    workflow: [
      { title: 'Monitor store health', description: 'Track sales, order volume, and store KPIs from one dashboard.', icon: 'insights' },
      { title: 'Manage products', description: 'Keep inventory and catalog changes inside a seller frame.', icon: 'edit_note' },
      { title: 'Handle fulfillment', description: 'Review orders, vouchers, and revenue without switching contexts.', icon: 'receipt_long' },
    ],
    backendChecks: [
      { label: 'Seller dashboard', status: 'ready', detail: 'Seller dashboard controller exists.' },
      { label: 'Products', status: 'ready', detail: 'Seller products API is implemented.' },
      { label: 'Orders & revenue', status: 'ready', detail: 'Seller order and revenue APIs are implemented.' },
      { label: 'Store settings', status: 'ready', detail: 'Store settings and voucher APIs are available.' },
    ],
    actions: [
      { label: 'Review products', description: 'Manage catalog content.', icon: 'inventory', targetId: 'workflow' },
      { label: 'Check revenue', description: 'Inspect performance and payouts.', icon: 'payments', targetId: 'backend' },
      { label: 'Update store settings', description: 'Control store identity and preferences.', icon: 'settings', targetId: 'summary' },
    ],
  },
  admin: {
    key: 'admin',
    label: 'Admin',
    badge: 'Governance console',
    title: 'Admin Shell',
    subtitle: 'Platform oversight for users, stores, payouts, vouchers, products, and system settings.',
    accent: 'from-fuchsia-500 to-violet-600',
    heroIcon: 'admin_panel_settings',
    routeHint: '/admin',
    summary:
      'This shell is backed by dedicated Admin controllers. The backend already exposes management APIs for users, stores, store applications, products, orders, payouts, vouchers, transactions, wallet, and settings.',
    metrics: [
      { label: 'Governance areas', value: '10+', detail: 'Users, stores, products, payouts, settings, and more' },
      { label: 'API readiness', value: 'Ready', detail: 'Admin controllers are present and role-protected' },
      { label: 'Control type', value: 'Policy', detail: 'Designed for moderation and platform operations' },
    ],
    workflow: [
      { title: 'Review platform health', description: 'Surface high-signal metrics, exceptions, and workflow queues.', icon: 'monitor_heart' },
      { title: 'Resolve disputes', description: 'Handle store applications, payouts, and transaction issues.', icon: 'gavel' },
      { title: 'Maintain catalog integrity', description: 'Control products, brands, categories, and vouchers centrally.', icon: 'fact_check' },
    ],
    backendChecks: [
      { label: 'Users & stores', status: 'ready', detail: 'Admin users and store management controllers exist.' },
      { label: 'Payouts & wallet', status: 'ready', detail: 'Payout, wallet, and transaction APIs are present.' },
      { label: 'Catalog governance', status: 'ready', detail: 'Products, brands, categories, and voucher APIs exist.' },
      { label: 'System settings', status: 'ready', detail: 'Admin settings controller is implemented.' },
    ],
    actions: [
      { label: 'Open governance', description: 'Review moderation queues.', icon: 'shield', targetId: 'workflow' },
      { label: 'Inspect payouts', description: 'Audit financial flows and batches.', icon: 'account_balance_wallet', targetId: 'backend' },
      { label: 'Manage system settings', description: 'Adjust platform-wide configuration.', icon: 'tune', targetId: 'summary' },
    ],
  },
  staff: {
    key: 'staff',
    label: 'Staff',
    badge: 'Support workspace',
    title: 'Staff Shell',
    subtitle: 'An operations shell for support, moderation, and internal handling across the platform.',
    accent: 'from-emerald-500 to-teal-600',
    heroIcon: 'badge',
    routeHint: '/staff',
    summary:
      'The staff shell is ready in the frontend, but I did not find a dedicated Staff API controller group in the backend. This should be treated as shell-only until the API contract is defined.',
    metrics: [
      { label: 'Internal modules', value: '4', detail: 'Support, moderation, queue triage, and escalation' },
      { label: 'API readiness', value: 'Partial', detail: 'Backend staff endpoints were not identified' },
      { label: 'Status', value: 'Shell ready', detail: 'UI scaffold exists, backend contract still pending' },
    ],
    workflow: [
      { title: 'Triage requests', description: 'Queue and prioritize customer or seller issues.', icon: 'support_agent' },
      { title: 'Resolve incidents', description: 'Provide a space for internal reviews and follow-up actions.', icon: 'report_problem' },
      { title: 'Escalate correctly', description: 'Route unresolved cases to admin or operations owners.', icon: 'call_split' },
    ],
    backendChecks: [
      { label: 'Dedicated Staff APIs', status: 'pending', detail: 'No Staff controller group was found in the backend scan.' },
      { label: 'Role seed', status: 'ready', detail: 'The Staff role is seeded in IdentitySeeder.' },
      { label: 'Auth support', status: 'ready', detail: 'Auth service can resolve the user role after login.' },
      { label: 'Integration plan', status: 'partial', detail: 'Frontend shell is ready, backend contract needs implementation.' },
    ],
    actions: [
      { label: 'Open queue', description: 'Review incoming tickets and cases.', icon: 'inbox', targetId: 'workflow' },
      { label: 'Escalate', description: 'Route blocked items upward.', icon: 'priority_high', targetId: 'backend' },
      { label: 'Check coverage', description: 'Verify missing backend contracts.', icon: 'fact_check', targetId: 'summary' },
    ],
  },
}

const roleRouteMap: Record<string, ShellRoleKey> = {
  'Super Admin': 'admin',
  Admin: 'admin',
  'Store Owner': 'store-owner',
  Customer: 'customer',
  Staff: 'staff',
}

export function resolveShellRole(role?: string | null): ShellRoleKey {
  if (!role) return 'customer'
  return roleRouteMap[role] ?? 'customer'
}

export function getShellPath(role: ShellRoleKey) {
  return `/${role}`
}

export function getRoleLabel(role?: string | null) {
  return shellConfigMap[resolveShellRole(role)].label
}
