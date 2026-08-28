import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { PageLoader } from './ui';

export default function ProtectedRoute({ children, role }) {
  const { user, isAuthenticated, isLoading } = useAuth();
  if (isLoading) return <PageLoader />;
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (role && user.role !== role) {
    return <Navigate to={user.role === 'Teacher' ? '/dashboard' : '/my-classes'} replace />;
  }
  return children;
}
