import React from 'react';
import { Navigate, Outlet, Link, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { useTheme } from '../../contexts/ThemeContext';
import { useLanguage } from '../../contexts/LanguageContext';
import { BookOpen, LogOut, Sun, Moon, Globe, GraduationCap, User as UserIcon, Calendar, BarChart2, Target, Settings as SettingsIcon, Sparkles } from 'lucide-react';
import { studentApi } from '../../api/studentApi';
import { NotificationCenter } from '../notifications/NotificationCenter';

export const StudentLayout: React.FC = () => {
  const { isAuthenticated, isLoading, user, logout, updateUser } = useAuth();
  const { theme, setTheme, toggleTheme } = useTheme();
  const { language, setLanguage, t } = useLanguage();
  const location = useLocation();
  const syncedRef = React.useRef(false);

  React.useEffect(() => {
    if (user && !syncedRef.current) {
      if (user.preferredTheme && (user.preferredTheme === 'light' || user.preferredTheme === 'dark')) {
        setTheme(user.preferredTheme as any);
      }
      if (user.preferredLanguage && (user.preferredLanguage === 'vi' || user.preferredLanguage === 'en')) {
        setLanguage(user.preferredLanguage as any);
      }
      syncedRef.current = true;
    }
  }, [user, theme, language, setTheme, setLanguage]);

  const handleToggleTheme = async () => {
    const newTheme = theme === 'light' ? 'dark' : 'light';
    toggleTheme();
    updateUser({ preferredTheme: newTheme });
    try {
      await studentApi.updatePreferences({
        preferredTheme: newTheme,
        preferredLanguage: language,
      });
    } catch (err) {
      console.error('Failed to sync theme preference:', err);
    }
  };

  const handleToggleLanguage = async () => {
    const newLang = language === 'vi' ? 'en' : 'vi';
    setLanguage(newLang);
    updateUser({ preferredLanguage: newLang });
    try {
      await studentApi.updatePreferences({
        preferredTheme: theme,
        preferredLanguage: newLang,
      });
    } catch (err) {
      console.error('Failed to sync language preference:', err);
    }
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

  // Student access guard
  if (user?.role !== 'Student') {
    return <Navigate to="/admin/dashboard" replace />;
  }

  const initials = user
    ? `${user.lastName.charAt(0)}${user.firstName.charAt(0)}`.toUpperCase()
    : 'US';

  return (
    <div className="flex h-screen bg-gray-100 dark:bg-gray-950">
      {/* Navigation Sidebar */}
      <aside className="hidden md:flex w-64 flex-col bg-white dark:bg-gray-900 border-r border-gray-200 dark:border-gray-800">
        <div className="flex h-16 items-center gap-2 px-6 border-b border-gray-200 dark:border-gray-800">
          <GraduationCap className="h-6 w-6 text-brand-500" />
          <span className="font-bold text-gray-800 dark:text-white">GPA Portal</span>
        </div>

        <nav className="flex-1 space-y-1 p-4">
          <Link
            to="/dashboard"
            className={`flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors ${
              location.pathname === '/dashboard'
                ? 'bg-brand-50 text-brand-600 dark:bg-brand-950/50 dark:text-brand-400'
                : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white'
            }`}
          >
            <BookOpen className="h-5 w-5" />
            <span>{t('dashboard')}</span>
          </Link>
          <Link
            to="/gpa"
            className={`flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors ${
              location.pathname === '/gpa'
                ? 'bg-brand-50 text-brand-600 dark:bg-brand-950/50 dark:text-brand-400'
                : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white'
            }`}
          >
            <GraduationCap className="h-5 w-5" />
            <span>{t('gpa.title')}</span>
          </Link>
          <Link
            to="/statistics"
            className={`flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors ${
              location.pathname === '/statistics'
                ? 'bg-brand-50 text-brand-600 dark:bg-brand-950/50 dark:text-brand-400'
                : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white'
            }`}
          >
            <BarChart2 className="h-5 w-5" />
            <span>{t('nav.statistics')}</span>
          </Link>
          <Link
            to="/goal-planner"
            className={`flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors ${
              location.pathname === '/goal-planner'
                ? 'bg-brand-50 text-brand-600 dark:bg-brand-950/50 dark:text-brand-400'
                : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white'
            }`}
          >
            <Target className="h-5 w-5" />
            <span>{t('nav.goalPlanner')}</span>
          </Link>
          <Link
            to="/ai-advisor"
            className={`flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors ${
              location.pathname === '/ai-advisor'
                ? 'bg-brand-50 text-brand-600 dark:bg-brand-950/50 dark:text-brand-400'
                : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white'
            }`}
          >
            <Sparkles className="h-5 w-5 text-brand-500" />
            <span>{t('nav.aiAdvisor')}</span>
          </Link>
          <Link
            to="/academic-years"
            className={`flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors ${
              location.pathname === '/academic-years'
                ? 'bg-brand-50 text-brand-600 dark:bg-brand-950/50 dark:text-brand-400'
                : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white'
            }`}
          >
            <Calendar className="h-5 w-5" />
            <span>{t('nav.academicYears')}</span>
          </Link>
          <Link
            to="/profile"
            className={`flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors ${
              location.pathname === '/profile'
                ? 'bg-brand-50 text-brand-600 dark:bg-brand-950/50 dark:text-brand-400'
                : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white'
            }`}
          >
            <UserIcon className="h-5 w-5" />
            <span>{t('profile.title')}</span>
          </Link>
          <Link
            to="/settings"
            className={`flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors ${
              location.pathname === '/settings'
                ? 'bg-brand-50 text-brand-600 dark:bg-brand-950/50 dark:text-brand-400'
                : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white'
            }`}
          >
            <SettingsIcon className="h-5 w-5" />
            <span>{t('nav.settings')}</span>
          </Link>
        </nav>

        {/* User Sidebar Footer */}
        <div className="p-4 border-t border-gray-200 dark:border-gray-800">
          <div className="flex items-center gap-3 mb-4">
            {user?.avatarUrl ? (
              <img
                src={user.avatarUrl.startsWith('http') || user.avatarUrl.startsWith('/') ? user.avatarUrl : `/${user.avatarUrl}`}
                alt="Avatar"
                className="h-10 w-10 rounded-full object-cover"
                onError={(e) => {
                  // Fallback if image fails to load
                  (e.target as HTMLElement).style.display = 'none';
                  const sibling = (e.target as HTMLElement).nextElementSibling;
                  if (sibling) (sibling as HTMLElement).style.display = 'flex';
                }}
              />
            ) : null}
            {(!user?.avatarUrl) ? (
              <div className="flex h-10 w-10 items-center justify-center rounded-full bg-brand-500 text-white font-bold">
                {initials}
              </div>
            ) : (
              <div className="hidden h-10 w-10 items-center justify-center rounded-full bg-brand-500 text-white font-bold">
                {initials}
              </div>
            )}
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-gray-900 dark:text-white truncate">
                {user?.firstName} {user?.lastName}
              </p>
              <p className="text-xs text-gray-500 dark:text-gray-400 truncate">
                {user?.email}
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
            GPA Portal
          </div>
          <div className="hidden md:block"></div>

          {/* Action Toolbar buttons */}
          <div className="flex items-center gap-4">
            {/* Notifications Dropdown */}
            <NotificationCenter />

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
export default StudentLayout;
