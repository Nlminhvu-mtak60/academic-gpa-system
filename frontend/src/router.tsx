import { createBrowserRouter, Navigate } from 'react-router-dom';
import GuestLayout from './components/layout/GuestLayout';
import StudentLayout from './components/layout/StudentLayout';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import ForgotPasswordPage from './pages/ForgotPasswordPage';
import DashboardPage from './pages/DashboardPage';
import ProfilePage from './pages/ProfilePage';
import AcademicYearsPage from './pages/AcademicYearsPage';
import SemestersPage from './pages/SemestersPage';
import CoursesPage from './pages/CoursesPage';
import GpaPage from './pages/GpaPage';
import StatisticsPage from './pages/StatisticsPage';
import GoalPlannerPage from './pages/GoalPlannerPage';
import SettingsPage from './pages/SettingsPage';
import AiAdvisorDashboard from './pages/AiAdvisorDashboard';
import NotFoundPage from './pages/NotFoundPage';
import AdminLayout from './components/layout/AdminLayout';
import AdminDashboardPage from './pages/AdminDashboardPage';
import AdminStudentManagementPage from './pages/AdminStudentManagementPage';
import AdminStudentDetailPage from './pages/AdminStudentDetailPage';
import AdminNotificationPage from './pages/AdminNotificationPage';
import AdminActivityLogsPage from './pages/AdminActivityLogsPage';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <Navigate to="/dashboard" replace />,
  },
  {
    element: <GuestLayout />,
    children: [
      {
        path: '/login',
        element: <LoginPage />,
      },
      {
        path: '/register',
        element: <RegisterPage />,
      },
      {
        path: '/forgot-password',
        element: <ForgotPasswordPage />,
      },
    ],
  },
  {
    element: <StudentLayout />,
    children: [
      {
        path: '/dashboard',
        element: <DashboardPage />,
      },
      {
        path: '/gpa',
        element: <GpaPage />,
      },
      {
        path: '/statistics',
        element: <StatisticsPage />,
      },
      {
        path: '/goal-planner',
        element: <GoalPlannerPage />,
      },
      {
        path: '/ai-advisor',
        element: <AiAdvisorDashboard />,
      },
      {
        path: '/profile',
        element: <ProfilePage />,
      },
      {
        path: '/academic-years',
        element: <AcademicYearsPage />,
      },
      {
        path: '/academic-years/:yearId/semesters',
        element: <SemestersPage />,
      },
      {
        path: '/semesters/:semesterId/courses',
        element: <CoursesPage />,
      },
      {
        path: '/settings',
        element: <SettingsPage />,
      },
    ],
  },
  {
    element: <AdminLayout />,
    children: [
      {
        path: '/admin/dashboard',
        element: <AdminDashboardPage />,
      },
      {
        path: '/admin/students',
        element: <AdminStudentManagementPage />,
      },
      {
        path: '/admin/students/:id',
        element: <AdminStudentDetailPage />,
      },
      {
        path: '/admin/notifications',
        element: <AdminNotificationPage />,
      },
      {
        path: '/admin/logs',
        element: <AdminActivityLogsPage />,
      },
    ],
  },
  {
    path: '/404',
    element: <NotFoundPage />,
  },
  {
    path: '*',
    element: <Navigate to="/404" replace />,
  },
]);
