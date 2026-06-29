import { create } from 'zustand';

// Interface bám sát DTO OrderItemResponseDto và Variant của Backend
export interface CartItem {
  variantId: string;
  productId: string;
  productName: string;
  size: string;
  color: string;
  unitPrice: number;
  quantity: number;
  maxStock: number; // Để validate không cho mua quá tồn kho
}

interface CartState {
  items: CartItem[];
  customerId: string | null;
  promotionCode: string | null;
  discountAmount: number;
  paymentMethod: 'Cash' | 'Transfer' | 'Card';
  
  // Actions
  addItem: (item: CartItem) => void;
  updateQuantity: (variantId: string, quantity: number) => void;
  removeItem: (variantId: string) => void;
  setCustomer: (id: string | null) => void;
  applyPromotion: (code: string, discount: number) => void;
  setPaymentMethod: (method: 'Cash' | 'Transfer' | 'Card') => void;
  clearCart: () => void;
}

export const useCartStore = create<CartState>((set) => ({
  items: [],
  customerId: null,
  promotionCode: null,
  discountAmount: 0,
  paymentMethod: 'Cash', // Mặc định theo Backend DTO

  addItem: (newItem) => set((state) => {
    const existingItem = state.items.find(i => i.variantId === newItem.variantId);
    if (existingItem) {
      return {
        items: state.items.map(i => 
          i.variantId === newItem.variantId 
            ? { ...i, quantity: Math.min(i.quantity + newItem.quantity, i.maxStock) } 
            : i
        )
      };
    }
    return { items: [...state.items, newItem] };
  }),

  updateQuantity: (variantId, quantity) => set((state) => ({
    items: state.items.map(i => i.variantId === variantId ? { ...i, quantity } : i)
  })),

  removeItem: (variantId) => set((state) => ({
    items: state.items.filter(i => i.variantId !== variantId)
  })),

  setCustomer: (id) => set({ customerId: id }),
  applyPromotion: (code, discount) => set({ promotionCode: code, discountAmount: discount }),
  setPaymentMethod: (method) => set({ paymentMethod: method }),
  clearCart: () => set({ items: [], customerId: null, promotionCode: null, discountAmount: 0, paymentMethod: 'Cash' })
}));