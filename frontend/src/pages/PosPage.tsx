import React, { useState } from 'react';
import { Row, Col, Input, Button, Card, Select, message, Typography, Divider, Modal } from 'antd';
import { ScanOutlined, PayCircleOutlined, CameraOutlined } from '@ant-design/icons';
import { useCartStore } from '../stores/useCartStore';
import { orderApi, variantApi } from '../services/api';
import type { ApiResponse } from '../services/api';
import WebcamScanner from '../components/WebcamScanner';
import { handleApiError } from '../utils/handleApiError';

const { Title, Text } = Typography;

const PosPage: React.FC = () => {
  const cart = useCartStore();
  const [barcode, setBarcode] = useState('');
  const [isCameraOpen, setIsCameraOpen] = useState(false);

  const subtotal = cart.items.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0);
  const tax = subtotal * 0.08;
  const finalAmount = subtotal + tax - cart.discountAmount;

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
    } catch (error) {
      handleApiError(error, 'Lỗi khi tạo đơn');
    }
  };

  return (
    <div style={{ padding: '16px', height: '100vh', display: 'flex', flexDirection: 'column' }}>
      <Row gutter={16} style={{ flex: 1 }}>
        <Col span={14} style={{ display: 'flex', flexDirection: 'column' }}>
          <Card size="small" style={{ marginBottom: '16px' }}>
            <div style={{ display: 'flex', gap: '8px' }}>
              <Input
                style={{ flex: 1 }}
                size="large"
                placeholder="Quét mã vạch USB hoặc nhập tay..."
                prefix={<ScanOutlined />}
                value={barcode}
                onChange={(e) => setBarcode(e.target.value)}
                onPressEnter={handleInputEnter}
                autoFocus
              />
              <Button size="large" icon={<CameraOutlined />} onClick={() => setIsCameraOpen(true)}>
                Webcam
              </Button>
            </div>
          </Card>

          <Card style={{ flex: 1, overflowY: 'auto' }}>
            <Text type="secondary">Danh sách sản phẩm (Grid View) sẽ được hiển thị ở đây...</Text>
          </Card>
        </Col>

        <Col span={10} style={{ display: 'flex', flexDirection: 'column' }}>
          <Card title="Khách hàng & Giỏ hàng" style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
            <Select showSearch placeholder="Tìm khách hàng theo SĐT..." style={{ width: '100%', marginBottom: '16px' }} />

            <div style={{ flex: 1, overflowY: 'auto', marginBottom: '16px' }}>
              {cart.items.map((item) => (
                <div key={item.variantId} style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
                  <div>
                    <Text strong>{item.productName}</Text>
                    <br />
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      {item.color} - {item.size}
                    </Text>
                  </div>
                  <div style={{ textAlign: 'right' }}>
                    <Text>
                      {item.quantity} x {item.unitPrice.toLocaleString()}đ
                    </Text>
                  </div>
                </div>
              ))}
            </div>

            <Divider />

            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <Text>Tạm tính:</Text>
              <Text>{subtotal.toLocaleString()}đ</Text>
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <Text>Giảm giá:</Text>
              <Text type="danger">-{cart.discountAmount.toLocaleString()}đ</Text>
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: '8px' }}>
              <Title level={4}>Khách phải trả:</Title>
              <Title level={4} type="success">{finalAmount.toLocaleString()}đ</Title>
            </div>

            <div style={{ marginTop: '16px', display: 'flex', gap: '8px' }}>
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

            <Button type="primary" size="large" block icon={<PayCircleOutlined />} style={{ marginTop: '16px', height: '50px' }} onClick={handleCheckout}>
              THANH TOÁN LƯU ĐƠN
            </Button>
          </Card>
        </Col>
      </Row>

      <Modal title="Quét mã vạch bằng Webcam" open={isCameraOpen} onCancel={() => setIsCameraOpen(false)} footer={null} destroyOnHidden>
        {isCameraOpen && <WebcamScanner onScanSuccess={handleCameraScanSuccess} />}
      </Modal>
    </div>
  );
};

export default PosPage;