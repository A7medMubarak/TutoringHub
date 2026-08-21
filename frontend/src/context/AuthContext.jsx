import { createContext, useContext, useState } from 'react';
import { authApi } from '../api';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const stored = localStorage.getItem('user');
    return stored ? JSON.parse(stored) : null;
  });

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
    isTeacher: user?.role === 'Teacher',
    isStudent: user?.role === 'Student',
    loginTeacher,
    loginStudent,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export const useAuth = () => useContext(AuthContext);
