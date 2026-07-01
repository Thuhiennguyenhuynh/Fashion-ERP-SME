import React from 'react';
import { Card, Row, Col, Statistic, Typography } from 'antd';
import { ShoppingCartOutlined, DollarOutlined, InboxOutlined, TeamOutlined } from '@ant-design/icons';
import { dashboardApi } from '../../api/orderApi';

const { Title, Text } = Typography;

export default function DashboardPage() {
  const [summary, setSummary] = React.useState<any>(null);

  React.useEffect(() => {
    dashboardApi.getSummary().then((res: any) => setSummary(res?.data ?? res)).catch(() => setSummary(null));
  }, []);

  return (
    <div className="space-y-6">
      <Title level={3}>Tổng quan</Title>
      <Row gutter={[16, 16]}>
        <Col xs={24} md={6}><Card><Statistic title="Doanh thu" value={summary?.revenue ?? 0} prefix={<DollarOutlined />} /></Card></Col>
        <Col xs={24} md={6}><Card><Statistic title="Đơn hàng" value={summary?.orderCount ?? 0} prefix={<ShoppingCartOutlined />} /></Card></Col>
        <Col xs={24} md={6}><Card><Statistic title="Sắp hết hàng" value={summary?.lowStockCount ?? 0} prefix={<InboxOutlined />} /></Card></Col>
        <Col xs={24} md={6}><Card><Statistic title="Khách mới" value={summary?.newCustomers ?? 0} prefix={<TeamOutlined />} /></Card></Col>
      </Row>
      <Card>
        <Text type="secondary">Bảng điều khiển ERP đã được tái cấu trúc theo module.</Text>
      </Card>
    </div>
  );
}
