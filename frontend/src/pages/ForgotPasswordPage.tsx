import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { authApi } from '../api/authApi';
import { useLanguage } from '../contexts/LanguageContext';
import { Mail, AlertCircle, CheckCircle2, ArrowLeft } from 'lucide-react';

export const ForgotPasswordPage: React.FC = () => {
  const { t } = useLanguage();

  const [email, setEmail] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email) {
      setError(t('common.required'));
      return;
    }

    setError(null);
    setSuccess(null);
    setSubmitting(true);

    try {
      await authApi.forgotPassword({ email });
      setSuccess("Nếu địa chỉ email được đăng ký trên hệ thống, chúng tôi đã gửi hướng dẫn khôi phục mật khẩu.");
    } catch (err: any) {
      setError("Đã xảy ra lỗi. Vui lòng thử lại sau.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="bg-white dark:bg-gray-800 p-8 rounded-2xl shadow-xl border border-gray-100 dark:border-gray-700">
      <div className="text-center mb-8">
        <h2 className="text-2xl font-bold text-gray-800 dark:text-white">
          {t('forgot.title')}
        </h2>
        <p className="text-sm text-gray-500 dark:text-gray-400 mt-2">
          Nhập email đăng ký của bạn để khôi phục mật khẩu.
        </p>
      </div>

      {error && (
        <div className="mb-6 flex items-start gap-2 bg-red-50 dark:bg-red-950/20 text-red-600 p-3 rounded-lg text-sm border border-red-200 dark:border-red-800/30">
          <AlertCircle className="h-5 w-5 shrink-0" />
          <span>{error}</span>
        </div>
      )}

      {success && (
        <div className="mb-6 flex items-start gap-2 bg-green-50 dark:bg-green-950/20 text-green-600 p-3 rounded-lg text-sm border border-green-200 dark:border-green-800/30">
          <CheckCircle2 className="h-5 w-5 shrink-0" />
          <span>{success}</span>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-5">
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            {t('login.email')}
          </label>
          <div className="relative">
            <span className="absolute inset-y-0 left-0 flex items-center pl-3 text-gray-400">
              <Mail className="h-5 w-5" />
            </span>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full pl-10 pr-4 py-2.5 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500"
              placeholder="name@example.com"
              required
            />
          </div>
        </div>

        <button
          type="submit"
          disabled={submitting}
          className="w-full py-2.5 bg-brand-500 hover:bg-brand-600 disabled:bg-brand-300 text-white font-semibold rounded-lg shadow-lg hover:shadow-xl transition-all"
        >
          {submitting ? "..." : t('forgot.submit')}
        </button>
      </form>

      <div className="text-center mt-6">
        <Link
          to="/login"
          className="inline-flex items-center gap-2 text-sm font-medium text-brand-500 hover:text-brand-600 transition-colors"
        >
          <ArrowLeft className="h-4 w-4" />
          <span>{t('forgot.back')}</span>
        </Link>
      </div>
    </div>
  );
};
export default ForgotPasswordPage;
