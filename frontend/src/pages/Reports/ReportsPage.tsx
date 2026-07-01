import { Card, Typography } from 'antd';

const { Title, Text } = Typography;

export default function ReportsPage() {
  return (
    <div className="space-y-4">
      <Title level={3}>Báo cáo</Title>
      <Card>
        <Text type="secondary">Module báo cáo sẽ được mở rộng sau.</Text>
      </Card>
    </div>
  );
}
