import { useEffect, useState } from 'react';
import { Table, Button, Input, Tag, Space, Modal, Form, message } from 'antd';
import { PlusOutlined, EditOutlined, UserOutlined } from '@ant-design/icons';
import { customerApi } from '../services/api';
import type { CustomerResponse, ApiResponse, PagedResult, CreateCustomerRequest, UpdateCustomerRequest } from '../services/api';
import { handleApiError } from '../utils/handleApiError';
import { formatVND } from '../utils/format';

export default function CustomersPage() {
  const [customers, setCustomers] = useState<CustomerResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [pagination, setPagination] = useState({ current: 1, pageSize: 10, total: 0 });
  const [keyword, setKeyword] = useState('');
  const [modalOpen, setModalOpen] = useState(false);
  const [editingCustomer, setEditingCustomer] = useState<CustomerResponse | null>(null);
  const [form] = Form.useForm();

  const fetchCustomers = async (page = 1, pageSize = 10, search = '') => {
    setLoading(true);
    try {
      const res = (await customerApi.getAll({ page, pageSize, keyword: search })) as unknown as ApiResponse<PagedResult<CustomerResponse>>;
      const paged = res.data;
      setCustomers(paged.items);
      setPagination({ current: paged.page, pageSize: paged.pageSize, total: paged.totalCount });
    } catch (error) {
      handleApiError(error, 'Lỗi tải danh sách khách hàng');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCustomers();
  }, []);

  const handleTableChange = (paginationConfig: { current?: number; pageSize?: number }) => {
    fetchCustomers(paginationConfig.current ?? 1, paginationConfig.pageSize ?? pagination.pageSize, keyword);
  };

  const onSearch = (value: string) => {
    setKeyword(value);
    fetchCustomers(1, pagination.pageSize, value);
  };

  const openCreateModal = () => {
    form.resetFields();
    setEditingCustomer(null);
    setModalOpen(true);
  };

  const openEditModal = (customer: CustomerResponse) => {
    setEditingCustomer(customer);
    form.setFieldsValue({
      fullName: customer.fullName,
      phone: customer.phone,
      email: customer.email,
      gender: customer.gender,
      dateOfBirth: customer.dateOfBirth,
      address: customer.address,
      note: customer.note,
    });
    setModalOpen(true);
  };

  const handleSubmit = async (values: CreateCustomerRequest | UpdateCustomerRequest) => {
    try {
      if (editingCustomer) {
        await customerApi.update(editingCustomer.id, values as UpdateCustomerRequest);
        message.success('Cập nhật khách hàng thành công');
      } else {
        await customerApi.create(values as CreateCustomerRequest);
        message.success('Thêm khách hàng thành công');
      }
      setModalOpen(false);
      fetchCustomers(pagination.current, pagination.pageSize, keyword);
    } catch (error) {
      handleApiError(error, editingCustomer ? 'Cập nhật khách hàng thất bại' : 'Thêm khách hàng thất bại');
    }
  };

  const columns = [
    {
      title: 'Khách hàng',
      key: 'customer',
      render: (_: unknown, record: CustomerResponse) => (
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-neutral-100 text-neutral-700">
            <UserOutlined />
          </div>
          <div>
            <div className="font-medium text-neutral-800">{record.fullName}</div>
            <div className="text-xs text-neutral-500">{record.phone}</div>
          </div>
        </div>
      ),
    },
    {
      title: 'Email',
      dataIndex: 'email',
      key: 'email',
      render: (value?: string) => value || '—',
    },
    {
      title: 'Hạng thành viên',
      dataIndex: 'memberLevel',
      key: 'memberLevel',
      render: (value: string) => <Tag color="blue">{value}</Tag>,
    },
    {
      title: 'Tổng chi tiêu',
      dataIndex: 'totalSpent',
      key: 'totalSpent',
      render: (value: number) => <span className="font-medium text-neutral-900">{formatVND(value)}</span>,
    },
    {
      title: 'Số đơn',
      dataIndex: 'totalOrders',
      key: 'totalOrders',
    },
    {
      title: 'Thao tác',
      key: 'action',
      align: 'right' as const,
      render: (_: unknown, record: CustomerResponse) => (
        <Button size="small" icon={<EditOutlined />} onClick={() => openEditModal(record)}>
          Sửa
        </Button>
      ),
    },
  ];

  return (
    <main className="min-h-full rounded-xl border border-neutral-100 bg-white p-6 shadow-sm md:p-8">
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold uppercase tracking-wide text-neutral-800">Khách hàng</h1>
          <p className="mt-1 text-sm text-neutral-500">Quản lý thông tin khách hàng và hành vi mua hàng.</p>
        </div>
        <Space>
          <Input.Search placeholder="Tìm tên, số điện thoại..." allowClear onSearch={onSearch} className="w-full sm:w-72" />
          <Button type="primary" icon={<PlusOutlined />} onClick={openCreateModal}>
            Thêm mới
          </Button>
        </Space>
      </div>

      <Table
        columns={columns}
        dataSource={customers}
        rowKey="id"
        loading={loading}
        pagination={{ ...pagination, showSizeChanger: true, showTotal: (total) => `Tổng cộng ${total} khách hàng` }}
        onChange={handleTableChange}
        className="overflow-hidden rounded-lg border border-neutral-100"
      />

      <Modal
        title={editingCustomer ? 'Cập nhật khách hàng' : 'Thêm khách hàng'}
        open={modalOpen}
        onCancel={() => setModalOpen(false)}
        onOk={() => form.submit()}
        destroyOnClose
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Form.Item name="fullName" label="Họ tên" rules={[{ required: true, message: 'Vui lòng nhập họ tên' }]}> 
            <Input />
          </Form.Item>
          <Form.Item name="phone" label="Số điện thoại" rules={[{ required: true, message: 'Vui lòng nhập số điện thoại' }]}> 
            <Input />
          </Form.Item>
          <Form.Item name="email" label="Email"> 
            <Input />
          </Form.Item>
          <Form.Item name="gender" label="Giới tính"> 
            <Input />
          </Form.Item>
          <Form.Item name="dateOfBirth" label="Ngày sinh"> 
            <Input />
          </Form.Item>
          <Form.Item name="address" label="Địa chỉ"> 
            <Input />
          </Form.Item>
          <Form.Item name="note" label="Ghi chú"> 
            <Input.TextArea rows={3} />
          </Form.Item>
        </Form>
      </Modal>
    </main>
  );
}
