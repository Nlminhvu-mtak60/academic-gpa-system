import React from 'react';
import { Link } from 'react-router-dom';
import { AlertCircle, ArrowLeft } from 'lucide-react';

export const NotFoundPage: React.FC = () => {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-gray-50 dark:bg-gray-900 px-4 text-center">
      <div className="mb-4 text-brand-500">
        <AlertCircle className="h-16 w-16" />
      </div>
      <h1 className="text-4xl font-extrabold text-gray-900 dark:text-white sm:text-5xl">
        404 - Trang Không Tìm Thấy
      </h1>
      <p className="mt-2 text-lg text-gray-500 dark:text-gray-400">
        Đường dẫn bạn truy cập không tồn tại hoặc đã bị gỡ bỏ.
      </p>
      <div className="mt-8">
        <Link
          to="/"
          className="inline-flex items-center gap-2 px-5 py-3 bg-brand-500 hover:bg-brand-600 text-white font-semibold rounded-lg shadow-md hover:shadow-lg transition-all"
        >
          <ArrowLeft className="h-5 w-5" />
          <span>Về trang chủ</span>
        </Link>
      </div>
    </div>
  );
};
export default NotFoundPage;
