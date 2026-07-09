import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { BookOpen } from 'lucide-react';

export const GuestLayout: React.FC = () => {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center bg-gray-50 dark:bg-gray-900">
        <div className="h-10 w-10 animate-spin rounded-full border-4 border-brand-500 border-t-transparent"></div>
      </div>
    );
  }

  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  return (
    <div className="flex min-h-screen bg-gray-50 dark:bg-gray-900">
      {/* Left Sidebar branding area */}
      <div className="hidden lg:flex w-1/2 flex-col justify-between bg-brand-600 p-12 text-white">
        <div className="flex items-center gap-2 text-xl font-bold">
          <BookOpen className="h-6 w-6" />
          <span>Academic GPA System</span>
        </div>
        
        <div className="space-y-4">
          <h1 className="text-4xl font-extrabold tracking-tight leading-tight">
            Manage your grades and plan academic goals.
          </h1>
          <p className="text-brand-100 text-lg">
            A secure performance tracking application with GenAI advice support.
          </p>
        </div>

        <div className="text-sm text-brand-100">
          © 2026 Academic GPA System. All rights reserved.
        </div>
      </div>

      {/* Right Login/Register forms outlet area */}
      <div className="flex w-full lg:w-1/2 items-center justify-center p-8">
        <div className="w-full max-w-md">
          <Outlet />
        </div>
      </div>
    </div>
  );
};
export default GuestLayout;
