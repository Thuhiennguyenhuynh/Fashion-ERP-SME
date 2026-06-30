/**
 * FashionERP – API Client
 * Base URL: http://localhost:5038  (dev)  |  https://<render-domain>  (prod)
 *
 * Dùng:
 *   import api, { authApi, productApi, ... } from './api'
 *
 * Tất cả response đều bọc trong ApiResponse<T>:
 *   { success: boolean; data: T; message: string; errors?: string[] }
 *
 * Axios interceptor tự gắn Bearer token và tự refresh khi 401.
 */

import axios from 'axios'
import type {
  AxiosInstance,
  AxiosRequestConfig,
  InternalAxiosRequestConfig,
} from 'axios'

// ─────────────────────────────────────────────
// 0. BASE CONFIG
// ─────────────────────────────────────────────

export const BASE_URL = import.meta.env.VITE_API_URL ?? ''

/** Wrapper chung backend trả về */
export interface ApiResponse<T = unknown> {
  success: boolean
  data: T
  message: string
  errors?: string[]
}

/** Phân trang */
export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface DashboardSummary {
  revenue: number
  orderCount: number
  lowStockCount: number
  newCustomers: number
  last7Days: { date: string; revenue: number }[]
}

// ─────────────────────────────────────────────
// 1. AXIOS INSTANCE + INTERCEPTORS
// ─────────────────────────────────────────────

const api: AxiosInstance = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
})

/** Đọc token từ localStorage (Zustand authStore nên persist vào đây) */
const getAccessToken = () =>
  localStorage.getItem('accessToken') || localStorage.getItem('access_token') || localStorage.getItem('token')
const getRefreshToken = () => localStorage.getItem('refreshToken') || localStorage.getItem('refresh_token')
const setTokens = (access: string, refresh: string) => {
  localStorage.setItem('accessToken', access)
  localStorage.setItem('access_token', access)
  localStorage.setItem('token', access)
  localStorage.setItem('refreshToken', refresh)
  localStorage.setItem('refresh_token', refresh)
}
const clearTokens = () => {
  localStorage.removeItem('accessToken')
  localStorage.removeItem('access_token')
  localStorage.removeItem('token')
  localStorage.removeItem('refreshToken')
  localStorage.removeItem('refresh_token')
}

// Gắn Bearer token vào mỗi request
api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = getAccessToken()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// Tự refresh khi 401
let isRefreshing = false
let failedQueue: Array<{
  resolve: (v: string) => void
  reject: (e: unknown) => void
}> = []

const processQueue = (error: unknown, token: string | null) => {
  failedQueue.forEach((p) => (token ? p.resolve(token) : p.reject(error)))
  failedQueue = []
}

api.interceptors.response.use(
  (res) => res.data,
  async (error) => {
    const original = error.config as AxiosRequestConfig & {
      _retry?: boolean
    }
    if (error.response?.status === 401 && !original._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        }).then((token) => {
          original.headers = {
            ...original.headers,
            Authorization: `Bearer ${token}`,
          }
          return api(original)
        })
      }
      original._retry = true
      isRefreshing = true
      try {
        const { data } = await axios.post<ApiResponse<AuthResponse>>(
          `${BASE_URL}/api/auth/refresh`,
          {
            accessToken: getAccessToken(),
            refreshToken: getRefreshToken(),
          },
        )
        const { accessToken, refreshToken } = normalizeAuthResponse(data.data)
        if (accessToken) setTokens(accessToken, refreshToken ?? '')
        processQueue(null, accessToken)
        original.headers = {
          ...original.headers,
          Authorization: `Bearer ${accessToken}`,
        }
        return api(original)
      } catch (err) {
        processQueue(err, null)
        clearTokens()
        window.location.href = '/login'
        return Promise.reject(err)
      } finally {
        isRefreshing = false
      }
    }
    return Promise.reject(error)
  },
)

export default api

// ─────────────────────────────────────────────
// 2. TYPES – AUTH
// ─────────────────────────────────────────────

export interface LoginRequest {
  email: string
  password: string
}

export interface RefreshTokenRequest {
  accessToken: string
  refreshToken: string
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
  confirmNewPassword: string
}

export interface CreateUserRequest {
  email: string
  password: string
  role: string          // Admin | Manager | Sales | Warehouse | Accountant
  employeeId?: string
}

export interface UserInfo {
  id: string
  email: string
  role: string
  fullName?: string
  avatarUrl?: string
}

export interface AuthResponse {
  accessToken?: string
  token?: string
  access_token?: string
  refreshToken?: string
  refresh_token?: string
  user?: UserInfo
  userInfo?: UserInfo
}

