import React, { useState, useEffect } from 'react';
import { Input, Button, Divider, Select, message, Empty, Spin } from 'antd';
import { 
  SearchOutlined, 
  BarcodeOutlined, 
  DeleteOutlined, 
  UserOutlined,
  ShoppingOutlined
} from '@ant-design/icons';
import axiosClient from '../api/axiosClient';

// Định nghĩa Interface dựa trên CreateOrderRequestDto của Backend
interface CartItem {
  variantId: string;
  productId: string;
  name: string;
  size: string;
  color: string;
  price: number;
  quantity: number;
}

export default function PosPage() {
  const [products, setProducts] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  
  // State quản lý Giỏ hàng và Checkout
  const [cart, setCart] = useState<CartItem[]>([]);
  const [paymentMethod, setPaymentMethod] = useState<string>('Cash');
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Lấy danh sách sản phẩm mẫu
  useEffect(() => {
    const fetchProducts = async () => {
      try {
        const res: any = await axiosClient.get('/products', { params: { pageSize: 20 } });
        setProducts(res.items || []);
      } catch (error) {
        message.error("Lỗi tải danh sách sản phẩm");
      } finally {
        setLoading(false);
      }
    };
    fetchProducts();
  }, []);

  // Thêm sản phẩm vào giỏ
  const addToCart = (product: any, variant: any) => {
    setCart((prev) => {
      const existing = prev.find(item => item.variantId === variant.id);
      if (existing) {
        return prev.map(item => 
          item.variantId === variant.id ? { ...item, quantity: item.quantity + 1 } : item
        );
      }
      return [...prev, {
        variantId: variant.id,
        productId: product.id,
        name: product.name,
        size: variant.size,
        color: variant.color,
        price: variant.price || product.basePrice,
        quantity: 1
      }];
    });
  };

  // Cập nhật số lượng
  const updateQuantity = (variantId: string, delta: number) => {
    setCart(prev => prev.map(item => {
      if (item.variantId === variantId) {
        const newQty = item.quantity + delta;
        return newQty > 0 ? { ...item, quantity: newQty } : item;
      }
      return item;
    }));
  };

  // Xóa khỏi giỏ
  const removeRow = (variantId: string) => {
    setCart(prev => prev.filter(item => item.variantId !== variantId));
  };

  // Tính toán tổng tiền
  const subtotal = cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
  const tax = subtotal * 0.08; // VAT 8% mặc định
  const finalTotal = subtotal + tax;

  // Xử lý Đặt hàng
  const handleCheckout = async () => {
    if (cart.length === 0) {
      message.warning("Giỏ hàng đang trống!");
      return;
    }
    
    setIsSubmitting(true);
    try {
      // Mapping dữ liệu theo CreateOrderRequestDto của Backend
      const payload = {
        paymentMethod: paymentMethod,
        items: cart.map(c => ({
          variantId: c.variantId,
          quantity: c.quantity
        }))
      };

      await axiosClient.post('/orders', payload);
      message.success("Tạo đơn hàng thành công!");
      setCart([]); // Reset giỏ hàng
    } catch (error: any) {
      message.error(error?.message || "Lỗi khi tạo đơn hàng");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    // Layout fixed không cuộn toàn trang, chiều cao bằng 100vh trừ đi Header (64px) và Padding
    <div className="flex h-[calc(100vh-112px)] -m-6 md:-m-8 bg-neutral-50 overflow-hidden font-sans">
      
      {/* CỘT TRÁI: TÌM KIẾM & SẢN PHẨM (Chiếm 65% width) */}
      <div className="flex-1 flex flex-col border-r border-neutral-200/80 bg-neutral-50/50">
        {/* Thanh tìm kiếm */}
        <div className="p-4 bg-white border-b border-neutral-200/80 flex gap-3 shadow-sm z-10">
          <Input 
            size="large" 
            placeholder="Tìm kiếm sản phẩm theo tên hoặc mã..." 
            prefix={<SearchOutlined className="text-neutral-400" />}
            className="rounded-md"
          />
          <Button size="large" icon={<BarcodeOutlined />} className="rounded-md">
            Quét mã
          </Button>
        </div>

        {/* Lưới sản phẩm (Scrollable) */}
        <div className="flex-1 overflow-y-auto p-4 md:p-6">
          {loading ? (
            <div className="flex h-full items-center justify-center"><Spin size="large" /></div>
          ) : (
            <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
              {products.map((p) => (
                <div key={p.id} className="bg-white rounded-xl border border-neutral-200 overflow-hidden hover:shadow-md transition-shadow">
                  <div className="aspect-square bg-neutral-100 relative">
                    <img 
                      src={p.mainImageUrl || 'https://placehold.co/400x400?text=No+Image'} 
                      alt={p.name}
                      className="w-full h-full object-cover"
                    />
                  </div>
                  <div className="p-3">
                    <h3 className="text-sm font-medium text-neutral-800 line-clamp-1">{p.name}</h3>
                    <p className="text-xs text-neutral-500 mt-1">{new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p.basePrice)}</p>
                    
                    {/* Danh sách Variant để chọn nhanh */}
                    <div className="mt-3 flex flex-wrap gap-1">
                      {p.variants?.map((v: any) => (
                        <button
                          key={v.id}
                          onClick={() => addToCart(p, v)}
                          className="px-2 py-1 text-[10px] uppercase tracking-wider font-medium border border-neutral-300 rounded hover:border-neutral-800 hover:bg-neutral-50 transition-colors"
                        >
                          {v.size} - {v.color}
                        </button>
                      ))}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* CỘT PHẢI: GIỎ HÀNG & CHECKOUT (Chiếm 35% width, min 380px) */}
      <div className="w-[380px] lg:w-[420px] bg-white flex flex-col shadow-[-4px_0_15px_-3px_rgba(0,0,0,0.05)] z-20">
        
        {/* Header Giỏ hàng */}
        <div className="p-4 border-b border-neutral-100 flex items-center gap-2">
          <ShoppingOutlined className="text-xl" />
          <h2 className="text-lg font-medium text-neutral-800 tracking-wide uppercase">Giỏ hàng</h2>
          <span className="ml-auto bg-neutral-100 text-neutral-600 px-2 py-0.5 rounded-full text-xs font-semibold">
            {cart.reduce((a, b) => a + b.quantity, 0)}
          </span>
        </div>

        {/* Danh sách Items (Scrollable) */}
        <div className="flex-1 overflow-y-auto p-4 space-y-4">
          {cart.length === 0 ? (
            <Empty description={<span className="text-neutral-400 font-light">Chưa có sản phẩm nào</span>} />
          ) : (
            cart.map((item) => (
              <div key={item.variantId} className="flex gap-3 group">
                <div className="flex-1">
                  <h4 className="text-sm font-medium text-neutral-800 line-clamp-1">{item.name}</h4>
                  <div className="text-xs text-neutral-500 mt-0.5">
                    {item.size} • {item.color}
                  </div>
                  <div className="font-medium text-neutral-900 mt-1">
                    {new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(item.price)}
                  </div>
                </div>
                
                {/* Bộ điều khiển Số lượng */}
                <div className="flex flex-col items-end justify-between">
                  <Button onClick={() => removeRow(item.variantId)} className="text-neutral-300 hover:text-red-500 transition-colors">
                    <DeleteOutlined />
                  </Button>
                  <div className="flex items-center border border-neutral-200 rounded-md">
                    <button onClick={() => updateQuantity(item.variantId, -1)} className="px-2 py-0.5 text-neutral-500 hover:bg-neutral-100">-</button>
                    <span className="px-2 py-0.5 text-sm font-medium text-neutral-800 min-w-[30px] text-center">{item.quantity}</span>
                    <button onClick={() => updateQuantity(item.variantId, 1)} className="px-2 py-0.5 text-neutral-500 hover:bg-neutral-100">+</button>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>

        {/* Footer Checkout */}
        <div className="p-4 bg-neutral-50/50 border-t border-neutral-200">
          <Button className="w-full text-left flex justify-between items-center text-neutral-600 bg-white border-neutral-200 h-10 mb-4 rounded-md">
            <span><UserOutlined className="mr-2"/> Chọn khách hàng...</span>
          </Button>

          <div className="space-y-2 mb-4 text-sm">
            <div className="flex justify-between text-neutral-600">
              <span>Tạm tính</span>
              <span>{new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(subtotal)}</span>
            </div>
            <div className="flex justify-between text-neutral-600">
              <span>Thuế VAT (8%)</span>
              <span>{new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(tax)}</span>
            </div>
            <Divider className="my-2" />
            <div className="flex justify-between text-base font-semibold text-neutral-900">
              <span className="uppercase tracking-wider">Tổng cộng</span>
              <span>{new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(finalTotal)}</span>
            </div>
          </div>

          <Select 
            value={paymentMethod} 
            onChange={setPaymentMethod}
            className="w-full mb-4 h-10"
            options={[
              { value: 'Cash', label: 'Thanh toán Tiền mặt' },
              { value: 'Transfer', label: 'Chuyển khoản (QR Code)' },
              { value: 'Card', label: 'Quẹt thẻ (POS)' },
            ]}
          />

          <Button 
            type="primary" 
            size="large" 
            onClick={handleCheckout}
            loading={isSubmitting}
            className="w-full bg-neutral-900 text-white font-medium tracking-widest uppercase h-12 rounded-md hover:!bg-neutral-800"
          >
            Thanh toán
          </Button>
        </div>

      </div>
    </div>
  );
}