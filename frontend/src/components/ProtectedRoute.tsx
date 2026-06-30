import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '../stores/useAuthStore';

interface Props {
  roles?: string[];
}

export default function ProtectedRoute({ roles }: Props) {
  const accessToken = useAuthStore((s) => s.accessToken);
  const user = useAuthStore((s) => s.user);

  if (!accessToken) return <Navigate to="/login" replace />;
  if (roles?.length && !roles.includes(user?.role ?? '')) {
    return <Navigate to="/" replace />;
  }
  return <Outlet />;
}