export const normalizeAuthResponse = (payload: Partial<AuthResponse> | null | undefined) => {
  const raw = (payload ?? {}) as Record<string, unknown>
  const accessToken =
    (raw.accessToken as string | undefined) ??
    (raw.token as string | undefined) ??
    (raw.access_token as string | undefined) ??
    null
  const refreshToken =
    (raw.refreshToken as string | undefined) ?? (raw.refresh_token as string | undefined) ?? null
  const user = (raw.user as UserInfo | undefined) ?? (raw.userInfo as UserInfo | undefined) ?? null

  return { accessToken, refreshToken, user }
}

export interface UserListItem {
  id: string
  email: string
  role: string
  employeeId?: string
  isActive: boolean
  lastLoginAt?: string
}

// ─────────────────────────────────────────────
// 3. TYPES – PRODUCT
// ─────────────────────────────────────────────

export interface ProductQueryParams {
  page?: number
  pageSize?: number
  keyword?: string
  sortBy?: string
  status?: string
  categoryId?: string
  brandId?: string
  gender?: string
  minPrice?: number
  maxPrice?: number
  size?: string
  color?: string
  inStock?: boolean
}

export interface CreateProductRequest {
  name: string
  description?: string
  categoryId: string
  brandId?: string
  gender: string        // Male | Female | Unisex
  basePrice: number
  tags?: string
  status?: string       // Draft | Active | Inactive
}

export type UpdateProductRequest = CreateProductRequest

export interface ProductVariantResponse {
  id: string
  sku: string
  size: string
  color: string
  colorHex?: string
  price?: number
  barcode?: string
  imageUrl?: string
  isActive: boolean
  stockQuantity: number
}

export interface ProductResponse {
  id: string
  productCode: string
  name: string
  description?: string
  categoryName: string
  brandName?: string
  gender: string
  basePrice: number
  mainImageUrl?: string
  tags?: string
  status: string
  createdAt: string
  variants: ProductVariantResponse[]
}

export interface CreateVariantRequest {
  productId: string
  size: string
  color: string
  colorHex?: string
  price?: number
  barcode?: string
}

// ─────────────────────────────────────────────
// 4. TYPES – INVENTORY
// ─────────────────────────────────────────────

export interface InventoryQueryParams {
  page?: number
  pageSize?: number
  keyword?: string
  lowStockOnly?: boolean
}

export interface ImportStockRequest {
  variantId: string
  quantity: number
  unitCost: number
  note?: string
}

export interface AdjustStockRequest {
  variantId: string
  newQuantity: number
  note?: string
}

export interface InventoryResponse {
  id: string
  variantId: string
  productName: string
  sku: string
  size: string
  color: string
  quantity: number
  minStock: number
  maxStock?: number
  location?: string
  avgCost: number
  lastImportDate?: string
  isLowStock: boolean
}

export interface InventoryTransactionQueryParams {
  page?: number
  pageSize?: number
  variantId?: string
  type?: 'IMPORT' | 'EXPORT' | 'ADJUST' | 'RETURN'
  from?: string
  to?: string
}

// ─────────────────────────────────────────────
// 5. TYPES – ORDER
// ─────────────────────────────────────────────

export interface OrderQueryParams {
  page?: number
  pageSize?: number
  keyword?: string
  status?: string
  from?: string
  to?: string
  paymentMethod?: string
}

export interface CreateOrderItemRequest {
  variantId: string
  quantity: number
}

export interface CreateOrderRequest {
  customerId?: string
  paymentMethod: string   // Cash | Transfer | Card
  promotionCode?: string
  note?: string
  items: CreateOrderItemRequest[]
}

export interface OrderItemResponse {
  variantId: string
  productName: string
  size: string
  color: string
  unitPrice: number
  quantity: number
  lineTotal: number
}

export interface OrderResponse {
  id: string
  orderCode: string
  customerName?: string
  staffName: string
  subtotal: number
  discountAmount: number
  taxAmount: number
  finalAmount: number
  paymentMethod: string
  promotionCode?: string
  status: string
  note?: string
  createdAt: string
  completedAt?: string
  items: OrderItemResponse[]
}

export interface CreateReturnRequest {
  orderId: string
  variantId: string
  quantity: number
  reason: string
  returnType: string    // Refund | Exchange
  refundAmount?: number
}

// ─────────────────────────────────────────────
// 6. TYPES – CUSTOMER
// ─────────────────────────────────────────────

export interface CustomerQueryParams {
  page?: number
  pageSize?: number
  keyword?: string
  gender?: string
  memberLevel?: string  // Bronze | Silver | Gold | Platinum
  minSpent?: number
  maxSpent?: number
}

export interface CreateCustomerRequest {
  fullName: string
  phone: string
  email?: string
  gender?: string
  dateOfBirth?: string
  address?: string
  note?: string
}

