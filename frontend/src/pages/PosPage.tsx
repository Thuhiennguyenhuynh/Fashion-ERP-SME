import React, { useEffect, useState } from 'react';
import { Input, Button, Card, Select, message, Typography, Divider, Modal, Tag, Space } from 'antd';
import { ScanOutlined, PayCircleOutlined, CameraOutlined, MinusOutlined, PlusOutlined, DeleteOutlined, ClearOutlined } from '@ant-design/icons';
import { useCartStore } from '../stores/useCartStore';
import { customerApi, orderApi, promotionApi, variantApi } from '../services/api';
import type { ApiResponse } from '../services/api';
import WebcamScanner from '../components/WebcamScanner';
import { handleApiError } from '../utils/handleApiError';

const { Title, Text } = Typography;

const PosPage: React.FC = () => {
  const cart = useCartStore();
  const [barcode, setBarcode] = useState('');
  const [isCameraOpen, setIsCameraOpen] = useState(false);
  const [customers, setCustomers] = useState<Array<{ id: string; fullName: string; phone?: string }>>([]);
  const [customerLoading, setCustomerLoading] = useState(false);
  const [promotionCode, setPromotionCode] = useState(cart.promotionCode ?? '');
  const [applyingPromotion, setApplyingPromotion] = useState(false);

  const subtotal = cart.items.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0);
  const tax = subtotal * 0.08;
  const finalAmount = subtotal + tax - cart.discountAmount;

  useEffect(() => {
    const fetchCustomers = async () => {
      setCustomerLoading(true);
      try {
        const res = (await customerApi.getAll({ page: 1, pageSize: 50 })) as unknown as ApiResponse<any>;
        const items = res.data?.items ?? [];
        setCustomers(items);
      } catch (error) {
        console.error(error);
      } finally {
        setCustomerLoading(false);
      }
    };

    fetchCustomers();
  }, []);

  const processBarcode = async (code: string) => {
    const cleanCode = code.trim();
    if (!cleanCode) return;

    message.loading({ content: `Đang tìm mã: ${cleanCode}...`, key: 'scan' });

    try {
      const res = (await variantApi.getByBarcode(cleanCode)) as unknown as ApiResponse<any>;
      const variant = res.data;

      if (!variant?.id) {
        throw new Error('Không tìm thấy biến thể với mã vạch này');
      }

      cart.addItem({
        variantId: variant.id,
        productId: variant.product.id,
        productName: variant.product.name,
        size: variant.size,
        color: variant.color,
        unitPrice: variant.price || 0,
        quantity: 1,
        maxStock: variant.stockQuantity || 0,
      });

      message.success({ content: `Đã thêm ${variant.product.name} vào giỏ`, key: 'scan' });
    } catch (error) {
      handleApiError(error, 'Không tìm thấy sản phẩm với mã vạch này!');
    }
  };

  const handleInputEnter = () => {
    processBarcode(barcode);
    setBarcode('');
  };

  const handleCameraScanSuccess = (scannedCode: string) => {
    setIsCameraOpen(false);
    processBarcode(scannedCode);
  };

  const handleApplyPromotion = async () => {
    const code = promotionCode.trim();
    if (!code) {
      return message.warning('Vui lòng nhập mã khuyến mãi');
    }

    setApplyingPromotion(true);
    try {
      const res = (await promotionApi.apply({ code, orderSubtotal: subtotal })) as unknown as ApiResponse<any>;
      const data = res.data;
      if (!data?.isValid) {
        throw new Error(data?.errorMessage || 'Mã khuyến mãi không hợp lệ');
      }
      cart.applyPromotion(code, Number(data.discountAmount || 0));
      message.success(`Áp dụng khuyến mãi thành công: -${Number(data.discountAmount || 0).toLocaleString()}đ`);
    } catch (error) {
      handleApiError(error, 'Áp dụng khuyến mãi thất bại');
    } finally {
      setApplyingPromotion(false);
    }
  };

  const handleCheckout = async () => {
    if (cart.items.length === 0) {
      return message.warning('Giỏ hàng đang trống!');
    }

    try {
      await orderApi.create({
        customerId: cart.customerId ?? undefined,
        paymentMethod: cart.paymentMethod,
        promotionCode: cart.promotionCode ?? undefined,
        items: cart.items.map((i) => ({ variantId: i.variantId, quantity: i.quantity })),
      });
      message.success('Tạo đơn hàng thành công!');
      cart.clearCart();
      setPromotionCode('');
    } catch (error) {
      handleApiError(error, 'Lỗi khi tạo đơn');
    }
  };

  const handleQuantityChange = (variantId: string, delta: number) => {
    const currentItem = cart.items.find((item) => item.variantId === variantId);
    if (!currentItem) return;

    const nextQuantity = Math.max(1, Math.min(currentItem.maxStock, currentItem.quantity + delta));
    cart.updateQuantity(variantId, nextQuantity);
  };

  return (
    <div className="flex h-full flex-col gap-4 p-1 md:p-2">
      <div className="grid gap-4 xl:grid-cols-[1.4fr_0.8fr]">
        <div className="flex flex-col gap-4">
          <Card className="shadow-sm">
            <div className="flex flex-col gap-3 md:flex-row">
              <Input
                className="flex-1"
                size="large"
                placeholder="Quét mã vạch hoặc nhập tay..."
                prefix={<ScanOutlined />}
                value={barcode}
                onChange={(e) => setBarcode(e.target.value)}
                onPressEnter={handleInputEnter}
                autoFocus
              />
              <Space>
                <Button size="large" icon={<CameraOutlined />} onClick={() => setIsCameraOpen(true)}>
                  Webcam
                </Button>
                <Button size="large" icon={<ClearOutlined />} onClick={() => { cart.clearCart(); setPromotionCode(''); }}>
                  Xóa giỏ
                </Button>
              </Space>
            </div>
          </Card>

          <Card className="flex-1 overflow-auto border border-neutral-200 shadow-sm">
            <div className="mb-4 flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
              <div>
                <Text strong className="text-base">Đơn hàng hiện tại</Text>
                <div className="text-sm text-neutral-500">Quét mã vạch để thêm sản phẩm vào giỏ bán hàng</div>
              </div>
              <Tag color="blue">{cart.items.length} sản phẩm</Tag>
            </div>

            {cart.items.length === 0 ? (
              <div className="rounded-lg border border-dashed border-neutral-300 p-8 text-center text-neutral-500">
                Chưa có sản phẩm nào trong giỏ. Hãy quét mã vạch để bắt đầu bán hàng.
              </div>
            ) : (
              <div className="grid gap-3 md:grid-cols-2">
                {cart.items.map((item) => (
                  <div key={item.variantId} className="rounded-lg border border-neutral-200 bg-white p-3 shadow-sm">
                    <div className="flex items-start justify-between gap-2">
                      <div>
                        <Text strong>{item.productName}</Text>
                        <div className="text-xs text-neutral-500">
                          {item.color} - {item.size}
                        </div>
                      </div>
                      <Text className="font-medium text-neutral-800">{(item.unitPrice * item.quantity).toLocaleString()}đ</Text>
                    </div>

                    <div className="mt-3 flex items-center justify-between">
                      <div className="flex items-center gap-2">
                        <Button size="small" icon={<MinusOutlined />} onClick={() => handleQuantityChange(item.variantId, -1)} />
                        <span className="min-w-8 text-center text-sm font-medium">{item.quantity}</span>
                        <Button size="small" icon={<PlusOutlined />} onClick={() => handleQuantityChange(item.variantId, 1)} />
                      </div>
                      <Button size="small" danger icon={<DeleteOutlined />} onClick={() => cart.removeItem(item.variantId)} />
                    </div>
                  </div>
                ))}
              </div>
            )}
          </Card>
        </div>

        <Card title="Thanh toán" className="shadow-sm">
          <div className="space-y-4">
            <div>
              <Text strong>Khách hàng</Text>
              <Select
                showSearch
                loading={customerLoading}
                placeholder="Chọn khách hàng"
                className="mt-2 w-full"
                options={customers.map((customer) => ({ label: `${customer.fullName} • ${customer.phone || 'Không có SĐT'}`, value: customer.id }))}
                value={cart.customerId ?? undefined}
                onChange={(value) => cart.setCustomer(value ?? null)}
              />
            </div>

            <div>
              <Text strong>Mã khuyến mãi</Text>
              <div className="mt-2 flex gap-2">
                <Input value={promotionCode} onChange={(e) => setPromotionCode(e.target.value)} placeholder="Nhập mã giảm giá" />
                <Button onClick={handleApplyPromotion} loading={applyingPromotion}>Áp dụng</Button>
              </div>
            </div>

            <Divider className="my-2" />

            <div className="space-y-2 text-sm text-neutral-700">
              <div className="flex items-center justify-between">
                <Text>Tạm tính</Text>
                <Text>{subtotal.toLocaleString()}đ</Text>
              </div>
              <div className="flex items-center justify-between">
                <Text>Thuế (8%)</Text>
                <Text>{tax.toLocaleString()}đ</Text>
              </div>
              <div className="flex items-center justify-between">
                <Text>Giảm giá</Text>
                <Text type="danger">-{cart.discountAmount.toLocaleString()}đ</Text>
              </div>
            </div>

            <div className="rounded-lg bg-neutral-50 p-3">
              <div className="flex items-center justify-between">
                <Title level={4} className="!mb-0">Khách phải trả</Title>
                <Title level={4} className="!mb-0 !text-emerald-600">{finalAmount.toLocaleString()}đ</Title>
              </div>
            </div>

            <div>
              <Text strong>Phương thức thanh toán</Text>
              <div className="mt-2 flex flex-wrap gap-2">
                <Button type={cart.paymentMethod === 'Cash' ? 'primary' : 'default'} onClick={() => cart.setPaymentMethod('Cash')}>
                  Tiền mặt
                </Button>
                <Button type={cart.paymentMethod === 'Transfer' ? 'primary' : 'default'} onClick={() => cart.setPaymentMethod('Transfer')}>
                  Chuyển khoản
                </Button>
                <Button type={cart.paymentMethod === 'Card' ? 'primary' : 'default'} onClick={() => cart.setPaymentMethod('Card')}>
                  Quẹt thẻ
                </Button>
              </div>
            </div>

            <Button type="primary" size="large" block icon={<PayCircleOutlined />} className="mt-2 h-12" onClick={handleCheckout} disabled={cart.items.length === 0}>
              THANH TOÁN LƯU ĐƠN
            </Button>
          </div>
        </Card>
      </div>

      <Modal title="Quét mã vạch bằng Webcam" open={isCameraOpen} onCancel={() => setIsCameraOpen(false)} footer={null} destroyOnClose>
        {isCameraOpen && <WebcamScanner onScanSuccess={handleCameraScanSuccess} />}
      </Modal>
    </div>
  );
};

export default PosPage;