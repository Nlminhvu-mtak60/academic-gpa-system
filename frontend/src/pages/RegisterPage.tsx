import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useLanguage } from '../contexts/LanguageContext';
import { KeyRound, Mail, User, AlertCircle, BookOpen, Eye, EyeOff } from 'lucide-react';

export const RegisterPage: React.FC = () => {
  const { register } = useAuth();
  const { t } = useLanguage();
  const navigate = useNavigate();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email || !password || !firstName || !lastName) {
      setError(t('common.required'));
      return;
    }

    setError(null);
    setSubmitting(true);

    try {
      await register({ email, password, firstName, lastName });
      navigate('/dashboard');
    } catch (err: any) {
      const errors = err.response?.data?.errors;
      let msg = "Đăng ký thất bại. Vui lòng kiểm tra lại thông tin đăng ký.";
      if (errors) {
        const firstErrorKey = Object.keys(errors)[0];
        if (firstErrorKey && Array.isArray(errors[firstErrorKey]) && errors[firstErrorKey].length > 0) {
          msg = errors[firstErrorKey][0];
        } else if (typeof errors === 'string') {
          msg = errors;
        } else if (errors.error?.[0]) {
          msg = errors.error[0];
        } else if (errors.Error?.[0]) {
          msg = errors.Error[0];
        }
      }
      setError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="bg-white dark:bg-gray-800 p-8 rounded-2xl shadow-xl border border-gray-100 dark:border-gray-700">
      <div className="text-center mb-8">
        <div className="flex justify-center mb-3 text-brand-500 lg:hidden">
          <BookOpen className="h-10 w-10" />
        </div>
        <h2 className="text-2xl font-bold text-gray-800 dark:text-white">
          {t('register.title')}
        </h2>
      </div>

      {error && (
        <div className="mb-6 flex items-start gap-2 bg-red-50 dark:bg-red-950/20 text-red-600 p-3 rounded-lg text-sm border border-red-200 dark:border-red-800/30">
          <AlertCircle className="h-5 w-5 shrink-0" />
          <span>{error}</span>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="flex gap-4">
          <div className="w-1/2">
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Họ
            </label>
            <input
              type="text"
              value={lastName}
              onChange={(e) => setLastName(e.target.value)}
              className="w-full px-4 py-2.5 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500"
              placeholder="Nguyen"
              required
            />
          </div>
          <div className="w-1/2">
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Tên
            </label>
            <input
              type="text"
              value={firstName}
              onChange={(e) => setFirstName(e.target.value)}
              className="w-full px-4 py-2.5 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500"
              placeholder="Van A"
              required
            />
          </div>
        </div>

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
              placeholder="student@example.com"
              required
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            {t('login.password')}
          </label>
          <div className="relative">
            <span className="absolute inset-y-0 left-0 flex items-center pl-3 text-gray-400">
              <KeyRound className="h-5 w-5" />
            </span>
            <input
              type={showPassword ? 'text' : 'password'}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full pl-10 pr-10 py-2.5 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500"
              placeholder="Min 8 chars, mixed casing, symbols"
              required
            />
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="absolute inset-y-0 right-0 flex items-center pr-3 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
            >
              {showPassword ? <EyeOff className="h-5 w-5" /> : <Eye className="h-5 w-5" />}
            </button>
          </div>
        </div>

        <button
          type="submit"
          disabled={submitting}
          className="w-full mt-4 py-2.5 bg-brand-500 hover:bg-brand-600 disabled:bg-brand-300 text-white font-semibold rounded-lg shadow-lg hover:shadow-xl transition-all"
        >
          {submitting ? "..." : t('register.submit')}
        </button>
      </form>

      <div className="text-center mt-6 text-sm text-gray-600 dark:text-gray-400">
        <Link
          to="/login"
          className="font-medium text-brand-500 hover:text-brand-600 transition-colors"
        >
          {t('register.loginLink')}
        </Link>
      </div>
    </div>
  );
};
export default RegisterPage;
