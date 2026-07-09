import React, { useEffect, useState, useRef } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useTheme } from '../contexts/ThemeContext';
import { useLanguage } from '../contexts/LanguageContext';
import { studentApi, StudentProfileDetailsDto } from '../api/studentApi';
import {
  User,
  GraduationCap,
  Lock,
  Settings,
  Mail,
  Camera,
  Save,
  Loader2,
  CheckCircle2,
  AlertTriangle,
  Moon,
  Sun,
  Globe,
  ChevronRight,
} from 'lucide-react';

type Tab = 'academic' | 'security' | 'preferences';

interface ToastState {
  type: 'success' | 'error';
  message: string;
}

export const ProfilePage: React.FC = () => {
  const { user, updateUser } = useAuth();
  const { theme, setTheme, toggleTheme } = useTheme();
  const { language, setLanguage, t } = useLanguage();

  const [activeTab, setActiveTab] = useState<Tab>('academic');
  const [profile, setProfile] = useState<StudentProfileDetailsDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmittingProfile, setIsSubmittingProfile] = useState(false);
  const [isSubmittingPassword, setIsSubmittingPassword] = useState(false);
  const [isUploadingAvatar, setIsUploadingAvatar] = useState(false);
  
  // Custom Toast notification state
  const [toast, setToast] = useState<ToastState | null>(null);
  const toastTimeoutRef = useRef<any | null>(null);

  // Field states for Academic Form
  const [studentCode, setStudentCode] = useState('');
  const [universityName, setUniversityName] = useState('');
  const [majorName, setMajorName] = useState('');
  const [enrollmentYear, setEnrollmentYear] = useState(new Date().getFullYear());
  const [totalRequiredCredits, setTotalRequiredCredits] = useState(120);

  // Field states for Password Form
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  // Validation & Error Handling states
  const [validationErrors, setValidationErrors] = useState<Record<string, string[]>>({});
  const [passwordError, setPasswordError] = useState('');

  const fileInputRef = useRef<HTMLInputElement>(null);

  const showToast = (type: 'success' | 'error', message: string) => {
    if (toastTimeoutRef.current) {
      clearTimeout(toastTimeoutRef.current);
    }
    setToast({ type, message });
    toastTimeoutRef.current = setTimeout(() => {
      setToast(null);
    }, 4000);
  };

  useEffect(() => {
    return () => {
      if (toastTimeoutRef.current) clearTimeout(toastTimeoutRef.current);
    };
  }, []);

  // Load student profile details
  const fetchProfile = async () => {
    setIsLoading(true);
    try {
      const response = await studentApi.getProfile();
      if (response.success && response.data) {
        const data = response.data as StudentProfileDetailsDto;
        setProfile(data);
        
        // Sync context preferences if they are returned
        if (data.preferredTheme && data.preferredTheme !== theme) {
          setTheme(data.preferredTheme as any);
        }
        if (data.preferredLanguage && data.preferredLanguage !== language) {
          setLanguage(data.preferredLanguage as any);
        }

        // Initialize academic fields
        if (data.profile) {
          setStudentCode(data.profile.studentCode || '');
          setUniversityName(data.profile.universityName || '');
          setMajorName(data.profile.majorName || '');
          setEnrollmentYear(data.profile.enrollmentYear || new Date().getFullYear());
          setTotalRequiredCredits(data.profile.totalRequiredCredits || 120);
        }
      }
    } catch (err: any) {
      console.error('Failed to load profile details:', err);
      showToast('error', err.response?.data?.message || 'Failed to load profile details.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchProfile();
  }, []);

  // Handle Profile Update submit
  const handleUpdateProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmittingProfile(true);
    setValidationErrors({});

    // Client-side validations
    const errors: Record<string, string[]> = {};
    if (!studentCode.trim()) errors.studentCode = ['Student code is required.'];
    else if (!/^[a-zA-Z0-9]+$/.test(studentCode)) errors.studentCode = ['Student code must be alphanumeric.'];
    if (!universityName.trim()) errors.universityName = ['University name is required.'];
    if (!majorName.trim()) errors.majorName = ['Major name is required.'];
    if (enrollmentYear < 2000 || enrollmentYear > 2100) errors.enrollmentYear = ['Year must be between 2000 and 2100.'];
    if (totalRequiredCredits < 30 || totalRequiredCredits > 300) errors.totalRequiredCredits = ['Required graduation credits must be between 30 and 300.'];

    if (Object.keys(errors).length > 0) {
      setValidationErrors(errors);
      setIsSubmittingProfile(false);
      showToast('error', 'Please correct the validation errors below.');
      return;
    }

    try {
      const response = await studentApi.updateProfile({
        studentCode,
        universityName,
        majorName,
        enrollmentYear,
        totalRequiredCredits,
      });

      if (response.success) {
        showToast('success', t('profile.success'));
        // Reload details to reflect new DB values
        fetchProfile();
      }
    } catch (err: any) {
      console.error('Failed to update academic profile:', err);
      if (err.response?.data?.errors) {
        setValidationErrors(err.response.data.errors);
      }
      showToast('error', err.response?.data?.message || 'Failed to update academic profile.');
    } finally {
      setIsSubmittingProfile(false);
    }
  };

  // Handle Password Update submit
  const handleUpdatePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setPasswordError('');

    if (!currentPassword) {
      setPasswordError('Current password is required.');
      return;
    }
    if (newPassword.length < 6) {
      setPasswordError('New password must be at least 6 characters.');
      return;
    }
    if (newPassword !== confirmPassword) {
      setPasswordError(t('profile.passwordMismatch'));
      return;
    }

    setIsSubmittingPassword(true);
    try {
      const response = await studentApi.changePassword({
        currentPassword,
        newPassword,
      });

      if (response.success) {
        showToast('success', t('profile.success'));
        setCurrentPassword('');
        setNewPassword('');
        setConfirmPassword('');
      }
    } catch (err: any) {
      console.error('Failed to change password:', err);
      setPasswordError(err.response?.data?.message || 'Incorrect current password or invalid password format.');
      showToast('error', err.response?.data?.message || 'Failed to change password.');
    } finally {
      setIsSubmittingPassword(false);
    }
  };

  // Handle Avatar Change
  const handleAvatarClick = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (!files || files.length === 0) return;

    const file = files[0];

    // Constraints check (max size 2MB, formats jpg, png, webp)
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      showToast('error', 'Only JPG, PNG and WebP files are supported.');
      return;
    }

    if (file.size > 2 * 1024 * 1024) {
      showToast('error', 'Image size must be less than 2MB.');
      return;
    }

    setIsUploadingAvatar(true);
    try {
      const response = await studentApi.uploadAvatar(file);
      if (response.success && response.data?.avatarUrl) {
        const newAvatarUrl = response.data.avatarUrl;
        
        // Update user inside context
        updateUser({ avatarUrl: newAvatarUrl });

        // Update local profile state
        setProfile((prev) => prev ? { ...prev, avatarUrl: newAvatarUrl } : null);
        showToast('success', 'Avatar updated successfully.');
      }
    } catch (err: any) {
      console.error('Failed to upload avatar:', err);
      showToast('error', err.response?.data?.message || 'Failed to upload avatar.');
    } finally {
      setIsUploadingAvatar(false);
      if (fileInputRef.current) fileInputRef.current.value = ''; // Reset file input
    }
  };

  // Preference Settings Sync
  const handlePreferenceChange = async (newTheme: 'light' | 'dark', newLang: 'vi' | 'en') => {
    setTheme(newTheme);
    setLanguage(newLang);
    updateUser({ preferredTheme: newTheme, preferredLanguage: newLang });

    try {
      await studentApi.updatePreferences({
        preferredTheme: newTheme,
        preferredLanguage: newLang,
      });
      showToast('success', t('profile.success'));
    } catch (err: any) {
      console.error('Failed to sync display preferences:', err);
      showToast('error', 'Failed to save preferences to server.');
    }
  };

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Loader2 className="h-10 w-10 animate-spin text-brand-500" />
      </div>
    );
  }

  // Display Initials if Avatar is missing
  const initials = user
    ? `${user.lastName.charAt(0)}${user.firstName.charAt(0)}`.toUpperCase()
    : 'US';

  const avatarUrlParsed = profile?.avatarUrl
    ? (profile.avatarUrl.startsWith('http') || profile.avatarUrl.startsWith('/') ? profile.avatarUrl : `/${profile.avatarUrl}`)
    : null;

  return (
    <div className="mx-auto max-w-6xl space-y-6">
      {/* Toast Notifications popup overlay */}
      {toast && (
        <div className="fixed top-4 right-4 z-50 flex items-center gap-3 rounded-xl border border-gray-200 dark:border-gray-800 bg-white/90 dark:bg-gray-900/90 backdrop-blur-md p-4 shadow-xl animate-in slide-in-from-top-5 duration-300">
          {toast.type === 'success' ? (
            <CheckCircle2 className="h-5 w-5 text-green-500" />
          ) : (
            <AlertTriangle className="h-5 w-5 text-red-500" />
          )}
          <span className="text-sm font-medium text-gray-800 dark:text-gray-100">{toast.message}</span>
        </div>
      )}

      {/* Main Settings Header */}
      <div>
        <h1 className="text-3xl font-extrabold text-gray-900 dark:text-white tracking-tight">
          {t('profile.title')}
        </h1>
        <p className="text-gray-500 dark:text-gray-400 mt-1">
          {t('profile.subtitle')}
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6 items-start">
        {/* Left Side menu Navigation / Small Card */}
        <div className="lg:col-span-1 space-y-6">
          <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-2xl shadow-sm overflow-hidden p-6 text-center">
            {/* Avatar Group */}
            <div className="relative mx-auto h-24 w-24 mb-4 group cursor-pointer" onClick={handleAvatarClick}>
              {avatarUrlParsed ? (
                <img
                  src={avatarUrlParsed}
                  alt="Student Avatar"
                  className="h-24 w-24 rounded-full object-cover border-2 border-brand-500 shadow-md group-hover:opacity-75 transition-opacity"
                />
              ) : (
                <div className="flex h-24 w-24 items-center justify-center rounded-full bg-brand-500 text-white text-3xl font-bold shadow-md group-hover:opacity-75 transition-opacity border-2 border-brand-500">
                  {initials}
                </div>
              )}
              {/* Camera Upload hover Overlay */}
              <div className="absolute inset-0 flex items-center justify-center bg-black/40 rounded-full opacity-0 group-hover:opacity-100 transition-opacity">
                {isUploadingAvatar ? (
                  <Loader2 className="h-6 w-6 text-white animate-spin" />
                ) : (
                  <Camera className="h-6 w-6 text-white" />
                )}
              </div>
              <input
                type="file"
                ref={fileInputRef}
                className="hidden"
                accept="image/jpeg,image/png,image/webp"
                onChange={handleFileChange}
              />
            </div>

            <h3 className="text-lg font-bold text-gray-900 dark:text-white truncate">
              {profile?.firstName} {profile?.lastName}
            </h3>
            <p className="text-xs text-gray-500 dark:text-gray-400 mt-1 flex items-center justify-center gap-1">
              <Mail className="h-3 w-3" />
              <span className="truncate">{profile?.email}</span>
            </p>

            <span className="inline-flex mt-3 px-3 py-1 rounded-full text-xs font-semibold bg-brand-50 text-brand-600 dark:bg-brand-950/40 dark:text-brand-400">
              {user?.role === 'Admin' ? t('admin') : t('student')}
            </span>
          </div>

          {/* Navigation vertical list */}
          <nav className="flex flex-col bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-2xl p-2 shadow-sm">
            <button
              onClick={() => setActiveTab('academic')}
              className={`flex items-center justify-between px-4 py-3 rounded-xl text-sm font-medium transition-colors ${
                activeTab === 'academic'
                  ? 'bg-brand-50 text-brand-600 dark:bg-brand-950/50 dark:text-brand-400'
                  : 'text-gray-600 hover:bg-gray-50 dark:text-gray-400 dark:hover:bg-gray-800'
              }`}
            >
              <div className="flex items-center gap-3">
                <GraduationCap className="h-5 w-5" />
                <span>{t('profile.academicInfo')}</span>
              </div>
              <ChevronRight className="h-4 w-4 opacity-50" />
            </button>

            <button
              onClick={() => setActiveTab('security')}
              className={`flex items-center justify-between px-4 py-3 rounded-xl text-sm font-medium transition-colors ${
                activeTab === 'security'
                  ? 'bg-brand-50 text-brand-600 dark:bg-brand-950/50 dark:text-brand-400'
                  : 'text-gray-600 hover:bg-gray-50 dark:text-gray-400 dark:hover:bg-gray-800'
              }`}
            >
              <div className="flex items-center gap-3">
                <Lock className="h-5 w-5" />
                <span>{t('profile.changePassword')}</span>
              </div>
              <ChevronRight className="h-4 w-4 opacity-50" />
            </button>

            <button
              onClick={() => setActiveTab('preferences')}
              className={`flex items-center justify-between px-4 py-3 rounded-xl text-sm font-medium transition-colors ${
                activeTab === 'preferences'
                  ? 'bg-brand-50 text-brand-600 dark:bg-brand-950/50 dark:text-brand-400'
                  : 'text-gray-600 hover:bg-gray-50 dark:text-gray-400 dark:hover:bg-gray-800'
              }`}
            >
              <div className="flex items-center gap-3">
                <Settings className="h-5 w-5" />
                <span>{t('profile.preferences')}</span>
              </div>
              <ChevronRight className="h-4 w-4 opacity-50" />
            </button>
          </nav>
        </div>

        {/* Right Side Settings Forms Panels */}
        <div className="lg:col-span-3">
          {activeTab === 'academic' && (
            <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-2xl shadow-sm p-6 space-y-6">
              <div className="border-b border-gray-100 dark:border-gray-800 pb-4">
                <h2 className="text-xl font-bold text-gray-900 dark:text-white">
                  {t('profile.academicInfo')}
                </h2>
                <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                  Cập nhật các thông tin liên quan đến mã số sinh viên, tên trường và chuyên ngành.
                </p>
              </div>

              <form onSubmit={handleUpdateProfile} className="space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  {/* Student Code */}
                  <div className="space-y-1.5">
                    <label className="text-sm font-semibold text-gray-700 dark:text-gray-300">
                      {t('profile.studentCode')}
                    </label>
                    <input
                      type="text"
                      value={studentCode}
                      onChange={(e) => setStudentCode(e.target.value)}
                      className={`w-full px-4 py-2.5 rounded-xl border bg-gray-50/50 dark:bg-gray-950/50 text-gray-950 dark:text-white transition-colors focus:ring-2 focus:ring-brand-500/25 focus:border-brand-500 outline-none ${
                        validationErrors.studentCode ? 'border-red-500' : 'border-gray-200 dark:border-gray-800'
                      }`}
                      placeholder="e.g. B21DCCN123"
                    />
                    {validationErrors.studentCode && (
                      <p className="text-xs text-red-500 mt-1 font-medium">{validationErrors.studentCode[0]}</p>
                    )}
                  </div>

                  {/* University Name */}
                  <div className="space-y-1.5">
                    <label className="text-sm font-semibold text-gray-700 dark:text-gray-300">
                      {t('profile.university')}
                    </label>
                    <input
                      type="text"
                      value={universityName}
                      onChange={(e) => setUniversityName(e.target.value)}
                      className={`w-full px-4 py-2.5 rounded-xl border bg-gray-50/50 dark:bg-gray-950/50 text-gray-950 dark:text-white transition-colors focus:ring-2 focus:ring-brand-500/25 focus:border-brand-500 outline-none ${
                        validationErrors.universityName ? 'border-red-500' : 'border-gray-200 dark:border-gray-800'
                      }`}
                      placeholder="e.g. PTIT"
                    />
                    {validationErrors.universityName && (
                      <p className="text-xs text-red-500 mt-1 font-medium">{validationErrors.universityName[0]}</p>
                    )}
                  </div>

                  {/* Major Name */}
                  <div className="space-y-1.5">
                    <label className="text-sm font-semibold text-gray-700 dark:text-gray-300">
                      {t('profile.major')}
                    </label>
                    <input
                      type="text"
                      value={majorName}
                      onChange={(e) => setMajorName(e.target.value)}
                      className={`w-full px-4 py-2.5 rounded-xl border bg-gray-50/50 dark:bg-gray-950/50 text-gray-950 dark:text-white transition-colors focus:ring-2 focus:ring-brand-500/25 focus:border-brand-500 outline-none ${
                        validationErrors.majorName ? 'border-red-500' : 'border-gray-200 dark:border-gray-800'
                      }`}
                      placeholder="e.g. Information Technology"
                    />
                    {validationErrors.majorName && (
                      <p className="text-xs text-red-500 mt-1 font-medium">{validationErrors.majorName[0]}</p>
                    )}
                  </div>

                  {/* Enrollment Year */}
                  <div className="space-y-1.5">
                    <label className="text-sm font-semibold text-gray-700 dark:text-gray-300">
                      {t('profile.enrollmentYear')}
                    </label>
                    <input
                      type="number"
                      value={enrollmentYear}
                      onChange={(e) => setEnrollmentYear(parseInt(e.target.value) || 0)}
                      className={`w-full px-4 py-2.5 rounded-xl border bg-gray-50/50 dark:bg-gray-950/50 text-gray-950 dark:text-white transition-colors focus:ring-2 focus:ring-brand-500/25 focus:border-brand-500 outline-none ${
                        validationErrors.enrollmentYear ? 'border-red-500' : 'border-gray-200 dark:border-gray-800'
                      }`}
                    />
                    {validationErrors.enrollmentYear && (
                      <p className="text-xs text-red-500 mt-1 font-medium">{validationErrors.enrollmentYear[0]}</p>
                    )}
                  </div>

                  {/* Required graduation credits */}
                  <div className="space-y-1.5">
                    <label className="text-sm font-semibold text-gray-700 dark:text-gray-300">
                      {t('profile.requiredCredits')}
                    </label>
                    <input
                      type="number"
                      value={totalRequiredCredits}
                      onChange={(e) => setTotalRequiredCredits(parseInt(e.target.value) || 0)}
                      className={`w-full px-4 py-2.5 rounded-xl border bg-gray-50/50 dark:bg-gray-950/50 text-gray-950 dark:text-white transition-colors focus:ring-2 focus:ring-brand-500/25 focus:border-brand-500 outline-none ${
                        validationErrors.totalRequiredCredits ? 'border-red-500' : 'border-gray-200 dark:border-gray-800'
                      }`}
                    />
                    {validationErrors.totalRequiredCredits && (
                      <p className="text-xs text-red-500 mt-1 font-medium">{validationErrors.totalRequiredCredits[0]}</p>
                    )}
                  </div>
                </div>

                <div className="flex justify-end pt-4 border-t border-gray-100 dark:border-gray-800">
                  <button
                    type="submit"
                    disabled={isSubmittingProfile}
                    className="flex items-center gap-2 px-6 py-2.5 rounded-xl font-semibold bg-brand-500 text-white hover:bg-brand-600 active:bg-brand-700 transition-colors shadow-sm disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {isSubmittingProfile ? (
                      <Loader2 className="h-5 w-5 animate-spin" />
                    ) : (
                      <Save className="h-5 w-5" />
                    )}
                    <span>{t('profile.save')}</span>
                  </button>
                </div>
              </form>
            </div>
          )}

          {activeTab === 'security' && (
            <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-2xl shadow-sm p-6 space-y-6">
              <div className="border-b border-gray-100 dark:border-gray-800 pb-4">
                <h2 className="text-xl font-bold text-gray-900 dark:text-white">
                  {t('profile.changePassword')}
                </h2>
                <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                  Đảm bảo tài khoản của bạn được bảo mật bằng cách cập nhật mật khẩu định kỳ.
                </p>
              </div>

              {passwordError && (
                <div className="flex items-start gap-3 rounded-xl border border-red-200 bg-red-50/50 p-4 dark:border-red-900/50 dark:bg-red-950/20 text-red-600 dark:text-red-400">
                  <AlertTriangle className="h-5 w-5 shrink-0 mt-0.5" />
                  <span className="text-sm font-medium">{passwordError}</span>
                </div>
              )}

              <form onSubmit={handleUpdatePassword} className="space-y-6">
                <div className="space-y-4 max-w-md">
                  {/* Current Password */}
                  <div className="space-y-1.5">
                    <label className="text-sm font-semibold text-gray-700 dark:text-gray-300">
                      {t('profile.currentPassword')}
                    </label>
                    <input
                      type="password"
                      value={currentPassword}
                      onChange={(e) => setCurrentPassword(e.target.value)}
                      className="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-800 bg-gray-50/50 dark:bg-gray-950/50 text-gray-950 dark:text-white transition-colors focus:ring-2 focus:ring-brand-500/25 focus:border-brand-500 outline-none"
                    />
                  </div>

                  {/* New Password */}
                  <div className="space-y-1.5">
                    <label className="text-sm font-semibold text-gray-700 dark:text-gray-300">
                      {t('profile.newPassword')}
                    </label>
                    <input
                      type="password"
                      value={newPassword}
                      onChange={(e) => setNewPassword(e.target.value)}
                      className="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-800 bg-gray-50/50 dark:bg-gray-950/50 text-gray-950 dark:text-white transition-colors focus:ring-2 focus:ring-brand-500/25 focus:border-brand-500 outline-none"
                      placeholder="At least 6 characters"
                    />
                  </div>

                  {/* Confirm New Password */}
                  <div className="space-y-1.5">
                    <label className="text-sm font-semibold text-gray-700 dark:text-gray-300">
                      {t('profile.confirmPassword')}
                    </label>
                    <input
                      type="password"
                      value={confirmPassword}
                      onChange={(e) => setConfirmPassword(e.target.value)}
                      className="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-800 bg-gray-50/50 dark:bg-gray-950/50 text-gray-950 dark:text-white transition-colors focus:ring-2 focus:ring-brand-500/25 focus:border-brand-500 outline-none"
                    />
                  </div>
                </div>

                <div className="flex justify-end pt-4 border-t border-gray-100 dark:border-gray-800">
                  <button
                    type="submit"
                    disabled={isSubmittingPassword}
                    className="flex items-center gap-2 px-6 py-2.5 rounded-xl font-semibold bg-brand-500 text-white hover:bg-brand-600 active:bg-brand-700 transition-colors shadow-sm disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {isSubmittingPassword ? (
                      <Loader2 className="h-5 w-5 animate-spin" />
                    ) : (
                      <Lock className="h-5 w-5" />
                    )}
                    <span>{t('profile.changePassword')}</span>
                  </button>
                </div>
              </form>
            </div>
          )}

          {activeTab === 'preferences' && (
            <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-2xl shadow-sm p-6 space-y-6">
              <div className="border-b border-gray-100 dark:border-gray-800 pb-4">
                <h2 className="text-xl font-bold text-gray-900 dark:text-white">
                  {t('profile.preferences')}
                </h2>
                <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                  Thay đổi chủ đề hiển thị và ngôn ngữ giao diện của hệ thống.
                </p>
              </div>

              <div className="space-y-6 max-w-lg">
                {/* Theme selection panel */}
                <div className="space-y-2">
                  <label className="text-sm font-semibold text-gray-700 dark:text-gray-300">
                    Chủ đề giao diện (Theme)
                  </label>
                  <div className="grid grid-cols-2 gap-4">
                    <button
                      onClick={() => handlePreferenceChange('light', language)}
                      className={`flex items-center gap-3 p-4 rounded-xl border text-sm font-medium transition-all ${
                        theme === 'light'
                          ? 'border-brand-500 bg-brand-50/50 text-brand-600 dark:bg-brand-950/20'
                          : 'border-gray-200 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800 text-gray-700 dark:text-gray-300'
                      }`}
                    >
                      <Sun className="h-5 w-5" />
                      <span>{t('theme.light')}</span>
                    </button>

                    <button
                      onClick={() => handlePreferenceChange('dark', language)}
                      className={`flex items-center gap-3 p-4 rounded-xl border text-sm font-medium transition-all ${
                        theme === 'dark'
                          ? 'border-brand-500 bg-brand-50/50 text-brand-600 dark:bg-brand-950/20'
                          : 'border-gray-200 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800 text-gray-700 dark:text-gray-300'
                      }`}
                    >
                      <Moon className="h-5 w-5" />
                      <span>{t('theme.dark')}</span>
                    </button>
                  </div>
                </div>

                {/* Language selection panel */}
                <div className="space-y-2">
                  <label className="text-sm font-semibold text-gray-700 dark:text-gray-300">
                    Ngôn ngữ hiển thị (Language)
                  </label>
                  <div className="grid grid-cols-2 gap-4">
                    <button
                      onClick={() => handlePreferenceChange(theme === 'system' ? 'light' : theme, 'vi')}
                      className={`flex items-center gap-3 p-4 rounded-xl border text-sm font-medium transition-all ${
                        language === 'vi'
                          ? 'border-brand-500 bg-brand-50/50 text-brand-600 dark:bg-brand-950/20'
                          : 'border-gray-200 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800 text-gray-700 dark:text-gray-300'
                      }`}
                    >
                      <Globe className="h-5 w-5" />
                      <span>{t('lang.vi')}</span>
                    </button>

                    <button
                      onClick={() => handlePreferenceChange(theme === 'system' ? 'light' : theme, 'en')}
                      className={`flex items-center gap-3 p-4 rounded-xl border text-sm font-medium transition-all ${
                        language === 'en'
                          ? 'border-brand-500 bg-brand-50/50 text-brand-600 dark:bg-brand-950/20'
                          : 'border-gray-200 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800 text-gray-700 dark:text-gray-300'
                      }`}
                    >
                      <Globe className="h-5 w-5" />
                      <span>{t('lang.en')}</span>
                    </button>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ProfilePage;
