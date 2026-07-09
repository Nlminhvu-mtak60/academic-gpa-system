import React, { useEffect, useState } from 'react';
import { Sun, Moon, Monitor, Globe, Bell, Mail, Key, Save, AlertCircle, CheckCircle2, Shield, Trash2, Download } from 'lucide-react';
import { settingsApi, UserSettingsDto } from '../api/settingsApi';
import { studentApi } from '../api/studentApi';
import { useLanguage } from '../contexts/LanguageContext';
import { useTheme } from '../contexts/ThemeContext';
import { useAuth } from '../contexts/AuthContext';

export const SettingsPage: React.FC = () => {
  const { t, language, setLanguage } = useLanguage();
  const { theme, setTheme } = useTheme();
  const { updateUser } = useAuth();

  // Settings State
  const [preferences, setPreferences] = useState<UserSettingsDto>({
    preferredLanguage: 'vi',
    preferredTheme: 'light',
    receiveSystem: true,
    receiveAcademic: true,
    receiveGoal: true,
    receiveGpaMilestone: true,
  });

  // Account Form State
  const [email, setEmail] = useState<string>('');
  const [currentPassword, setCurrentPassword] = useState<string>('');
  const [newPassword, setNewPassword] = useState<string>('');
  const [confirmPassword, setConfirmPassword] = useState<string>('');

  // Status Alerts
  const [preferenceStatus, setPreferenceStatus] = useState<{ type: 'success' | 'error'; message: string } | null>(null);
  const [emailStatus, setEmailStatus] = useState<{ type: 'success' | 'error'; message: string } | null>(null);
  const [passwordStatus, setPasswordStatus] = useState<{ type: 'success' | 'error'; message: string } | null>(null);

  // Loading flags
  const [loadingPrefs, setLoadingPrefs] = useState<boolean>(true);
  const [savingPrefs, setSavingPrefs] = useState<boolean>(false);
  const [savingEmail, setSavingEmail] = useState<boolean>(false);
  const [savingPassword, setSavingPassword] = useState<boolean>(false);

  // Load Settings and Profile Email
  useEffect(() => {
    const loadData = async () => {
      try {
        const [settingsRes, profileRes] = await Promise.all([
          settingsApi.getSettings(),
          studentApi.getProfile(),
        ]);

        if (settingsRes.success) {
          setPreferences(settingsRes.data);
        }
        if (profileRes.success) {
          setEmail(profileRes.data.email);
        }
      } catch (err) {
        console.error('Failed to load settings data:', err);
      } finally {
        setLoadingPrefs(false);
      }
    };
    loadData();
  }, []);

  const handlePreferenceSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setSavingPrefs(true);
    setPreferenceStatus(null);
    try {
      const res = await settingsApi.updateSettings(preferences);
      if (res.success) {
        setPreferenceStatus({ type: 'success', message: t('settings.success') });
        // Sync context states immediately
        setLanguage(preferences.preferredLanguage as any);
        setTheme(preferences.preferredTheme as any);
        updateUser({
          preferredLanguage: preferences.preferredLanguage,
          preferredTheme: preferences.preferredTheme,
        });
      } else {
        setPreferenceStatus({ type: 'error', message: res.message || 'Error saving settings' });
      }
    } catch (err: any) {
      console.error(err);
      setPreferenceStatus({ type: 'error', message: err.response?.data?.message || 'Failed to connect to the server.' });
    } finally {
      setSavingPrefs(false);
    }
  };

  const handleEmailSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setSavingEmail(true);
    setEmailStatus(null);
    try {
      const res = await settingsApi.updateEmail(email);
      if (res.success) {
        setEmailStatus({ type: 'success', message: t('settings.emailSuccess') });
        updateUser({ email });
      } else {
        setEmailStatus({ type: 'error', message: res.message || 'Error updating email' });
      }
    } catch (err: any) {
      console.error(err);
      const errors = err.response?.data?.errors;
      const errorMsg = errors?.Email ? errors.Email[0] : (err.response?.data?.message || 'Failed to update email.');
      setEmailStatus({ type: 'error', message: errorMsg });
    } finally {
      setSavingEmail(false);
    }
  };

  const handlePasswordSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setPasswordStatus(null);

    if (newPassword !== confirmPassword) {
      setPasswordStatus({ type: 'error', message: t('profile.passwordMismatch') });
      return;
    }

    setSavingPassword(true);
    try {
      const res = await studentApi.changePassword({ currentPassword, newPassword });
      if (res.success) {
        setPasswordStatus({ type: 'success', message: t('settings.passwordSuccess') });
        setCurrentPassword('');
        setNewPassword('');
        setConfirmPassword('');
      } else {
        setPasswordStatus({ type: 'error', message: res.message || 'Error changing password' });
      }
    } catch (err: any) {
      console.error(err);
      setPasswordStatus({ type: 'error', message: err.response?.data?.message || 'Failed to change password.' });
    } finally {
      setSavingPassword(false);
    }
  };

  if (loadingPrefs) {
    return (
      <div className="flex h-[60vh] items-center justify-center">
        <div className="h-10 w-10 animate-spin rounded-full border-4 border-brand-500 border-t-transparent"></div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-4xl space-y-6">
      {/* Title */}
      <div>
        <h1 className="text-3xl font-extrabold tracking-tight text-gray-900 dark:text-white">
          {t('settings.title')}
        </h1>
        <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
          {t('settings.subtitle')}
        </p>
      </div>

      <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
        {/* Sidebar Nav */}
        <div className="md:col-span-1 space-y-2">
          <div className="rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-4">
            <h2 className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-3 px-2">
              Menu
            </h2>
            <div className="space-y-1">
              <a href="#preferences-section" className="flex items-center gap-3 px-3 py-2 text-sm font-medium text-brand-600 dark:text-brand-400 bg-brand-50/50 dark:bg-brand-950/20 rounded-lg">
                <Monitor className="h-4 w-4" />
                <span>{t('settings.preferences')}</span>
              </a>
              <a href="#notifications-section" className="flex items-center gap-3 px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 dark:text-gray-300 dark:hover:bg-gray-800 rounded-lg">
                <Bell className="h-4 w-4" />
                <span>{t('settings.notifications')}</span>
              </a>
              <a href="#account-section" className="flex items-center gap-3 px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 dark:text-gray-300 dark:hover:bg-gray-800 rounded-lg">
                <Mail className="h-4 w-4" />
                <span>{t('settings.account')}</span>
              </a>
              <a href="#privacy-section" className="flex items-center gap-3 px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 dark:text-gray-300 dark:hover:bg-gray-800 rounded-lg">
                <Shield className="h-4 w-4" />
                <span>Privacy & Data</span>
              </a>
            </div>
          </div>
        </div>

        {/* Content sections */}
        <div className="md:col-span-2 space-y-6">
          {/* Preferences Card */}
          <form id="preferences-section" onSubmit={handlePreferenceSave} className="rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-sm overflow-hidden">
            <div className="p-6 border-b border-gray-100 dark:border-gray-800">
              <h2 className="text-lg font-bold text-gray-900 dark:text-white flex items-center gap-2">
                <Monitor className="h-5 w-5 text-brand-500" />
                <span>{t('settings.preferences')}</span>
              </h2>
              <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                Customize languages and appearance options.
              </p>
            </div>

            <div className="p-6 space-y-6">
              {/* Status Alert */}
              {preferenceStatus && (
                <div className={`flex items-center gap-2 p-3 rounded-lg text-sm ${
                  preferenceStatus.type === 'success'
                    ? 'bg-emerald-50 dark:bg-emerald-950/30 text-emerald-800 dark:text-emerald-300'
                    : 'bg-red-50 dark:bg-red-950/30 text-red-800 dark:text-red-300'
                }`}>
                  {preferenceStatus.type === 'success' ? <CheckCircle2 className="h-5 w-5 flex-shrink-0" /> : <AlertCircle className="h-5 w-5 flex-shrink-0" />}
                  <span>{preferenceStatus.message}</span>
                </div>
              )}

              {/* Theme Picker */}
              <div className="space-y-3">
                <label className="text-sm font-semibold text-gray-800 dark:text-gray-200">
                  Theme Appearance
                </label>
                <div className="grid grid-cols-3 gap-3">
                  {/* Light */}
                  <button
                    type="button"
                    onClick={() => setPreferences(prev => ({ ...prev, preferredTheme: 'light' }))}
                    className={`flex flex-col items-center justify-center p-3 rounded-lg border text-sm font-medium transition-all ${
                      preferences.preferredTheme === 'light'
                        ? 'border-brand-500 bg-brand-50/20 text-brand-700 dark:text-brand-400 ring-2 ring-brand-500/10'
                        : 'border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 text-gray-700 dark:text-gray-300 hover:border-gray-300 dark:hover:border-gray-700'
                    }`}
                  >
                    <Sun className="h-5 w-5 mb-1.5" />
                    <span>{t('theme.light')}</span>
                  </button>

                  {/* Dark */}
                  <button
                    type="button"
                    onClick={() => setPreferences(prev => ({ ...prev, preferredTheme: 'dark' }))}
                    className={`flex flex-col items-center justify-center p-3 rounded-lg border text-sm font-medium transition-all ${
                      preferences.preferredTheme === 'dark'
                        ? 'border-brand-500 bg-brand-50/20 text-brand-700 dark:text-brand-400 ring-2 ring-brand-500/10'
                        : 'border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 text-gray-700 dark:text-gray-300 hover:border-gray-300 dark:hover:border-gray-700'
                    }`}
                  >
                    <Moon className="h-5 w-5 mb-1.5" />
                    <span>{t('theme.dark')}</span>
                  </button>

                  {/* System */}
                  <button
                    type="button"
                    onClick={() => setPreferences(prev => ({ ...prev, preferredTheme: 'system' }))}
                    className={`flex flex-col items-center justify-center p-3 rounded-lg border text-sm font-medium transition-all ${
                      preferences.preferredTheme === 'system'
                        ? 'border-brand-500 bg-brand-50/20 text-brand-700 dark:text-brand-400 ring-2 ring-brand-500/10'
                        : 'border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 text-gray-700 dark:text-gray-300 hover:border-gray-300 dark:hover:border-gray-700'
                    }`}
                  >
                    <Monitor className="h-5 w-5 mb-1.5" />
                    <span>System</span>
                  </button>
                </div>
              </div>

              {/* Language Selector */}
              <div className="space-y-2">
                <label className="text-sm font-semibold text-gray-800 dark:text-gray-200 flex items-center gap-2">
                  <Globe className="h-4 w-4 text-gray-400" />
                  <span>Language / Ngôn ngữ</span>
                </label>
                <select
                  value={preferences.preferredLanguage}
                  onChange={(e) => setPreferences(prev => ({ ...prev, preferredLanguage: e.target.value }))}
                  className="w-full rounded-lg border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-sm text-gray-900 dark:text-white focus:border-brand-500 focus:outline-none focus:ring-1 focus:ring-brand-500"
                >
                  <option value="vi">{t('lang.vi')}</option>
                  <option value="en">{t('lang.en')}</option>
                </select>
              </div>
            </div>

            <div className="px-6 py-4 bg-gray-50 dark:bg-gray-900/50 border-t border-gray-100 dark:border-gray-800 flex justify-end">
              <button
                type="submit"
                disabled={savingPrefs}
                className="flex items-center gap-2 px-4 py-2 rounded-lg bg-brand-500 hover:bg-brand-600 disabled:opacity-50 text-white font-medium text-sm transition-colors"
              >
                <Save className="h-4 w-4" />
                <span>{savingPrefs ? 'Saving...' : t('settings.save')}</span>
              </button>
            </div>
          </form>

          {/* Notifications Card */}
          <form id="notifications-section" onSubmit={handlePreferenceSave} className="rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-sm overflow-hidden">
            <div className="p-6 border-b border-gray-100 dark:border-gray-800">
              <h2 className="text-lg font-bold text-gray-900 dark:text-white flex items-center gap-2">
                <Bell className="h-5 w-5 text-brand-500" />
                <span>{t('settings.notifications')}</span>
              </h2>
              <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                Toggle categories of push alerts and system triggers.
              </p>
            </div>

            <div className="p-6 space-y-4">
              {/* Option: System */}
              <div className="flex items-center justify-between py-2 border-b border-gray-50 dark:border-gray-800/40">
                <div>
                  <p className="text-sm font-semibold text-gray-800 dark:text-gray-200">
                    {t('settings.receiveSystem')}
                  </p>
                  <p className="text-xs text-gray-500">General system notifications and warnings.</p>
                </div>
                <input
                  type="checkbox"
                  checked={preferences.receiveSystem}
                  onChange={(e) => setPreferences(prev => ({ ...prev, receiveSystem: e.target.checked }))}
                  className="h-4 w-4 rounded text-brand-500 border-gray-300 dark:border-gray-700 focus:ring-brand-500 cursor-pointer"
                />
              </div>

              {/* Option: Academic */}
              <div className="flex items-center justify-between py-2 border-b border-gray-50 dark:border-gray-800/40">
                <div>
                  <p className="text-sm font-semibold text-gray-800 dark:text-gray-200">
                    {t('settings.receiveAcademic')}
                  </p>
                  <p className="text-xs text-gray-500">Alerts when course component scores or grades are updated.</p>
                </div>
                <input
                  type="checkbox"
                  checked={preferences.receiveAcademic}
                  onChange={(e) => setPreferences(prev => ({ ...prev, receiveAcademic: e.target.checked }))}
                  className="h-4 w-4 rounded text-brand-500 border-gray-300 dark:border-gray-700 focus:ring-brand-500 cursor-pointer"
                />
              </div>

              {/* Option: Goal */}
              <div className="flex items-center justify-between py-2 border-b border-gray-50 dark:border-gray-800/40">
                <div>
                  <p className="text-sm font-semibold text-gray-800 dark:text-gray-200">
                    {t('settings.receiveGoal')}
                  </p>
                  <p className="text-xs text-gray-500">Notifications on target GPA goal activations and achievements.</p>
                </div>
                <input
                  type="checkbox"
                  checked={preferences.receiveGoal}
                  onChange={(e) => setPreferences(prev => ({ ...prev, receiveGoal: e.target.checked }))}
                  className="h-4 w-4 rounded text-brand-500 border-gray-300 dark:border-gray-700 focus:ring-brand-500 cursor-pointer"
                />
              </div>

              {/* Option: Milestone */}
              <div className="flex items-center justify-between py-2">
                <div>
                  <p className="text-sm font-semibold text-gray-800 dark:text-gray-200">
                    {t('settings.receiveGpaMilestone')}
                  </p>
                  <p className="text-xs text-gray-500">Congratulatory alerts when crossing GPA thresholds (7.0, 8.0, 9.0).</p>
                </div>
                <input
                  type="checkbox"
                  checked={preferences.receiveGpaMilestone}
                  onChange={(e) => setPreferences(prev => ({ ...prev, receiveGpaMilestone: e.target.checked }))}
                  className="h-4 w-4 rounded text-brand-500 border-gray-300 dark:border-gray-700 focus:ring-brand-500 cursor-pointer"
                />
              </div>
            </div>

            <div className="px-6 py-4 bg-gray-50 dark:bg-gray-900/50 border-t border-gray-100 dark:border-gray-800 flex justify-end">
              <button
                type="submit"
                disabled={savingPrefs}
                className="flex items-center gap-2 px-4 py-2 rounded-lg bg-brand-500 hover:bg-brand-600 disabled:opacity-50 text-white font-medium text-sm transition-colors"
              >
                <Save className="h-4 w-4" />
                <span>{savingPrefs ? 'Saving...' : t('settings.save')}</span>
              </button>
            </div>
          </form>

          {/* Account Settings Card */}
          <div id="account-section" className="space-y-6">
            {/* Email Form */}
            <form onSubmit={handleEmailSave} className="rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-sm overflow-hidden">
              <div className="p-6 border-b border-gray-100 dark:border-gray-800">
                <h2 className="text-lg font-bold text-gray-900 dark:text-white flex items-center gap-2">
                  <Mail className="h-5 w-5 text-brand-500" />
                  <span>{t('settings.email')}</span>
                </h2>
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                  Change the primary email associated with your account.
                </p>
              </div>

              <div className="p-6 space-y-4">
                {emailStatus && (
                  <div className={`flex items-center gap-2 p-3 rounded-lg text-sm ${
                    emailStatus.type === 'success'
                      ? 'bg-emerald-50 dark:bg-emerald-950/30 text-emerald-800 dark:text-emerald-300'
                      : 'bg-red-50 dark:bg-red-950/30 text-red-800 dark:text-red-300'
                  }`}>
                    {emailStatus.type === 'success' ? <CheckCircle2 className="h-5 w-5 flex-shrink-0" /> : <AlertCircle className="h-5 w-5 flex-shrink-0" />}
                    <span>{emailStatus.message}</span>
                  </div>
                )}

                <div className="space-y-1">
                  <label className="text-sm font-semibold text-gray-800 dark:text-gray-200">
                    {t('settings.email')}
                  </label>
                  <input
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    required
                    className="w-full rounded-lg border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-sm text-gray-900 dark:text-white focus:border-brand-500 focus:outline-none focus:ring-1 focus:ring-brand-500"
                  />
                </div>
              </div>

              <div className="px-6 py-4 bg-gray-50 dark:bg-gray-900/50 border-t border-gray-100 dark:border-gray-800 flex justify-end">
                <button
                  type="submit"
                  disabled={savingEmail}
                  className="flex items-center gap-2 px-4 py-2 rounded-lg bg-brand-500 hover:bg-brand-600 disabled:opacity-50 text-white font-medium text-sm transition-colors"
                >
                  <Save className="h-4 w-4" />
                  <span>{savingEmail ? 'Updating...' : t('settings.changeEmail')}</span>
                </button>
              </div>
            </form>

            {/* Password Form */}
            <form onSubmit={handlePasswordSave} className="rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-sm overflow-hidden">
              <div className="p-6 border-b border-gray-100 dark:border-gray-800">
                <h2 className="text-lg font-bold text-gray-900 dark:text-white flex items-center gap-2">
                  <Key className="h-5 w-5 text-brand-500" />
                  <span>{t('profile.changePassword')}</span>
                </h2>
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                  Secure your account by creating a new password.
                </p>
              </div>

              <div className="p-6 space-y-4">
                {passwordStatus && (
                  <div className={`flex items-center gap-2 p-3 rounded-lg text-sm ${
                    passwordStatus.type === 'success'
                      ? 'bg-emerald-50 dark:bg-emerald-950/30 text-emerald-800 dark:text-emerald-300'
                      : 'bg-red-50 dark:bg-red-950/30 text-red-800 dark:text-red-300'
                  }`}>
                    {passwordStatus.type === 'success' ? <CheckCircle2 className="h-5 w-5 flex-shrink-0" /> : <AlertCircle className="h-5 w-5 flex-shrink-0" />}
                    <span>{passwordStatus.message}</span>
                  </div>
                )}

                <div className="space-y-1">
                  <label className="text-sm font-semibold text-gray-800 dark:text-gray-200">
                    {t('profile.currentPassword')}
                  </label>
                  <input
                    type="password"
                    value={currentPassword}
                    onChange={(e) => setCurrentPassword(e.target.value)}
                    required
                    className="w-full rounded-lg border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-sm text-gray-900 dark:text-white focus:border-brand-500 focus:outline-none focus:ring-1 focus:ring-brand-500"
                  />
                </div>

                <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                  <div className="space-y-1">
                    <label className="text-sm font-semibold text-gray-800 dark:text-gray-200">
                      {t('profile.newPassword')}
                    </label>
                    <input
                      type="password"
                      value={newPassword}
                      onChange={(e) => setNewPassword(e.target.value)}
                      required
                      className="w-full rounded-lg border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-sm text-gray-900 dark:text-white focus:border-brand-500 focus:outline-none focus:ring-1 focus:ring-brand-500"
                    />
                  </div>

                  <div className="space-y-1">
                    <label className="text-sm font-semibold text-gray-800 dark:text-gray-200">
                      {t('profile.confirmPassword')}
                    </label>
                    <input
                      type="password"
                      value={confirmPassword}
                      onChange={(e) => setConfirmPassword(e.target.value)}
                      required
                      className="w-full rounded-lg border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-sm text-gray-900 dark:text-white focus:border-brand-500 focus:outline-none focus:ring-1 focus:ring-brand-500"
                    />
                  </div>
                </div>
              </div>

              <div className="px-6 py-4 bg-gray-50 dark:bg-gray-900/50 border-t border-gray-100 dark:border-gray-800 flex justify-end">
                <button
                  type="submit"
                  disabled={savingPassword}
                  className="flex items-center gap-2 px-4 py-2 rounded-lg bg-brand-500 hover:bg-brand-600 disabled:opacity-50 text-white font-medium text-sm transition-colors"
                >
                  <Save className="h-4 w-4" />
                  <span>{savingPassword ? 'Changing...' : t('profile.changePassword')}</span>
                </button>
              </div>
            </form>
          </div>

          {/* Privacy Settings Card */}
          <div id="privacy-section" className="rounded-xl border border-red-200 dark:border-red-900/50 bg-white dark:bg-gray-900 shadow-sm overflow-hidden">
            <div className="p-6 border-b border-gray-100 dark:border-gray-800">
              <h2 className="text-lg font-bold text-gray-900 dark:text-white flex items-center gap-2">
                <Shield className="h-5 w-5 text-red-500" />
                <span>Privacy & Data Management</span>
              </h2>
              <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                Manage your personal data, export your information, or delete your account.
              </p>
            </div>

            <div className="p-6 space-y-6">
              {/* Export Data */}
              <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 py-3 border-b border-gray-50 dark:border-gray-800/40">
                <div>
                  <p className="text-sm font-semibold text-gray-800 dark:text-gray-200">
                    Export Personal Data
                  </p>
                  <p className="text-xs text-gray-500 mt-1">
                    Download a copy of all your data (JSON format).
                  </p>
                </div>
                <button
                  type="button"
                  onClick={async () => {
                    try {
                      const response = await fetch('http://localhost:5000/api/v1/privacy/data-export', {
                        headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
                      });
                      if (!response.ok) throw new Error();
                      const blob = await response.blob();
                      const url = window.URL.createObjectURL(blob);
                      const a = document.createElement('a');
                      a.href = url;
                      a.download = `PersonalData_${new Date().toISOString().split('T')[0]}.json`;
                      a.click();
                    } catch (e) {
                      alert('Error exporting data');
                    }
                  }}
                  className="flex items-center gap-2 px-4 py-2 rounded-lg bg-gray-100 hover:bg-gray-200 dark:bg-gray-800 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300 font-medium text-sm transition-colors whitespace-nowrap"
                >
                  <Download className="h-4 w-4" />
                  <span>Request Export</span>
                </button>
              </div>

              {/* Delete Academic History */}
              <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 py-3 border-b border-gray-50 dark:border-gray-800/40">
                <div>
                  <p className="text-sm font-semibold text-gray-800 dark:text-gray-200">
                    Delete Academic History
                  </p>
                  <p className="text-xs text-gray-500 mt-1">
                    Permanently delete all years, semesters, courses, and scores. Your account will remain.
                  </p>
                </div>
                <button
                  type="button"
                  onClick={async () => {
                    if (window.confirm('Are you sure you want to delete all academic history? This action cannot be undone.')) {
                      try {
                        const response = await fetch('http://localhost:5000/api/v1/privacy/academic-history', {
                          method: 'DELETE',
                          headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
                        });
                        if (response.ok) {
                          alert('Academic history deleted successfully.');
                          window.location.reload();
                        }
                      } catch (e) {}
                    }
                  }}
                  className="flex items-center gap-2 px-4 py-2 rounded-lg bg-red-50 hover:bg-red-100 dark:bg-red-950/30 dark:hover:bg-red-900/50 text-red-600 dark:text-red-400 font-medium text-sm transition-colors whitespace-nowrap"
                >
                  <Trash2 className="h-4 w-4" />
                  <span>Delete History</span>
                </button>
              </div>

              {/* Delete Account */}
              <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 py-3">
                <div>
                  <p className="text-sm font-semibold text-gray-800 dark:text-gray-200">
                    Delete Account
                  </p>
                  <p className="text-xs text-gray-500 mt-1">
                    Permanently delete your account and all associated data.
                  </p>
                </div>
                <button
                  type="button"
                  onClick={async () => {
                    if (window.confirm('Are you absolutely sure you want to delete your account? This will erase all data completely.')) {
                      try {
                        const response = await fetch('http://localhost:5000/api/v1/privacy/account', {
                          method: 'DELETE',
                          headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
                        });
                        if (response.ok) {
                          localStorage.clear();
                          window.location.href = '/login';
                        }
                      } catch (e) {}
                    }
                  }}
                  className="flex items-center gap-2 px-4 py-2 rounded-lg bg-red-600 hover:bg-red-700 text-white font-medium text-sm transition-colors whitespace-nowrap"
                >
                  <Trash2 className="h-4 w-4" />
                  <span>Delete Account</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
export default SettingsPage;
