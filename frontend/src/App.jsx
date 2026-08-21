import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ThemeProvider } from './context/ThemeContext';
import { ToastProvider } from './context/ToastContext';
import ProtectedRoute from './components/ProtectedRoute';
import TeacherLayout from './components/layout/TeacherLayout';
import StudentLayout from './components/layout/StudentLayout';

import Login from './pages/Login';
import Dashboard from './pages/teacher/Dashboard';
import Students from './pages/teacher/Students';
import StudentDetail from './pages/teacher/StudentDetail';
import Classes from './pages/teacher/Classes';
import ClassDetail from './pages/teacher/ClassDetail';
import Attendance from './pages/teacher/Attendance';
import Quotas from './pages/teacher/Quotas';
import Quizzes from './pages/teacher/Quizzes';
import QuizCreate from './pages/teacher/QuizCreate';
import QuizResults from './pages/teacher/QuizResults';

import MyClasses from './pages/student/MyClasses';
import MyQuizzes from './pages/student/MyQuizzes';
import TakeQuiz from './pages/student/TakeQuiz';
import MyAttendance from './pages/student/MyAttendance';
import MyQuotas from './pages/student/MyQuotas';
import Profile from './pages/student/Profile';

function RootRedirect() {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" replace />;
  return <Navigate to={user.role === 'Teacher' ? '/dashboard' : '/my-classes'} replace />;
}

export default function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <ToastProvider>
          <BrowserRouter>
            <Routes>
              <Route path="/login" element={<Login />} />
              <Route path="/" element={<RootRedirect />} />

              <Route
                element={
                  <ProtectedRoute role="Teacher">
                    <TeacherLayout />
                  </ProtectedRoute>
                }
              >
                <Route path="/dashboard" element={<Dashboard />} />
                <Route path="/students" element={<Students />} />
                <Route path="/students/:id" element={<StudentDetail />} />
                <Route path="/classes" element={<Classes />} />
                <Route path="/classes/:id" element={<ClassDetail />} />
                <Route path="/attendance" element={<Attendance />} />
                <Route path="/quotas" element={<Quotas />} />
                <Route path="/quizzes" element={<Quizzes />} />
                <Route path="/quizzes/new" element={<QuizCreate />} />
                <Route path="/quizzes/:id/results" element={<QuizResults />} />
              </Route>

              <Route
                element={
                  <ProtectedRoute role="Student">
                    <StudentLayout />
                  </ProtectedRoute>
                }
              >
                <Route path="/my-classes" element={<MyClasses />} />
                <Route path="/my-quizzes" element={<MyQuizzes />} />
                <Route path="/my-quizzes/:id" element={<TakeQuiz />} />
                <Route path="/my-attendance" element={<MyAttendance />} />
                <Route path="/my-quotas" element={<MyQuotas />} />
                <Route path="/profile" element={<Profile />} />
              </Route>

              <Route path="*" element={<RootRedirect />} />
            </Routes>
          </BrowserRouter>
        </ToastProvider>
      </AuthProvider>
    </ThemeProvider>
  );
}
