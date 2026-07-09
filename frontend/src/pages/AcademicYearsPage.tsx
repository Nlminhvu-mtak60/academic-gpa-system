import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useLanguage } from '../contexts/LanguageContext';
import { academicYearApi, AcademicYearDto } from '../api/academicYearApi';
import { 
  Calendar, 
  Plus, 
  Edit2, 
  Trash2, 
  AlertTriangle, 
  X, 
  Sparkles,
  Award,
  BookOpen,
  ChevronRight
} from 'lucide-react';

export const AcademicYearsPage: React.FC = () => {
  const { t } = useLanguage();
  const [academicYears, setAcademicYears] = useState<AcademicYearDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Dialog State
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [dialogMode, setDialogMode] = useState<'create' | 'edit' | 'import'>('create');
  const [editingId, setEditingId] = useState<string | null>(null);
  
  // Form State
  const [yearName, setYearName] = useState('');
  const [startYear, setStartYear] = useState<number>(new Date().getFullYear());
  const [endYear, setEndYear] = useState<number>(new Date().getFullYear() + 1);
  const [importedCredits, setImportedCredits] = useState<number>(0);
  const [importedGpa10, setImportedGpa10] = useState<string>('0.00');
  const [importedGpa4, setImportedGpa4] = useState<string>('0.00');
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);

  // Delete Confirmation State
  const [deleteConfirmId, setDeleteConfirmId] = useState<string | null>(null);
  const [deleting, setDeleting] = useState(false);

  const fetchAcademicYears = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await academicYearApi.getAcademicYears();
      if (res.success) {
        setAcademicYears(res.data || []);
      } else {
        setError(res.message || 'Failed to fetch academic years');
      }
    } catch (err: any) {
      console.error(err);
      setError('An error occurred while loading academic years.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAcademicYears();
  }, []);

  // Auto-fill YearName helper when start/end changes
  useEffect(() => {
    if (dialogMode === 'create' || !yearName) {
      setYearName(`${startYear}-${endYear}`);
    }
  }, [startYear, endYear]);

  const validateForm = (): boolean => {
    const errors: Record<string, string> = {};
    if (!yearName.trim()) {
      errors.yearName = t('common.required');
    } else if (yearName.length > 20) {
      errors.yearName = 'Year name cannot exceed 20 characters.';
    }

    if (!startYear || startYear < 2000 || startYear > 2100) {
      errors.startYear = 'Start year must be between 2000 and 2100.';
    }

    if (!endYear || endYear < 2000 || endYear > 2100) {
      errors.endYear = 'End year must be between 2000 and 2100.';
    } else if (endYear < startYear || endYear > startYear + 1) {
      errors.endYear = 'End year must be equal to start year or start year + 1.';
    }

    const isEditingImported = dialogMode === 'edit' && editingId && academicYears.find(ay => ay.id === editingId)?.isImported;
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
    const currentYear = new Date().getFullYear();
    setStartYear(currentYear);
    setEndYear(currentYear + 1);
    setYearName(`${currentYear}-${currentYear + 1}`);
    setFormErrors({});
    setIsDialogOpen(true);
  };

  const handleOpenImport = () => {
    setDialogMode('import');
    setEditingId(null);
    const currentYear = new Date().getFullYear();
    setStartYear(currentYear - 1);
    setEndYear(currentYear);
    setYearName(`${currentYear - 1}-${currentYear}`);
    setImportedCredits(30);
    setImportedGpa10('8.00');
    setImportedGpa4('3.20');
    setFormErrors({});
    setIsDialogOpen(true);
  };

  const handleOpenEdit = (ay: AcademicYearDto) => {
    setDialogMode('edit');
    setEditingId(ay.id);
    setYearName(ay.yearName);
    setStartYear(ay.startYear);
    setEndYear(ay.endYear);
    if (ay.isImported) {
      setImportedCredits(ay.importedCredits || 0);
      setImportedGpa10((ay.importedGpa10 || 0).toFixed(2));
      setImportedGpa4((ay.importedGpa4 || 0).toFixed(2));
    }
    setFormErrors({});
    setIsDialogOpen(true);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) return;

    setSubmitting(true);
    try {
      if (dialogMode === 'create') {
        const res = await academicYearApi.createAcademicYear({
          yearName,
          startYear,
          endYear
        });
        if (res.success) {
          setIsDialogOpen(false);
          fetchAcademicYears();
        } else {
          setFormErrors({ api: res.message || 'Failed to create academic year' });
        }
      } else if (dialogMode === 'import') {
        const res = await academicYearApi.importHistoricalYear({
          yearName,
          startYear,
          endYear,
          importedCredits,
          importedGpa10: parseFloat(importedGpa10),
          importedGpa4: parseFloat(importedGpa4)
        });
        if (res.success) {
          setIsDialogOpen(false);
          fetchAcademicYears();
        } else {
          setFormErrors({ api: res.message || 'Failed to import academic year' });
        }
      } else if (dialogMode === 'edit' && editingId) {
        const payload: any = {
          yearName,
          startYear,
          endYear
        };
        const editingYear = academicYears.find(ay => ay.id === editingId);
        if (editingYear?.isImported) {
          payload.importedCredits = importedCredits;
          payload.importedGpa10 = parseFloat(importedGpa10);
          payload.importedGpa4 = parseFloat(importedGpa4);
        }
        const res = await academicYearApi.updateAcademicYear(editingId, payload);
        if (res.success) {
          setIsDialogOpen(false);
          fetchAcademicYears();
        } else {
          setFormErrors({ api: res.message || 'Failed to update academic year' });
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

  const handleToggleCurrent = async (id: string, currentlyIsCurrent: boolean) => {
    if (currentlyIsCurrent) return; // Cannot turn off current directly; must set another year as current.
    try {
      const res = await academicYearApi.setCurrentAcademicYear(id);
      if (res.success) {
        fetchAcademicYears();
      }
    } catch (err) {
      console.error(err);
      setError('Failed to update active current academic year.');
    }
  };

  const handleDelete = async () => {
    if (!deleteConfirmId) return;
    setDeleting(true);
    try {
      await academicYearApi.deleteAcademicYear(deleteConfirmId);
      setDeleteConfirmId(null);
      fetchAcademicYears();
    } catch (err: any) {
      console.error(err);
      const errMsg = err.response?.data?.message || 'Failed to delete academic year.';
      setError(errMsg);
    } finally {
      setDeleting(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header section with rich aesthetics */}
      <div className="relative overflow-hidden bg-gradient-to-r from-brand-600 to-indigo-700 dark:from-brand-900 dark:to-indigo-950 p-8 rounded-2xl shadow-lg text-white border border-brand-500/20">
        <div className="absolute right-0 top-0 -mt-6 -mr-6 w-32 h-32 bg-white/10 rounded-full blur-xl pointer-events-none"></div>
        <div className="relative z-10 flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div>
            <h1 className="text-3xl font-extrabold tracking-tight flex items-center gap-2">
              <Calendar className="h-8 w-8 text-brand-200" />
              {t('academicyears.title')}
            </h1>
            <p className="text-brand-100/90 mt-2 text-base max-w-xl">
              {t('academicyears.subtitle')}
            </p>
          </div>
          <div className="flex flex-col sm:flex-row gap-3 self-start md:self-auto">
            <button
              onClick={handleOpenImport}
              className="flex items-center justify-center gap-2 px-5 py-3 bg-white/10 hover:bg-white/20 text-white font-bold rounded-xl border border-white/20 shadow-md hover:shadow-lg transition-all transform hover:-translate-y-0.5 active:translate-y-0"
            >
              <Award className="h-5 w-5 text-brand-200" />
              Nhập năm học lịch sử
            </button>
            <button
              onClick={handleOpenCreate}
              className="flex items-center justify-center gap-2 px-5 py-3 bg-white text-brand-700 hover:bg-brand-50 font-bold rounded-xl shadow-md hover:shadow-lg transition-all transform hover:-translate-y-0.5 active:translate-y-0"
            >
              <Plus className="h-5 w-5" />
              {t('academicyears.addNew')}
            </button>
          </div>
        </div>
      </div>

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
          {[1, 2, 3].map((n) => (
            <div key={n} className="bg-white dark:bg-gray-800 p-6 rounded-2xl border border-gray-200 dark:border-gray-800 shadow-sm animate-pulse space-y-4">
              <div className="h-6 w-2/3 bg-gray-200 dark:bg-gray-700 rounded-md"></div>
              <div className="h-4 w-1/3 bg-gray-200 dark:bg-gray-700 rounded-md"></div>
              <div className="grid grid-cols-3 gap-4 pt-4 border-t border-gray-100 dark:border-gray-700">
                <div className="h-8 bg-gray-200 dark:bg-gray-700 rounded-md"></div>
                <div className="h-8 bg-gray-200 dark:bg-gray-700 rounded-md"></div>
                <div className="h-8 bg-gray-200 dark:bg-gray-700 rounded-md"></div>
              </div>
            </div>
          ))}
        </div>
      ) : academicYears.length === 0 ? (
        /* Empty State with elegant design */
        <div className="flex flex-col items-center justify-center p-12 bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 text-center max-w-lg mx-auto">
          <div className="p-4 bg-brand-50 dark:bg-brand-950/30 text-brand-500 dark:text-brand-400 rounded-full mb-4">
            <Calendar className="h-10 w-10" />
          </div>
          <h3 className="text-lg font-bold text-gray-900 dark:text-white">
            Chưa có năm học nào
          </h3>
          <p className="text-gray-500 dark:text-gray-400 mt-2 text-sm max-w-sm">
            Bắt đầu thiết kế hành trình học tập của bạn bằng cách thêm năm học đầu tiên.
          </p>
          <button
            onClick={handleOpenCreate}
            className="mt-6 px-5 py-2.5 bg-brand-600 hover:bg-brand-700 text-white font-semibold rounded-lg shadow transition-colors"
          >
            {t('academicyears.addNew')}
          </button>
        </div>
      ) : (
        /* Grid of Academic Year Cards */
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {academicYears.map((ay) => (
            <div 
              key={ay.id} 
              className={`relative bg-white dark:bg-gray-900 p-6 rounded-2xl border transition-all duration-300 shadow-sm hover:shadow-md ${
                ay.isCurrent 
                  ? 'border-brand-500 ring-2 ring-brand-500/20 dark:border-brand-500' 
                  : 'border-gray-200 dark:border-gray-800 hover:border-gray-300 dark:hover:border-gray-700'
              }`}
            >
              {/* Current year glow banner */}
              {ay.isCurrent ? (
                <span className="absolute -top-3 left-6 px-3 py-1 bg-brand-500 text-white text-xs font-bold rounded-full uppercase tracking-wider flex items-center gap-1 shadow-sm">
                  <Sparkles className="h-3.5 w-3.5" />
                  {t('academicyears.current')}
                </span>
              ) : ay.isImported ? (
                <span className="absolute -top-3 left-6 px-3 py-1 bg-amber-600 text-white text-xs font-bold rounded-full uppercase tracking-wider flex items-center gap-1 shadow-sm">
                  <Award className="h-3.5 w-3.5" />
                  Dữ liệu lịch sử
                </span>
              ) : null}

              {/* Title & Info */}
              <div className="flex items-start justify-between mt-2">
                <div>
                  <h3 className="text-xl font-bold text-gray-900 dark:text-white flex items-center gap-1.5">
                    {ay.yearName}
                  </h3>
                  <p className="text-xs text-gray-400 dark:text-gray-500 mt-1">
                    {ay.startYear} &rarr; {ay.endYear}
                  </p>
                </div>

                {/* Actions Toolbar */}
                <div className="flex items-center gap-1">
                  <button
                    onClick={() => handleOpenEdit(ay)}
                    className="p-1.5 text-gray-400 hover:text-brand-500 hover:bg-gray-50 dark:hover:bg-gray-800 rounded-lg transition-colors"
                    title={t('academicyears.edit')}
                  >
                    <Edit2 className="h-4 w-4" />
                  </button>
                  <button
                    onClick={() => setDeleteConfirmId(ay.id)}
                    className="p-1.5 text-gray-400 hover:text-red-500 hover:bg-gray-50 dark:hover:bg-gray-800 rounded-lg transition-colors"
                    title={t('academicyears.delete')}
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>

              {/* GPA stats placeholder area */}
              <div className="grid grid-cols-3 gap-3 my-5 p-3.5 bg-gray-50 dark:bg-gray-950/60 rounded-xl border border-gray-100 dark:border-gray-800/40">
                <div className="text-center">
                  <span className="block text-xs text-gray-400 dark:text-gray-500 font-medium">Credits</span>
                  <span className="block text-sm font-bold text-gray-800 dark:text-gray-200 mt-0.5">{ay.completedCredits}</span>
                </div>
                <div className="text-center border-x border-gray-200 dark:border-gray-800">
                  <span className="block text-xs text-gray-400 dark:text-gray-500 font-medium">GPA (10)</span>
                  <span className="block text-sm font-bold text-gray-800 dark:text-gray-200 mt-0.5">{ay.yearGpa10.toFixed(2)}</span>
                </div>
                <div className="text-center">
                  <span className="block text-xs text-gray-400 dark:text-gray-500 font-medium">GPA (4)</span>
                  <span className="block text-sm font-bold text-gray-800 dark:text-gray-200 mt-0.5">{ay.yearGpa4.toFixed(2)}</span>
                </div>
              </div>

              <div className="mb-4">
                {ay.isImported ? (
                  <div className="w-full flex items-center justify-between px-4 py-2.5 bg-amber-50 dark:bg-amber-950/20 text-amber-700 dark:text-amber-400 text-sm font-semibold rounded-xl select-none">
                    <span className="flex items-center gap-1.5">
                      <Award className="h-4 w-4" />
                      Năm học lịch sử nhập sẵn
                    </span>
                  </div>
                ) : (
                  <Link
                    to={`/academic-years/${ay.id}/semesters`}
                    className="w-full flex items-center justify-between px-4 py-2.5 bg-brand-50 hover:bg-brand-100 dark:bg-brand-950/20 dark:hover:bg-brand-950/30 text-brand-600 dark:text-brand-400 hover:text-brand-700 dark:hover:text-brand-300 text-sm font-semibold rounded-xl transition-all"
                  >
                    <span className="flex items-center gap-1.5">
                      <BookOpen className="h-4 w-4" />
                      {t('semesters.title')}
                    </span>
                    <ChevronRight className="h-4 w-4" />
                  </Link>
                )}
              </div>

              {/* Current Active toggle switch */}
              <div className="flex items-center justify-between pt-3 border-t border-gray-100 dark:border-gray-800/80">
                <span className="text-xs text-gray-500 dark:text-gray-400 font-medium">
                  {ay.isCurrent ? t('academicyears.current') : t('academicyears.completed')}
                </span>
                
                <button
                  onClick={() => handleToggleCurrent(ay.id, ay.isCurrent)}
                  disabled={ay.isCurrent}
                  className={`relative inline-flex h-6 w-11 shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-brand-500 focus:ring-offset-2 ${
                    ay.isCurrent ? 'bg-brand-500' : 'bg-gray-200 dark:bg-gray-800 cursor-pointer'
                  } ${ay.isCurrent ? 'cursor-default' : ''}`}
                >
                  <span
                    className={`pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out ${
                      ay.isCurrent ? 'translate-x-5' : 'translate-x-0'
                    }`}
                  />
                </button>
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
                {dialogMode === 'create' ? t('academicyears.addNew') : dialogMode === 'import' ? 'Nhập năm học lịch sử' : t('academicyears.edit')}
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

              {/* Year Name Input */}
              <div>
                <label className="block text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-1.5">
                  {t('academicyears.yearName')}
                </label>
                <input
                  type="text"
                  value={yearName}
                  onChange={(e) => setYearName(e.target.value)}
                  className={`w-full px-3 py-2 border rounded-lg bg-transparent text-sm focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 transition-colors dark:text-white ${
                    formErrors.yearName ? 'border-red-500' : 'border-gray-300 dark:border-gray-700'
                  }`}
                  placeholder="e.g. 2024-2025"
                />
                {formErrors.yearName && (
                  <p className="text-xs text-red-500 mt-1">{formErrors.yearName}</p>
                )}
              </div>

              {/* Start & End Years Grid */}
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-1.5">
                    {t('academicyears.startYear')}
                  </label>
                  <input
                    type="number"
                    value={startYear}
                    onChange={(e) => setStartYear(parseInt(e.target.value) || 0)}
                    min={2000}
                    max={2100}
                    className={`w-full px-3 py-2 border rounded-lg bg-transparent text-sm focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 transition-colors dark:text-white ${
                      formErrors.startYear ? 'border-red-500' : 'border-gray-300 dark:border-gray-700'
                    }`}
                  />
                  {formErrors.startYear && (
                    <p className="text-xs text-red-500 mt-1">{formErrors.startYear}</p>
                  )}
                </div>

                <div>
                  <label className="block text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-1.5">
                    {t('academicyears.endYear')}
                  </label>
                  <input
                    type="number"
                    value={endYear}
                    onChange={(e) => setEndYear(parseInt(e.target.value) || 0)}
                    min={2000}
                    max={2100}
                    className={`w-full px-3 py-2 border rounded-lg bg-transparent text-sm focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 transition-colors dark:text-white ${
                      formErrors.endYear ? 'border-red-500' : 'border-gray-300 dark:border-gray-700'
                    }`}
                  />
                  {formErrors.endYear && (
                    <p className="text-xs text-red-500 mt-1">{formErrors.endYear}</p>
                  )}
                </div>
              </div>

              {/* Imported Fields (in Import mode or editing imported year) */}
              {(dialogMode === 'import' || (dialogMode === 'edit' && editingId && academicYears.find(ay => ay.id === editingId)?.isImported)) && (
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
                      className={`w-full px-3 py-2 border rounded-lg bg-transparent text-sm focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 transition-colors dark:text-white ${
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
                        className={`w-full px-3 py-2 border rounded-lg bg-transparent text-sm focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 transition-colors dark:text-white ${
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
                        className={`w-full px-3 py-2 border rounded-lg bg-transparent text-sm focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 transition-colors dark:text-white ${
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
                  {t('academicyears.cancel')}
                </button>
                <button
                  type="submit"
                  disabled={submitting}
                  className="px-4 py-2 text-sm font-semibold text-white bg-brand-600 hover:bg-brand-700 disabled:bg-brand-400 rounded-lg transition-colors shadow"
                >
                  {submitting ? '...' : t('academicyears.save')}
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
                {t('academicyears.delete')}
              </h3>
            </div>
            
            <p className="text-sm text-gray-500 dark:text-gray-400">
              {t('academicyears.deleteConfirm')}
            </p>

            <div className="flex items-center justify-end gap-3">
              <button
                type="button"
                onClick={() => setDeleteConfirmId(null)}
                className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 rounded-lg transition-colors border border-gray-200 dark:border-gray-700"
              >
                {t('academicyears.cancel')}
              </button>
              <button
                type="button"
                onClick={handleDelete}
                disabled={deleting}
                className="px-4 py-2 text-sm font-semibold text-white bg-red-600 hover:bg-red-700 disabled:bg-red-400 rounded-lg transition-colors shadow"
              >
                {deleting ? '...' : t('academicyears.delete')}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default AcademicYearsPage;
