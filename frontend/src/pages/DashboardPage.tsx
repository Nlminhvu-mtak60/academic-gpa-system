import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useLanguage } from '../contexts/LanguageContext';
import { dashboardApi, DashboardSummaryDto } from '../api/dashboardApi';
import {
  GraduationCap,
  Award,
  Calendar,
  BookOpen,
  TrendingUp,
  Loader2,
  CheckCircle2,
  XCircle,
  Clock,
  ArrowRight,
  BookMarked,
  Sparkles,
  Bookmark,
  Target
} from 'lucide-react';

export const DashboardPage: React.FC = () => {
  const { user } = useAuth();
  const { t, language } = useLanguage();

  const [summary, setSummary] = useState<DashboardSummaryDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchDashboardData = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await dashboardApi.getSummary();
      if (res.success) {
        setSummary(res.data);
      } else {
        setError(res.message || 'Failed to load dashboard data.');
      }
    } catch (err: any) {
      console.error(err);
      setError('An error occurred while loading dashboard summary.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const formatGpaVal = (value: number | null | undefined): string => {
    if (value === null || value === undefined) return '—';
    return value.toFixed(2);
  };

  const getPassFailBadge = (isPass: boolean | null | undefined, score: number | null | undefined) => {
    if (score === null || score === undefined) return <span className="text-gray-450 dark:text-gray-500">—</span>;
    return isPass ? (
      <span className="inline-flex items-center gap-0.5 px-2 py-0.5 rounded-full text-[10px] font-bold bg-green-50 text-green-700 border border-green-200 dark:bg-green-950/20 dark:text-green-400 dark:border-green-900/40">
        {t('scores.pass')}
      </span>
    ) : (
      <span className="inline-flex items-center gap-0.5 px-2 py-0.5 rounded-full text-[10px] font-bold bg-red-50 text-red-700 border border-red-200 dark:bg-red-950/20 dark:text-red-400 dark:border-red-900/40">
        {t('scores.fail')}
      </span>
    );
  };

  if (loading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Loader2 className="h-10 w-10 animate-spin text-brand-500" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="mx-auto max-w-xl bg-red-50 border border-red-200 rounded-3xl p-6 dark:bg-red-950/10 dark:border-red-900/50 text-red-700 dark:text-red-400 mt-8 text-center">
        <AlertIcon className="h-10 w-10 text-red-500 mx-auto mb-3" />
        <h3 className="font-extrabold text-lg">Failed to load Dashboard</h3>
        <p className="mt-1 text-sm">{error}</p>
        <button
          onClick={fetchDashboardData}
          className="mt-4 px-5 py-2.5 rounded-xl font-bold bg-red-600 hover:bg-red-700 text-white transition-colors"
        >
          Try Again
        </button>
      </div>
    );
  }

  const perf = summary?.performanceSummary;
  const recent = summary?.recentCourses || [];
  
  const hasAcademicData = perf && (perf.totalCredits > 0 || recent.length > 0 || perf.currentAcademicYearName);

  return (
    <div className="mx-auto max-w-6xl space-y-8 animate-in fade-in duration-500">
      
      {/* Welcome Banner Card */}
      <div className="relative overflow-hidden bg-gradient-to-br from-brand-500 via-brand-600 to-indigo-700 text-white rounded-3xl p-8 shadow-xl shadow-brand-500/10 transition-all duration-300">
        <div className="relative z-10 space-y-2">
          <span className="bg-white/10 px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider backdrop-blur-md">
            {t('student')} Portal
          </span>
          <h1 className="text-3xl md:text-4xl font-black tracking-tight">
            {language === 'vi' 
              ? `Chào mừng trở lại, ${user?.firstName} ${user?.lastName}!`
              : `Welcome back, ${user?.firstName} ${user?.lastName}!`}
          </h1>
          <p className="text-white/80 max-w-xl text-base">
            {language === 'vi'
              ? 'Theo dõi điểm trung bình tích lũy, tiến độ tín chỉ học tập và phân tích biểu đồ xếp loại học lực cá nhân.'
              : 'Track your cumulative GPA, credit progression status, and analyze your academic classification breakdown.'}
          </p>
        </div>
        {/* Dynamic circular mesh graphic background */}
        <div className="absolute right-0 bottom-0 top-0 w-1/3 opacity-20 hidden md:block select-none pointer-events-none">
          <svg viewBox="0 0 100 100" className="h-full w-full fill-white">
            <circle cx="90" cy="50" r="40" />
            <circle cx="90" cy="50" r="30" className="opacity-40" />
            <circle cx="90" cy="50" r="20" className="opacity-40" />
          </svg>
        </div>
      </div>

      {!hasAcademicData ? (
        <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-12 text-center shadow-sm">
          <div className="flex justify-center mb-4">
            <div className="p-4 bg-brand-50 rounded-2xl dark:bg-brand-950/30 text-brand-500">
              <BookMarked className="h-12 w-12" />
            </div>
          </div>
          <h3 className="text-xl font-bold text-gray-800 dark:text-white mb-2">
            {language === 'vi' ? 'Chưa có dữ liệu học tập nào' : 'No Academic Data Available'}
          </h3>
          <p className="text-gray-500 dark:text-gray-400 max-w-md mx-auto mb-6">
            {language === 'vi'
              ? 'Hãy bắt đầu thiết lập Năm Học và Học Kỳ để nhập các môn học và tính toán GPA.'
              : 'Get started by creating your first academic year and adding semesters to begin tracking courses.'}
          </p>
          <Link
            to="/academic-years"
            className="inline-flex items-center gap-2 px-6 py-3 rounded-xl font-bold bg-brand-500 text-white hover:bg-brand-600 active:bg-brand-700 transition-colors shadow-lg shadow-brand-500/15"
          >
            <span>{t('academicyears.addNew')}</span>
            <ArrowRight className="h-4 w-4" />
          </Link>
        </div>
      ) : (
        <>
          {/* Active Status Ribbon */}
          {perf.currentAcademicYearName && (
            <div className="bg-brand-50/50 dark:bg-brand-950/15 border border-brand-100 dark:border-brand-900/60 rounded-2xl px-5 py-3.5 flex flex-wrap items-center justify-between gap-3 text-sm text-brand-750 dark:text-brand-350">
              <div className="flex items-center gap-2">
                <Calendar className="h-4.5 w-4.5 text-brand-500" />
                <span className="font-semibold">{t('academicyears.current')}:</span>
                <span className="font-bold text-brand-600 dark:text-brand-400">{perf.currentAcademicYearName}</span>
                <span className="text-gray-300 dark:text-gray-700 select-none">|</span>
                <span className="font-bold text-brand-600 dark:text-brand-400">{perf.currentSemesterName || '—'}</span>
              </div>
              <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold bg-brand-500/10 border border-brand-500/20 text-brand-650 dark:text-brand-300 uppercase tracking-wide">
                <Sparkles className="h-3.5 w-3.5" />
                Active Session
              </span>
            </div>
          )}

          {/* Quick Metrics Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            
            {/* Semester GPA Card */}
            <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm flex flex-col justify-between transition-all hover:shadow-md">
              <div className="flex justify-between items-start">
                <div>
                  <p className="text-xs text-gray-400 font-bold uppercase tracking-wider">{t('gpa.semester')}</p>
                  <h3 className="text-4xl font-black text-gray-900 dark:text-white mt-3">
                    {formatGpaVal(perf.currentSemesterGpa10)}
                  </h3>
                  <p className="text-xs text-gray-500 dark:text-gray-400 mt-1 font-medium">
                    Hệ 4: <strong className="text-gray-800 dark:text-gray-200 text-sm font-bold">{formatGpaVal(perf.currentSemesterGpa4)}</strong>
                  </p>
                </div>
                <div className="p-3 bg-brand-50 text-brand-500 dark:bg-brand-950/30 rounded-2xl">
                  <TrendingUp className="h-6 w-6" />
                </div>
              </div>
              <div className="mt-5 pt-3 border-t border-gray-100 dark:border-gray-800 text-xs text-gray-500">
                Current active semester average score.
              </div>
            </div>

            {/* Academic Year GPA Card */}
            <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm flex flex-col justify-between transition-all hover:shadow-md">
              <div className="flex justify-between items-start">
                <div>
                  <p className="text-xs text-gray-400 font-bold uppercase tracking-wider">{t('gpa.year')}</p>
                  <h3 className="text-4xl font-black text-gray-900 dark:text-white mt-3">
                    {formatGpaVal(perf.currentAcademicYearGpa10)}
                  </h3>
                  <p className="text-xs text-gray-500 dark:text-gray-400 mt-1 font-medium">
                    Hệ 4: <strong className="text-gray-800 dark:text-gray-200 text-sm font-bold">{formatGpaVal(perf.currentAcademicYearGpa4)}</strong>
                  </p>
                </div>
                <div className="p-3 bg-indigo-50 text-indigo-500 dark:bg-indigo-950/30 rounded-2xl">
                  <Calendar className="h-6 w-6" />
                </div>
              </div>
              <div className="mt-5 pt-3 border-t border-gray-100 dark:border-gray-800 text-xs text-gray-500">
                Average across all semesters of active year.
              </div>
            </div>

            {/* Cumulative GPA Card */}
            <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm flex flex-col justify-between transition-all hover:shadow-md">
              <div className="flex justify-between items-start">
                <div>
                  <p className="text-xs text-gray-400 font-bold uppercase tracking-wider">{t('gpa.cumulative')}</p>
                  <h3 className="text-4xl font-black text-brand-600 dark:text-brand-400 mt-3">
                    {formatGpaVal(perf.cumulativeGpa10)}
                  </h3>
                  <p className="text-xs text-gray-500 dark:text-gray-400 mt-1 font-medium">
                    Hệ 4: <strong className="text-gray-850 dark:text-gray-200 text-sm font-bold">{formatGpaVal(perf.cumulativeGpa4)}</strong>
                  </p>
                </div>
                <div className="p-3 bg-purple-50 text-purple-500 dark:bg-purple-950/30 rounded-2xl">
                  <Award className="h-6 w-6" />
                </div>
              </div>
              <div className="mt-5 pt-3 border-t border-gray-100 dark:border-gray-800 flex justify-between items-center text-xs">
                <span className="text-gray-500">Classification:</span>
                <span className="font-bold text-purple-600 dark:text-purple-400">
                  {perf.classificationVn}
                </span>
              </div>
            </div>

            {/* Goal Progress Card */}
            <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm flex flex-col justify-between transition-all hover:shadow-md">
              <div className="flex justify-between items-start">
                <div>
                  <p className="text-xs text-gray-400 font-bold uppercase tracking-wider">
                    {language === 'vi' ? 'Mục tiêu GPA' : 'GPA Goal'}
                  </p>
                  {summary?.goalProgress?.targetGpa10 ? (
                    <>
                      <h3 className="text-4xl font-black text-indigo-650 dark:text-indigo-400 mt-3">
                        {summary.goalProgress.targetGpa10.toFixed(2)}
                      </h3>
                      <p className="text-[10px] text-gray-500 dark:text-gray-400 mt-1 font-medium leading-tight">
                        {summary.goalProgress.isAchieved ? (
                          <span className="text-green-600 dark:text-green-400 font-bold">
                            {language === 'vi' ? 'Đã đạt 🎉' : 'Achieved 🎉'}
                          </span>
                        ) : (
                          <>
                            {language === 'vi' ? 'Cần đạt thêm: ' : 'Need: '}
                            <strong className="text-gray-850 dark:text-gray-200 font-black">
                              {summary.goalProgress.requiredRemainingGpa !== null && summary.goalProgress.requiredRemainingGpa !== undefined
                                ? summary.goalProgress.requiredRemainingGpa.toFixed(2)
                                : '—'}
                            </strong>
                          </>
                        )}
                      </p>
                    </>
                  ) : (
                    <>
                      <h3 className="text-2xl font-black text-gray-400 mt-5">
                        {language === 'vi' ? 'Chưa đặt' : 'Not Set'}
                      </h3>
                      <p className="text-xs text-gray-400 mt-2 font-medium">
                        {language === 'vi' ? 'Thiết lập ngay' : 'Click to set target'}
                      </p>
                    </>
                  )}
                </div>
                <div className="p-3 bg-pink-50 text-pink-500 dark:bg-pink-950/30 rounded-2xl">
                  <Target className="h-6 w-6" />
                </div>
              </div>
              <div className="mt-5 pt-3 border-t border-gray-100 dark:border-gray-800 flex justify-between items-center text-xs">
                <Link to="/goal-planner" className="text-indigo-600 dark:text-indigo-400 font-bold hover:underline flex items-center gap-0.5">
                  <span>{language === 'vi' ? 'Kế hoạch mục tiêu' : 'Goal Planner'}</span>
                  <ArrowRight className="h-3.5 w-3.5" />
                </Link>
              </div>
            </div>
          </div>

          {/* Credits Summary Progression Card */}
          <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm space-y-5">
            <div className="flex justify-between items-center border-b border-gray-100 dark:border-gray-800 pb-3">
              <h3 className="text-lg font-extrabold text-gray-900 dark:text-white flex items-center gap-2">
                <BookOpen className="h-5 w-5 text-brand-500" />
                Credit Progression Status
              </h3>
              <div className="text-xs font-bold text-gray-500">
                Completed: {perf.totalCreditsCompleted} / {perf.totalCreditsRequired} Credits
              </div>
            </div>

            {/* Split progression bar */}
            <div className="space-y-3">
              <div className="w-full bg-gray-100 dark:bg-gray-800 h-4 rounded-full overflow-hidden flex">
                <div 
                  className="bg-green-500 h-full transition-all duration-500" 
                  style={{ width: `${Math.min((perf.passedCredits / (perf.totalCreditsRequired || 1)) * 100, 100)}%` }} 
                  title={`Passed: ${perf.passedCredits} Credits`}
                />
                <div 
                  className="bg-red-500 h-full transition-all duration-500" 
                  style={{ width: `${Math.min((perf.failedCredits / (perf.totalCreditsRequired || 1)) * 100, 100)}%` }} 
                  title={`Failed: ${perf.failedCredits} Credits`}
                />
                <div 
                  className="bg-amber-500 h-full transition-all duration-500" 
                  style={{ width: `${Math.min(((perf.totalCredits - perf.passedCredits - perf.failedCredits) / (perf.totalCreditsRequired || 1)) * 100, 100)}%` }} 
                  title={`In Progress: ${perf.totalCredits - perf.passedCredits - perf.failedCredits} Credits`}
                />
              </div>

              {/* Legends split values */}
              <div className="flex flex-wrap gap-x-6 gap-y-2 text-xs font-semibold justify-center md:justify-start">
                <div className="flex items-center gap-1.5">
                  <div className="h-3 w-3 rounded-full bg-green-500" />
                  <span className="text-gray-600 dark:text-gray-400">{t('gpa.passedCredits')}:</span>
                  <span className="text-gray-900 dark:text-white font-bold">{perf.passedCredits}</span>
                </div>
                <div className="flex items-center gap-1.5">
                  <div className="h-3 w-3 rounded-full bg-red-500" />
                  <span className="text-gray-600 dark:text-gray-400">{t('gpa.failedCredits')}:</span>
                  <span className="text-gray-900 dark:text-white font-bold">{perf.failedCredits}</span>
                </div>
                <div className="flex items-center gap-1.5">
                  <div className="h-3 w-3 rounded-full bg-amber-500" />
                  <span className="text-gray-600 dark:text-gray-400">In Progress:</span>
                  <span className="text-gray-900 dark:text-white font-bold">
                    {perf.totalCredits - perf.passedCredits - perf.failedCredits}
                  </span>
                </div>
                <div className="flex items-center gap-1.5 ml-auto text-gray-400">
                  <Bookmark className="h-3.5 w-3.5" />
                  <span>Total Registered: {perf.totalCredits}</span>
                </div>
              </div>
            </div>
          </div>

          {/* Bottom Split Layout: Recent courses and Quick tools */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            
            {/* Left: Recent courses list */}
            <div className="lg:col-span-2 bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm space-y-4">
              <div className="flex justify-between items-center border-b border-gray-100 dark:border-gray-800 pb-3">
                <h3 className="text-base font-extrabold text-gray-900 dark:text-white flex items-center gap-2">
                  <Clock className="h-4.5 w-4.5 text-gray-400" />
                  {language === 'vi' ? 'Môn học cập nhật gần đây' : 'Recently Updated Courses'}
                </h3>
              </div>

              {recent.length === 0 ? (
                <p className="text-center py-6 text-sm text-gray-500 dark:text-gray-400 italic">
                  {t('courses.empty')}
                </p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-left text-sm border-collapse">
                    <thead>
                      <tr className="border-b border-gray-100 dark:border-gray-800 text-xs text-gray-400 font-bold uppercase select-none">
                        <th className="py-2.5">{t('courses.courseCode')}</th>
                        <th className="py-2.5">{t('courses.courseName')}</th>
                        <th className="py-2.5 text-center">{t('courses.credits')}</th>
                        <th className="py-2.5 text-center">{t('scores.courseScore')}</th>
                        <th className="py-2.5 text-center">{t('scores.letterGrade')}</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-50 dark:divide-gray-850/50">
                      {recent.map(c => (
                        <tr key={c.id} className="hover:bg-gray-50/20 dark:hover:bg-gray-850/10 transition-colors">
                          <td className="py-3 font-semibold text-gray-700 dark:text-gray-300">{c.courseCode}</td>
                          <td className="py-3 font-bold text-gray-900 dark:text-white truncate max-w-[200px]" title={c.courseName}>
                            {c.courseName}
                          </td>
                          <td className="py-3 text-center text-gray-800 dark:text-gray-250">{c.credits}</td>
                          <td className="py-3 text-center font-extrabold">
                            {c.courseScore !== null && c.courseScore !== undefined 
                              ? c.courseScore.toFixed(1) 
                              : <span className="text-gray-400 font-normal">—</span>}
                          </td>
                          <td className="py-3 text-center">
                            <div className="flex flex-col items-center gap-1">
                              <span className="font-bold text-brand-600 dark:text-brand-400">
                                {c.letterGrade || '—'}
                              </span>
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>

            {/* Right: Actions and Guidelines */}
            <div className="lg:col-span-1 bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm flex flex-col justify-between gap-6">
              <div>
                <h3 className="text-base font-extrabold text-gray-900 dark:text-white flex items-center gap-2">
                  <Sparkles className="h-5 w-5 text-yellow-500" />
                  Quick Actions
                </h3>
                <p className="text-xs text-gray-500 dark:text-gray-450 mt-1">
                  Navigate instantly to manage academic terms, course catalogs, or visualize detailed statistics reports.
                </p>
                
                <div className="mt-5 space-y-3">
                  <Link
                    to="/gpa"
                    className="flex items-center justify-between p-3.5 rounded-2xl border border-gray-100 dark:border-gray-800/80 hover:bg-gray-50 dark:hover:bg-gray-850/50 text-sm font-semibold text-gray-700 dark:text-gray-200 transition-all hover:scale-[1.01]"
                  >
                    <span>{t('gpa.title')}</span>
                    <ArrowRight className="h-4 w-4 text-gray-400" />
                  </Link>
                  <Link
                    to="/statistics"
                    className="flex items-center justify-between p-3.5 rounded-2xl border border-gray-100 dark:border-gray-800/80 hover:bg-gray-50 dark:hover:bg-gray-850/50 text-sm font-semibold text-gray-700 dark:text-gray-200 transition-all hover:scale-[1.01]"
                  >
                    <span>View Academic Charts & Stats</span>
                    <ArrowRight className="h-4 w-4 text-gray-400" />
                  </Link>
                  <Link
                    to="/academic-years"
                    className="flex items-center justify-between p-3.5 rounded-2xl border border-gray-100 dark:border-gray-800/80 hover:bg-gray-50 dark:hover:bg-gray-850/50 text-sm font-semibold text-gray-700 dark:text-gray-200 transition-all hover:scale-[1.01]"
                  >
                    <span>{t('nav.academicYears')}</span>
                    <ArrowRight className="h-4 w-4 text-gray-400" />
                  </Link>
                </div>
              </div>

              <div className="bg-gray-50 dark:bg-gray-950 p-4 rounded-2xl border border-gray-100 dark:border-gray-850 text-[11px] text-gray-500 leading-relaxed">
                <strong>GPA Rule Summary:</strong> attendance (10%) + continuous (30%) + final exam (60%) rounded to nearest 0.5 for components and 1 decimal place overall.
              </div>
            </div>

          </div>
        </>
      )}
    </div>
  );
};

const AlertIcon: React.FC<React.SVGProps<SVGSVGElement>> = (props) => (
  <svg
    {...props}
    xmlns="http://www.w3.org/2000/svg"
    width="24"
    height="24"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <circle cx="12" cy="12" r="10" />
    <line x1="12" y1="8" x2="12" y2="12" />
    <line x1="12" y1="16" x2="12.01" y2="16" />
  </svg>
);

export default DashboardPage;
