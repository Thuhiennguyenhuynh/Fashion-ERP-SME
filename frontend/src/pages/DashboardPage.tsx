import React, { useEffect, useState } from 'react';
import { Card, Statistic, Row, Col } from 'antd';
import { ArrowUpOutlined, ArrowDownOutlined } from '@ant-design/icons';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import axiosClient from '../api/axiosClient';

export default function DashboardPage() {
  const [data, setData] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchDashboard = async () => {
      try {
        const res: any = await axiosClient.get('/dashboard/summary');
        setData(res);
      } catch (error) {
        console.error("Lỗi lấy dữ liệu dashboard", error);
      } finally {
        setLoading(false);
      }
    };
    fetchDashboard();
  }, []);

  if (loading) return <div className="animate-pulse">Đang tải dữ liệu tổng quan...</div>;

  return (
    <main className="space-y-6">
      <header>
        <h1 className="text-2xl font-semibold text-neutral-800 uppercase tracking-wide">Tổng quan kinh doanh</h1>
        <p className="text-sm text-neutral-500 mt-1">Số liệu thống kê tính đến thời điểm hiện tại.</p>
      </header>

      {/* KPI CARDS */}
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Card bordered={false} className="shadow-sm rounded-xl">
            <Statistic
              title="Doanh thu tháng"
              value={data?.revenue || 0}
              precision={0}
              valueStyle={{ color: '#000', fontWeight: 600 }}
              prefix="₫"
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card bordered={false} className="shadow-sm rounded-xl">
            <Statistic
              title="Tổng đơn hàng"
              value={data?.orderCount || 0}
              valueStyle={{ color: '#000', fontWeight: 600 }}
              suffix="Đơn"
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card bordered={false} className="shadow-sm rounded-xl">
            <Statistic
              title="Cảnh báo tồn kho"
              value={data?.lowStockCount || 0}
              valueStyle={{ color: '#cf1322', fontWeight: 600 }}
              suffix="Sản phẩm"
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card bordered={false} className="shadow-sm rounded-xl">
            <Statistic
              title="Khách hàng mới"
              value={data?.newCustomers || 0}
              valueStyle={{ color: '#3f8600', fontWeight: 600 }}
              prefix={<ArrowUpOutlined />}
            />
          </Card>
        </Col>
      </Row>

      {/* CHART SECTION */}
      <div className="bg-white p-6 rounded-xl shadow-sm border border-neutral-100">
        <h3 className="text-lg font-medium text-neutral-800 mb-6">Biểu đồ doanh thu 7 ngày gần nhất</h3>
        <div className="h-80 w-full">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={data?.last7Days || []}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f5f5f5" />
              <XAxis 
                dataKey="date" 
                tickFormatter={(tick) => new Date(tick).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' })}
                axisLine={false}
                tickLine={false}
                tick={{ fill: '#8c8c8c', fontSize: 12 }}
                dy={10}
              />
              <YAxis 
                axisLine={false} 
                tickLine={false}
                tick={{ fill: '#8c8c8c', fontSize: 12 }}
                tickFormatter={(value) => `${(value / 1000000).toFixed(0)}M`}
              />
            <Tooltip 
  formatter={(value: any) => [
    new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(Number(value)),
    'Doanh thu' // Đây là NameType sẽ thay thế cho dataKey="revenue"
  ]}
  labelFormatter={(label) => new Date(label).toLocaleDateString('vi-VN')}
/>
              <Line 
                type="monotone" 
                dataKey="revenue" 
                stroke="#171717" 
                strokeWidth={3}
                dot={{ r: 4, fill: '#171717', strokeWidth: 2, stroke: '#fff' }}
                activeDot={{ r: 6 }} 
              />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </div>
    </main>
  );
}