export type UpdateCustomerRequest = CreateCustomerRequest

export interface CustomerMeasurement {
  height?: number
  weight?: number
  chest?: number
  waist?: number
  hip?: number
}

export interface CustomerResponse {
  id: string
  fullName: string
  phone: string
  email?: string
  gender?: string
  dateOfBirth?: string
  address?: string
  avatarUrl?: string
  memberLevel: string
  totalSpent: number
  totalOrders: number
  note?: string
  measurement?: CustomerMeasurement
}

// ─────────────────────────────────────────────
// 7. TYPES – EMPLOYEE / HR
// ─────────────────────────────────────────────

export interface EmployeeQueryParams {
  page?: number
  pageSize?: number
  keyword?: string
  departmentId?: string
  status?: string       // Active | Probation | Resigned
  position?: string
  minSalary?: number
  maxSalary?: number
}

export interface CreateEmployeeRequest {
  fullName: string
  phone: string
  email?: string
  gender?: string
  dateOfBirth?: string
  address?: string
  departmentId: string
  position: string
  baseSalary: number
  workingDaysPerMonth?: number
  startDate: string
}

export interface UpdateEmployeeRequest extends Omit<CreateEmployeeRequest, 'startDate'> {
  status: string
  workingDaysPerMonth: number
}

export interface UpdateEmployeeStatusRequest {
  status: string  // Active | Probation | Resigned
}

export interface EmployeeResponse {
  id: string
  fullName: string
  phone: string
  email?: string
  gender?: string
  dateOfBirth?: string
  address?: string
  departmentName: string
  position: string
  baseSalary: number
  workingDaysPerMonth: number
  startDate: string
  status: string
  avatarUrl?: string
}

export interface CheckInRequest {
  employeeId: string
  note?: string
}

export interface CheckOutRequest {
  employeeId: string
  workDate: string
}

export interface CreateAttendanceManualRequest {
  employeeId: string
  workDate: string
  checkIn?: string
  checkOut?: string
  type?: string
  note?: string
}

export interface AttendanceResponse {
  id: string
  employeeName: string
  workDate: string
  checkIn?: string
  checkOut?: string
  totalHours?: number
  overtimeHours: number
  type: string
  note?: string
}

export interface CreateLeaveRequest {
  employeeId: string
  fromDate: string
  toDate: string
  reason: string
}

export interface ApproveLeaveRequest {
  status: string   // Approved | Rejected
  note?: string
}

export interface LeaveResponse {
  id: string
  employeeName: string
  fromDate: string
  toDate: string
  days: number
  reason: string
  status: string
  approverName?: string
}

export interface GeneratePayrollRequest {
  employeeId: string
  month: number
  year: number
  allowance?: number
  deduction?: number
}

export interface PayrollResponse {
  id: string
  employeeName: string
  month: number
  year: number
  workingDaysActual: number
  baseSalary: number
  allowance: number
  overtimePay: number
  deduction: number
  netSalary: number
  status: string  // Draft | Confirmed | Paid
}

// ─────────────────────────────────────────────
// 8. TYPES – PROMOTION
// ─────────────────────────────────────────────

export interface PromotionQueryParams {
  page?: number
  pageSize?: number
  keyword?: string
  isActive?: boolean
}

export interface CreatePromotionRequest {
  code: string
  name: string
  type: string          // Percent | FixedAmount
  discountValue: number
  maxDiscount?: number
  minOrderValue?: number
  usageLimit?: number
  startDate: string
  endDate: string
}

export interface PromotionResponse {
  id: string
  code: string
  name: string
  type: string
  discountValue: number
  maxDiscount?: number
  minOrderValue?: number
  usageLimit?: number
  usedCount: number
  startDate: string
  endDate: string
  isActive: boolean
}

export interface ApplyPromotionRequest {
  code: string
  orderSubtotal: number
}

export interface ApplyPromotionResponse {
  isValid: boolean
  errorMessage?: string
  discountAmount: number
  promotionName: string
}

// ─────────────────────────────────────────────
// 9. TYPES – PROCUREMENT (Supplier + PO)
// ─────────────────────────────────────────────

export interface SupplierQueryParams {
  page?: number
  pageSize?: number
  keyword?: string
  isActive?: boolean
}

export interface CreateSupplierRequest {
  name: string
  contactPerson?: string
  phone: string
  email?: string
  address?: string
  taxCode?: string
  bankAccount?: string
  bankName?: string
  note?: string
}

export interface SupplierResponse {
  id: string
  name: string
  contactPerson?: string
  phone: string
  email?: string
  address?: string
  taxCode?: string
  bankAccount?: string
  bankName?: string
  totalDebt: number
  note?: string
  isActive: boolean
}

