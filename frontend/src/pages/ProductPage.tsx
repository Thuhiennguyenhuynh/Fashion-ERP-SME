import { useEffect, useState } from 'react';
import { Table, Button, Input, Tag, Space, Tooltip, Avatar } from 'antd';
import { PlusOutlined, EditOutlined, PictureOutlined } from '@ant-design/icons';
import { productApi } from '../services/api';
import type { ProductResponse, ApiResponse, PagedResult } from '../services/api';
import { handleApiError } from '../utils/handleApiError';
import { formatVND } from '../utils/format';

export default function ProductPage() {
  const [products, setProducts] = useState<ProductResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [pagination, setPagination] = useState({ current: 1, pageSize: 10, total: 0 });
  const [keyword, setKeyword] = useState('');

  const fetchProducts = async (page = 1, pageSize = 10, search = '') => {
    setLoading(true);
    try {
      const res = (await productApi.getAll({ page, pageSize, keyword: search })) as unknown as ApiResponse<PagedResult<ProductResponse>>;
      const paged = res.data;
      setProducts(paged.items);
      setPagination({ current: paged.page, pageSize: paged.pageSize, total: paged.totalCount });
    } catch (error) {
      handleApiError(error, 'Lỗi tải danh sách sản phẩm');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts();
  }, []);

  const handleTableChange = (newPagination: any) => {
    fetchProducts(newPagination.current, newPagination.pageSize, keyword);
  };

  const onSearch = (value: string) => {
    setKeyword(value);
    fetchProducts(1, pagination.pageSize, value);
  };

  const columns = [
    {
      title: 'Sản phẩm',
      dataIndex: 'name',
      key: 'name',
      render: (text: string, record: ProductResponse) => (
        <div className="flex items-center gap-3">
          <Avatar shape="square" size={48} src={record.mainImageUrl} icon={<PictureOutlined />} className="bg-neutral-100 border border-neutral-200 rounded-md" />
          <div>
            <div className="font-medium text-neutral-800">{text}</div>
            <div className="text-xs text-neutral-500 font-mono mt-0.5">{record.productCode}</div>
          </div>
        </div>
      ),
    },
    { title: 'Danh mục', dataIndex: 'categoryName', key: 'categoryName' },
    { title: 'Giá bán cơ bản', dataIndex: 'basePrice', key: 'basePrice', render: (price: number) => <span className="font-medium text-neutral-900">{formatVND(price)}</span> },
    {
      title: 'Phân loại',
      dataIndex: 'gender',
      key: 'gender',
      render: (gender: string) => {
        const colorMap: Record<string, string> = { Male: 'blue', Female: 'magenta', Unisex: 'purple', Kids: 'green' };
        return <Tag color={colorMap[gender] || 'default'}>{gender}</Tag>;
      },
    },
    { title: 'Biến thể', key: 'variants', render: (_: any, record: ProductResponse) => <span className="text-sm text-neutral-600">{record.variants?.length || 0} SKU</span> },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => {
        const color = status === 'Active' ? 'success' : status === 'Draft' ? 'warning' : 'default';
        return <Tag color={color} className="uppercase tracking-wider text-[10px]">{status}</Tag>;
      },
    },
    {
      title: 'Thao tác',
      key: 'action',
      align: 'right' as const,
      render: () => (
        <Space size="middle">
          <Tooltip title="Chỉnh sửa sản phẩm">
            <Button type="text" icon={<EditOutlined className="text-neutral-500 hover:text-blue-600 transition-colors" />} />
          </Tooltip>
        </Space>
      ),
    },
  ];

  return (
    <main className="bg-white p-6 md:p-8 rounded-xl shadow-sm border border-neutral-100 font-sans min-h-full">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-neutral-800 tracking-wide uppercase">Quản lý Sản phẩm</h1>
          <p className="text-sm text-neutral-500 mt-1 font-light">Xem, thêm mới và cập nhật thông tin sản phẩm, kho hàng.</p>
        </div>
        <div className="flex gap-3 w-full sm:w-auto">
          <Input.Search placeholder="Tìm theo tên, mã SP..." allowClear onSearch={onSearch} className="w-full sm:w-64" size="large" />
          <Button type="primary" size="large" icon={<PlusOutlined />} className="bg-neutral-900 text-white hover:!bg-neutral-800">
            Thêm Mới
          </Button>
        </div>
      </div>

      <Table
        columns={columns}
        dataSource={products}
        rowKey="id"
        loading={loading}
        pagination={{ ...pagination, showSizeChanger: true, showTotal: (total) => `Tổng cộng ${total} sản phẩm`, className: 'mt-6' }}
        onChange={handleTableChange}
        className="border border-neutral-100 rounded-lg overflow-hidden"
        rowClassName="hover:bg-neutral-50/50 transition-colors"
      />
    </main>
  );
}