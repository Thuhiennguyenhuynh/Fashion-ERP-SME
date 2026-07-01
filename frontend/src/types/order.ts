export interface OrderSummaryItem {
  id: string;
  orderCode: string;
  customerName?: string;
  finalAmount: number;
  paymentMethod?: string;
  status?: string;
  createdAt?: string;
}
