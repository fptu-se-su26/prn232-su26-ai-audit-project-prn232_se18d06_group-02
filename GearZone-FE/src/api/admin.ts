import apiClient, { unwrap } from './apiClient'

export interface DashboardQueryParams {
  period?: string
  startDate?: string
  endDate?: string
}

export interface DashboardKpis {
  grossRevenue: number
  revenueGrowth: number
  totalOrders: number
  orderGrowth: number
  activeStores: number
  storeGrowth: number
  newUsers: number
  userGrowth: number
  disputeRate: number
  disputeGrowth: number
}

export interface ChartDataPoint {
  label: string
  value: number
  secondaryValue?: number | null
}

export interface CategoryRevenueDto {
  categoryName: string
  revenue: number
  percentage: number
}

export interface OrderStatusBreakdownDto {
  status: string
  count: number
  percentage: number
  colorClass: string
}

export interface DashboardStoreDto {
  storeId: string
  storeName: string
  category: string
  revenue: number
  orders: number
  rating: number
  commission: number
  growth: number
  status: string
  logoUrl?: string | null
}

export interface AdminDashboardDto {
  kpiCards: DashboardKpis
  revenueOverview: ChartDataPoint[]
  revenueByCategory: CategoryRevenueDto[]
  orderStatusBreakdown: OrderStatusBreakdownDto[]
  topStores: DashboardStoreDto[]
  userGrowth: ChartDataPoint[]
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface UserQueryParams {
  searchTerm?: string
  role?: string
  isActive?: boolean | ''
  pageNumber?: number
  pageSize?: number
}

export interface AdminUserStatsDto {
  totalUsers: number
  activeUsers: number
  customerCount: number
  storeOwnerCount: number
}

export interface AdminUserDto {
  id: string
  fullName?: string | null
  email?: string | null
  phoneNumber?: string | null
  avatarUrl?: string | null
  isActive: boolean
  createdAt: string
  role?: string | null
  isDeleted: boolean
}

export interface AdminUsersListDto {
  users: PagedResult<AdminUserDto>
  stats: AdminUserStatsDto
  roles: string[]
}

export interface CreateAdminUserRequest {
  fullName: string
  email: string
  phoneNumber?: string | null
  password: string
  confirmPassword: string
  role: string
  isActive: boolean
}

export interface EditAdminUserRequest {
  id: string
  fullName: string
  email?: string | null
  phoneNumber?: string | null
  role: string
  isActive: boolean
}

export interface AdminOrderQueryParams {
  searchTerm?: string
  isPaid?: boolean | ''
  startDate?: string
  endDate?: string
  dateRange?: string
  minPrice?: number | ''
  maxPrice?: number | ''
  sortBy?: string
  sortDirection?: string
  pageNumber?: number
  pageSize?: number
}

export interface AdminOrderStatsDto {
  totalOrders: number
  paidOrders: number
  unpaidOrders: number
  totalRevenue: number
}

export interface AdminOrderDto {
  id: string
  orderCode: number
  customerName: string
  receiverName: string
  grandTotal: number
  shippingAddress: string
  receiverPhone: string
  paidAt?: string | null
  createdAt: string
}

export interface AdminOrdersListDto {
  orders: PagedResult<AdminOrderDto>
  stats: AdminOrderStatsDto
}

export const orderStatus = {
  pending: 0,
  awaitingPayment: 1,
  approved: 2,
  rejected: 3,
  paid: 4,
  processing: 5,
  delivered: 6,
  cancelled: 7,
  completed: 8,
  refunded: 9,
} as const

export type OrderStatus = (typeof orderStatus)[keyof typeof orderStatus]

export interface AdminOrderDetailDto {
  id: string
  orderCode: number
  userId: string
  userName: string
  userEmail: string
  shippingFee: number
  grandTotal: number
  receiverName: string
  receiverPhone: string
  shippingAddress: string
  shippingProvider?: string | null
  trackingNumber?: string | null
  createdAt: string
  paidAt?: string | null
  updatedAt?: string | null
  subOrders: AdminSubOrderDto[]
  statusHistories: AdminOrderStatusHistoryDto[]
  payments: AdminOrderPaymentDto[]
}

export interface AdminSubOrderDto {
  id: string
  storeId: string
  storeName: string
  storeEmail: string
  status: OrderStatus
  subtotal: number
  commissionRate: number
  commissionAmount: number
  netAmount: number
  items: AdminOrderItemDto[]
}

export interface AdminOrderItemDto {
  id: string
  variantId: string
  productName: string
  variantName: string
  productImage?: string | null
  unitPrice: number
  quantity: number
  lineTotal: number
}

export interface AdminOrderStatusHistoryDto {
  oldStatus: OrderStatus
  newStatus: OrderStatus
  note?: string | null
  changedAt: string
}

export interface AdminOrderPaymentDto {
  id: string
  method: string
  provider: string
  amount: number
  status: string
  transactionRef?: string | null
  checkoutUrl?: string | null
  paidAt?: string | null
  createdAt: string
}

export interface AdminProductQueryParams {
  searchTerm?: string
  status?: string
  categoryId?: number | ''
  brandId?: number | ''
  storeId?: string
  minPrice?: number | ''
  maxPrice?: number | ''
  startDate?: string
  endDate?: string
  outOfStock?: boolean
  sortBy?: string
  sortDirection?: string
  pageNumber?: number
  pageSize?: number
}

export interface AdminProductStatsDto {
  totalProducts: number
  activeProducts: number
  pendingApproval: number
  outOfStock: number
}

export interface AdminProductDto {
  id: string
  name: string
  sku: string
  storeName: string
  category: string
  price: number
  stock: number
  status: string
  thumbnailUrl?: string | null
  createdAt: string
}

export interface AdminProductsListDto {
  products: PagedResult<AdminProductDto>
  stats: AdminProductStatsDto
}

export interface AdminCategoryDto {
  id: number
  name: string
  slug: string
  isActive: boolean
  isDeleted: boolean
  parentId?: number | null
  parentName?: string | null
  productCount: number
  revenue: number
  children: AdminCategoryDto[]
}

export interface AdminBrandDto {
  id: number
  name: string
  slug: string
  logoUrl?: string | null
  isApproved: boolean
  productCount: number
}

export interface AdminProductMetadataDto {
  categories: AdminCategoryDto[]
  stores: StoreApplicationDto[]
  brands: AdminBrandDto[]
}

export interface AdminProductDetailDto {
  id: string
  name: string
  sku: string
  description: string
  status: string
  basePrice: number
  stock: number
  soldCount: number
  slug: string
  commissionRate: number
  createdAt: string
  updatedAt?: string | null
  category: AdminProductCategoryInfoDto
  brand: AdminProductBrandInfoDto
  store: AdminProductStoreInfoDto
  images: string[]
  specs: AdminProductSpecDto[]
  variants: AdminProductVariantDto[]
}

export interface AdminProductCategoryInfoDto {
  id: number
  name: string
  slug: string
}

export interface AdminProductBrandInfoDto {
  id: number
  name: string
  slug: string
}

export interface AdminProductStoreInfoDto {
  id: string
  name: string
  slug: string
  vendorId: string
  joinedAt: string
  avatarUrl: string
}

export interface AdminProductSpecDto {
  attributeName: string
  values: string[]
}

export interface AdminProductVariantDto {
  sku: string
  name: string
  price: number
  stock: number
  attributes: AdminVariantAttributeDto[]
}

export interface AdminVariantAttributeDto {
  attributeName: string
  value: string
}

export interface ProductReasonRequest {
  reason?: string
}

export interface BulkProductStatusRequest {
  productIds: string[]
  action: string
  reason?: string
}

export const storeStatus = {
  draft: 0,
  pending: 1,
  approved: 2,
  rejected: 3,
  locked: 4,
} as const

export type StoreStatus = (typeof storeStatus)[keyof typeof storeStatus]

export interface StoreApplicationQueryParams {
  searchTerm?: string
  status?: StoreStatus | ''
  startDate?: string
  endDate?: string
  pageNumber?: number
  pageSize?: number
}

export interface StoreApplicationStatsDto {
  totalCount: number
  pendingCount: number
  approvedCount: number
  rejectedCount: number
}

export interface StoreApplicationDto {
  id: string
  storeName: string
  taxCode: string
  businessType: string
  ownerName: string
  ownerEmail: string
  ownerPhone: string
  email: string
  phone: string
  description: string
  addressLine: string
  province: string
  commissionRate: number
  bankAccountName: string
  bankAccountNumber: string
  bankName: string
  bankBin: string
  logoUrl: string
  slug: string
  identityNumber?: string | null
  identityIssuedDate?: string | null
  identityIssuedPlace?: string | null
  identityCardFrontImageUrl?: string | null
  identityCardBackImageUrl?: string | null
  createdAt: string
  approvedAt?: string | null
  updatedAt?: string | null
  rejectReason?: string | null
  status: StoreStatus
}

export interface RejectStoreRequest {
  reason: string
}

export interface RequestInfoRequest {
  note: string
}

export interface ChangeStoreStatusRequest {
  reason?: string
  status: StoreStatus
}

function toDashboardParams(params?: DashboardQueryParams) {
  return {
    Period: params?.period,
    StartDate: params?.startDate,
    EndDate: params?.endDate,
  }
}

function toStoreApplicationParams(params?: StoreApplicationQueryParams) {
  return {
    SearchTerm: params?.searchTerm,
    Status: params?.status === '' ? undefined : params?.status,
    StartDate: params?.startDate,
    EndDate: params?.endDate,
    PageNumber: params?.pageNumber,
    PageSize: params?.pageSize,
  }
}

function toUserParams(params?: UserQueryParams) {
  return {
    SearchTerm: params?.searchTerm,
    Role: params?.role,
    IsActive: params?.isActive === '' ? undefined : params?.isActive,
    PageNumber: params?.pageNumber,
    PageSize: params?.pageSize,
  }
}

function toOrderParams(params?: AdminOrderQueryParams) {
  return {
    SearchTerm: params?.searchTerm,
    IsPaid: params?.isPaid === '' ? undefined : params?.isPaid,
    StartDate: params?.startDate,
    EndDate: params?.endDate,
    DateRange: params?.dateRange,
    MinPrice: params?.minPrice === '' ? undefined : params?.minPrice,
    MaxPrice: params?.maxPrice === '' ? undefined : params?.maxPrice,
    SortBy: params?.sortBy,
    SortDirection: params?.sortDirection,
    PageNumber: params?.pageNumber,
    PageSize: params?.pageSize,
  }
}

function toProductParams(params?: AdminProductQueryParams) {
  return {
    SearchTerm: params?.searchTerm,
    Status: params?.status,
    CategoryId: params?.categoryId === '' ? undefined : params?.categoryId,
    BrandId: params?.brandId === '' ? undefined : params?.brandId,
    StoreId: params?.storeId,
    MinPrice: params?.minPrice === '' ? undefined : params?.minPrice,
    MaxPrice: params?.maxPrice === '' ? undefined : params?.maxPrice,
    StartDate: params?.startDate,
    EndDate: params?.endDate,
    OutOfStock: params?.outOfStock,
    SortBy: params?.sortBy,
    SortDirection: params?.sortDirection,
    PageNumber: params?.pageNumber,
    PageSize: params?.pageSize,
  }
}

export const adminApi = {
  getDashboard: (params?: DashboardQueryParams) =>
    apiClient.get('/admin/dashboard', { params: toDashboardParams(params) }).then((response) => unwrap<AdminDashboardDto>(response)),

  storeApplications: {
    list: (params?: StoreApplicationQueryParams) =>
      apiClient
        .get('/admin/store-applications', { params: toStoreApplicationParams(params) })
        .then((response) => unwrap<PagedResult<StoreApplicationDto>>(response)),

    stats: () => apiClient.get('/admin/store-applications/stats').then((response) => unwrap<StoreApplicationStatsDto>(response)),

    get: (id: string) => apiClient.get(`/admin/store-applications/${id}`).then((response) => unwrap<StoreApplicationDto>(response)),

    approve: (id: string) => apiClient.post(`/admin/store-applications/${id}/approve`).then((response) => unwrap<string>(response)),

    reject: (id: string, request: RejectStoreRequest) =>
      apiClient.post(`/admin/store-applications/${id}/reject`, request).then((response) => unwrap<string>(response)),

    requestInfo: (id: string, request: RequestInfoRequest) =>
      apiClient.post(`/admin/store-applications/${id}/request-info`, request).then((response) => unwrap<string>(response)),
  },

  stores: {
    list: (params?: StoreApplicationQueryParams) =>
      apiClient
        .get('/admin/stores', { params: toStoreApplicationParams(params) })
        .then((response) => unwrap<PagedResult<StoreApplicationDto>>(response)),

    stats: () => apiClient.get('/admin/stores/stats').then((response) => unwrap<StoreApplicationStatsDto>(response)),

    get: (id: string) => apiClient.get(`/admin/stores/${id}`).then((response) => unwrap<StoreApplicationDto>(response)),

    changeStatus: (id: string, request: ChangeStoreStatusRequest) =>
      apiClient.post(`/admin/stores/${id}/change-status`, request).then((response) => unwrap<string>(response)),
  },

  users: {
    list: (params?: UserQueryParams) =>
      apiClient.get('/admin/users', { params: toUserParams(params) }).then((response) => unwrap<AdminUsersListDto>(response)),

    get: (id: string) => apiClient.get(`/admin/users/${id}`).then((response) => unwrap<AdminUserDto>(response)),

    create: (request: CreateAdminUserRequest) => apiClient.post('/admin/users', request).then((response) => unwrap<string>(response)),

    update: (request: EditAdminUserRequest) => apiClient.put('/admin/users', request).then((response) => unwrap<string>(response)),

    delete: (id: string) => apiClient.delete(`/admin/users/${id}`).then((response) => unwrap<string>(response)),

    restore: (id: string) => apiClient.post(`/admin/users/${id}/restore`).then((response) => unwrap<string>(response)),
  },

  orders: {
    list: (params?: AdminOrderQueryParams) =>
      apiClient.get('/admin/orders', { params: toOrderParams(params) }).then((response) => unwrap<AdminOrdersListDto>(response)),

    get: (id: string) => apiClient.get(`/admin/orders/${id}`).then((response) => unwrap<AdminOrderDetailDto>(response)),
  },

  products: {
    list: (params?: AdminProductQueryParams) =>
      apiClient.get('/admin/products', { params: toProductParams(params) }).then((response) => unwrap<AdminProductsListDto>(response)),

    metadata: () => apiClient.get('/admin/products/metadata').then((response) => unwrap<AdminProductMetadataDto>(response)),

    get: (id: string) => apiClient.get(`/admin/products/${id}`).then((response) => unwrap<AdminProductDetailDto>(response)),

    approve: (id: string) => apiClient.post(`/admin/products/${id}/approve`).then((response) => unwrap<string>(response)),

    reject: (id: string, request: ProductReasonRequest) =>
      apiClient.post(`/admin/products/${id}/reject`, request).then((response) => unwrap<string>(response)),

    suspend: (id: string, request: ProductReasonRequest) =>
      apiClient.post(`/admin/products/${id}/suspend`, request).then((response) => unwrap<string>(response)),

    delete: (id: string, request: ProductReasonRequest) =>
      apiClient.delete(`/admin/products/${id}`, { data: request }).then((response) => unwrap<string>(response)),

    bulkUpdateStatus: (request: BulkProductStatusRequest) =>
      apiClient.post('/admin/products/bulk-update-status', request).then((response) => unwrap<string>(response)),
  },
}
