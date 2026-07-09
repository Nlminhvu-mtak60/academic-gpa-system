import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { adminApi, AdminStudentDto } from '../api/adminApi';
import { useLanguage } from '../contexts/LanguageContext';
import { Search, Filter, Lock, Unlock, Trash2, Eye, ChevronLeft, ChevronRight, X } from 'lucide-react';

export const AdminStudentManagementPage: React.FC = () => {
  const { t } = useLanguage();
  const [students, setStudents] = useState<AdminStudentDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);

  // Filter and pagination states
  const [search, setSearch] = useState('');
  const [isActiveFilter, setIsActiveFilter] = useState<boolean | undefined>(undefined);
  const [page, setPage] = useState(1);
  const pageSize = 10;

  // Modals
  const [lockModalOpen, setLockModalOpen] = useState(false);
  const [targetStudent, setTargetStudent] = useState<AdminStudentDto | null>(null);
  const [lockReason, setLockReason] = useState('');

  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  const fetchStudents = async () => {
    try {
      setLoading(true);
      const data = await adminApi.getStudents({
        page,
        pageSize,
        search: search.trim() || undefined,
        isActive: isActiveFilter,
        sortBy: 'registrationdate',
        sortOrder: 'desc',
      });
      setStudents(data.items);
      setTotalCount(data.totalCount);
    } catch (err) {
      console.error('Failed to load students:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchStudents();
  }, [page, isActiveFilter]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    fetchStudents();
  };

  const openLockModal = (student: AdminStudentDto) => {
    setTargetStudent(student);
    setLockReason('');
    setLockModalOpen(true);
  };

  const handleLockSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!targetStudent || !lockReason.trim()) return;

    try {
      await adminApi.lockStudent(targetStudent.id, lockReason.trim());
      setLockModalOpen(false);
      fetchStudents();
    } catch (err) {
      console.error('Failed to lock student:', err);
    }
  };

  const handleUnlock = async (student: AdminStudentDto) => {
    if (!window.confirm(`Are you sure you want to unlock account for ${student.firstName} ${student.lastName}?`)) return;
    try {
      await adminApi.unlockStudent(student.id);
      fetchStudents();
    } catch (err) {
      console.error('Failed to unlock student:', err);
    }
  };

  const openDeleteModal = (student: AdminStudentDto) => {
    setTargetStudent(student);
    setDeleteConfirmOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!targetStudent) return;
    try {
      await adminApi.deleteStudent(targetStudent.id);
      setDeleteConfirmOpen(false);
      fetchStudents();
    } catch (err) {
      console.error('Failed to delete student:', err);
    }
  };

  const totalPages = Math.max(Math.ceil(totalCount / pageSize), 1);

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">{t('admin.nav.students')}</h1>
          <p className="text-sm text-gray-500 dark:text-gray-400">{t('admin.students.title')}</p>
        </div>
      </div>

      {/* Filter and Search Bar */}
      <div className="bg-white dark:bg-gray-900 p-4 rounded-xl shadow-sm border border-gray-100 dark:border-gray-800 flex flex-col md:flex-row gap-4 justify-between items-center">
        <form onSubmit={handleSearchSubmit} className="relative w-full md:w-96">
          <input
            type="text"
            placeholder={t('admin.students.search')}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-10 pr-4 py-2 border border-gray-200 dark:border-gray-800 rounded-lg bg-gray-50 dark:bg-gray-950 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-red-500 transition-all text-sm"
          />
          <button type="submit" className="absolute left-3 top-2.5 text-gray-400">
            <Search className="h-4.5 w-4.5" />
          </button>
        </form>

        <div className="flex items-center gap-2 w-full md:w-auto">
          <Filter className="h-4 w-4 text-gray-400" />
          <select
            value={isActiveFilter === undefined ? 'all' : isActiveFilter ? 'active' : 'locked'}
            onChange={(e) => {
              const val = e.target.value;
              setIsActiveFilter(val === 'all' ? undefined : val === 'active');
              setPage(1);
            }}
            className="border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-950 text-gray-900 dark:text-white rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-red-500"
          >
            <option value="all">All Accounts</option>
            <option value="active">Active</option>
            <option value="locked">Locked</option>
          </select>
        </div>
      </div>

      {/* Student Registry Table */}
      <div className="bg-white dark:bg-gray-900 rounded-2xl shadow-sm border border-gray-100 dark:border-gray-800 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-gray-50 dark:bg-gray-950 border-b border-gray-155 dark:border-gray-800 text-xs font-semibold text-gray-500 uppercase tracking-wider">
                <th className="px-6 py-4">Student Code</th>
                <th className="px-6 py-4">Name</th>
                <th className="px-6 py-4">Email</th>
                <th className="px-6 py-4">Major / University</th>
                <th className="px-6 py-4">Status</th>
                <th className="px-6 py-4 text-center">GPA 10 / 4</th>
                <th className="px-6 py-4 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-800 text-sm">
              {loading ? (
                <tr>
                  <td colSpan={7} className="text-center py-8">
                    <div className="h-6 w-6 animate-spin rounded-full border-2 border-red-500 border-t-transparent mx-auto"></div>
                  </td>
                </tr>
              ) : students.length === 0 ? (
                <tr>
                  <td colSpan={7} className="text-center py-8 text-gray-400">
                    No students matching criteria found.
                  </td>
                </tr>
              ) : (
                students.map((student) => (
                  <tr key={student.id} className="hover:bg-gray-50/50 dark:hover:bg-gray-800/20 transition-colors">
                    <td className="px-6 py-4 font-mono font-semibold text-gray-800 dark:text-gray-200">
                      {student.studentCode || 'N/A'}
                    </td>
                    <td className="px-6 py-4 font-medium text-gray-900 dark:text-white">
                      {student.lastName} {student.firstName}
                    </td>
                    <td className="px-6 py-4 text-gray-500 dark:text-gray-400">
                      {student.email}
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-gray-900 dark:text-white font-medium">{student.majorName || 'N/A'}</div>
                      <div className="text-xs text-gray-500">{student.universityName || 'N/A'}</div>
                    </td>
                    <td className="px-6 py-4">
                      {student.isActive ? (
                        <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-semibold bg-green-50 text-green-700 dark:bg-green-950/30 dark:text-green-400">
                          Active
                        </span>
                      ) : (
                        <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-semibold bg-red-50 text-red-700 dark:bg-red-950/30 dark:text-red-400">
                          Locked
                        </span>
                      )}
                    </td>
                    <td className="px-6 py-4 text-center">
                      {student.cumulativeGpa10 !== null ? (
                        <div className="font-semibold text-gray-900 dark:text-white">
                          {student.cumulativeGpa10.toFixed(2)}{' '}
                          <span className="text-xs text-gray-400 font-normal">/ {student.cumulativeGpa4?.toFixed(2)}</span>
                        </div>
                      ) : (
                        <span className="text-gray-400 text-xs">Uncalculated</span>
                      )}
                    </td>
                    <td className="px-6 py-4 text-right space-x-2">
                      <Link
                        to={`/admin/students/${student.id}`}
                        className="inline-flex p-1.5 text-gray-500 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100 dark:hover:bg-gray-800 rounded-lg transition-colors"
                        title={t('admin.students.details')}
                      >
                        <Eye className="h-4.5 w-4.5" />
                      </Link>

                      {student.isActive ? (
                        <button
                          onClick={() => openLockModal(student)}
                          className="inline-flex p-1.5 text-orange-600 hover:bg-orange-50 dark:hover:bg-orange-950/20 rounded-lg transition-colors"
                          title={t('admin.students.lock')}
                        >
                          <Lock className="h-4.5 w-4.5" />
                        </button>
                      ) : (
                        <button
                          onClick={() => handleUnlock(student)}
                          className="inline-flex p-1.5 text-green-600 hover:bg-green-50 dark:hover:bg-green-950/20 rounded-lg transition-colors"
                          title={t('admin.students.unlock')}
                        >
                          <Unlock className="h-4.5 w-4.5" />
                        </button>
                      )}

                      <button
                        onClick={() => openDeleteModal(student)}
                        className="inline-flex p-1.5 text-red-600 hover:bg-red-50 dark:hover:bg-red-950/20 rounded-lg transition-colors"
                        title={t('admin.students.delete')}
                      >
                        <Trash2 className="h-4.5 w-4.5" />
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination Controls */}
        <div className="bg-white dark:bg-gray-900 px-6 py-4 border-t border-gray-100 dark:border-gray-800 flex justify-between items-center">
          <span className="text-sm text-gray-500 dark:text-gray-400">
            Total {totalCount} registry items
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

      {/* Lock Student Account Modal */}
      {lockModalOpen && targetStudent && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/55 backdrop-blur-xs p-4">
          <div className="bg-white dark:bg-gray-900 w-full max-w-md rounded-2xl p-6 border border-gray-100 dark:border-gray-800 shadow-xl space-y-4">
            <div className="flex justify-between items-center">
              <h3 className="text-lg font-bold text-gray-900 dark:text-white">
                {t('admin.students.lock')}: {targetStudent.lastName} {targetStudent.firstName}
              </h3>
              <button
                onClick={() => setLockModalOpen(false)}
                className="text-gray-400 hover:text-gray-600 dark:hover:text-white"
              >
                <X className="h-5 w-5" />
              </button>
            </div>
            <form onSubmit={handleLockSubmit} className="space-y-4">
              <div className="space-y-1">
                <label className="text-sm font-medium text-gray-700 dark:text-gray-300">Reason for Account Lockout</label>
                <textarea
                  required
                  placeholder="e.g. Plagiarism inspection, fee overdue..."
                  value={lockReason}
                  onChange={(e) => setLockReason(e.target.value)}
                  className="w-full border border-gray-200 dark:border-gray-800 rounded-lg bg-gray-50 dark:bg-gray-950 text-gray-900 dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-500 h-24"
                />
              </div>
              <div className="flex justify-end gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => setLockModalOpen(false)}
                  className="px-4 py-2 border border-gray-200 dark:border-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 rounded-lg text-sm transition-colors"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg text-sm font-medium shadow-sm transition-colors"
                >
                  Confirm Lock
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Delete Confirmation Modal */}
      {deleteConfirmOpen && targetStudent && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/55 backdrop-blur-xs p-4">
          <div className="bg-white dark:bg-gray-900 w-full max-w-md rounded-2xl p-6 border border-gray-100 dark:border-gray-800 shadow-xl space-y-4">
            <h3 className="text-lg font-bold text-gray-900 dark:text-white">
              {t('admin.students.delete')}
            </h3>
            <p className="text-sm text-gray-500 dark:text-gray-400">
              Are you sure you want to soft-delete the account for <strong>{targetStudent.lastName} {targetStudent.firstName}</strong>? This hides the profile and blocks all future authentication, but keeps database logs intact.
            </p>
            <div className="flex justify-end gap-3 pt-2">
              <button
                type="button"
                onClick={() => setDeleteConfirmOpen(false)}
                className="px-4 py-2 border border-gray-200 dark:border-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 rounded-lg text-sm transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleDeleteConfirm}
                className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg text-sm font-medium shadow-sm transition-colors"
              >
                Confirm Delete
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminStudentManagementPage;
