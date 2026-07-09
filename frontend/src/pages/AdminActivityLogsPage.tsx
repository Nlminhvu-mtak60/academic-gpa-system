import React, { useEffect, useState } from 'react';
import { adminApi, UserActivityLogDto, AdminStudentDto } from '../api/adminApi';
import { useLanguage } from '../contexts/LanguageContext';
import { FileText, Filter, ChevronLeft, ChevronRight, RefreshCw } from 'lucide-react';

export const AdminActivityLogsPage: React.FC = () => {
  const { t } = useLanguage();
  const [logs, setLogs] = useState<UserActivityLogDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);

  // Pagination & Filtering
  const [page, setPage] = useState(1);
  const [students, setStudents] = useState<AdminStudentDto[]>([]);
  const [selectedUserId, setSelectedUserId] = useState<string>('');
  const pageSize = 15;

  const fetchLogs = async () => {
    try {
      setLoading(true);
      const data = await adminApi.getActivityLogs({
        page,
        pageSize,
        userId: selectedUserId || undefined,
      });
      setLogs(data.items);
      setTotalCount(data.totalCount);
    } catch (err) {
      console.error('Failed to load activity logs:', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchStudentsList = async () => {
    try {
      const data = await adminApi.getStudents({
        page: 1,
        pageSize: 100,
      });
      setStudents(data.items);
    } catch (err) {
      console.error('Failed to load students filter list:', err);
    }
  };

  useEffect(() => {
    fetchLogs();
  }, [page, selectedUserId]);

  useEffect(() => {
    fetchStudentsList();
  }, []);

  const totalPages = Math.max(Math.ceil(totalCount / pageSize), 1);

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">{t('admin.logs.title')}</h1>
          <p className="text-sm text-gray-500 dark:text-gray-400">Security audit history tracking administrative and student access logs</p>
        </div>
        <button
          onClick={fetchLogs}
          className="p-2 border border-gray-200 dark:border-gray-800 rounded-lg text-gray-500 hover:text-gray-900 dark:hover:text-white bg-white dark:bg-gray-950 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
          title="Refresh Logs"
        >
          <RefreshCw className="h-4.5 w-4.5" />
        </button>
      </div>

      {/* Filter Toolbar */}
      <div className="bg-white dark:bg-gray-900 p-4 rounded-xl shadow-sm border border-gray-100 dark:border-gray-800 flex items-center gap-4">
        <Filter className="h-4 w-4 text-gray-400" />
        <div className="flex items-center gap-2">
          <label className="text-sm font-medium text-gray-700 dark:text-gray-300">Filter by User:</label>
          <select
            value={selectedUserId}
            onChange={(e) => {
              setSelectedUserId(e.target.value);
              setPage(1);
            }}
            className="border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-950 text-gray-900 dark:text-white rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-red-500"
          >
            <option value="">All Users</option>
            {students.map((student) => (
              <option key={student.id} value={student.id}>
                {student.lastName} {student.firstName} ({student.studentCode || student.email})
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Logs Table */}
      <div className="bg-white dark:bg-gray-900 rounded-2xl shadow-sm border border-gray-100 dark:border-gray-800 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-gray-50 dark:bg-gray-950 border-b border-gray-155 dark:border-gray-800 text-xs font-semibold text-gray-500 uppercase tracking-wider">
                <th className="px-6 py-4">Timestamp</th>
                <th className="px-6 py-4">User Account</th>
                <th className="px-6 py-4">Activity Description</th>
                <th className="px-6 py-4">IP Address</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-800 text-sm">
              {loading ? (
                <tr>
                  <td colSpan={4} className="text-center py-8">
                    <div className="h-6 w-6 animate-spin rounded-full border-2 border-red-500 border-t-transparent mx-auto"></div>
                  </td>
                </tr>
              ) : logs.length === 0 ? (
                <tr>
                  <td colSpan={4} className="text-center py-8 text-gray-400">
                    No activity logs found.
                  </td>
                </tr>
              ) : (
                logs.map((log) => (
                  <tr key={log.id} className="hover:bg-gray-50/50 dark:hover:bg-gray-800/20 transition-colors">
                    <td className="px-6 py-4 text-gray-500 dark:text-gray-400 font-mono text-xs">
                      {new Date(log.timestamp).toLocaleString()}
                    </td>
                    <td className="px-6 py-4 font-medium text-gray-900 dark:text-white">
                      {log.userEmail}
                    </td>
                    <td className="px-6 py-4 text-gray-800 dark:text-gray-200 font-medium">
                      {log.activity}
                    </td>
                    <td className="px-6 py-4 font-mono text-xs text-gray-500 dark:text-gray-400">
                      {log.ipAddress}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        <div className="bg-white dark:bg-gray-900 px-6 py-4 border-t border-gray-100 dark:border-gray-800 flex justify-between items-center">
          <span className="text-sm text-gray-500 dark:text-gray-400">
            Total {totalCount} log rows
          </span>
          <div className="flex gap-2">
            <button
              onClick={() => setPage((prev) => Math.max(prev - 1, 1))}
              disabled={page === 1}
              className="p-1.5 border border-gray-200 dark:border-gray-800 rounded-lg disabled:opacity-50 text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-950 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
            >
              <ChevronLeft className="h-5 w-5" />
            </button>
            <span className="px-3 py-1.5 text-sm font-semibold text-gray-700 dark:text-gray-300">
              Page {page} of {totalPages}
            </span>
            <button
              onClick={() => setPage((prev) => Math.min(prev + 1, totalPages))}
              disabled={page === totalPages}
              className="p-1.5 border border-gray-200 dark:border-gray-800 rounded-lg disabled:opacity-50 text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-950 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
            >
              <ChevronRight className="h-5 w-5" />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AdminActivityLogsPage;
