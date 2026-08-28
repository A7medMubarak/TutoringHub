import { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { authApi } from '../api';
import { isTokenExpired } from '../lib/jwt';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const stored = localStorage.getItem('user');
    return stored ? JSON.parse(stored) : null;
  });
  const [isLoading, setIsLoading] = useState(true);

  const clearSession = useCallback(() => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    setUser(null);
  }, []);

  useEffect(() => {
    let cancelled = false;

    const validate = async () => {
      const token = localStorage.getItem('accessToken');

      // Layer 0: No token → not authenticated
      if (!token) {
        clearSession();
        if (!cancelled) setIsLoading(false);
        return;
      }

      // Layer 1: Client-side JWT decode — fast expiry check
      if (isTokenExpired(token)) {
        const refreshToken = localStorage.getItem('refreshToken');
        if (!refreshToken) {
          clearSession();
          if (!cancelled) setIsLoading(false);
          return;
        }
        try {
          const data = await authApi.refresh(refreshToken);
          localStorage.setItem('accessToken', data.accessToken);
          localStorage.setItem('refreshToken', data.refreshToken);
        } catch {
          clearSession();
          if (!cancelled) setIsLoading(false);
          return;
        }
      }

      // Layer 2: Server-side validation via /me
      try {
        const me = await authApi.me();
        const profile = {
          role: me.role === 'Teacher' || Number(me.role) === 0 ? 'Teacher' : 'Student',
          userId: me.userId,
          teacherId: me.teacherId,
          name: me.name,
        };
        localStorage.setItem('user', JSON.stringify(profile));
        if (!cancelled) setUser(profile);
      } catch (err) {
        // 401 = session revoked/invalid, 404 = account deleted
        if (err.response?.status === 401 || err.response?.status === 404) {
          clearSession();
        }
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    };

    validate();
    return () => { cancelled = true; };
  }, [clearSession]);

  const persist = (data) => {
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    const profile = {
      role: data.role === 'Teacher' || Number(data.role) === 0 ? 'Teacher' : 'Student',
      userId: data.userId,
      teacherId: data.teacherId,
      name: data.username || data.phone || null,
    };
    localStorage.setItem('user', JSON.stringify(profile));
    setUser(profile);
    return profile;
  };

  const loginTeacher = async (username, password) => persist(await authApi.loginTeacher(username, password));
  const loginStudent = async (phone, pin) => persist(await authApi.loginStudent(phone, pin));

  const logout = async () => {
    const refreshToken = localStorage.getItem('refreshToken');
    try {
      if (refreshToken) await authApi.logout(refreshToken);
    } catch {
      /* ignore */
    }
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    setUser(null);
  };

  const value = {
    user,
    isAuthenticated: !!user,
    isLoading,
    isTeacher: user?.role === 'Teacher',
    isStudent: user?.role === 'Student',
    loginTeacher,
    loginStudent,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export const useAuth = () => useContext(AuthContext);
