import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '../stores/useAuthStore';

interface Props {
  roles?: string[];
}

const normalizeRoleKey = (value: unknown) => {
  const role = typeof value === 'string' ? value.trim() : '';
  return role.toLowerCase().replace(/^role_/, '');
};

export default function ProtectedRoute({ roles }: Props) {
  const accessToken = useAuthStore((s) => s.accessToken);
  const user = useAuthStore((s) => s.user);
  const userRole = user?.role ?? '';

  if (!accessToken) return <Navigate to="/login" replace />;

  if (roles?.length) {
    const hasRequiredRole = roles.some((role) => normalizeRoleKey(role) === normalizeRoleKey(userRole));
    if (userRole && !hasRequiredRole) {
      return <Navigate to="/" replace />;
    }
  }

  return <Outlet />;
}
