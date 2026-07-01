import { Card, Typography } from 'antd';

const { Text } = Typography;

export default function CartPanel() {
  return (
    <Card title="Giỏ hàng POS">
      <Text type="secondary">Panel giỏ hàng sẽ được tách riêng ở bước tiếp theo.</Text>
    </Card>
  );
}
