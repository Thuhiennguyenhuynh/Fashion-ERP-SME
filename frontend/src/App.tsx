import React from 'react';
import { BrowserRouter, Routes, Route, Navigate, Outlet } from 'react-router-dom';

import MainLayout from './layouts/MainLayout';
import DashboardPage from './pages/DashboardPage';
import LoginPage from './pages/LoginPage';
import ProductPage from './pages/ProductPage';
import PosPage from './pages/PosPage';

// Component này sẽ đóng vai trò làm "Bảo vệ cổng"
// Mỗi khi URL thay đổi, nó sẽ tự động soi lại localStorage xem có token không
const PrivateRoute = () => {
  const isAuthenticated = !!localStorage.getItem('access_token');
  return isAuthenticated ? <MainLayout /> : <Navigate to="/login" replace />;
};

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Route Công khai */}
        <Route path="/login" element={<LoginPage />} />

        {/* Route Nội bộ - Bọc bằng PrivateRoute */}
        <Route path="/" element={<PrivateRoute />}>
          <Route index element={<DashboardPage />} />
          <Route path="products" element={<ProductPage />} />
          <Route path="pos" element={<PosPage />} />
        </Route>

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;