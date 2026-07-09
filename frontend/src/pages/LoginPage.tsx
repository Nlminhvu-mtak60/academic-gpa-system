import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useLanguage } from '../contexts/LanguageContext';
import { KeyRound, Mail, AlertCircle, BookOpen } from 'lucide-react';

export const LoginPage: React.FC = () => {
  const { login, googleLogin } = useAuth();
  const { t } = useLanguage();
  const navigate = useNavigate();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email || !password) {
      setError(t('common.required'));
      return;
    }

    setError(null);
    setSubmitting(true);

    try {
      const loggedInUser = await login({ email, password });
      if (loggedInUser.role === 'Admin') {
        navigate('/admin/dashboard');
      } else {
        navigate('/dashboard');
      }
    } catch (err: any) {
      const msg = err.response?.data?.errors?.credentials?.[0] 
        || err.response?.data?.errors?.error?.[0]
        || "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.";
      setError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  const googleClientId = (import.meta as any).env.VITE_GOOGLE_CLIENT_ID;

  const handleGoogleCredentialResponse = async (response: any) => {
    setError(null);
    setSubmitting(true);
    try {
      const loggedInUser = await googleLogin(response.credential);
      if (loggedInUser.role === 'Admin') {
        navigate('/admin/dashboard');
      } else {
        navigate('/dashboard');
      }
    } catch (err: any) {
      setError("Xác thực Google thất bại.");
    } finally {
      setSubmitting(false);
    }
  };

  useEffect(() => {
    if (!googleClientId) return;

    let checkGoogle: any;
    const initGoogle = () => {
      const g = (window as any).google;
      if (g) {
        clearInterval(checkGoogle);
        g.accounts.id.initialize({
          client_id: googleClientId,
          callback: handleGoogleCredentialResponse,
        });
        const container = document.getElementById("google-signin-button");
        if (container) {
          g.accounts.id.renderButton(container, {
            theme: "outline",
            size: "large",
            width: 320,
          });
        }
      }
    };

    checkGoogle = setInterval(initGoogle, 200);
    return () => clearInterval(checkGoogle);
  }, [googleClientId]);

  return (
    <>
      <div className="bg-white dark:bg-gray-800 p-8 rounded-2xl shadow-xl border border-gray-100 dark:border-gray-700">
        <div className="text-center mb-8">
          <div className="flex justify-center mb-3 text-brand-500 lg:hidden">
            <BookOpen className="h-10 w-10" />
          </div>
          <h2 className="text-2xl font-bold text-gray-800 dark:text-white">
            {t('login.title')}
          </h2>
        </div>

        {error && (
          <div className="mb-6 flex items-start gap-2 bg-red-50 dark:bg-red-950/20 text-red-600 p-3 rounded-lg text-sm border border-red-200 dark:border-red-800/30">
            <AlertCircle className="h-5 w-5 shrink-0" />
            <span>{error}</span>
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
                className="w-full pl-10 pr-4 py-2.5 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500 transition-shadow"
                placeholder="name@example.com"
                required
              />
            </div>
          </div>

          <div>
            <div className="flex justify-between items-center mb-2">
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                {t('login.password')}
              </label>
              <Link
                to="/forgot-password"
                className="text-sm font-medium text-brand-500 hover:text-brand-600 transition-colors"
              >
                {t('login.forgot')}
              </Link>
            </div>
            <div className="relative">
              <span className="absolute inset-y-0 left-0 flex items-center pl-3 text-gray-400">
                <KeyRound className="h-5 w-5" />
              </span>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full pl-10 pr-4 py-2.5 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500 transition-shadow"
                placeholder="••••••••"
                required
              />
            </div>
          </div>

          <button
            type="submit"
            disabled={submitting}
            className="w-full py-2.5 bg-brand-500 hover:bg-brand-600 disabled:bg-brand-300 text-white font-semibold rounded-lg shadow-lg hover:shadow-xl transition-all"
          >
            {submitting ? "..." : t('login.submit')}
          </button>
        </form>

        <div className="relative my-6">
          <div className="absolute inset-0 flex items-center">
            <div className="w-full border-t border-gray-200 dark:border-gray-700"></div>
          </div>
          <div className="relative flex justify-center text-xs uppercase">
            <span className="bg-white dark:bg-gray-800 px-3 text-gray-500 dark:text-gray-400">Hoặc</span>
          </div>
        </div>

        {!googleClientId ? (
          <div className="w-full text-center py-2.5 px-3 border border-gray-250 dark:border-gray-700 rounded-lg text-sm text-gray-500 bg-gray-50 dark:bg-gray-800/40 select-none">
            Google Sign-In is not configured.
          </div>
        ) : (
          <div className="w-full flex justify-center min-h-[44px]">
            <div id="google-signin-button"></div>
          </div>
        )}

        <div className="text-center mt-6 text-sm text-gray-600 dark:text-gray-400">
          <Link
            to="/register"
            className="font-medium text-brand-500 hover:text-brand-600 transition-colors"
          >
            {t('login.registerLink')}
          </Link>
        </div>
      </div>
    </>
  );
};
export default LoginPage;
