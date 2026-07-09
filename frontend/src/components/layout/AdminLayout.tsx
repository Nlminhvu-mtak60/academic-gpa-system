import React from 'react';
import { Navigate, Outlet, Link, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { useTheme } from '../../contexts/ThemeContext';
import { useLanguage } from '../../contexts/LanguageContext';
import { LayoutDashboard, Users, Bell, FileText, LogOut, Sun, Moon, Globe, ShieldAlert } from 'lucide-react';

export const AdminLayout: React.FC = () => {
  const { isAuthenticated, isLoading, user, logout } = useAuth();
  const { theme, toggleTheme } = useTheme();
  const { language, setLanguage, t } = useLanguage();
  const location = useLocation();

  const handleToggleTheme = () => {
    toggleTheme();
  };

  const handleToggleLanguage = () => {
    const newLang = language === 'vi' ? 'en' : 'vi';
    setLanguage(newLang);
  };

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center bg-gray-50 dark:bg-gray-900">
        <div className="h-10 w-10 animate-spin rounded-full border-4 border-brand-500 border-t-transparent"></div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Admin access guard
  if (user?.role !== 'Admin') {
    return <Navigate to="/dashboard" replace />;
  }

  const initials = user
    ? `${user.lastName.charAt(0)}${user.firstName.charAt(0)}`.toUpperCase()
    : 'AD';

  return (
    <div className="flex h-screen bg-gray-100 dark:bg-gray-950">
      {/* Admin Sidebar Navigation */}
      <aside className="hidden md:flex w-64 flex-col bg-white dark:bg-gray-900 border-r border-gray-200 dark:border-gray-800">
        <div className="flex h-16 items-center gap-2 px-6 border-b border-gray-200 dark:border-gray-800">
          <ShieldAlert className="h-6 w-6 text-red-500" />
          <span className="font-bold text-gray-800 dark:text-white">Admin Console</span>
        </div>

        <nav className="flex-1 space-y-1 p-4">
          <Link
            to="/admin/dashboard"
            className={`flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors ${
              location.pathname === '/admin/dashboard'
                ? 'bg-red-50 text-red-600 dark:bg-red-950/20 dark:text-red-400 border border-red-100 dark:border-red-900/35'
                : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white'
            }`}
          >
            <LayoutDashboard className="h-5 w-5" />
            <span>{t('admin.nav.dashboard')}</span>
          </Link>
          <Link
            to="/admin/students"
            className={`flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors ${
              location.pathname.startsWith('/admin/students')
                ? 'bg-red-50 text-red-600 dark:bg-red-950/20 dark:text-red-400 border border-red-100 dark:border-red-900/35'
                : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white'
            }`}
          >
            <Users className="h-5 w-5" />
            <span>{t('admin.nav.students')}</span>
          </Link>
          <Link
            to="/admin/notifications"
            className={`flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors ${
              location.pathname === '/admin/notifications'
                ? 'bg-red-50 text-red-600 dark:bg-red-950/20 dark:text-red-400 border border-red-100 dark:border-red-900/35'
                : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white'
            }`}
          >
            <Bell className="h-5 w-5" />
            <span>{t('admin.nav.notifications')}</span>
          </Link>
          <Link
            to="/admin/logs"
            className={`flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors ${
              location.pathname === '/admin/logs'
                ? 'bg-red-50 text-red-600 dark:bg-red-950/20 dark:text-red-400 border border-red-100 dark:border-red-900/35'
                : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white'
            }`}
          >
            <FileText className="h-5 w-5" />
            <span>{t('admin.nav.logs')}</span>
          </Link>
        </nav>

        {/* User Sidebar Footer */}
        <div className="p-4 border-t border-gray-200 dark:border-gray-800">
          <div className="flex items-center gap-3 mb-4">
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-red-600 text-white font-bold">
              {initials}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-gray-900 dark:text-white truncate">
                {user?.firstName} {user?.lastName}
              </p>
              <p className="text-xs text-red-500 font-semibold truncate">
                {t('admin')}
              </p>
            </div>
          </div>
          <button
            onClick={() => logout()}
            className="flex w-full items-center gap-3 px-4 py-2 text-sm font-medium text-red-600 hover:bg-red-50 dark:hover:bg-red-950/20 rounded-lg transition-colors"
          >
            <LogOut className="h-5 w-5" />
            <span>{t('logout')}</span>
          </button>
        </div>
      </aside>

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Top Header Navbar */}
        <header className="flex h-16 items-center justify-between px-6 bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-800">
          <div className="font-semibold text-gray-800 dark:text-white md:hidden">
            Admin Console
          </div>
          <div className="hidden md:block"></div>

          {/* Action Toolbar buttons */}
          <div className="flex items-center gap-4">
            {/* Language Switcher */}
            <button
              onClick={handleToggleLanguage}
              className="p-2 rounded-lg text-gray-500 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800 transition-colors"
              title="Switch Language"
            >
              <div className="flex items-center gap-1 text-sm font-medium">
                <Globe className="h-4 w-4" />
                <span>{language.toUpperCase()}</span>
              </div>
            </button>

            {/* Dark/Light mode theme switch */}
            <button
              onClick={handleToggleTheme}
              className="p-2 rounded-lg text-gray-500 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800 transition-colors"
              title="Toggle Theme"
            >
              {theme === 'light' ? <Moon className="h-5 w-5" /> : <Sun className="h-5 w-5" />}
            </button>
          </div>
        </header>

        {/* View Router Outlet wrapper */}
        <main className="flex-1 overflow-y-auto p-6 bg-gray-50 dark:bg-gray-950">
          <Outlet />
        </main>
      </div>
    </div>
  );
};

export default AdminLayout;