export interface PurchaseOrderQueryParams {
  page?: number
  pageSize?: number
  supplierId?: string
  status?: string
  from?: string
  to?: string
}

export interface CreatePoItemRequest {
  variantId: string
  orderedQty: number
  unitCost: number
}

export interface CreatePurchaseOrderRequest {
  supplierId: string
  expectedDate?: string
  note?: string
  items: CreatePoItemRequest[]
}

export interface ReceivePoItemRequest {
  purchaseOrderItemId: string
  receivedQtyThisTime: number
}

export interface ReceivePurchaseOrderRequest {
  items: ReceivePoItemRequest[]
}

export interface PayPurchaseOrderRequest {
  amount: number
  note?: string
}

export interface PurchaseOrderItemResponse {
  id: string
  variantId: string
  productName: string
  size: string
  color: string
  orderedQty: number
  receivedQty: number
  unitCost: number
  lineTotal: number
}

export interface PurchaseOrderResponse {
  id: string
  poCode: string
  supplierId: string
  supplierName: string
  status: string        // Draft | Confirmed | PartialReceived | Received | Cancelled
  totalAmount: number
  paidAmount: number
  debtAmount: number
  expectedDate?: string
  receivedDate?: string
  note?: string
  createdAt: string
  createdBy?: string
  items: PurchaseOrderItemResponse[]
}

// ─────────────────────────────────────────────
// 10. TYPES – FINANCE
// ─────────────────────────────────────────────

export interface CashTransactionQueryParams {
  page?: number
  pageSize?: number
  type?: 'INCOME' | 'EXPENSE'
  from?: string
  to?: string
}

export interface CreateCashTransactionRequest {
  type: 'INCOME' | 'EXPENSE'
  category: string
  amount: number
  note?: string
  transactionDate?: string
}

export interface CashTransactionResponse {
  id: string
  type: string
  category: string
  amount: number
  note?: string
  refType?: string
  refId?: string
  balanceAfter: number
  transactionDate: string
  createdAt: string
  createdBy?: string
}

export interface CashBalanceResponse {
  currentBalance: number
  asOf: string
}

// ─────────────────────────────────────────────
// 11. TYPES – REPORTS / DASHBOARD
// ─────────────────────────────────────────────

export interface RevenueReportParams {
  from: string
  to: string
  groupBy?: 'day' | 'week' | 'month'
}

export interface RevenueReportItem {
  period: string
  revenue: number
  orderCount: number
}

export interface TopProductItem {
  productId: string
  productName: string
  sku: string
  qtySold: number
  revenue: number
}

export interface InventoryValueItem {
  variantId: string
  sku: string
  productName: string
  size: string
  color: string
  quantity: number
  avgCost: number
  totalValue: number
}

export interface ProfitLossReport {
  revenue: number
  cogs: number
  grossProfit: number
  grossMarginPercent: number
  profitByProduct: Array<{
    productName: string
    sku: string
    qtySold: number
    revenue: number
    cogs: number
    grossProfit: number
  }>
}

export interface CashFlowReportItem {
  period: string
  income: number
  expense: number
  net: number
}

export interface ExpenseByCategoryItem {
  category: string
  totalAmount: number
  count: number
}

// ─────────────────────────────────────────────
// 12. TYPES – AI
// ─────────────────────────────────────────────

export interface ChatMessage {
  role: 'user' | 'assistant'
  content: string
}

export interface ChatbotRequest {
  message: string
  history?: ChatMessage[]
}

export interface SuggestedProduct {
  productId: string
  name: string
  basePrice: number
  mainImageUrl?: string
}

export interface ChatbotResponse {
  reply: string
  suggestedProducts?: SuggestedProduct[]
}

export interface SizeRecommendRequest {
  productType: string   // ao | quan | vay ...
  gender: string
  height: number
  weight: number
  chest?: number
  waist?: number
  hip?: number
  customerId?: string
}

export interface SizeRecommendResponse {
  recommendedSize: string
  confidence: number    // 0..1
  explanation: string
  alternatives?: Array<{ size: string; confidence: number }>
}

export interface InventoryForecastRequest {
  variantId: string
  horizonDays?: number  // default 30
}

export interface InventoryForecastPoint {
  date: string
  predictedQuantitySold: number
}

export interface InventoryForecastResponse {
  variantId: string
  currentStock: number
  forecast: InventoryForecastPoint[]
  willRunOutInDays?: number
  needReorder: boolean
  note?: string
}

export interface TrendAnalysisParams {
  from: string
  to: string
  category?: string
}

export interface TrendAnalysisItem {
  productName: string
  sku: string
  totalSold: number
  revenue: number
  growthRate: number
}

export interface TrendAnalysisResponse {
  topTrends: TrendAnalysisItem[]
  decliningItems: TrendAnalysisItem[]
  summary: string
}

