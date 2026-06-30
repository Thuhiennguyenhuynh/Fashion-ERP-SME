import { useState } from 'react';
import { Card, DatePicker, Button, Row, Col, Table, Statistic } from 'antd';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { reportApi } from '../services/api';
import { handleApiError } from '../utils/handleApiError';
import { formatVND } from '../utils/format';

const { RangePicker } = DatePicker;

const sampleData = [
  { month: 'Th1', revenue: 180000000 },
  { month: 'Th2', revenue: 220000000 },
  { month: 'Th3', revenue: 260000000 },
  { month: 'Th4', revenue: 300000000 },
];

export default function ReportsPage() {
  const [loading, setLoading] = useState(false);
  const [summary, setSummary] = useState({ revenue: 0, orderCount: 0, profit: 0 });

  const loadReport = async () => {
    setLoading(true);
    try {
      const res = await reportApi.getRevenue({ from: '2026-01-01', to: '2026-06-30' });
      const revenueData = (res as any).data ?? [];
      const totalRevenue = revenueData.reduce((sum: number, item: any) => sum + (item.revenue || 0), 0);
      setSummary({ revenue: totalRevenue, orderCount: revenueData.length, profit: Math.round(totalRevenue * 0.25) });
    } catch (error) {
      handleApiError(error, 'Lỗi tải báo cáo');
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="min-h-full rounded-xl border border-neutral-100 bg-white p-6 shadow-sm md:p-8">
      <div className="mb-6 flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-2xl font-semibold uppercase tracking-wide text-neutral-800">Báo cáo</h1>
          <p className="mt-1 text-sm text-neutral-500">Theo dõi doanh thu, đơn hàng và lợi nhuận.</p>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <RangePicker />
          <Button type="primary" onClick={loadReport} loading={loading}>Xem báo cáo</Button>
        </div>
      </div>

      <Row gutter={[16, 16]} className="mb-6">
        <Col xs={24} md={8}>
          <Card>
            <Statistic title="Doanh thu" value={summary.revenue} precision={0} prefix="₫" />
          </Card>
        </Col>
        <Col xs={24} md={8}>
          <Card>
            <Statistic title="Số đơn" value={summary.orderCount} />
          </Card>
        </Col>
        <Col xs={24} md={8}>
          <Card>
            <Statistic title="Lợi nhuận ước tính" value={summary.profit} precision={0} prefix="₫" />
          </Card>
        </Col>
      </Row>

      <Card title="Doanh thu theo tháng" className="mb-6">
        <div className="h-80 w-full">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={sampleData}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} />
              <XAxis dataKey="month" />
              <YAxis tickFormatter={(value) => `${(Number(value) / 1000000).toFixed(0)}M`} />
              <Tooltip formatter={(value: any) => [formatVND(Number(value)), 'Doanh thu']} />
              <Bar dataKey="revenue" fill="#171717" radius={[8, 8, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </Card>

      <Card title="Chi tiết báo cáo">
        <Table
          columns={[
            { title: 'Tháng', dataIndex: 'month' },
            { title: 'Doanh thu', dataIndex: 'revenue', render: (value: number) => formatVND(value) },
          ]}
          dataSource={sampleData}
          pagination={false}
        />
      </Card>
    </main>
  );
}
