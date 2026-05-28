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
}
