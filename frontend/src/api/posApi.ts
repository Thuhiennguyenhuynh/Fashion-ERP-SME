import axiosClient from './axiosClient';

// Theo DTO: CreateOrderRequestDto
export interface CreateOrderRequest {
  customerId?: string | null;
  paymentMethod: string;
  promotionCode?: string | null;
  note?: string;
  items: { variantId: string; quantity: number }[];
}

export const posApi = {
  // Lấy sản phẩm cho grid
  getProducts: (params?: any) => axiosClient.get('/products', { params }),
  
  // Quét mã vạch gọi API GET /variants?barcode=...
  getVariantByBarcode: (barcode: string) => axiosClient.get(`/variants`, { params: { barcode } }),
  
  // Validate mã khuyến mãi
  validatePromotion: (data: { code: string; orderSubtotal: number }) => 
    axiosClient.post('/promotions/apply', data),

  // Chốt đơn hàng
  createOrder: (data: CreateOrderRequest) => axiosClient.post('/orders', data)

  // Gọi API tạo khách hàng mới
//   createCustomer: (data: { fullName: string; phone: string }) => axiosClient.post('/customers', data),
};