// ─────────────────────────────────────────────
// 13. API MODULES
// ─────────────────────────────────────────────

// ── Auth ──────────────────────────────────────
export const authApi = {
  login: (data: LoginRequest) =>
    api.post<ApiResponse<AuthResponse>>('/api/auth/login', data),

  refresh: (data: RefreshTokenRequest) =>
    api.post<ApiResponse<AuthResponse>>('/api/auth/refresh', data),

  logout: () =>
    api.post<ApiResponse<null>>('/api/auth/logout'),

  me: () =>
    api.get<ApiResponse<UserInfo>>('/api/auth/me'),

  changePassword: (data: ChangePasswordRequest) =>
    api.post<ApiResponse<null>>('/api/auth/change-password', data),

  createUser: (data: CreateUserRequest) =>
    api.post<ApiResponse<UserListItem>>('/api/auth/users', data),

  deactivateUser: (id: string) =>
    api.delete<ApiResponse<null>>(`/api/auth/users/${id}`),
}

// ── Users (Admin) ─────────────────────────────
export const usersApi = {
  getAll: () =>
    api.get<ApiResponse<UserListItem[]>>('/api/users'),

  create: (data: CreateUserRequest) =>
    api.post<ApiResponse<UserListItem>>('/api/users', data),

  toggleActive: (id: string) =>
    api.patch<ApiResponse<UserListItem>>(`/api/users/${id}/toggle-active`),
}

