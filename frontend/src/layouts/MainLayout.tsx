import React, { useState } from 'react';
import { Layout, Menu, Dropdown, Avatar } from 'antd';
import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { 
  DashboardOutlined, 
  ShoppingOutlined, 
  AppstoreOutlined, 
  UserOutlined, 
  LogoutOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined
} from '@ant-design/icons';

const { Header, Sider, Content } = Layout;

export default function MainLayout() {
  const [collapsed, setCollapsed] = useState(false);
  const location = useLocation();
  const navigate = useNavigate();

  const handleLogout = () => {
    localStorage.clear();
    navigate('/login');
  };

  // Mảng cấu hình cho Dropdown menu chuẩn Ant Design v5
  const userMenuItems = [
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: 'Đăng xuất',
      onClick: handleLogout,
    },
  ];

  return (
    <Layout className="min-h-screen font-sans">
      {/* SIDEBAR TỐI GIẢN */}
      <Sider 
        trigger={null} 
        collapsible 
        collapsed={collapsed}
        theme="light"
        className="border-r border-neutral-200/60 shadow-sm"
      >
        <div className="h-16 flex items-center justify-center font-serif text-xl font-bold tracking-widest text-neutral-800 border-b border-neutral-100">
          {collapsed ? 'ERP' : 'FASHION SME'}
        </div>
        <Menu
          mode="inline"
          selectedKeys={[location.pathname]}
          className="border-r-0 pt-4"
          items={[
            { key: '/', icon: <DashboardOutlined />, label: <Link to="/">Tổng quan</Link> },
            { key: '/pos', icon: <ShoppingOutlined />, label: <Link to="/pos">Bán hàng (POS)</Link> },
            { key: '/products', icon: <AppstoreOutlined />, label: <Link to="/products">Sản phẩm</Link> },
            { key: '/customers', icon: <UserOutlined />, label: <Link to="/customers">Khách hàng</Link> },
          ]}
        />
      </Sider>

      <Layout className="bg-neutral-50">
        {/* HEADER */}
        <Header className="bg-white px-6 flex justify-between items-center shadow-sm border-b border-neutral-200/60 h-16">
          <button 
            onClick={() => setCollapsed(!collapsed)}
            className="text-lg text-neutral-600 hover:text-neutral-900 transition-colors"
          >
            {collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
          </button>

          <div className="flex items-center gap-4">
            {/* ĐÃ SỬA: Dùng thuộc tính menu={{ items: ... }} thay cho overlay */}
            <Dropdown menu={{ items: userMenuItems }} placement="bottomRight">
              <div className="cursor-pointer flex items-center gap-2 hover:bg-neutral-50 px-3 py-1 rounded-md transition-colors">
                <Avatar icon={<UserOutlined />} className="bg-neutral-800" />
                <span className="text-sm font-medium text-neutral-700 hidden sm:block">Quản trị viên</span>
              </div>
            </Dropdown>
          </div>
        </Header>

        {/* NỘI DUNG CHÍNH (Các trang sẽ render ở đây) */}
        <Content className="p-6 md:p-8 overflow-auto">
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  );
}