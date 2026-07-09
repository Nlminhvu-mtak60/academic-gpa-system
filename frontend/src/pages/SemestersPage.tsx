import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useLanguage } from '../contexts/LanguageContext';
import { semesterApi, SemesterDto } from '../api/semesterApi';
import { academicYearApi, AcademicYearDto } from '../api/academicYearApi';
import { 
  Plus, 
  Edit2, 
  Trash2, 
  AlertTriangle, 
  X, 
  ChevronLeft, 
  BookOpen, 
  ChevronRight,
  Award
} from 'lucide-react';

export const SemestersPage: React.FC = () => {
  const { yearId } = useParams<{ yearId: string }>();
  const { t } = useLanguage();
  
  const [semesters, setSemesters] = useState<SemesterDto[]>([]);
  const [academicYear, setAcademicYear] = useState<AcademicYearDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Dialog State
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [dialogMode, setDialogMode] = useState<'create' | 'edit' | 'import'>('create');
  const [editingId, setEditingId] = useState<string | null>(null);

  // Form State
  const [semesterName, setSemesterName] = useState('');
  const [importedCredits, setImportedCredits] = useState<number>(0);
  const [importedGpa10, setImportedGpa10] = useState<string>('0.00');
  const [importedGpa4, setImportedGpa4] = useState<string>('0.00');
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);

  // Delete Confirmation State
  const [deleteConfirmId, setDeleteConfirmId] = useState<string | null>(null);
  const [deleting, setDeleting] = useState(false);

  const fetchData = async () => {
    if (!yearId) return;
    try {
      setLoading(true);
      setError(null);
      
      // Fetch Academic Years to find the current year info
      const yearRes = await academicYearApi.getAcademicYears();
      if (yearRes.success) {
        const foundYear = (yearRes.data || []).find((y: AcademicYearDto) => y.id === yearId);
        if (foundYear) {
          setAcademicYear(foundYear);
        }
      }

      // Fetch Semesters for the Year
      const semesterRes = await semesterApi.getSemesters(yearId);
      if (semesterRes.success) {
        setSemesters(semesterRes.data || []);
      } else {
        setError(semesterRes.message || 'Failed to fetch semesters');
      }
    } catch (err: any) {
      console.error(err);
      setError('An error occurred while loading semester data.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [yearId]);

  const validateForm = (): boolean => {
    const errors: Record<string, string> = {};
    if (!semesterName.trim()) {
      errors.semesterName = t('common.required');
    } else if (semesterName.length > 50) {
      errors.semesterName = 'Semester name cannot exceed 50 characters.';
    }

    const isEditingImported = dialogMode === 'edit' && editingId && semesters.find(s => s.id === editingId)?.isImported;
    if (dialogMode === 'import' || isEditingImported) {
      if (importedCredits === undefined || importedCredits < 0) {
        errors.importedCredits = 'Imported credits cannot be negative.';
      }
      const g10 = parseFloat(importedGpa10);
      if (isNaN(g10) || g10 < 0 || g10 > 10) {
        errors.importedGpa10 = 'GPA 10 must be between 0.0 and 10.0.';
      }
      const g4 = parseFloat(importedGpa4);
      if (isNaN(g4) || g4 < 0 || g4 > 4) {
        errors.importedGpa4 = 'GPA 4 must be between 0.0 and 4.0.';
      }
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleOpenCreate = () => {
    setDialogMode('create');
    setEditingId(null);
    setSemesterName('');
    setFormErrors({});
    setIsDialogOpen(true);
  };

  const handleOpenImport = () => {
    setDialogMode('import');
    setEditingId(null);
    setSemesterName('Học kỳ 1');
    setImportedCredits(15);
    setImportedGpa10('8.00');
    setImportedGpa4('3.20');
    setFormErrors({});
    setIsDialogOpen(true);
  };

  const handleOpenEdit = (sem: SemesterDto) => {
    setDialogMode('edit');
    setEditingId(sem.id);
    setSemesterName(sem.semesterName);
    if (sem.isImported) {
      setImportedCredits(sem.importedCredits || 0);
      setImportedGpa10((sem.importedGpa10 || 0).toFixed(2));
      setImportedGpa4((sem.importedGpa4 || 0).toFixed(2));
    }
    setFormErrors({});
    setIsDialogOpen(true);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) return;
    if (!yearId) return;

    setSubmitting(true);
    try {
      if (dialogMode === 'create') {
        const res = await semesterApi.createSemester(yearId, { semesterName });
        if (res.success) {
          setIsDialogOpen(false);
          fetchData();
        } else {
          setFormErrors({ api: res.message || 'Failed to create semester' });
        }
      } else if (dialogMode === 'import') {
        const res = await semesterApi.importHistoricalSemester(yearId, {
          semesterName,
          importedCredits,
          importedGpa10: parseFloat(importedGpa10),
          importedGpa4: parseFloat(importedGpa4)
        });
        if (res.success) {
          setIsDialogOpen(false);
          fetchData();
        } else {
          setFormErrors({ api: res.message || 'Failed to import semester' });
        }
      } else if (dialogMode === 'edit' && editingId) {
        const payload: any = { semesterName };
        const editingSem = semesters.find(s => s.id === editingId);
        if (editingSem?.isImported) {
          payload.importedCredits = importedCredits;
          payload.importedGpa10 = parseFloat(importedGpa10);
          payload.importedGpa4 = parseFloat(importedGpa4);
        }
        const res = await semesterApi.updateSemester(editingId, payload);
        if (res.success) {
          setIsDialogOpen(false);
          fetchData();
        } else {
          setFormErrors({ api: res.message || 'Failed to update semester' });
        }
      }
    } catch (err: any) {
      console.error(err);
      const apiMsg = err.response?.data?.message || 'Server error occurred.';
      setFormErrors({ api: apiMsg });
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!deleteConfirmId) return;
    setDeleting(true);
    try {
      await semesterApi.deleteSemester(deleteConfirmId);
      setDeleteConfirmId(null);
      fetchData();
    } catch (err: any) {
      console.error(err);
      const errMsg = err.response?.data?.message || 'Failed to delete semester.';
      setError(errMsg);
    } finally {
      setDeleting(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Back Button */}
      <div>
        <Link 
          to="/academic-years" 
          className="inline-flex items-center gap-1.5 text-sm font-semibold text-gray-500 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white transition-colors"
        >
          <ChevronLeft className="h-4 w-4" />
          {t('semesters.backToYears')}
        </Link>
      </div>

      {/* Header section with rich aesthetics */}
      <div className="relative overflow-hidden bg-gradient-to-r from-indigo-600 to-brand-700 dark:from-indigo-900 dark:to-brand-950 p-8 rounded-2xl shadow-lg text-white border border-indigo-500/20">
        <div className="absolute right-0 top-0 -mt-6 -mr-6 w-32 h-32 bg-white/10 rounded-full blur-xl pointer-events-none"></div>
        <div className="relative z-10 flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div>
            <h1 className="text-3xl font-extrabold tracking-tight flex items-center gap-2">
              <BookOpen className="h-8 w-8 text-indigo-200" />
              {t('semesters.title')} {academicYear ? `— ${academicYear.yearName}` : ''}
            </h1>
            <p className="text-indigo-100/90 mt-2 text-base max-w-xl">
              {t('semesters.subtitle')}
            </p>
          </div>
          {academicYear?.isImported !== true && (
            <div className="flex flex-col sm:flex-row gap-3 self-start md:self-auto">
              <button
                onClick={handleOpenImport}
                disabled={semesters.length >= 3}
                className="flex items-center justify-center gap-2 px-5 py-3 bg-white/10 hover:bg-white/20 text-white font-bold rounded-xl border border-white/20 shadow-md hover:shadow-lg transition-all transform hover:-translate-y-0.5 active:translate-y-0 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Nhập học kỳ lịch sử
              </button>
              <button
                onClick={handleOpenCreate}
                disabled={semesters.length >= 3}
                className="flex items-center justify-center gap-2 px-5 py-3 bg-white text-indigo-700 hover:bg-indigo-50 font-bold rounded-xl shadow-md hover:shadow-lg transition-all transform hover:-translate-y-0.5 active:translate-y-0 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <Plus className="h-5 w-5" />
                {t('semesters.addNew')}
              </button>
            </div>
          )}
        </div>
      </div>

      {/* Limit Warning Alert */}
      {semesters.length >= 3 && (
        <div className="p-4 bg-amber-50 dark:bg-amber-950/20 border border-amber-200 dark:border-amber-900/40 rounded-xl text-amber-800 dark:text-amber-300 flex items-start gap-3">
          <AlertTriangle className="h-5 w-5 shrink-0 mt-0.5" />
          <div className="text-sm">
            {t('semesters.maxLimit')}
          </div>
        </div>
      )}

      {/* Error Alert */}
      {error && (
        <div className="p-4 bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-800/50 rounded-xl text-red-700 dark:text-red-400 flex items-start gap-3">
          <AlertTriangle className="h-5 w-5 shrink-0 mt-0.5" />
          <div className="flex-1 text-sm">{error}</div>
          <button onClick={() => setError(null)} className="text-red-500 hover:text-red-700">
            <X className="h-4 w-4" />
          </button>
        </div>
      )}

      {/* Loading state skeleton */}
      {loading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {[1, 2].map((n) => (
            <div key={n} className="bg-white dark:bg-gray-800 p-6 rounded-2xl border border-gray-200 dark:border-gray-800 shadow-sm animate-pulse space-y-4">
              <div className="h-6 w-2/3 bg-gray-200 dark:bg-gray-700 rounded-md"></div>
              <div className="h-4 w-1/3 bg-gray-200 dark:bg-gray-700 rounded-md"></div>
              <div className="grid grid-cols-3 gap-4 pt-4 border-t border-gray-100 dark:border-gray-700">
                <div className="h-8 bg-gray-200 dark:bg-gray-700 rounded-md"></div>
                <div className="h-8 bg-gray-200 dark:bg-gray-700 rounded-md"></div>
              </div>
            </div>
          ))}
        </div>
      ) : semesters.length === 0 ? (
        /* Empty State with elegant design */
        <div className="flex flex-col items-center justify-center p-12 bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 text-center max-w-lg mx-auto">
          <div className="p-4 bg-indigo-50 dark:bg-indigo-950/30 text-indigo-500 dark:text-indigo-400 rounded-full mb-4">
            <BookOpen className="h-10 w-10" />
          </div>
          <h3 className="text-lg font-bold text-gray-900 dark:text-white">
            {t('semesters.empty')}
          </h3>
          <p className="text-gray-500 dark:text-gray-400 mt-2 text-sm max-w-sm">
            Bắt đầu quản lý môn học và điểm số bằng cách thêm học kỳ đầu tiên cho năm học này.
          </p>
          <button
            onClick={handleOpenCreate}
            className="mt-6 px-5 py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white font-semibold rounded-lg shadow transition-colors"
          >
            {t('semesters.addNew')}
          </button>
        </div>
      ) : (
        /* Grid of Semester Cards */
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {semesters.map((sem) => (
            <div 
              key={sem.id} 
              className={`relative bg-white dark:bg-gray-900 p-6 rounded-2xl border border-gray-200 dark:border-gray-800 hover:border-indigo-300 dark:hover:border-indigo-800/60 transition-all duration-300 shadow-sm hover:shadow-md`}
            >
              {/* Title & Info */}
              <div className="flex items-start justify-between">
                <div>
                  <h3 className="text-xl font-bold text-gray-900 dark:text-white flex items-center gap-1.5">
                    {sem.semesterName}
                    {sem.isImported && (
                      <span className="px-2 py-0.5 bg-amber-100 dark:bg-amber-950/40 text-amber-700 dark:text-amber-400 text-xs font-bold rounded-full border border-amber-200/50">
                        Lịch sử
                      </span>
                    )}
                  </h3>
                  <span className="inline-block mt-2 px-2.5 py-0.5 bg-indigo-50 dark:bg-indigo-950/40 text-indigo-600 dark:text-indigo-400 text-xs font-semibold rounded-full">
                    Thứ tự: {sem.sortOrder + 1}
                  </span>
                </div>

                {/* Actions Toolbar */}
                <div className="flex items-center gap-1">
                  <button
                    onClick={() => handleOpenEdit(sem)}
                    className="p-1.5 text-gray-400 hover:text-indigo-500 hover:bg-gray-50 dark:hover:bg-gray-800 rounded-lg transition-colors"
                    title={t('semesters.edit')}
                  >
                    <Edit2 className="h-4 w-4" />
                  </button>
                  <button
                    onClick={() => setDeleteConfirmId(sem.id)}
                    className="p-1.5 text-gray-400 hover:text-red-500 hover:bg-gray-50 dark:hover:bg-gray-800 rounded-lg transition-colors"
                    title={t('semesters.delete')}
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>

              {/* Stats Grid */}
              <div className="grid grid-cols-3 gap-3 mt-6 p-3.5 bg-gray-50 dark:bg-gray-950/60 rounded-xl border border-gray-100 dark:border-gray-800/40">
                <div className="text-center">
                  <span className="block text-xs text-gray-400 dark:text-gray-500 font-medium">Credits</span>
                  <span className="block text-sm font-bold text-gray-800 dark:text-gray-200 mt-0.5">{sem.completedCredits}</span>
                </div>
                <div className="text-center border-x border-gray-200 dark:border-gray-800">
                  <span className="block text-xs text-gray-400 dark:text-gray-500 font-medium">GPA (10)</span>
                  <span className="block text-sm font-bold text-gray-800 dark:text-gray-200 mt-0.5">{sem.semesterGpa10.toFixed(2)}</span>
                </div>
                <div className="text-center">
                  <span className="block text-xs text-gray-400 dark:text-gray-500 font-medium">GPA (4)</span>
                  <span className="block text-sm font-bold text-gray-800 dark:text-gray-200 mt-0.5">{sem.semesterGpa4.toFixed(2)}</span>
                </div>
              </div>

              <div className="mt-4 pt-4 border-t border-gray-100 dark:border-gray-800/80">
                {sem.isImported ? (
                  <div className="w-full flex items-center justify-between px-4 py-2.5 bg-amber-50 dark:bg-amber-950/20 text-amber-700 dark:text-amber-400 text-sm font-semibold rounded-xl select-none">
                    <span className="flex items-center gap-1.5">
                      <Award className="h-4 w-4" />
                      Học kỳ lịch sử nhập sẵn
                    </span>
                  </div>
                ) : (
                  <Link
                    to={`/semesters/${sem.id}/courses`}
                    className="w-full flex items-center justify-between px-4 py-2.5 bg-indigo-50 hover:bg-indigo-100 dark:bg-indigo-950/20 dark:hover:bg-indigo-950/30 text-indigo-600 dark:text-indigo-400 hover:text-indigo-700 dark:hover:text-indigo-300 text-sm font-semibold rounded-xl transition-all"
                  >
                    <span className="flex items-center gap-1.5">
                      <BookOpen className="h-4 w-4" />
                      {t('courses.title')}
                    </span>
                    <ChevronRight className="h-4 w-4" />
                  </Link>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* CREATE / EDIT DIALOG */}
      {isDialogOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm transition-opacity">
          <div 
            className="w-full max-w-md bg-white dark:bg-gray-900 rounded-2xl shadow-xl border border-gray-100 dark:border-gray-800 overflow-hidden transform transition-all scale-100"
          >
            {/* Dialog Header */}
            <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 dark:border-gray-800">
              <h3 className="text-lg font-bold text-gray-900 dark:text-white">
                {dialogMode === 'create' ? t('semesters.addNew') : dialogMode === 'import' ? 'Nhập học kỳ lịch sử' : t('semesters.edit')}
              </h3>
              <button
                onClick={() => setIsDialogOpen(false)}
                className="p-1 rounded-lg text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
              >
                <X className="h-5 w-5" />
              </button>
            </div>

            {/* Dialog Content/Form */}
            <form onSubmit={handleSave} className="p-6 space-y-4">
              {formErrors.api && (
                <div className="p-3 bg-red-50 dark:bg-red-950/20 border border-red-150 dark:border-red-900/40 rounded-lg text-xs text-red-600 dark:text-red-400">
                  {formErrors.api}
                </div>
              )}

              {/* Semester Name Input */}
              <div>
                <label className="block text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-1.5">
                  {t('semesters.semesterName')}
                </label>
                <input
                  type="text"
                  value={semesterName}
                  onChange={(e) => setSemesterName(e.target.value)}
                  className={`w-full px-3 py-2 border rounded-lg bg-transparent text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-colors dark:text-white ${
                    formErrors.semesterName ? 'border-red-500' : 'border-gray-300 dark:border-gray-700'
                  }`}
                  placeholder="e.g. Semester 1, Summer Semester"
                />
                {formErrors.semesterName && (
                  <p className="text-xs text-red-500 mt-1">{formErrors.semesterName}</p>
                )}
              </div>

              {/* Imported Fields (only in Import mode) */}
              {(dialogMode === 'import' || (dialogMode === 'edit' && editingId && semesters.find(s => s.id === editingId)?.isImported)) && (
                <div className="space-y-4 pt-2 border-t border-gray-100 dark:border-gray-800">
                  <h4 className="text-sm font-bold text-gray-700 dark:text-gray-300">Thông tin điểm tổng hợp</h4>
                  
                  <div>
                    <label className="block text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-1.5">
                      Số tín chỉ đã hoàn thành
                    </label>
                    <input
                      type="number"
                      value={importedCredits}
                      onChange={(e) => setImportedCredits(parseInt(e.target.value) || 0)}
                      min={0}
                      className={`w-full px-3 py-2 border rounded-lg bg-transparent text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-colors dark:text-white ${
                        formErrors.importedCredits ? 'border-red-500' : 'border-gray-300 dark:border-gray-700'
                      }`}
                    />
                    {formErrors.importedCredits && (
                      <p className="text-xs text-red-500 mt-1">{formErrors.importedCredits}</p>
                    )}
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-1.5">
                        Điểm trung bình (Hệ 10)
                      </label>
                      <input
                        type="text"
                        value={importedGpa10}
                        onChange={(e) => setImportedGpa10(e.target.value)}
                        placeholder="e.g. 8.00"
                        className={`w-full px-3 py-2 border rounded-lg bg-transparent text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-colors dark:text-white ${
                          formErrors.importedGpa10 ? 'border-red-500' : 'border-gray-300 dark:border-gray-700'
                        }`}
                      />
                      {formErrors.importedGpa10 && (
                        <p className="text-xs text-red-500 mt-1">{formErrors.importedGpa10}</p>
                      )}
                    </div>

                    <div>
                      <label className="block text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-1.5">
                        Điểm trung bình (Hệ 4)
                      </label>
                      <input
                        type="text"
                        value={importedGpa4}
                        onChange={(e) => setImportedGpa4(e.target.value)}
                        placeholder="e.g. 3.20"
                        className={`w-full px-3 py-2 border rounded-lg bg-transparent text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-colors dark:text-white ${
                          formErrors.importedGpa4 ? 'border-red-500' : 'border-gray-300 dark:border-gray-700'
                        }`}
                      />
                      {formErrors.importedGpa4 && (
                        <p className="text-xs text-red-500 mt-1">{formErrors.importedGpa4}</p>
                      )}
                    </div>
                  </div>
                </div>
              )}

              {/* Action Buttons */}
              <div className="flex items-center justify-end gap-3 pt-4 border-t border-gray-100 dark:border-gray-800">
                <button
                  type="button"
                  onClick={() => setIsDialogOpen(false)}
                  className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 rounded-lg transition-colors border border-gray-200 dark:border-gray-700"
                >
                  {t('semesters.cancel')}
                </button>
                <button
                  type="submit"
                  disabled={submitting}
                  className="px-4 py-2 text-sm font-semibold text-white bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-400 rounded-lg transition-colors shadow"
                >
                  {submitting ? '...' : t('semesters.save')}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* DELETE CONFIRMATION MODAL */}
      {deleteConfirmId && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm transition-opacity">
          <div className="w-full max-w-md bg-white dark:bg-gray-900 rounded-2xl shadow-xl border border-gray-100 dark:border-gray-800 p-6 space-y-6">
            <div className="flex items-center gap-3 text-red-600">
              <div className="p-2 bg-red-50 dark:bg-red-950/30 rounded-lg">
                <AlertTriangle className="h-6 w-6" />
              </div>
              <h3 className="text-lg font-bold text-gray-900 dark:text-white">
                {t('semesters.delete')}
              </h3>
            </div>
            
            <p className="text-sm text-gray-500 dark:text-gray-400">
              {t('semesters.deleteConfirm')}
            </p>

            <div className="flex items-center justify-end gap-3">
              <button
                type="button"
                onClick={() => setDeleteConfirmId(null)}
                className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 rounded-lg transition-colors border border-gray-200 dark:border-gray-700"
              >
                {t('semesters.cancel')}
              </button>
              <button
                type="button"
                onClick={handleDelete}
                disabled={deleting}
                className="px-4 py-2 text-sm font-semibold text-white bg-red-600 hover:bg-red-700 disabled:bg-red-400 rounded-lg transition-colors shadow"
              >
                {deleting ? '...' : t('semesters.delete')}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default SemestersPage;
