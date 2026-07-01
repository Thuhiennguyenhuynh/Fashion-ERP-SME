import { Table, Tag, Typography } from 'antd';
import { useProducts } from '../../hooks/useProducts';

const { Title } = Typography;

export default function ProductList() {
  const { data, isLoading } = useProducts();
  const products = (data as any)?.data?.items ?? [];

  return (
    <div className="space-y-4">
      <Title level={3}>Sản phẩm</Title>
      <Table
        rowKey="id"
        loading={isLoading}
        dataSource={products}
        columns={[
          { title: 'Tên', dataIndex: 'name', key: 'name' },
          { title: 'Danh mục', dataIndex: 'categoryName', key: 'categoryName' },
          { title: 'Trạng thái', dataIndex: 'status', key: 'status', render: (status: string) => <Tag>{status}</Tag> },
        ]}
      />
    </div>
  );
}
