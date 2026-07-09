import React, { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { adminApi, AdminStudentDetailDto, EditStudentInfoRequest } from '../api/adminApi';
import { useLanguage } from '../contexts/LanguageContext';
import { ArrowLeft, User, Key, Save, AlertCircle, Copy, Check } from 'lucide-react';

export const AdminStudentDetailPage: React.FC = () => {
  const { t } = useLanguage();
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [student, setStudent] = useState<AdminStudentDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Edit form states
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [studentCode, setStudentCode] = useState('');
  const [universityName, setUniversityName] = useState('');
  const [majorName, setMajorName] = useState('');
  const [enrollmentYear, setEnrollmentYear] = useState(2024);
  const [totalRequiredCredits, setTotalRequiredCredits] = useState(120);
  const [saving, setSaving] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);

  // Password reset states
  const [tempPassword, setTempPassword] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);

  const fetchDetails = async () => {
    if (!id) return;
    try {
      setLoading(true);
      const data = await adminApi.getStudentDetails(id);
      setStudent(data);

      // Populate form
      setFirstName(data.firstName);
      setLastName(data.lastName);
      setStudentCode(data.studentCode || '');
      setUniversityName(data.universityName || '');
      setMajorName(data.majorName || '');
      setEnrollmentYear(data.enrollmentYear || 2024);
      setTotalRequiredCredits(data.totalRequiredCredits || 120);
    } catch (err) {
      console.error('Failed to load student details:', err);
      setError('Could not retrieve student details.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDetails();
  }, [id]);

  const handleEditSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id || !student) return;

    try {
      setSaving(true);
      setSaveSuccess(false);
      const request: EditStudentInfoRequest = {
        firstName,
        lastName,
        studentCode,
        universityName,
        majorName,
        enrollmentYear,
        totalRequiredCredits,
      };
      await adminApi.editStudentInfo(id, request);
      setSaveSuccess(true);
      
      // Update student details state locally
      setStudent((prev) =>
        prev
          ? {
              ...prev,
              firstName,
              lastName,
              studentCode,
              universityName,
              majorName,
              enrollmentYear,
              totalRequiredCredits,
            }
          : null
      );
    } catch (err) {
      console.error('Failed to update student info:', err);
    } finally {
      setSaving(false);
    }
  };

  const handlePasswordReset = async () => {
    if (!id || !student) return;
    if (!window.confirm(`Are you sure you want to reset the password for ${student.firstName} ${student.lastName}?`)) return;

    try {
      const tempPass = await adminApi.resetPassword(id);
      setTempPassword(tempPass);
      setCopied(false);
    } catch (err) {
      console.error('Failed to reset password:', err);
    }
  };

  const handleCopyPassword = () => {
    if (!tempPassword) return;
    navigator.clipboard.writeText(tempPassword);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  if (loading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-red-500 border-t-transparent"></div>
      </div>
    );
  }

  if (error || !student) {
    return (
      <div className="rounded-xl bg-red-50 dark:bg-red-950/20 border border-red-200 dark:border-red-900/35 p-6 text-center text-red-600 dark:text-red-400">
        {error || 'Error loading student details.'}
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header and Back Link */}
      <div className="flex items-center gap-4">
        <Link
          to="/admin/students"
          className="p-2 border border-gray-200 dark:border-gray-800 rounded-lg text-gray-500 hover:text-gray-900 dark:hover:text-white bg-white dark:bg-gray-950 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
        >
          <ArrowLeft className="h-5 w-5" />
        </Link>
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
            {student.lastName} {student.firstName}
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400">
            Student details and profile administrator tools
          </p>
        </div>
      </div>

      {/* Grid of Profile Details & Statistics */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        {/* GPA & Status Summary Card */}
        <div className="rounded-2xl bg-white dark:bg-gray-900 p-6 shadow-sm border border-gray-100 dark:border-gray-800 space-y-6 flex flex-col justify-between">
          <div className="space-y-4">
            <div className="flex items-center gap-3">
              <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-red-50 text-red-600 dark:bg-red-950/30 dark:text-red-400">
                <User className="h-6 w-6" />
              </div>
              <div>
                <div className="text-sm font-semibold text-gray-900 dark:text-white">{student.email}</div>
                <div className="text-xs text-gray-400">Account Username</div>
              </div>
            </div>

            <div className="border-t border-gray-100 dark:border-gray-800 pt-4 space-y-3">
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Account Status:</span>
                {student.isActive ? (
                  <span className="font-semibold text-green-600 dark:text-green-400">Active</span>
                ) : (
                  <span className="font-semibold text-red-600 dark:text-red-400">Locked / Deactivated</span>
                )}
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Student Code:</span>
                <span className="font-mono font-semibold text-gray-900 dark:text-white">{student.studentCode || 'N/A'}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Major / Program:</span>
                <span className="font-semibold text-gray-900 dark:text-white">{student.majorName || 'N/A'}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Enrollment Year:</span>
                <span className="font-semibold text-gray-900 dark:text-white">{student.enrollmentYear || 'N/A'}</span>
              </div>
            </div>
          </div>

          {/* Academic GPAs display */}
          <div className="bg-gray-50 dark:bg-gray-950 p-4 rounded-xl space-y-3 border border-gray-100 dark:border-gray-800">
            <div className="flex justify-between items-baseline">
              <span className="text-xs text-gray-500 font-medium">Cumulative GPA (10)</span>
              <span className="text-2xl font-bold text-gray-900 dark:text-white">
                {student.cumulativeGpa10 !== null ? student.cumulativeGpa10.toFixed(2) : 'N/A'}
              </span>
            </div>
            <div className="flex justify-between items-baseline border-t border-gray-200/50 dark:border-gray-800/50 pt-2">
              <span className="text-xs text-gray-500 font-medium">Cumulative GPA (4)</span>
              <span className="text-xl font-bold text-gray-900 dark:text-white">
                {student.cumulativeGpa4 !== null ? student.cumulativeGpa4.toFixed(2) : 'N/A'}
              </span>
            </div>
          </div>
        </div>

        {/* Profile Edit Form Card */}
        <div className="lg:col-span-2 rounded-2xl bg-white dark:bg-gray-900 p-6 shadow-sm border border-gray-100 dark:border-gray-800 space-y-4">
          <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-2">{t('admin.students.edit')}</h3>
          
          <form onSubmit={handleEditSubmit} className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1">
                <label className="text-xs font-semibold text-gray-500 uppercase">First Name</label>
                <input
                  type="text"
                  required
                  value={firstName}
                  onChange={(e) => setFirstName(e.target.value)}
                  className="w-full border border-gray-200 dark:border-gray-800 rounded-lg bg-gray-50 dark:bg-gray-950 text-gray-900 dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-500"
                />
              </div>
              <div className="space-y-1">
                <label className="text-xs font-semibold text-gray-500 uppercase">Last Name</label>
                <input
                  type="text"
                  required
                  value={lastName}
                  onChange={(e) => setLastName(e.target.value)}
                  className="w-full border border-gray-200 dark:border-gray-800 rounded-lg bg-gray-50 dark:bg-gray-950 text-gray-900 dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-500"
                />
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1">
                <label className="text-xs font-semibold text-gray-500 uppercase">Student Code</label>
                <input
                  type="text"
                  required
                  value={studentCode}
                  onChange={(e) => setStudentCode(e.target.value)}
                  className="w-full border border-gray-200 dark:border-gray-800 rounded-lg bg-gray-50 dark:bg-gray-950 text-gray-900 dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-500 font-mono"
                />
              </div>
              <div className="space-y-1">
                <label className="text-xs font-semibold text-gray-500 uppercase">University</label>
                <input
                  type="text"
                  required
                  value={universityName}
                  onChange={(e) => setUniversityName(e.target.value)}
                  className="w-full border border-gray-200 dark:border-gray-800 rounded-lg bg-gray-50 dark:bg-gray-950 text-gray-900 dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-500"
                />
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="md:col-span-1 space-y-1">
                <label className="text-xs font-semibold text-gray-500 uppercase">Major</label>
                <input
                  type="text"
                  required
                  value={majorName}
                  onChange={(e) => setMajorName(e.target.value)}
                  className="w-full border border-gray-200 dark:border-gray-800 rounded-lg bg-gray-50 dark:bg-gray-950 text-gray-900 dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-500"
                />
              </div>
              <div className="space-y-1">
                <label className="text-xs font-semibold text-gray-500 uppercase">Enrollment Year</label>
                <input
                  type="number"
                  required
                  value={enrollmentYear}
                  onChange={(e) => setEnrollmentYear(parseInt(e.target.value) || 2024)}
                  className="w-full border border-gray-200 dark:border-gray-800 rounded-lg bg-gray-50 dark:bg-gray-950 text-gray-900 dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-500"
                />
              </div>
              <div className="space-y-1">
                <label className="text-xs font-semibold text-gray-500 uppercase">Required Graduation Credits</label>
                <input
                  type="number"
                  required
                  value={totalRequiredCredits}
                  onChange={(e) => setTotalRequiredCredits(parseInt(e.target.value) || 120)}
                  className="w-full border border-gray-200 dark:border-gray-800 rounded-lg bg-gray-50 dark:bg-gray-950 text-gray-900 dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-500"
                />
              </div>
            </div>

            <div className="flex items-center justify-between pt-2">
              {saveSuccess && (
                <div className="text-sm text-green-600 dark:text-green-400 flex items-center gap-1.5 font-medium">
                  <Check className="h-4.5 w-4.5" />
                  <span>Changes saved successfully!</span>
                </div>
              )}
              {!saveSuccess && <div></div>}
              
              <button
                type="submit"
                disabled={saving}
                className="px-5 py-2.5 bg-red-600 hover:bg-red-700 text-white rounded-lg text-sm font-semibold shadow-sm flex items-center gap-2 transition-colors disabled:opacity-50"
              >
                <Save className="h-4.5 w-4.5" />
                <span>{saving ? 'Saving...' : 'Save Changes'}</span>
              </button>
            </div>
          </form>
        </div>
      </div>

      {/* Security Actions Card (Password Reset) */}
      <div className="rounded-2xl bg-white dark:bg-gray-900 p-6 shadow-sm border border-gray-100 dark:border-gray-800 space-y-4">
        <div className="flex items-center gap-2">
          <Key className="h-5 w-5 text-gray-500" />
          <h3 className="text-lg font-bold text-gray-900 dark:text-white">Security & Password Management</h3>
        </div>
        <p className="text-sm text-gray-500 dark:text-gray-400">
          Resetting the password generates a random temporary string. The student will be forced to choose a new password on their next login attempt.
        </p>

        <div className="pt-2 flex flex-col md:flex-row gap-4 items-start md:items-center">
          <button
            onClick={handlePasswordReset}
            className="px-4 py-2 border border-orange-200 dark:border-orange-900 text-orange-600 dark:text-orange-400 hover:bg-orange-50 dark:hover:bg-orange-950/20 rounded-lg text-sm font-semibold transition-colors"
          >
            Reset Account Password
          </button>

          {tempPassword && (
            <div className="flex-1 w-full max-w-lg bg-orange-50 dark:bg-orange-950/20 border border-orange-200 dark:border-orange-900/35 rounded-xl p-4 flex items-center justify-between gap-4">
              <div className="space-y-1">
                <div className="text-[10px] text-orange-600 font-bold uppercase tracking-wider">Temporary Password Generated</div>
                <div className="text-base font-mono font-bold text-gray-800 dark:text-gray-200 tracking-wider">
                  {tempPassword}
                </div>
              </div>
              <button
                onClick={handleCopyPassword}
                className="p-2 bg-white dark:bg-gray-900 hover:bg-gray-100 dark:hover:bg-gray-800 text-gray-500 hover:text-gray-900 dark:hover:text-white rounded-lg border border-gray-200 dark:border-gray-800 transition-all flex items-center gap-1.5 text-xs font-semibold"
              >
                {copied ? <Check className="h-4 w-4 text-green-500" /> : <Copy className="h-4 w-4" />}
                <span>{copied ? 'Copied' : 'Copy'}</span>
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default AdminStudentDetailPage;
