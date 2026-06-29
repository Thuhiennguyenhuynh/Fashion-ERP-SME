import React, { useState } from 'react';
import { Outlet, NavLink } from 'react-router-dom';
import { 
  AppstoreOutlined, 
  ShoppingCartOutlined, 
  InboxOutlined, 
  TeamOutlined, 
  MenuFoldOutlined, 
  MenuUnfoldOutlined,
  LogoutOutlined
} from '@ant-design/icons';

const MainLayout: React.FC = () => {
  const [collapsed, setCollapsed] = useState(false);

  const menuItems = [
    { path: '/', label: 'Tổng quan', icon: <AppstoreOutlined /> },
    { path: '/pos', label: 'Bán hàng', icon: <ShoppingCartOutlined /> },
    { path: '/inventory', label: 'Kho hàng', icon: <InboxOutlined /> },
    { path: '/hr', label: 'Nhân sự', icon: <TeamOutlined /> },
  ];

  return (
    <div className="flex h-screen bg-gray-50/50 w-full">
      {/* Cấu trúc chuẩn SEO: aside cho thanh bên */}
      <aside 
        className={`${
          collapsed ? 'w-20' : 'w-64'
        } bg-white border-r border-gray-200 transition-all duration-300 ease-in-out flex flex-col shadow-sm z-20`}
      >
        <div className="h-16 flex items-center justify-center border-b border-gray-100">
          <span className="text-xl font-bold tracking-wider text-slate-800 animate-fade-in">
            {collapsed ? 'ERP' : 'FASHION'}
          </span>
        </div>

        {/* Cấu trúc chuẩn SEO: nav cho điều hướng */}
        <nav className="flex-1 py-6 px-3 space-y-2 overflow-y-auto">
          {menuItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              className={({ isActive }) =>
                `flex items-center gap-4 px-3 py-3 rounded-xl transition-all duration-200 group ${
                  isActive 
                    ? 'bg-slate-900 text-white shadow-md' 
                    : 'text-slate-500 hover:bg-slate-100 hover:text-slate-900'
                }`
              }
              title={collapsed ? item.label : undefined}
            >
              <span className="text-lg transition-transform duration-200 group-hover:scale-110">
                {item.icon}
              </span>
              {!collapsed && <span className="font-medium text-sm animate-fade-in">{item.label}</span>}
            </NavLink>
          ))}
        </nav>

        <div className="p-4 border-t border-gray-100">
          <button className="flex items-center gap-4 w-full px-3 py-3 text-red-500 rounded-xl hover:bg-red-50 transition-colors duration-200">
            <LogoutOutlined className="text-lg" />
            {!collapsed && <span className="font-medium text-sm animate-fade-in">Đăng xuất</span>}
          </button>
        </div>
      </aside>

      <div className="flex-1 flex flex-col min-w-0">
        {/* Cấu trúc chuẩn SEO: header */}
        <header className="h-16 bg-white border-b border-gray-200 flex items-center justify-between px-6 shadow-sm z-10">
          <button 
            onClick={() => setCollapsed(!collapsed)}
            className="p-2 -ml-2 rounded-lg text-slate-500 hover:bg-slate-100 transition-colors"
            aria-label="Toggle Menu"
          >
            {collapsed ? <MenuUnfoldOutlined className="text-xl" /> : <MenuFoldOutlined className="text-xl" />}
          </button>

          <div className="flex items-center gap-4">
            <div className="text-right hidden sm:block">
              <p className="text-sm font-bold text-slate-700">Nguyễn Văn Admin</p>
              <p className="text-xs text-slate-400 font-medium">Quản trị viên</p>
            </div>
            <div className="h-10 w-10 rounded-full bg-slate-900 flex items-center justify-center text-white font-bold shadow-md cursor-pointer hover:ring-2 hover:ring-slate-300 transition-all">
              A
            </div>
          </div>
        </header>

        {/* Cấu trúc chuẩn SEO: main cho nội dung chính */}
        <main className="flex-1 overflow-x-hidden overflow-y-auto bg-gray-50/50 p-6">
          <div className="max-w-7xl mx-auto h-full animate-slide-up">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
};

export default MainLayout;