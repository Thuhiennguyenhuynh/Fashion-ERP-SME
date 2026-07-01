import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import MainLayout from './layouts/MainLayout';
import ProtectedRoute from './components/ProtectedRoute';
import DashboardPage from './pages/Dashboard/DashboardPage';
import LoginPage from './pages/Auth/LoginPage';
import ProductList from './pages/Products/ProductList';
import InventoryPage from './pages/InventoryPage';
import POSPage from './pages/POS/POSPage';
import CustomersPage from './pages/CustomersPage';
import ReportsPage from './pages/Reports/ReportsPage';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />

        <Route element={<ProtectedRoute />}>
          <Route path="/" element={<MainLayout />}>
            <Route index element={<DashboardPage />} />
            <Route path="products" element={<ProductList />} />
            <Route path="inventory" element={<InventoryPage />} />
            <Route path="customers" element={<CustomersPage />} />
            <Route path="reports" element={<ReportsPage />} />
            <Route element={<ProtectedRoute roles={['Admin', 'Manager', 'Sales']} />}>
              <Route path="pos" element={<POSPage />} />
            </Route>
          </Route>
        </Route>

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;