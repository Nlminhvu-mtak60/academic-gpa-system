import React, { useEffect, useState } from 'react';
import { adminApi, AdminStatisticsDto, UserActivityLogDto } from '../api/adminApi';
import { useLanguage } from '../contexts/LanguageContext';
import { Users, UserX, Star, GraduationCap, Award, FileText } from 'lucide-react';

export const AdminDashboardPage: React.FC = () => {
  const { t } = useLanguage();
  const [stats, setStats] = useState<AdminStatisticsDto | null>(null);
  const [logs, setLogs] = useState<UserActivityLogDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadDashboardData = async () => {
      try {
        setLoading(true);
        const [statsData, logsData] = await Promise.all([
          adminApi.getStatistics(),
          adminApi.getActivityLogs({ page: 1, pageSize: 5 }),
        ]);
        setStats(statsData);
        setLogs(logsData.items);
      } catch (err) {
        console.error('Failed to load admin dashboard data:', err);
        setError('Could not retrieve dashboard statistics.');
      } finally {
        setLoading(false);
      }
    };
    loadDashboardData();
  }, []);

  if (loading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-red-500 border-t-transparent"></div>
      </div>
    );
  }

  if (error || !stats) {
    return (
      <div className="rounded-xl bg-red-50 dark:bg-red-950/20 border border-red-200 dark:border-red-900/35 p-6 text-center text-red-600 dark:text-red-400">
        {error || 'Error loading dashboard.'}
      </div>
    );
  }

  const { userStats, academicOverview, gpaDistribution } = stats;

  // Compute distribution max for scaling charts
  const distValues = [
    gpaDistribution.excellent,
    gpaDistribution.veryGood,
    gpaDistribution.good,
    gpaDistribution.average,
    gpaDistribution.belowAverage,
    gpaDistribution.fail
  ];
  const maxVal = Math.max(...distValues, 1);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">{t('admin.nav.dashboard')}</h1>
        <p className="text-sm text-gray-500 dark:text-gray-400">System overview and system statistics aggregator</p>
      </div>

      {/* Grid of Metric Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* User stats card */}
        <div className="relative overflow-hidden rounded-2xl bg-white dark:bg-gray-900 p-6 shadow-sm border border-gray-100 dark:border-gray-800 transition-all hover:shadow-md">
          <div className="absolute top-0 right-0 h-24 w-24 translate-x-4 -translate-y-4 rounded-full bg-blue-500/5 blur-xl"></div>
          <div className="flex items-center justify-between mb-4">
            <div className="rounded-xl bg-blue-50 dark:bg-blue-950/40 p-3 text-blue-600 dark:text-blue-400">
              <Users className="h-6 w-6" />
            </div>
          </div>
          <h3 className="text-gray-500 dark:text-gray-400 text-sm font-medium">{t('admin.stats.totalStudents')}</h3>
          <div className="mt-1 flex items-baseline justify-between">
            <span className="text-3xl font-bold text-gray-900 dark:text-white">{userStats.totalStudents}</span>
            <span className="text-xs text-green-500 dark:text-green-400 font-semibold bg-green-50 dark:bg-green-950/20 px-2 py-0.5 rounded-full">
              {userStats.activeStudents} active
            </span>
          </div>
          <div className="mt-4 pt-4 border-t border-gray-100 dark:border-gray-800 flex justify-between text-xs text-gray-500">
            <span>{t('admin.stats.lockedAccounts')}: {userStats.lockedAccounts}</span>
          </div>
        </div>

        {/* GPA 10 card */}
        <div className="relative overflow-hidden rounded-2xl bg-white dark:bg-gray-900 p-6 shadow-sm border border-gray-100 dark:border-gray-800 transition-all hover:shadow-md">
          <div className="absolute top-0 right-0 h-24 w-24 translate-x-4 -translate-y-4 rounded-full bg-red-500/5 blur-xl"></div>
          <div className="flex items-center justify-between mb-4">
            <div className="rounded-xl bg-red-50 dark:bg-red-950/40 p-3 text-red-600 dark:text-red-400">
              <GraduationCap className="h-6 w-6" />
            </div>
          </div>
          <h3 className="text-gray-500 dark:text-gray-400 text-sm font-medium">{t('admin.stats.sysAverageGpa10')}</h3>
          <div className="mt-1 flex items-baseline justify-between">
            <span className="text-3xl font-bold text-gray-900 dark:text-white">
              {academicOverview.systemAverageGpa10 !== null ? academicOverview.systemAverageGpa10.toFixed(2) : 'N/A'}
            </span>
            <span className="text-xs text-gray-400 dark:text-gray-500">10-scale</span>
          </div>
          <div className="mt-4 pt-4 border-t border-gray-100 dark:border-gray-800 flex justify-between text-xs text-gray-500">
            <span>4.0-scale: {academicOverview.systemAverageGpa4 !== null ? academicOverview.systemAverageGpa4.toFixed(2) : 'N/A'}</span>
          </div>
        </div>

        {/* Credits Completed card */}
        <div className="relative overflow-hidden rounded-2xl bg-white dark:bg-gray-900 p-6 shadow-sm border border-gray-100 dark:border-gray-800 transition-all hover:shadow-md">
          <div className="absolute top-0 right-0 h-24 w-24 translate-x-4 -translate-y-4 rounded-full bg-orange-500/5 blur-xl"></div>
          <div className="flex items-center justify-between mb-4">
            <div className="rounded-xl bg-orange-50 dark:bg-orange-950/40 p-3 text-orange-600 dark:text-orange-400">
              <Star className="h-6 w-6" />
            </div>
          </div>
          <h3 className="text-gray-500 dark:text-gray-400 text-sm font-medium">{t('admin.stats.creditsCompleted')}</h3>
          <div className="mt-1 flex items-baseline justify-between">
            <span className="text-3xl font-bold text-gray-900 dark:text-white">{academicOverview.totalCreditsEarned}</span>
            <span className="text-xs text-orange-500 dark:text-orange-400 font-semibold bg-orange-50 dark:bg-orange-950/20 px-2 py-0.5 rounded-full">
              Credits earned
            </span>
          </div>
          <div className="mt-4 pt-4 border-t border-gray-100 dark:border-gray-800 flex justify-between text-xs text-gray-500">
            <span>Aggregated across all courses</span>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* GPA Distribution Chart */}
        <div className="lg:col-span-2 rounded-2xl bg-white dark:bg-gray-900 p-6 shadow-sm border border-gray-100 dark:border-gray-800">
          <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-6">{t('admin.stats.gpaDistribution')}</h3>
          
          <div className="space-y-4">
            {/* Excellent */}
            <div>
              <div className="flex justify-between text-sm mb-1">
                <span className="font-semibold text-gray-700 dark:text-gray-300">Excellent (≥ 9.0)</span>
                <span className="text-gray-500">{gpaDistribution.excellent} students</span>
              </div>
              <div className="w-full bg-gray-100 dark:bg-gray-800 h-4 rounded-full overflow-hidden">
                <div
                  className="bg-purple-500 h-full rounded-full transition-all duration-500"
                  style={{ width: `${(gpaDistribution.excellent / maxVal) * 100}%` }}
                ></div>
              </div>
            </div>

            {/* Very Good */}
            <div>
              <div className="flex justify-between text-sm mb-1">
                <span className="font-semibold text-gray-700 dark:text-gray-300">Very Good (8.0 - 8.9)</span>
                <span className="text-gray-500">{gpaDistribution.veryGood} students</span>
              </div>
              <div className="w-full bg-gray-100 dark:bg-gray-800 h-4 rounded-full overflow-hidden">
                <div
                  className="bg-blue-500 h-full rounded-full transition-all duration-500"
                  style={{ width: `${(gpaDistribution.veryGood / maxVal) * 100}%` }}
                ></div>
              </div>
            </div>

            {/* Good */}
            <div>
              <div className="flex justify-between text-sm mb-1">
                <span className="font-semibold text-gray-700 dark:text-gray-300">Good (7.0 - 7.9)</span>
                <span className="text-gray-500">{gpaDistribution.good} students</span>
              </div>
              <div className="w-full bg-gray-100 dark:bg-gray-800 h-4 rounded-full overflow-hidden">
                <div
                  className="bg-teal-500 h-full rounded-full transition-all duration-500"
                  style={{ width: `${(gpaDistribution.good / maxVal) * 100}%` }}
                ></div>
              </div>
            </div>

            {/* Average */}
            <div>
              <div className="flex justify-between text-sm mb-1">
                <span className="font-semibold text-gray-700 dark:text-gray-300">Average (5.0 - 6.9)</span>
                <span className="text-gray-500">{gpaDistribution.average} students</span>
              </div>
              <div className="w-full bg-gray-100 dark:bg-gray-800 h-4 rounded-full overflow-hidden">
                <div
                  className="bg-yellow-500 h-full rounded-full transition-all duration-500"
                  style={{ width: `${(gpaDistribution.average / maxVal) * 100}%` }}
                ></div>
              </div>
            </div>

            {/* Below Average */}
            <div>
              <div className="flex justify-between text-sm mb-1">
                <span className="font-semibold text-gray-700 dark:text-gray-300">Below Average (4.0 - 4.9)</span>
                <span className="text-gray-500">{gpaDistribution.belowAverage} students</span>
              </div>
              <div className="w-full bg-gray-100 dark:bg-gray-800 h-4 rounded-full overflow-hidden">
                <div
                  className="bg-orange-500 h-full rounded-full transition-all duration-500"
                  style={{ width: `${(gpaDistribution.belowAverage / maxVal) * 100}%` }}
                ></div>
              </div>
            </div>

            {/* Fail */}
            <div>
              <div className="flex justify-between text-sm mb-1">
                <span className="font-semibold text-gray-700 dark:text-gray-300">Fail (&lt; 4.0)</span>
                <span className="text-gray-500">{gpaDistribution.fail} students</span>
              </div>
              <div className="w-full bg-gray-100 dark:bg-gray-800 h-4 rounded-full overflow-hidden">
                <div
                  className="bg-red-500 h-full rounded-full transition-all duration-500"
                  style={{ width: `${(gpaDistribution.fail / maxVal) * 100}%` }}
                ></div>
              </div>
            </div>
          </div>
        </div>

        {/* Recent Activity Logs */}
        <div className="rounded-2xl bg-white dark:bg-gray-900 p-6 shadow-sm border border-gray-100 dark:border-gray-800 flex flex-col">
          <div className="flex items-center gap-2 mb-4">
            <FileText className="h-5 w-5 text-gray-500" />
            <h3 className="text-lg font-bold text-gray-900 dark:text-white">Recent Activities</h3>
          </div>
          
          <div className="flex-1 space-y-4 overflow-y-auto max-h-80 pr-1">
            {logs.length === 0 ? (
              <div className="text-sm text-gray-400 text-center py-8">No recent logs recorded.</div>
            ) : (
              logs.map((log) => (
                <div key={log.id} className="border-b border-gray-50 dark:border-gray-800 pb-3 last:border-b-0 last:pb-0">
                  <div className="flex justify-between text-xs text-gray-500 dark:text-gray-400 mb-1">
                    <span className="font-medium truncate max-w-[150px]">{log.userEmail}</span>
                    <span>{new Date(log.timestamp).toLocaleTimeString()}</span>
                  </div>
                  <p className="text-sm text-gray-800 dark:text-gray-200 font-medium">{log.activity}</p>
                  <span className="text-[10px] text-gray-400">{log.ipAddress}</span>
                </div>
              ))
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default AdminDashboardPage;
