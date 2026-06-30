import { useEffect, useState } from 'react';
import { Table, Input, Tag, Space, Button, Switch, Modal, Form, InputNumber, message } from 'antd';
import { ImportOutlined, EditOutlined } from '@ant-design/icons';
import { inventoryApi } from '../services/api';
import type { InventoryResponse, ApiResponse, PagedResult } from '../services/api';
import { handleApiError } from '../utils/handleApiError';

export default function InventoryPage() {
  const [items, setItems] = useState<InventoryResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [pagination, setPagination] = useState({ current: 1, pageSize: 10, total: 0 });
  const [keyword, setKeyword] = useState('');
  const [lowStockOnly, setLowStockOnly] = useState(false);
  const [modal, setModal] = useState<{ open: boolean; item: InventoryResponse | null; mode: 'import' | 'adjust' }>({
    open: false,
    item: null,
    mode: 'import',
  });
  const [form] = Form.useForm();

  const fetchInventory = async (page = 1, pageSize = 10, search = '', lowOnly = false) => {
    setLoading(true);
    try {
      const res = (await inventoryApi.getAll({ page, pageSize, keyword: search, lowStockOnly: lowOnly })) as unknown as ApiResponse<PagedResult<InventoryResponse>>;
      const paged = res.data;
      setItems(paged.items);
      setPagination({ current: paged.page, pageSize: paged.pageSize, total: paged.totalCount });
    } catch (error) {
      handleApiError(error, 'Lỗi tải dữ liệu kho hàng');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchInventory(1, pagination.pageSize, keyword, lowStockOnly);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [lowStockOnly]);

  const handleTableChange = (newPagination: any) => {
    fetchInventory(newPagination.current, newPagination.pageSize, keyword, lowStockOnly);
  };

  const onSearch = (value: string) => {
    setKeyword(value);
    fetchInventory(1, pagination.pageSize, value, lowStockOnly);
  };

  const openModal = (item: InventoryResponse, mode: 'import' | 'adjust') => {
    form.resetFields();
    setModal({ open: true, item, mode });
  };

  const handleSubmitModal = async (values: any) => {
    if (!modal.item) return;
    try {
      if (modal.mode === 'import') {
        await inventoryApi.import({ variantId: modal.item.variantId, ...values });
        message.success('Nhập kho thành công');
      } else {
        await inventoryApi.adjust({ variantId: modal.item.variantId, ...values });
        message.success('Điều chỉnh kho thành công');
      }
      setModal((m) => ({ ...m, open: false }));
      fetchInventory(pagination.current, pagination.pageSize, keyword, lowStockOnly);
    } catch (error) {
      handleApiError(error, 'Thao tác kho thất bại');
    }
  };

  const columns = [
    { title: 'SKU', dataIndex: 'sku' },
    { title: 'Sản phẩm', dataIndex: 'productName' },
    { title: 'Size', dataIndex: 'size' },
    { title: 'Màu', dataIndex: 'color' },
    {
      title: 'Tồn kho',
      dataIndex: 'quantity',
      render: (q: number, r: InventoryResponse) => (
        <Tag color={r.isLowStock ? (q === 0 ? 'red' : 'orange') : 'green'}>
          {q} (min {r.minStock})
        </Tag>
      ),
    },
    { title: 'Vị trí', dataIndex: 'location' },
    {
      title: 'Hành động',
      render: (_: any, record: InventoryResponse) => (
        <Space>
          <Button size="small" icon={<ImportOutlined />} onClick={() => openModal(record, 'import')}>
            Nhập
          </Button>
          <Button size="small" icon={<EditOutlined />} onClick={() => openModal(record, 'adjust')}>
            Điều chỉnh
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <main className="bg-white p-6 md:p-8 rounded-xl shadow-sm border border-neutral-100 font-sans min-h-full">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-neutral-800 tracking-wide uppercase">Kho hàng</h1>
          <p className="text-sm text-neutral-500 mt-1 font-light">Theo dõi tồn kho, nhập hàng và điều chỉnh số lượng.</p>
        </div>
        <Space>
          <span className="text-sm text-neutral-600">Chỉ hiện hàng sắp hết:</span>
          <Switch checked={lowStockOnly} onChange={setLowStockOnly} />
          <Input.Search placeholder="Tìm SKU, tên SP..." allowClear onSearch={onSearch} className="w-64" size="large" />
        </Space>
      </div>

      <Table
        columns={columns}
        dataSource={items}
        rowKey="id"
        loading={loading}
        pagination={{ ...pagination, showSizeChanger: true, showTotal: (total) => `Tổng cộng ${total} dòng tồn kho` }}
        onChange={handleTableChange}
        className="border border-neutral-100 rounded-lg overflow-hidden"
      />

      <Modal title={`${modal.mode === 'import' ? 'Nhập kho' : 'Điều chỉnh tồn'} — ${modal.item?.sku ?? ''}`} open={modal.open} onCancel={() => setModal((m) => ({ ...m, open: false }))} onOk={() => form.submit()} destroyOnClose>
        <Form form={form} layout="vertical" onFinish={handleSubmitModal}>
          {modal.mode === 'import' ? (
            <>
              <Form.Item name="quantity" label="Số lượng nhập" rules={[{ required: true }]}> 
                <InputNumber min={1} style={{ width: '100%' }} />
              </Form.Item>
              <Form.Item name="unitCost" label="Giá vốn / đơn vị" rules={[{ required: true }]}> 
                <InputNumber min={0} style={{ width: '100%' }} />
              </Form.Item>
            </>
          ) : (
            <Form.Item name="newQuantity" label="Số lượng tồn mới" rules={[{ required: true }]}> 
              <InputNumber min={0} style={{ width: '100%' }} />
            </Form.Item>
          )}
          <Form.Item name="note" label="Ghi chú">
            <Input.TextArea rows={2} />
          </Form.Item>
        </Form>
      </Modal>
    </main>
  );
}