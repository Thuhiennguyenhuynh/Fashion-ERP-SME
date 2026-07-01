export interface ProductListItem {
  id: string;
  name: string;
  categoryName?: string;
  status?: string;
  basePrice?: number;
}

export interface ProductVariantItem {
  id: string;
  sku: string;
  size: string;
  color: string;
  price?: number;
  barcode?: string;
  stockQuantity?: number;
}