// ── Products ──────────────────────────────────
export const productApi = {
  getAll: (params?: ProductQueryParams) =>
    api.get<ApiResponse<PagedResult<ProductResponse>>>('/api/products', { params }),

  getById: (id: string) =>
    api.get<ApiResponse<ProductResponse>>(`/api/products/${id}`),

  create: (data: CreateProductRequest) =>
    api.post<ApiResponse<ProductResponse>>('/api/products', data),

  update: (id: string, data: UpdateProductRequest) =>
    api.put<ApiResponse<ProductResponse>>(`/api/products/${id}`, data),

  delete: (id: string) =>
    api.delete<ApiResponse<null>>(`/api/products/${id}`),

  /** Upload ảnh chính qua multipart (FE nên dùng Cloudinary widget thay thế) */
  uploadMainImage: (id: string, file: File) => {
    const form = new FormData()
    form.append('file', file)
    return api.post<ApiResponse<null>>(`/api/products/${id}/main-image`, form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
  },

  addVariant: (productId: string, data: Omit<CreateVariantRequest, 'productId'>) =>
    api.post<ApiResponse<ProductVariantResponse>>(`/api/products/${productId}/variants`, {
      ...data,
      productId,
    }),

  updateVariant: (variantId: string, data: Partial<CreateVariantRequest>) =>
    api.put<ApiResponse<ProductVariantResponse>>(`/api/products/variants/${variantId}`, data),

  deleteVariant: (variantId: string) =>
    api.delete<ApiResponse<null>>(`/api/products/variants/${variantId}`),
}

// ── Variants (POS barcode lookup) ─────────────
export const variantApi = {
  getByBarcode: (barcode: string) =>
    api.get<ApiResponse<ProductVariantResponse & { product: Pick<ProductResponse, 'id' | 'name' | 'mainImageUrl'> }>>(
      '/api/variants',
      { params: { barcode } },
    ),
}

// ── Categories ──────────────────────────────────
export const categoryApi = {
  getAll: () =>
    api.get<ApiResponse<Array<{ id: string; name: string; description?: string; productCount: number }>>>('/api/categories'),

  create: (data: { name: string; description?: string }) =>
    api.post('/api/categories', data),

  update: (id: string, data: { name: string; description?: string }) =>
    api.put(`/api/categories/${id}`, data),

  delete: (id: string) =>
    api.delete(`/api/categories/${id}`),
}

// ── Brands ────────────────────────────────────
export const brandApi = {
  getAll: () =>
    api.get<ApiResponse<Array<{ id: string; name: string; logoUrl?: string }>>>('/api/brands'),

  create: (data: { name: string; description?: string }) =>
    api.post('/api/brands', data),

  uploadLogo: (id: string, file: File) => {
    const form = new FormData()
    form.append('file', file)
    return api.post(`/api/brands/${id}/logo`, form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
  },

  delete: (id: string) =>
    api.delete(`/api/brands/${id}`),
}

// ── Inventory ─────────────────────────────────
export const inventoryApi = {
  getAll: (params?: InventoryQueryParams) =>
    api.get<ApiResponse<PagedResult<InventoryResponse>>>('/api/inventory', { params }),

  getByVariant: (variantId: string) =>
    api.get<ApiResponse<InventoryResponse>>(`/api/inventory/variant/${variantId}`),

  import: (data: ImportStockRequest) =>
    api.post<ApiResponse<null>>('/api/inventory/import', data),

  adjust: (data: AdjustStockRequest) =>
    api.post<ApiResponse<null>>('/api/inventory/adjust', data),

  getTransactions: (params?: InventoryTransactionQueryParams) =>
    api.get<ApiResponse<PagedResult<unknown>>>('/api/inventory/transactions', { params }),

  getTransactionsByVariant: (variantId: string, params?: { page?: number; pageSize?: number }) =>
    api.get<ApiResponse<PagedResult<unknown>>>(`/api/inventory/transactions/${variantId}`, { params }),
}

// ── Orders ──────────────────────────────────
export const orderApi = {
  getAll: (params?: OrderQueryParams) =>
    api.get<ApiResponse<PagedResult<OrderResponse>>>('/api/orders', { params }),

  getById: (id: string) =>
    api.get<ApiResponse<OrderResponse>>(`/api/orders/${id}`),

  create: (data: CreateOrderRequest) =>
    api.post<ApiResponse<OrderResponse>>('/api/orders', data),

  complete: (id: string) =>
    api.patch<ApiResponse<null>>(`/api/orders/${id}/complete`),

  cancel: (id: string) =>
    api.patch<ApiResponse<null>>(`/api/orders/${id}/cancel`),

  createReturn: (data: CreateReturnRequest) =>
    api.post<ApiResponse<null>>(`/api/orders/${data.orderId}/return`, data),
}

// ── Customers ─────────────────────────────────
export const customerApi = {
  getAll: (params?: CustomerQueryParams) =>
    api.get<ApiResponse<PagedResult<CustomerResponse>>>('/api/customers', { params }),

  getById: (id: string) =>
    api.get<ApiResponse<CustomerResponse>>(`/api/customers/${id}`),

  getOrders: (id: string, params?: { page?: number; pageSize?: number }) =>
    api.get<ApiResponse<PagedResult<OrderResponse>>>(`/api/customers/${id}/orders`, { params }),

  create: (data: CreateCustomerRequest) =>
    api.post<ApiResponse<CustomerResponse>>('/api/customers', data),

  update: (id: string, data: UpdateCustomerRequest) =>
    api.put<ApiResponse<CustomerResponse>>(`/api/customers/${id}`, data),

  delete: (id: string) =>
    api.delete<ApiResponse<null>>(`/api/customers/${id}`),

  saveMeasurement: (id: string, data: CustomerMeasurement) =>
    api.post<ApiResponse<null>>(`/api/customers/${id}/measurements`, data),

  uploadAvatar: (id: string, file: File) => {
    const form = new FormData()
    form.append('file', file)
    return api.post(`/api/customers/${id}/avatar`, form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
  },
}

// ── Employees ─────────────────────────────────
export const employeeApi = {
  getAll: (params?: EmployeeQueryParams) =>
    api.get<ApiResponse<PagedResult<EmployeeResponse>>>('/api/employees', { params }),

  getById: (id: string) =>
    api.get<ApiResponse<EmployeeResponse>>(`/api/employees/${id}`),

  create: (data: CreateEmployeeRequest) =>
    api.post<ApiResponse<EmployeeResponse>>('/api/employees', data),

  update: (id: string, data: UpdateEmployeeRequest) =>
    api.put<ApiResponse<EmployeeResponse>>(`/api/employees/${id}`, data),

  delete: (id: string) =>
    api.delete<ApiResponse<null>>(`/api/employees/${id}`),

  updateStatus: (id: string, data: UpdateEmployeeStatusRequest) =>
    api.patch<ApiResponse<null>>(`/api/employees/${id}/status`, data),

  uploadAvatar: (id: string, file: File) => {
    const form = new FormData()
    form.append('file', file)
    return api.post(`/api/employees/${id}/avatar`, form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
  },
}

// ── Departments ───────────────────────────────
export const departmentApi = {
  getAll: () =>
    api.get<ApiResponse<Array<{ id: string; name: string; description?: string }>>>('/api/departments'),

  create: (data: { name: string; description?: string }) =>
    api.post('/api/departments', data),

  update: (id: string, data: { name: string; description?: string }) =>
    api.put(`/api/departments/${id}`, data),

  delete: (id: string) =>
    api.delete(`/api/departments/${id}`),
}

// ── Attendance ────────────────────────────────
export const attendanceApi = {
  checkIn: (data: CheckInRequest) =>
    api.post<ApiResponse<AttendanceResponse>>('/api/attendances/check-in', data),

  checkOut: (data: CheckOutRequest) =>
    api.post<ApiResponse<AttendanceResponse>>('/api/attendances/check-out', data),

  createManual: (data: CreateAttendanceManualRequest) =>
    api.post<ApiResponse<AttendanceResponse>>('/api/attendances/manual', data),

  getByEmployee: (employeeId: string, params?: { from?: string; to?: string }) =>
    api.get<ApiResponse<AttendanceResponse[]>>(`/api/attendances/employee/${employeeId}`, { params }),
}

// ── Leaves ────────────────────────────────
export const leaveApi = {
  create: (data: CreateLeaveRequest) =>
    api.post<ApiResponse<LeaveResponse>>('/api/leaves', data),

  approve: (id: string, data: ApproveLeaveRequest) =>
    api.patch<ApiResponse<LeaveResponse>>(`/api/leaves/${id}/approve`, data),

  getByEmployee: (employeeId: string) =>
    api.get<ApiResponse<LeaveResponse[]>>(`/api/leaves/employee/${employeeId}`),

  getPending: () =>
    api.get<ApiResponse<LeaveResponse[]>>('/api/leaves/pending'),
}

// ── Payrolls ────────────────────────────────
export const payrollApi = {
  getByMonthYear: (month: number, year: number) =>
    api.get<ApiResponse<PayrollResponse[]>>('/api/payrolls', { params: { month, year } }),

  generate: (data: GeneratePayrollRequest) =>
    api.post<ApiResponse<PayrollResponse>>('/api/payrolls/generate', data),

  confirm: (id: string) =>
    api.patch<ApiResponse<null>>(`/api/payrolls/${id}/confirm`),

  markPaid: (id: string) =>
    api.patch<ApiResponse<null>>(`/api/payrolls/${id}/mark-paid`),

  getByEmployee: (employeeId: string, year: number, month: number) =>
    api.get<ApiResponse<PayrollResponse>>(`/api/payrolls/${employeeId}/${year}/${month}`),
}

// ── Promotions ────────────────────────────────
export const promotionApi = {
  getAll: (params?: PromotionQueryParams) =>
    api.get<ApiResponse<PagedResult<PromotionResponse>>>('/api/promotions', { params }),

  create: (data: CreatePromotionRequest) =>
    api.post<ApiResponse<PromotionResponse>>('/api/promotions', data),

  deactivate: (id: string) =>
    api.patch<ApiResponse<null>>(`/api/promotions/${id}/deactivate`),

  apply: (data: ApplyPromotionRequest) =>
    api.post<ApiResponse<ApplyPromotionResponse>>('/api/promotions/apply', data),
}

// ── Suppliers ─────────────────────────────────
export const supplierApi = {
  getAll: (params?: SupplierQueryParams) =>
    api.get<ApiResponse<PagedResult<SupplierResponse>>>('/api/suppliers', { params }),

  getById: (id: string) =>
    api.get<ApiResponse<SupplierResponse>>(`/api/suppliers/${id}`),

  create: (data: CreateSupplierRequest) =>
    api.post<ApiResponse<SupplierResponse>>('/api/suppliers', data),

  update: (id: string, data: CreateSupplierRequest) =>
    api.put<ApiResponse<SupplierResponse>>(`/api/suppliers/${id}`, data),

  toggleActive: (id: string) =>
    api.patch<ApiResponse<SupplierResponse>>(`/api/suppliers/${id}/toggle-active`),
}

// ── Purchase Orders ───────────────────────────
export const purchaseOrderApi = {
  getAll: (params?: PurchaseOrderQueryParams) =>
    api.get<ApiResponse<PagedResult<PurchaseOrderResponse>>>('/api/purchaseorders', { params }),

  getById: (id: string) =>
    api.get<ApiResponse<PurchaseOrderResponse>>(`/api/purchaseorders/${id}`),

  create: (data: CreatePurchaseOrderRequest) =>
    api.post<ApiResponse<PurchaseOrderResponse>>('/api/purchaseorders', data),

  confirm: (id: string) =>
    api.patch<ApiResponse<null>>(`/api/purchaseorders/${id}/confirm`),

  receive: (id: string, data: ReceivePurchaseOrderRequest) =>
    api.post<ApiResponse<PurchaseOrderResponse>>(`/api/purchaseorders/${id}/receive`, data),

  pay: (id: string, data: PayPurchaseOrderRequest) =>
    api.post<ApiResponse<null>>(`/api/purchaseorders/${id}/payments`, data),

  cancel: (id: string) =>
    api.patch<ApiResponse<null>>(`/api/purchaseorders/${id}/cancel`),
}

// ── Cash Transactions ─────────────────────────
export const cashApi = {
  getAll: (params?: CashTransactionQueryParams) =>
    api.get<ApiResponse<PagedResult<CashTransactionResponse>>>('/api/cashtransactions', { params }),

  getBalance: () =>
    api.get<ApiResponse<CashBalanceResponse>>('/api/cashtransactions/balance'),

  create: (data: CreateCashTransactionRequest) =>
    api.post<ApiResponse<CashTransactionResponse>>('/api/cashtransactions', data),
}

// ── Expenses ─────────────────────────────────
export const expenseApi = {
  getAll: (params?: { page?: number; pageSize?: number; from?: string; to?: string; category?: string }) =>
    api.get<ApiResponse<PagedResult<unknown>>>('/api/expenses', { params }),

  create: (data: { category: string; amount: number; description?: string; expenseDate?: string }) =>
    api.post('/api/expenses', data),

  update: (id: string, data: unknown) =>
    api.put(`/api/expenses/${id}`, data),

  delete: (id: string) =>
    api.delete(`/api/expenses/${id}`),
}

// ── Dashboard ─────────────────────────────────
export const dashboardApi = {
  /** Lưu ý: response KHÔNG bọc ApiResponse<T>, trả thẳng object phẳng */
  getSummary: (params?: { month?: number; year?: number }) =>
    api.get<DashboardSummary>('/api/dashboard/summary', { params }),

  getRevenueByMonth: (year?: number) =>
    api.get<{ month: number; revenue: number }[]>('/api/dashboard/revenue-by-month', {
      params: { year },
    }),

  getPaymentMethods: (params?: { month?: number; year?: number }) =>
    api.get<{ method: string; count: number; total: number }[]>('/api/dashboard/payment-methods', { params }),
}

// ── Reports ─────────────────────────────────
export const reportApi = {
  getRevenue: (params: RevenueReportParams) =>
    api.get<ApiResponse<RevenueReportItem[]>>('/api/reports/revenue', { params }),

  getTopProducts: (params: { from: string; to: string; top?: number }) =>
    api.get<ApiResponse<TopProductItem[]>>('/api/reports/top-products', { params }),

  getInventoryValue: () =>
    api.get<ApiResponse<InventoryValueItem[]>>('/api/reports/inventory-value'),

  /** Xuất CSV cho Power BI: reportType = revenue | top-products | inventory-value */
  exportCsv: (reportType: string, from: string, to: string) =>
    api.get('/api/reports/export', {
      params: { reportType, from, to },
      responseType: 'blob',
    }),

  getProfitLoss: (from: string, to: string) =>
    api.get<ApiResponse<ProfitLossReport>>('/api/reports/profit-loss', {
      params: { from, to },
    }),

  getCashFlow: (params: { from: string; to: string; groupBy?: 'day' | 'week' | 'month' }) =>
    api.get<ApiResponse<CashFlowReportItem[]>>('/api/reports/cash-flow', { params }),

  getExpensesByCategory: (from: string, to: string) =>
    api.get<ApiResponse<ExpenseByCategoryItem[]>>('/api/reports/expenses-by-category', {
      params: { from, to },
    }),
}

// ── AI ────────────────────────────────────────
export const aiApi = {
  chat: (data: ChatbotRequest) =>
    api.post<ApiResponse<ChatbotResponse>>('/api/ai/chatbot', data),

  recommendSize: (data: SizeRecommendRequest) =>
    api.post<ApiResponse<SizeRecommendResponse>>('/api/ai/size-recommend', data),

  forecastInventory: (data: InventoryForecastRequest) =>
    api.post<ApiResponse<InventoryForecastResponse>>('/api/ai/inventory-forecast', data),

  getTrendAnalysis: (params: TrendAnalysisParams) =>
    api.get<ApiResponse<TrendAnalysisResponse>>('/api/ai/trend-analysis', { params }),
}

// ─────────────────────────────────────────────
// 14. HELPERS
// ─────────────────────────────────────────────

/** Lấy data từ response hoặc throw message lỗi */
export function unwrap<T>(res: { data: ApiResponse<T> }): T {
  if (!res.data.success) throw new Error(res.data.message)
  return res.data.data
}

/** Download blob từ reportApi.exportCsv */
export function downloadBlob(blobRes: { data: Blob }, filename: string) {
  const url = URL.createObjectURL(blobRes.data)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  a.click()
  URL.revokeObjectURL(url)
}

/**
 * ROLES có trong hệ thống
 * Dùng để check quyền trong component: if (user.role === ROLES.Admin) ...
 */
export const ROLES = {
  Admin: 'Admin',
  Manager: 'Manager',
  Sales: 'Sales',
  Warehouse: 'Warehouse',
  Accountant: 'Accountant',
} as const

export type Role = (typeof ROLES)[keyof typeof ROLES]

/**
 * MEMBER LEVELS
 */
export const MEMBER_LEVELS = ['Bronze', 'Silver', 'Gold', 'Platinum'] as const
export type MemberLevel = (typeof MEMBER_LEVELS)[number]
