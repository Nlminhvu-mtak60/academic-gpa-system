import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useLanguage } from '../contexts/LanguageContext';
import { courseApi, CourseDto } from '../api/courseApi';
import { semesterApi, SemesterDto } from '../api/semesterApi';
import { academicYearApi, AcademicYearDto } from '../api/academicYearApi';
import { scoreApi, ScoreDto, ScoreAuditLogDto } from '../api/scoreApi';
import { transcriptApi } from '../api/transcriptApi';
import { 
  Plus, 
  Edit2, 
  Trash2, 
  AlertTriangle, 
  X, 
  ChevronLeft, 
  BookOpen, 
  Search, 
  Check, 
  Sparkles,
  TrendingUp,
  FolderMinus,
  RefreshCw,
  Eye,
  History,
  FileText,
  Calculator,
  Calendar,
  AlertCircle,
  Upload
} from 'lucide-react';
import { TranscriptImportModal, TranscriptImportPreviewResult } from '../components/TranscriptImportModal';
import { ImportHistoryPanel } from '../components/import/ImportHistoryPanel';

export const CoursesPage: React.FC = () => {
  const { semesterId } = useParams<{ semesterId: string }>();
  const { t, language } = useLanguage();

  const [courses, setCourses] = useState<CourseDto[]>([]);
  const [semester, setSemester] = useState<SemesterDto | null>(null);
  const [academicYearId, setAcademicYearId] = useState<string | null>(null);
  
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Search & Filter state
  const [searchQuery, setSearchQuery] = useState('');

  // Course Dialog State
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [dialogMode, setDialogMode] = useState<'create' | 'edit'>('create');
  const [editingId, setEditingId] = useState<string | null>(null);

  // Course Form State
  const [courseCode, setCourseCode] = useState('');
  const [courseName, setCourseName] = useState('');
  const [credits, setCredits] = useState<number>(3);
  const [isRetake, setIsRetake] = useState(false);
  const [originalCourseId, setOriginalCourseId] = useState<string>('');
  
  // Eligible originals list for retake
  const [eligibleOriginals, setEligibleOriginals] = useState<CourseDto[]>([]);
  const [loadingOriginals, setLoadingOriginals] = useState(false);
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);

  // Delete Course Confirmation State
  const [deleteConfirmId, setDeleteConfirmId] = useState<string | null>(null);
  const [deleting, setDeleting] = useState(false);

  // --- Grade Management State ---
  const [isScoreModalOpen, setIsScoreModalOpen] = useState(false);
  const [selectedCourse, setSelectedCourse] = useState<CourseDto | null>(null);
  const [attendanceInput, setAttendanceInput] = useState('');
  const [continuousInput, setContinuousInput] = useState('');
  const [finalInput, setFinalInput] = useState('');
  const [scoreErrors, setScoreErrors] = useState<Record<string, string>>({});
  const [savingScores, setSavingScores] = useState(false);

  // Details Modal State
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
  const [selectedDetailCourse, setSelectedDetailCourse] = useState<CourseDto | null>(null);
  const [detailScores, setDetailScores] = useState<ScoreDto | null>(null);
  const [auditLogs, setAuditLogs] = useState<ScoreAuditLogDto[]>([]);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [detailError, setDetailError] = useState<string | null>(null);

  // Clear Scores state
  const [clearConfirmId, setClearConfirmId] = useState<string | null>(null);
  const [clearing, setClearing] = useState(false);

  // Transcript Import Modal State
  const [isImportModalOpen, setIsImportModalOpen] = useState(false);
  const [importHistory, setImportHistory] = useState<any[]>([
    { id: 'batch-1', importedAt: new Date().toISOString(), sourceType: 'Excel', courseCount: 5 }
  ]);

  const fetchData = async () => {
    if (!semesterId) return;
    try {
      setLoading(true);
      setError(null);

      // Find parent Academic Year ID by fetching semesters of all years
      const yearsRes = await academicYearApi.getAcademicYears();
      if (yearsRes.success) {
        let foundYearId: string | null = null;
        for (const yr of yearsRes.data || []) {
          const semRes = await semesterApi.getSemesters(yr.id);
          if (semRes.success) {
            const foundSem = (semRes.data || []).find((s: SemesterDto) => s.id === semesterId);
            if (foundSem) {
              setSemester(foundSem);
              foundYearId = yr.id;
              break;
            }
          }
        }
        setAcademicYearId(foundYearId);
      }

      // Fetch Courses
      const coursesRes = await courseApi.getCourses(semesterId);
      if (coursesRes.success) {
        setCourses(coursesRes.data || []);
      } else {
        setError(coursesRes.message || 'Failed to fetch courses');
      }
    } catch (err: any) {
      console.error(err);
      setError('An error occurred while loading courses.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [semesterId]);

  // Load eligible original courses when courseCode changes and isRetake is checked
  useEffect(() => {
    const fetchOriginals = async () => {
      if (isRetake && courseCode.trim().length > 0) {
        try {
          setLoadingOriginals(true);
          const res = await courseApi.getEligibleOriginalCourses(courseCode.trim());
          if (res.success) {
            const filtered = (res.data || []).filter((c: CourseDto) => c.id !== editingId);
            setEligibleOriginals(filtered);
            if (filtered.length > 0) {
              setOriginalCourseId(filtered[0].id);
            } else {
              setOriginalCourseId('');
            }
          }
        } catch (err) {
          console.error(err);
        } finally {
          setLoadingOriginals(false);
        }
      } else {
        setEligibleOriginals([]);
        setOriginalCourseId('');
      }
    };

    fetchOriginals();
  }, [isRetake, courseCode, editingId]);

  const validateForm = (): boolean => {
    const errors: Record<string, string> = {};
    if (!courseCode.trim()) {
      errors.courseCode = t('common.required');
    } else if (courseCode.length > 20) {
      errors.courseCode = 'Course code cannot exceed 20 characters.';
    } else if (!/^[a-zA-Z0-9]+$/.test(courseCode.trim())) {
      errors.courseCode = 'Course code must be alphanumeric (A-Z, 0-9).';
    }

    if (!courseName.trim()) {
      errors.courseName = t('common.required');
    } else if (courseName.length > 200) {
      errors.courseName = 'Course name cannot exceed 200 characters.';
    }

    if (!credits || credits < 1 || credits > 6) {
      errors.credits = 'Credits must be between 1 and 6.';
    }

    if (isRetake && !originalCourseId) {
      errors.originalCourseId = 'Original attempt course must be selected.';
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleOpenCreate = () => {
    setDialogMode('create');
    setEditingId(null);
    setCourseCode('');
    setCourseName('');
    setCredits(3);
    setIsRetake(false);
    setOriginalCourseId('');
    setFormErrors({});
    setIsDialogOpen(true);
  };

  const handleOpenEdit = (c: CourseDto) => {
    setDialogMode('edit');
    setEditingId(c.id);
    setCourseCode(c.courseCode);
    setCourseName(c.courseName);
    setCredits(c.credits);
    setIsRetake(c.isRetake);
    setOriginalCourseId(c.originalCourseId || '');
    setFormErrors({});
    setIsDialogOpen(true);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) return;
    if (!semesterId) return;

    setSubmitting(true);
    try {
      const payload = {
        courseCode: courseCode.trim(),
        courseName: courseName.trim(),
        credits,
        isRetake,
        originalCourseId: isRetake && originalCourseId ? originalCourseId : null
      };

      if (dialogMode === 'create') {
        const res = await courseApi.createCourse(semesterId, payload);
        if (res.success) {
          setIsDialogOpen(false);
          fetchData();
        } else {
          setFormErrors({ api: res.message || 'Failed to create course' });
        }
      } else if (dialogMode === 'edit' && editingId) {
        const res = await courseApi.updateCourse(editingId, payload);
        if (res.success) {
          setIsDialogOpen(false);
          fetchData();
        } else {
          setFormErrors({ api: res.message || 'Failed to update course' });
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
      await courseApi.deleteCourse(deleteConfirmId);
      setDeleteConfirmId(null);
      fetchData();
    } catch (err: any) {
      console.error(err);
      const errMsg = err.response?.data?.message || 'Failed to delete course.';
      setError(errMsg);
    } finally {
      setDeleting(false);
    }
  };

  // --- Grade Actions implementation ---
  const handleOpenScoreModal = (course: CourseDto) => {
    setSelectedCourse(course);
    setAttendanceInput(course.score?.attendanceScore !== undefined && course.score?.attendanceScore !== null ? course.score.attendanceScore.toString() : '');
    setContinuousInput(course.score?.continuousScore !== undefined && course.score?.continuousScore !== null ? course.score.continuousScore.toString() : '');
    setFinalInput(course.score?.finalExamScore !== undefined && course.score?.finalExamScore !== null ? course.score.finalExamScore.toString() : '');
    setScoreErrors({});
    setIsScoreModalOpen(true);
  };

  const validateScores = (att: string, con: string, fin: string): boolean => {
    const errors: Record<string, string> = {};
    const checkVal = (fieldName: string, val: string) => {
      if (val.trim() === '') return;
      const num = parseFloat(val);
      if (isNaN(num)) {
        errors[fieldName] = 'Điểm số phải là một số hợp lệ.';
      } else if (num < 0 || num > 10) {
        errors[fieldName] = 'Điểm số phải nằm trong khoảng từ 0.0 đến 10.0.';
      }
    };
    checkVal('attendance', att);
    checkVal('continuous', con);
    checkVal('finalExam', fin);
    setScoreErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSaveScores = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedCourse) return;
    if (!validateScores(attendanceInput, continuousInput, finalInput)) return;

    setSavingScores(true);
    try {
      const payload = {
        attendanceScore: attendanceInput.trim() !== '' ? parseFloat(attendanceInput) : null,
        continuousScore: continuousInput.trim() !== '' ? parseFloat(continuousInput) : null,
        finalExamScore: finalInput.trim() !== '' ? parseFloat(finalInput) : null
      };

      const res = await scoreApi.updateScores(selectedCourse.id, payload);
      if (res.success) {
        setIsScoreModalOpen(false);
        fetchData();
      } else {
        setScoreErrors({ api: res.message || 'Cập nhật điểm thất bại.' });
      }
    } catch (err: any) {
      console.error(err);
      setScoreErrors({ api: err.response?.data?.message || 'Có lỗi xảy ra khi lưu điểm.' });
    } finally {
      setSavingScores(false);
    }
  };

  const handleOpenDetailModal = async (course: CourseDto) => {
    setSelectedDetailCourse(course);
    setDetailScores(null);
    setAuditLogs([]);
    setLoadingDetail(true);
    setDetailError(null);
    setIsDetailModalOpen(true);

    try {
      const scoreRes = await scoreApi.getScores(course.id);
      if (scoreRes.success) {
        setDetailScores(scoreRes.data);
      }
      const auditRes = await scoreApi.getScoreAuditLogs(course.id);
      if (auditRes.success) {
        setAuditLogs(auditRes.data || []);
      }
    } catch (err: any) {
      console.error(err);
      setDetailError('Không thể tải lịch sử và chi tiết điểm số.');
    } finally {
      setLoadingDetail(false);
    }
  };

  const handleClearScores = async () => {
    if (!clearConfirmId) return;
    setClearing(true);
    try {
      const payload = {
        attendanceScore: null,
        continuousScore: null,
        finalExamScore: null
      };
      await scoreApi.updateScores(clearConfirmId, payload);
      setClearConfirmId(null);
      fetchData();
    } catch (err: any) {
      console.error(err);
      setError(err.response?.data?.message || 'Không thể xóa điểm số.');
    } finally {
      setClearing(false);
    }
  };

  const handleParseTranscript = async (file: File, sourceType: string): Promise<TranscriptImportPreviewResult | null> => {
    if (!semesterId) return null;
    const response = await transcriptApi.parseTranscript(semesterId, file, sourceType);
    if (response.success && response.data) {
      return response.data;
    } else {
      throw new Error(response.message || 'Lỗi bóc tách điểm từ AI');
    }
  };

  const handleConfirmTranscriptImport = async (data: any) => {
    if (!semesterId) return;
    const response = await transcriptApi.confirmImport(semesterId, data);
    if (response.success) {
      fetchData(); // Refresh the courses list after saving
    } else {
      throw new Error(response.message || 'Lỗi lưu bảng điểm');
    }
  };

  // Helper for live preview in modal
  const getLivePreview = (attendance: string, continuous: string, final: string) => {
    const att = parseFloat(attendance);
    const con = parseFloat(continuous);
    const fin = parseFloat(final);

    if (isNaN(att) || isNaN(con) || isNaN(fin)) {
      return null;
    }
    if (att < 0 || att > 10 || con < 0 || con > 10 || fin < 0 || fin > 10) {
      return null;
    }

    const roundToHalf = (val: number) => {
      return Math.round(val * 2) / 2;
    };

    const rAtt = roundToHalf(att);
    const rCon = roundToHalf(con);
    const rFin = roundToHalf(fin);

    const rawScore = (Math.round(rAtt * 10) * 10 + Math.round(rCon * 10) * 30 + Math.round(rFin * 10) * 60) / 1000;
    const courseScore = Math.round(rawScore * 10) / 10;

    let letterGrade = 'F';
    let gpa4Value = 0.0;
    let classification = 'Poor';
    let isPass = false;

    if (courseScore >= 9.0) {
      letterGrade = 'A+';
      gpa4Value = 4.0;
      classification = 'Outstanding';
      isPass = true;
    } else if (courseScore >= 8.5) {
      letterGrade = 'A';
      gpa4Value = 3.7;
      classification = 'Excellent';
      isPass = true;
    } else if (courseScore >= 8.0) {
      letterGrade = 'B+';
      gpa4Value = 3.5;
      classification = 'Very Good';
      isPass = true;
    } else if (courseScore >= 7.0) {
      letterGrade = 'B';
      gpa4Value = 3.0;
      classification = 'Good';
      isPass = true;
    } else if (courseScore >= 6.5) {
      letterGrade = 'C+';
      gpa4Value = 2.5;
      classification = 'Average Good';
      isPass = true;
    } else if (courseScore >= 5.5) {
      letterGrade = 'C';
      gpa4Value = 2.0;
      classification = 'Average';
      isPass = true;
    } else if (courseScore >= 5.0) {
      letterGrade = 'D+';
      gpa4Value = 1.5;
      classification = 'Average';
      isPass = true;
    } else if (courseScore >= 4.0) {
      letterGrade = 'D';
      gpa4Value = 1.0;
      classification = 'Weak';
      isPass = true;
    }

    return { courseScore, letterGrade, gpa4Value, classification, isPass };
  };

  const livePreview = getLivePreview(attendanceInput, continuousInput, finalInput);

  // Localization for classifications
  const getLocalizedClassification = (enClassification: string) => {
    if (language === 'vi') {
      switch (enClassification) {
        case 'Outstanding': return 'Xuất sắc';
        case 'Excellent': return 'Giỏi';
        case 'Very Good': return 'Khá giỏi';
        case 'Good': return 'Khá';
        case 'Average Good': return 'Trung bình khá';
        case 'Average': return 'Trung bình';
        case 'Weak': return 'Yếu';
        case 'Poor': return 'Kém';
        default: return enClassification;
      }
    }
    return enClassification;
  };

  // Letter Grade badge coloring styles
  const getLetterGradeClass = (letter: string | null) => {
    if (!letter) return 'bg-gray-100 text-gray-500 border-gray-200';
    if (letter === 'A+' || letter === 'A') {
      return 'bg-emerald-50 text-emerald-700 dark:bg-emerald-950/30 dark:text-emerald-400 border border-emerald-200/40 dark:border-emerald-800/20';
    }
    if (letter === 'B+' || letter === 'B') {
      return 'bg-blue-50 text-blue-700 dark:bg-blue-950/30 dark:text-blue-400 border border-blue-200/40 dark:border-blue-800/20';
    }
    if (letter === 'C+' || letter === 'C' || letter === 'D+' || letter === 'D') {
      return 'bg-amber-50 text-amber-700 dark:bg-amber-950/30 dark:text-amber-400 border border-amber-200/40 dark:border-amber-800/20';
    }
    return 'bg-red-50 text-red-700 dark:bg-red-950/30 dark:text-red-400 border border-red-200/40 dark:border-red-800/20';
  };

  const getPassFailBadge = (isPass: boolean | null) => {
    if (isPass === null) return null;
    return isPass ? (
      <span className="px-2.5 py-1 text-xs font-bold bg-emerald-100 text-emerald-800 dark:bg-emerald-950/40 dark:text-emerald-400 rounded-md">
        {t('scores.pass')}
      </span>
    ) : (
      <span className="px-2.5 py-1 text-xs font-bold bg-red-100 text-red-800 dark:bg-red-950/40 dark:text-red-400 rounded-md">
        {t('scores.fail')}
      </span>
    );
  };

  // Filter courses by search query
  const filteredCourses = courses.filter(c => 
    c.courseName.toLowerCase().includes(searchQuery.toLowerCase()) ||
    c.courseCode.toLowerCase().includes(searchQuery.toLowerCase())
  );

  // Sum credits
  const totalCreditsSum = courses.reduce((sum, c) => sum + c.credits, 0);

  return (
    <div className="space-y-6">
      {/* Back Button */}
      <div>
        {academicYearId ? (
          <Link 
            to={`/academic-years/${academicYearId}/semesters`} 
            className="inline-flex items-center gap-1.5 text-sm font-semibold text-gray-500 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white transition-colors"
          >
            <ChevronLeft className="h-4 w-4" />
            {t('courses.backToSemesters')}
          </Link>
        ) : (
          <Link 
            to="/academic-years" 
            className="inline-flex items-center gap-1.5 text-sm font-semibold text-gray-500 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white transition-colors"
          >
            <ChevronLeft className="h-4 w-4" />
            Quay lại
          </Link>
        )}
      </div>

      {/* Header section with rich aesthetics */}
      <div className="relative overflow-hidden bg-gradient-to-r from-violet-600 to-indigo-700 dark:from-violet-900 dark:to-indigo-950 p-8 rounded-2xl shadow-lg text-white border border-violet-500/20">
        <div className="absolute right-0 top-0 -mt-6 -mr-6 w-32 h-32 bg-white/10 rounded-full blur-xl pointer-events-none"></div>
        <div className="relative z-10 flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div>
            <h1 className="text-3xl font-extrabold tracking-tight flex items-center gap-2">
              <BookOpen className="h-8 w-8 text-violet-200" />
              {t('courses.title')} {semester ? `— ${semester.semesterName}` : ''}
            </h1>
            <p className="text-violet-100/90 mt-2 text-base max-w-xl">
              {t('courses.subtitle')}
            </p>
          </div>
          <div className="flex flex-col sm:flex-row gap-3 self-start md:self-auto">
            <button
              onClick={() => setIsImportModalOpen(true)}
              className="flex items-center justify-center gap-2 px-5 py-3 bg-white/10 hover:bg-white/20 text-white font-bold rounded-xl border border-white/20 shadow-md hover:shadow-lg transition-all transform hover:-translate-y-0.5 active:translate-y-0"
            >
              <Upload className="h-5 w-5" />
              Nhập từ Bảng Điểm
            </button>
            <button
              onClick={handleOpenCreate}
              className="flex items-center justify-center gap-2 px-5 py-3 bg-white text-violet-700 hover:bg-violet-50 font-bold rounded-xl shadow-md hover:shadow-lg transition-all transform hover:-translate-y-0.5 active:translate-y-0"
            >
              <Plus className="h-5 w-5" />
              {t('courses.addNew')}
            </button>
          </div>
        </div>
      </div>

      {/* Summary Banner & Search Filters */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 p-4 bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-2xl shadow-sm">
        <div className="flex items-center gap-4">
          <div className="p-3 bg-violet-50 dark:bg-violet-950/20 text-violet-600 dark:text-violet-400 rounded-xl">
            <TrendingUp className="h-6 w-6" />
          </div>
          <div>
            <span className="text-xs text-gray-400 dark:text-gray-500 font-medium uppercase tracking-wider block">
              {t('courses.totalCredits')}
            </span>
            <span className="text-2xl font-black text-gray-800 dark:text-gray-100">
              {totalCreditsSum} tín chỉ
            </span>
          </div>
        </div>

        {/* Search Input bar */}
        <div className="relative max-w-md w-full">
          <span className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-gray-400">
            <Search className="h-4 w-4" />
          </span>
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full pl-9 pr-4 py-2 bg-gray-50 dark:bg-gray-950/40 border border-gray-200 dark:border-gray-800 focus:outline-none focus:ring-2 focus:ring-violet-500/20 focus:border-violet-500 rounded-xl text-sm dark:text-white transition-colors"
            placeholder={t('courses.searchPlaceholder')}
          />
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
        <div className="space-y-4">
          <div className="h-12 bg-gray-200 dark:bg-gray-800 rounded-xl animate-pulse"></div>
          <div className="h-20 bg-gray-200 dark:bg-gray-800 rounded-xl animate-pulse"></div>
          <div className="h-20 bg-gray-200 dark:bg-gray-800 rounded-xl animate-pulse"></div>
        </div>
      ) : filteredCourses.length === 0 ? (
        /* Empty State with elegant design */
        <div className="flex flex-col items-center justify-center p-12 bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 text-center max-w-lg mx-auto">
          <div className="p-4 bg-violet-50 dark:bg-violet-950/30 text-violet-500 dark:text-violet-400 rounded-full mb-4">
            <FolderMinus className="h-10 w-10" />
          </div>
          <h3 className="text-lg font-bold text-gray-900 dark:text-white">
            {t('courses.empty')}
          </h3>
          <p className="text-gray-500 dark:text-gray-400 mt-2 text-sm max-w-sm">
            {searchQuery 
              ? 'Không tìm thấy học phần nào trùng khớp với từ khóa tìm kiếm của bạn.' 
              : 'Hãy thêm học phần của bạn vào học kỳ này để bắt đầu ghi nhận kết quả điểm số.'}
          </p>
          {!searchQuery && (
            <button
              onClick={handleOpenCreate}
              className="mt-6 px-5 py-2.5 bg-violet-600 hover:bg-violet-700 text-white font-semibold rounded-lg shadow transition-colors"
            >
              {t('courses.addNew')}
            </button>
          )}
        </div>
      ) : (
        <>
          {/* DESKTOP/TABLET TABLE VIEW (sm:block) */}
          <div className="hidden sm:block overflow-x-auto bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-2xl shadow-sm">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="bg-gray-50 dark:bg-gray-950/30 text-xs font-bold text-gray-500 dark:text-gray-400 uppercase tracking-wider border-b border-gray-200 dark:border-gray-800">
                  <th className="px-6 py-4">Mã HP</th>
                  <th className="px-6 py-4">Tên học phần</th>
                  <th className="px-6 py-4 text-center">Tín chỉ</th>
                  <th className="px-6 py-4 text-center">{t('scores.attendance')} (10%)</th>
                  <th className="px-6 py-4 text-center">{t('scores.continuous')} (30%)</th>
                  <th className="px-6 py-4 text-center">{t('scores.final')} (60%)</th>
                  <th className="px-6 py-4 text-center">{t('scores.courseScore')}</th>
                  <th className="px-6 py-4 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 dark:divide-gray-800/80 text-sm dark:text-gray-300">
                {filteredCourses.map((c) => (
                  <tr 
                    key={c.id} 
                    className={`hover:bg-gray-50/50 dark:hover:bg-gray-800/20 transition-colors ${
                      c.isRetake ? 'bg-amber-500/5 dark:bg-amber-500/2' : ''
                    }`}
                  >
                    <td className="px-6 py-4 font-semibold text-gray-600 dark:text-gray-400">
                      {c.courseCode}
                      {c.isRetake && (
                        <span className="ml-1.5 inline-flex items-center px-1.5 py-0.5 rounded text-[10px] font-bold bg-amber-500 text-white uppercase">
                          Lại
                        </span>
                      )}
                    </td>
                    <td className="px-6 py-4 font-bold text-gray-900 dark:text-white">
                      {c.courseName}
                    </td>
                    <td className="px-6 py-4 text-center font-medium">{c.credits}</td>
                    
                    {/* Component scores */}
                    <td className="px-6 py-4 text-center font-medium">
                      {c.score?.attendanceScore !== null && c.score?.attendanceScore !== undefined 
                        ? c.score.attendanceScore.toFixed(1) 
                        : '—'}
                    </td>
                    <td className="px-6 py-4 text-center font-medium">
                      {c.score?.continuousScore !== null && c.score?.continuousScore !== undefined 
                        ? c.score.continuousScore.toFixed(1) 
                        : '—'}
                    </td>
                    <td className="px-6 py-4 text-center font-medium">
                      {c.score?.finalExamScore !== null && c.score?.finalExamScore !== undefined 
                        ? c.score.finalExamScore.toFixed(1) 
                        : '—'}
                    </td>

                    {/* Calculated course score badge */}
                    <td className="px-6 py-4 text-center">
                      {c.score?.courseScore !== null && c.score?.courseScore !== undefined ? (
                        <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-black border ${getLetterGradeClass(c.score.letterGrade)}`}>
                          {c.score.courseScore.toFixed(1)} ({c.score.letterGrade})
                        </span>
                      ) : (
                        <span className="text-gray-400">—</span>
                      )}
                    </td>

                    {/* Table actions */}
                    <td className="px-6 py-4 text-right">
                      <div className="flex items-center justify-end gap-1.5">
                        <button
                          onClick={() => handleOpenScoreModal(c)}
                          className="p-1.5 text-gray-400 hover:text-violet-600 hover:bg-violet-50 dark:hover:bg-violet-950/20 rounded-lg transition-colors"
                          title="Nhập điểm"
                        >
                          <Calculator className="h-4 w-4" />
                        </button>
                        {(c.score?.attendanceScore !== null || c.score?.continuousScore !== null || c.score?.finalExamScore !== null) && (
                          <button
                            onClick={() => handleOpenDetailModal(c)}
                            className="p-1.5 text-gray-400 hover:text-indigo-600 hover:bg-indigo-50 dark:hover:bg-indigo-950/20 rounded-lg transition-colors"
                            title="Lịch sử & chi tiết"
                          >
                            <Eye className="h-4 w-4" />
                          </button>
                        )}
                        {(c.score?.attendanceScore !== null || c.score?.continuousScore !== null || c.score?.finalExamScore !== null) && (
                          <button
                            onClick={() => setClearConfirmId(c.id)}
                            className="p-1.5 text-gray-400 hover:text-amber-600 hover:bg-amber-50 dark:hover:bg-amber-950/20 rounded-lg transition-colors"
                            title="Xóa điểm số"
                          >
                            <Trash2 className="h-4 w-4" />
                          </button>
                        )}
                        <span className="h-4 w-px bg-gray-200 dark:bg-gray-800 mx-1"></span>
                        <button
                          onClick={() => handleOpenEdit(c)}
                          className="p-1.5 text-gray-400 hover:text-gray-600 hover:bg-gray-50 dark:hover:bg-gray-800 rounded-lg transition-colors"
                          title={t('courses.edit')}
                        >
                          <Edit2 className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => setDeleteConfirmId(c.id)}
                          className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-950/20 rounded-lg transition-colors"
                          title={t('courses.delete')}
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* MOBILE CARD VIEW (sm:hidden) */}
          <div className="block sm:hidden space-y-4">
            {filteredCourses.map((c) => (
              <div 
                key={c.id} 
                className={`relative bg-white dark:bg-gray-900 p-5 rounded-2xl border transition-all duration-300 shadow-sm ${
                  c.isRetake 
                    ? 'border-amber-400/50 dark:border-amber-800/40 ring-1 ring-amber-400/10' 
                    : 'border-gray-200 dark:border-gray-800 hover:border-violet-300'
                }`}
              >
                {c.isRetake && (
                  <span className="absolute -top-3 left-6 px-3 py-0.5 bg-amber-500 text-white text-[10px] font-bold rounded-full uppercase tracking-wider flex items-center gap-1 shadow-sm">
                    <RefreshCw className="h-3 w-3" />
                    Học lại
                  </span>
                )}

                {/* Card Title & Actions */}
                <div className="flex items-start justify-between mt-1">
                  <div>
                    <span className="text-xs text-gray-400 dark:text-gray-500 font-semibold tracking-wider block">
                      {c.courseCode}
                    </span>
                    <h3 className="text-base font-bold text-gray-900 dark:text-white mt-0.5">
                      {c.courseName}
                    </h3>
                  </div>

                  <div className="flex items-center gap-1.5 shrink-0">
                    <button
                      onClick={() => handleOpenEdit(c)}
                      className="p-1.5 text-gray-400 hover:text-gray-700 rounded-lg"
                      title={t('courses.edit')}
                    >
                      <Edit2 className="h-4 w-4" />
                    </button>
                    <button
                      onClick={() => setDeleteConfirmId(c.id)}
                      className="p-1.5 text-gray-400 hover:text-red-500 rounded-lg"
                      title={t('courses.delete')}
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </div>
                </div>

                {/* Credit indicator */}
                <p className="text-xs text-gray-500 mt-1">{c.credits} tín chỉ</p>

                {/* Scores Stack */}
                <div className="mt-4 p-3 bg-gray-50 dark:bg-gray-950/20 rounded-xl space-y-2 text-xs border border-gray-100 dark:border-gray-800/40">
                  <div className="flex justify-between">
                    <span className="text-gray-400">{t('scores.attendance')}:</span>
                    <span className="font-semibold text-gray-800 dark:text-gray-200">
                      {c.score?.attendanceScore !== null && c.score?.attendanceScore !== undefined ? c.score.attendanceScore.toFixed(1) : '—'}
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-400">{t('scores.continuous')}:</span>
                    <span className="font-semibold text-gray-800 dark:text-gray-200">
                      {c.score?.continuousScore !== null && c.score?.continuousScore !== undefined ? c.score.continuousScore.toFixed(1) : '—'}
                    </span>
                  </div>
                  <div className="flex justify-between border-b border-gray-100 dark:border-gray-800/50 pb-2">
                    <span className="text-gray-400">{t('scores.final')}:</span>
                    <span className="font-semibold text-gray-800 dark:text-gray-200">
                      {c.score?.finalExamScore !== null && c.score?.finalExamScore !== undefined ? c.score.finalExamScore.toFixed(1) : '—'}
                    </span>
                  </div>
                  <div className="flex justify-between items-center pt-1.5">
                    <span className="text-gray-800 dark:text-gray-200 font-bold">{t('scores.courseScore')}:</span>
                    {c.score?.courseScore !== null && c.score?.courseScore !== undefined ? (
                      <span className={`inline-flex items-center px-2 py-0.5 rounded-md text-[11px] font-black border ${getLetterGradeClass(c.score.letterGrade)}`}>
                        {c.score.courseScore.toFixed(1)} ({c.score.letterGrade})
                      </span>
                    ) : (
                      <span className="text-gray-400 font-medium">—</span>
                    )}
                  </div>
                </div>

                {/* Score Action Buttons for Mobile */}
                <div className="flex items-center gap-2 mt-4 pt-3 border-t border-gray-100 dark:border-gray-800/60">
                  <button
                    onClick={() => handleOpenScoreModal(c)}
                    className="flex-1 py-2 px-3 text-xs font-semibold text-violet-700 dark:text-violet-400 bg-violet-50 dark:bg-violet-950/20 hover:bg-violet-100 dark:hover:bg-violet-950/40 rounded-xl transition-all flex items-center justify-center gap-1.5 border border-violet-100 dark:border-violet-950/20"
                  >
                    <Calculator className="h-3.5 w-3.5" />
                    Nhập điểm
                  </button>
                  
                  {(c.score?.attendanceScore !== null || c.score?.continuousScore !== null || c.score?.finalExamScore !== null) && (
                    <button
                      onClick={() => handleOpenDetailModal(c)}
                      className="p-2 text-indigo-700 dark:text-indigo-400 bg-indigo-50 dark:bg-indigo-950/20 rounded-xl border border-indigo-100 dark:border-indigo-950/20"
                      title="Chi tiết & Lịch sử"
                    >
                      <Eye className="h-4 w-4" />
                    </button>
                  )}

                  {(c.score?.attendanceScore !== null || c.score?.continuousScore !== null || c.score?.finalExamScore !== null) && (
                    <button
                      onClick={() => setClearConfirmId(c.id)}
                      className="p-2 text-amber-700 dark:text-amber-400 bg-amber-50 dark:bg-amber-950/20 rounded-xl border border-amber-100 dark:border-amber-950/20"
                      title="Xóa điểm"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        </>
      )}

      {/* CREATE / EDIT COURSE DIALOG */}
      {isDialogOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm transition-opacity">
          <div className="w-full max-w-md bg-white dark:bg-gray-900 rounded-2xl shadow-xl border border-gray-100 dark:border-gray-800 overflow-hidden transform transition-all scale-100">
            {/* Dialog Header */}
            <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 dark:border-gray-800">
              <h3 className="text-lg font-bold text-gray-900 dark:text-white">
                {dialogMode === 'create' ? t('courses.addNew') : t('courses.edit')}
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

              {/* Course Code Input */}
              <div>
                <label className="block text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-1.5">
                  {t('courses.courseCode')}
                </label>
                <input
                  type="text"
                  value={courseCode}
                  onChange={(e) => setCourseCode(e.target.value)}
                  className={`w-full px-3 py-2 border rounded-lg bg-transparent text-sm focus:outline-none focus:ring-2 focus:ring-violet-500/20 focus:border-violet-500 transition-colors dark:text-white ${
                    formErrors.courseCode ? 'border-red-500' : 'border-gray-300 dark:border-gray-700'
                  }`}
                  placeholder="e.g. CS101"
                />
                {formErrors.courseCode && (
                  <p className="text-xs text-red-500 mt-1">{formErrors.courseCode}</p>
                )}
              </div>

              {/* Course Name Input */}
              <div>
                <label className="block text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-1.5">
                  {t('courses.courseName')}
                </label>
                <input
                  type="text"
                  value={courseName}
                  onChange={(e) => setCourseName(e.target.value)}
                  className={`w-full px-3 py-2 border rounded-lg bg-transparent text-sm focus:outline-none focus:ring-2 focus:ring-violet-500/20 focus:border-violet-500 transition-colors dark:text-white ${
                    formErrors.courseName ? 'border-red-500' : 'border-gray-300 dark:border-gray-700'
                  }`}
                  placeholder="e.g. Intro to Computer Science"
                />
                {formErrors.courseName && (
                  <p className="text-xs text-red-500 mt-1">{formErrors.courseName}</p>
                )}
              </div>

              {/* Credits Selection Dropdown */}
              <div>
                <label className="block text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-1.5">
                  {t('courses.credits')}
                </label>
                <select
                  value={credits}
                  onChange={(e) => setCredits(parseInt(e.target.value) || 3)}
                  className={`w-full px-3 py-2 border rounded-lg bg-white dark:bg-gray-900 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500/20 focus:border-violet-500 transition-colors dark:text-white ${
                    formErrors.credits ? 'border-red-500' : 'border-gray-300 dark:border-gray-700'
                  }`}
                >
                  {[1, 2, 3, 4, 5, 6].map(n => (
                    <option key={n} value={n}>{n} tín chỉ</option>
                  ))}
                </select>
                {formErrors.credits && (
                  <p className="text-xs text-red-500 mt-1">{formErrors.credits}</p>
                )}
              </div>

              {/* Is Retake Checkbox */}
              <div className="flex items-center gap-2 pt-2">
                <input
                  type="checkbox"
                  id="isRetakeCheckbox"
                  checked={isRetake}
                  onChange={(e) => setIsRetake(e.target.checked)}
                  className="h-4 w-4 rounded border-gray-300 text-violet-600 focus:ring-violet-500 bg-transparent"
                />
                <label htmlFor="isRetakeCheckbox" className="text-sm font-semibold text-gray-700 dark:text-gray-300">
                  {t('courses.isRetake')}
                </label>
              </div>

              {/* Original Attempt Course Dropdown (conditional) */}
              {isRetake && (
                <div className="pt-2 pl-6 border-l-2 border-amber-400 space-y-2">
                  <label className="block text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    {t('courses.originalCourse')}
                  </label>
                  
                  {loadingOriginals ? (
                    <div className="text-xs text-gray-400 flex items-center gap-1.5">
                      <RefreshCw className="h-3.5 w-3.5 animate-spin" />
                      Đang tìm các lần học trước...
                    </div>
                  ) : eligibleOriginals.length === 0 ? (
                    <div className="p-3 bg-amber-50 dark:bg-amber-950/20 border border-amber-100 dark:border-amber-900/40 rounded-lg text-xs text-amber-700 dark:text-amber-400 flex items-start gap-2">
                      <AlertTriangle className="h-4 w-4 shrink-0 mt-0.5" />
                      <span>
                        Không tìm thấy học phần gốc nào trùng khớp mã '{courseCode || 'N/A'}' để liên kết.
                      </span>
                    </div>
                  ) : (
                    <select
                      value={originalCourseId}
                      onChange={(e) => setOriginalCourseId(e.target.value)}
                      className={`w-full px-3 py-2 border rounded-lg bg-white dark:bg-gray-900 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500/20 focus:border-violet-500 transition-colors dark:text-white ${
                        formErrors.originalCourseId ? 'border-red-500' : 'border-gray-300 dark:border-gray-700'
                      }`}
                    >
                      {eligibleOriginals.map(c => (
                        <option key={c.id} value={c.id}>
                          {c.courseName} (Mã: {c.courseCode})
                        </option>
                      ))}
                    </select>
                  )}
                  {formErrors.originalCourseId && (
                    <p className="text-xs text-red-500 mt-1">{formErrors.originalCourseId}</p>
                  )}
                </div>
              )}

              {/* Action Buttons */}
              <div className="flex items-center justify-end gap-3 pt-4 border-t border-gray-100 dark:border-gray-800">
                <button
                  type="button"
                  onClick={() => setIsDialogOpen(false)}
                  className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 rounded-lg transition-colors border border-gray-200 dark:border-gray-700"
                >
                  {t('courses.cancel')}
                </button>
                <button
                  type="submit"
                  disabled={submitting}
                  className="px-4 py-2 text-sm font-semibold text-white bg-violet-600 hover:bg-violet-700 disabled:bg-violet-400 rounded-lg transition-colors shadow"
                >
                  {submitting ? '...' : t('courses.save')}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* DELETE COURSE CONFIRMATION MODAL */}
      {deleteConfirmId && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm transition-opacity">
          <div className="w-full max-w-md bg-white dark:bg-gray-900 rounded-2xl shadow-xl border border-gray-100 dark:border-gray-800 p-6 space-y-6">
            <div className="flex items-center gap-3 text-red-600">
              <div className="p-2 bg-red-50 dark:bg-red-950/30 rounded-lg">
                <AlertTriangle className="h-6 w-6" />
              </div>
              <h3 className="text-lg font-bold text-gray-900 dark:text-white">
                {t('courses.delete')}
              </h3>
            </div>
            
            <p className="text-sm text-gray-500 dark:text-gray-400">
              {t('courses.deleteConfirm')}
            </p>

            <div className="flex items-center justify-end gap-3">
              <button
                type="button"
                onClick={() => setDeleteConfirmId(null)}
                className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 rounded-lg transition-colors border border-gray-200 dark:border-gray-700"
              >
                {t('courses.cancel')}
              </button>
              <button
                type="button"
                onClick={handleDelete}
                disabled={deleting}
                className="px-4 py-2 text-sm font-semibold text-white bg-red-600 hover:bg-red-700 disabled:bg-red-400 rounded-lg transition-colors shadow"
              >
                {deleting ? '...' : t('courses.delete')}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* --- EDIT scores MODAL (Score Calculator & Conversions) --- */}
      {isScoreModalOpen && selectedCourse && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm transition-opacity">
          <div className="w-full max-w-md bg-white dark:bg-gray-900 rounded-2xl shadow-xl border border-gray-100 dark:border-gray-800 overflow-hidden">
            <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 dark:border-gray-800">
              <h3 className="text-lg font-bold text-gray-900 dark:text-white flex items-center gap-2">
                <Calculator className="h-5 w-5 text-violet-500" />
                {t('scores.editTitle')}: {selectedCourse.courseName}
              </h3>
              <button
                onClick={() => setIsScoreModalOpen(false)}
                className="p-1 rounded-lg text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
              >
                <X className="h-5 w-5" />
              </button>
            </div>

            <form onSubmit={handleSaveScores} className="p-6 space-y-4">
              {scoreErrors.api && (
                <div className="p-3 bg-red-50 dark:bg-red-950/20 border border-red-150 dark:border-red-900/40 rounded-lg text-xs text-red-600 dark:text-red-400">
                  {scoreErrors.api}
                </div>
              )}

              {/* Attendance input with step 0.5 & slider */}
              <div>
                <div className="flex justify-between mb-1">
                  <label className="block text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    {t('scores.attendance')} (Weight: 10%)
                  </label>
                  <span className="text-xs font-bold text-gray-700 dark:text-gray-300">
                    {attendanceInput || '0.0'} / 10
                  </span>
                </div>
                <div className="flex items-center gap-3">
                  <input
                    type="range"
                    min="0"
                    max="10"
                    step="0.5"
                    value={attendanceInput === '' ? '0' : attendanceInput}
                    onChange={(e) => setAttendanceInput(e.target.value)}
                    className="flex-1 accent-violet-600"
                  />
                  <input
                    type="number"
                    min="0"
                    max="10"
                    step="0.5"
                    value={attendanceInput}
                    onChange={(e) => setAttendanceInput(e.target.value)}
                    className={`w-20 px-2.5 py-1.5 text-center text-sm border rounded-lg bg-transparent dark:text-white ${
                      scoreErrors.attendance ? 'border-red-500' : 'border-gray-300 dark:border-gray-700'
                    }`}
                    placeholder="—"
                  />
                </div>
                {scoreErrors.attendance && (
                  <p className="text-xs text-red-500 mt-1">{scoreErrors.attendance}</p>
                )}
              </div>

              {/* Continuous Assessment input with step 0.5 & slider */}
              <div>
                <div className="flex justify-between mb-1">
                  <label className="block text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    {t('scores.continuous')} (Weight: 30%)
                  </label>
                  <span className="text-xs font-bold text-gray-700 dark:text-gray-300">
                    {continuousInput || '0.0'} / 10
                  </span>
                </div>
                <div className="flex items-center gap-3">
                  <input
                    type="range"
                    min="0"
                    max="10"
                    step="0.5"
                    value={continuousInput === '' ? '0' : continuousInput}
                    onChange={(e) => setContinuousInput(e.target.value)}
                    className="flex-1 accent-violet-600"
                  />
                  <input
                    type="number"
                    min="0"
                    max="10"
                    step="0.5"
                    value={continuousInput}
                    onChange={(e) => setContinuousInput(e.target.value)}
                    className={`w-20 px-2.5 py-1.5 text-center text-sm border rounded-lg bg-transparent dark:text-white ${
                      scoreErrors.continuous ? 'border-red-500' : 'border-gray-300 dark:border-gray-700'
                    }`}
                    placeholder="—"
                  />
                </div>
                {scoreErrors.continuous && (
                  <p className="text-xs text-red-500 mt-1">{scoreErrors.continuous}</p>
                )}
              </div>

              {/* Final Exam input with step 0.5 & slider */}
              <div>
                <div className="flex justify-between mb-1">
                  <label className="block text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    {t('scores.final')} (Weight: 60%)
                  </label>
                  <span className="text-xs font-bold text-gray-700 dark:text-gray-300">
                    {finalInput || '0.0'} / 10
                  </span>
                </div>
                <div className="flex items-center gap-3">
                  <input
                    type="range"
                    min="0"
                    max="10"
                    step="0.5"
                    value={finalInput === '' ? '0' : finalInput}
                    onChange={(e) => setFinalInput(e.target.value)}
                    className="flex-1 accent-violet-600"
                  />
                  <input
                    type="number"
                    min="0"
                    max="10"
                    step="0.5"
                    value={finalInput}
                    onChange={(e) => setFinalInput(e.target.value)}
                    className={`w-20 px-2.5 py-1.5 text-center text-sm border rounded-lg bg-transparent dark:text-white ${
                      scoreErrors.finalExam ? 'border-red-500' : 'border-gray-300 dark:border-gray-700'
                    }`}
                    placeholder="—"
                  />
                </div>
                {scoreErrors.finalExam && (
                  <p className="text-xs text-red-500 mt-1">{scoreErrors.finalExam}</p>
                )}
              </div>

              {/* Real-time preview block (Automatic conversion) */}
              <div className="p-4 bg-gradient-to-br from-violet-50 to-indigo-50 dark:from-violet-950/20 dark:to-indigo-950/10 rounded-2xl border border-violet-100/50 dark:border-violet-900/10 flex items-center justify-between">
                <div>
                  <span className="text-xs text-violet-500 dark:text-violet-400 font-bold block mb-1">
                    ƯỚC TÍNH ĐIỂM SỐ (REAL-TIME)
                  </span>
                  {livePreview ? (
                    <div className="space-y-1">
                      <span className="text-2xl font-black text-violet-800 dark:text-violet-300 block">
                        {livePreview.courseScore.toFixed(1)} ({livePreview.letterGrade})
                      </span>
                      <span className="text-xs text-gray-500 dark:text-gray-400 font-medium block">
                        GPA: {livePreview.gpa4Value.toFixed(1)} • {getLocalizedClassification(livePreview.classification)}
                      </span>
                    </div>
                  ) : (
                    <span className="text-sm font-semibold text-gray-400">
                      Hãy nhập đầy đủ 3 thành phần điểm số.
                    </span>
                  )}
                </div>
                {livePreview && (
                  <div>
                    {livePreview.isPass ? (
                      <span className="px-3 py-1 bg-emerald-500 text-white text-xs font-bold rounded-lg uppercase shadow-sm">
                        {t('scores.pass')}
                      </span>
                    ) : (
                      <span className="px-3 py-1 bg-red-500 text-white text-xs font-bold rounded-lg uppercase shadow-sm">
                        {t('scores.fail')}
                      </span>
                    )}
                  </div>
                )}
              </div>

              {/* Form Buttons */}
              <div className="flex items-center justify-end gap-3 pt-4 border-t border-gray-100 dark:border-gray-800">
                <button
                  type="button"
                  onClick={() => setIsScoreModalOpen(false)}
                  className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700"
                >
                  {t('courses.cancel')}
                </button>
                <button
                  type="submit"
                  disabled={savingScores}
                  className="px-5 py-2 text-sm font-semibold text-white bg-violet-600 hover:bg-violet-700 disabled:bg-violet-400 rounded-lg transition-colors shadow"
                >
                  {savingScores ? '...' : t('scores.save')}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* --- GRADE DETAILS & AUDIT TRAIL LOGS MODAL --- */}
      {isDetailModalOpen && selectedDetailCourse && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm transition-opacity">
          <div className="w-full max-w-lg bg-white dark:bg-gray-900 rounded-2xl shadow-xl border border-gray-100 dark:border-gray-800 overflow-hidden">
            
            {/* Header */}
            <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 dark:border-gray-800 bg-gray-50/50 dark:bg-gray-950/20">
              <div>
                <span className="text-xs text-gray-400 font-bold block">{selectedDetailCourse.courseCode}</span>
                <h3 className="text-lg font-bold text-gray-900 dark:text-white">
                  {selectedDetailCourse.courseName}
                </h3>
              </div>
              <button
                onClick={() => setIsDetailModalOpen(false)}
                className="p-1 rounded-lg text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
              >
                <X className="h-5 w-5" />
              </button>
            </div>

            <div className="p-6 space-y-6 max-h-[75vh] overflow-y-auto">
              {loadingDetail ? (
                <div className="flex flex-col items-center justify-center py-10 space-y-2">
                  <RefreshCw className="h-8 w-8 text-violet-500 animate-spin" />
                  <span className="text-sm text-gray-400">Đang tải chi tiết điểm...</span>
                </div>
              ) : detailError ? (
                <div className="p-4 bg-red-50 dark:bg-red-950/20 border border-red-200 dark:border-red-900/40 rounded-xl text-sm text-red-600 dark:text-red-400 flex items-center gap-2">
                  <AlertCircle className="h-5 w-5 shrink-0" />
                  <span>{detailError}</span>
                </div>
              ) : (
                <>
                  {/* Detailed component grid */}
                  <div>
                    <h4 className="text-xs font-bold text-gray-400 uppercase tracking-wider mb-3">
                      THÀNH PHẦN ĐIỂM SỐ
                    </h4>
                    <div className="grid grid-cols-3 gap-4">
                      <div className="p-4 bg-gray-50 dark:bg-gray-950/40 border border-gray-100 dark:border-gray-800 rounded-xl text-center">
                        <span className="text-[10px] text-gray-400 uppercase block font-bold mb-1">Chuyên cần (10%)</span>
                        <span className="text-lg font-black text-gray-900 dark:text-white">
                          {detailScores?.attendanceScore !== null && detailScores?.attendanceScore !== undefined 
                            ? detailScores.attendanceScore.toFixed(1) 
                            : '—'}
                        </span>
                      </div>
                      <div className="p-4 bg-gray-50 dark:bg-gray-950/40 border border-gray-100 dark:border-gray-800 rounded-xl text-center">
                        <span className="text-[10px] text-gray-400 uppercase block font-bold mb-1">Thường xuyên (30%)</span>
                        <span className="text-lg font-black text-gray-900 dark:text-white">
                          {detailScores?.continuousScore !== null && detailScores?.continuousScore !== undefined 
                            ? detailScores.continuousScore.toFixed(1) 
                            : '—'}
                        </span>
                      </div>
                      <div className="p-4 bg-gray-50 dark:bg-gray-950/40 border border-gray-100 dark:border-gray-800 rounded-xl text-center">
                        <span className="text-[10px] text-gray-400 uppercase block font-bold mb-1">Thi cuối kỳ (60%)</span>
                        <span className="text-lg font-black text-gray-900 dark:text-white">
                          {detailScores?.finalExamScore !== null && detailScores?.finalExamScore !== undefined 
                            ? detailScores.finalExamScore.toFixed(1) 
                            : '—'}
                        </span>
                      </div>
                    </div>
                  </div>

                  {/* Calculations Overview */}
                  {detailScores?.courseScore !== null && detailScores?.courseScore !== undefined && (
                    <div className="p-5 bg-gradient-to-br from-violet-50 to-indigo-50 dark:from-violet-950/20 dark:to-indigo-950/10 border border-violet-100/50 dark:border-violet-900/10 rounded-2xl">
                      <h4 className="text-xs font-bold text-violet-500 dark:text-violet-400 uppercase tracking-wider mb-4">
                        KẾT QUẢ QUY ĐỔI HỌC PHẦN
                      </h4>
                      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 text-center sm:text-left">
                        <div>
                          <span className="text-[10px] text-gray-400 uppercase block font-bold">Điểm tổng kết</span>
                          <span className="text-2xl font-black text-violet-700 dark:text-violet-300 block mt-0.5">
                            {detailScores.courseScore.toFixed(1)}
                          </span>
                        </div>
                        <div>
                          <span className="text-[10px] text-gray-400 uppercase block font-bold">Điểm chữ</span>
                          <span className="text-2xl font-black text-violet-700 dark:text-violet-300 block mt-0.5">
                            {detailScores.letterGrade}
                          </span>
                        </div>
                        <div>
                          <span className="text-[10px] text-gray-400 uppercase block font-bold">GPA Hệ 4.0</span>
                          <span className="text-2xl font-black text-violet-700 dark:text-violet-300 block mt-0.5">
                            {detailScores.gpa4Value?.toFixed(1)}
                          </span>
                        </div>
                        <div>
                          <span className="text-[10px] text-gray-400 uppercase block font-bold">Xếp loại</span>
                          <span className="text-lg font-black text-violet-700 dark:text-violet-300 block mt-1.5">
                            {detailScores.academicClassification ? getLocalizedClassification(detailScores.academicClassification) : '—'}
                          </span>
                        </div>
                      </div>
                      <div className="flex items-center justify-between border-t border-violet-200/40 dark:border-violet-800/20 pt-3 mt-4">
                        <span className="text-xs text-gray-400 flex items-center gap-1">
                          <Calendar className="h-3.5 w-3.5" />
                          Cập nhật: {detailScores.calculatedAt ? new Date(detailScores.calculatedAt).toLocaleString('vi-VN') : 'N/A'}
                        </span>
                        {getPassFailBadge(detailScores.isPass)}
                      </div>
                    </div>
                  )}

                  {/* Change History Audit Log */}
                  <div>
                    <h4 className="text-xs font-bold text-gray-400 uppercase tracking-wider mb-3 flex items-center gap-1.5">
                      <History className="h-4 w-4" />
                      {t('scores.auditLogs')}
                    </h4>
                    {auditLogs.length === 0 ? (
                      <div className="p-6 text-center border border-dashed border-gray-200 dark:border-gray-800 rounded-xl text-sm text-gray-400">
                        Chưa ghi nhận sự thay đổi điểm số nào.
                      </div>
                    ) : (
                      <div className="overflow-x-auto border border-gray-100 dark:border-gray-800 rounded-xl">
                        <table className="w-full text-left text-xs border-collapse">
                          <thead>
                            <tr className="bg-gray-50 dark:bg-gray-950/40 text-gray-500 font-bold border-b border-gray-100 dark:border-gray-800">
                              <th className="px-4 py-3">{t('scores.field')}</th>
                              <th className="px-4 py-3 text-center">{t('scores.oldVal')}</th>
                              <th className="px-4 py-3 text-center">{t('scores.newVal')}</th>
                              <th className="px-4 py-3 text-right">{t('scores.changedAt')}</th>
                            </tr>
                          </thead>
                          <tbody className="divide-y divide-gray-100 dark:divide-gray-800/80 text-gray-600 dark:text-gray-400 font-medium">
                            {auditLogs.map((log, i) => (
                              <tr key={i} className="hover:bg-gray-50/50 dark:hover:bg-gray-800/20">
                                <td className="px-4 py-3">
                                  {log.fieldChanged === 'AttendanceScore' ? t('scores.attendance') :
                                   log.fieldChanged === 'ContinuousScore' ? t('scores.continuous') :
                                   log.fieldChanged === 'FinalExamScore' ? t('scores.final') : log.fieldChanged}
                                </td>
                                <td className="px-4 py-3 text-center font-bold text-gray-400 line-through">
                                  {log.oldValue || '—'}
                                </td>
                                <td className="px-4 py-3 text-center font-bold text-violet-600 dark:text-violet-400">
                                  {log.newValue || '—'}
                                </td>
                                <td className="px-4 py-3 text-right text-gray-400">
                                  {new Date(log.changedAt).toLocaleString('vi-VN')}
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    )}
                  </div>
                </>
              )}
            </div>

            {/* Footer buttons */}
            <div className="flex items-center justify-end px-6 py-4 border-t border-gray-100 dark:border-gray-800 bg-gray-50/50 dark:bg-gray-950/20">
              <button
                type="button"
                onClick={() => setIsDetailModalOpen(false)}
                className="px-5 py-2 text-sm font-semibold text-white bg-violet-600 hover:bg-violet-700 rounded-lg transition-colors shadow"
              >
                Đóng
              </button>
            </div>

          </div>
        </div>
      )}

      {/* --- CLEAR SCORE CONFIRMATION MODAL --- */}
      {clearConfirmId && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm transition-opacity">
          <div className="w-full max-w-md bg-white dark:bg-gray-900 rounded-2xl shadow-xl border border-gray-100 dark:border-gray-800 p-6 space-y-6">
            <div className="flex items-center gap-3 text-amber-600">
              <div className="p-2 bg-amber-50 dark:bg-amber-950/30 rounded-lg">
                <AlertTriangle className="h-6 w-6" />
              </div>
              <h3 className="text-lg font-bold text-gray-900 dark:text-white">
                {t('scores.clear')}
              </h3>
            </div>
            
            <p className="text-sm text-gray-500 dark:text-gray-400">
              {t('scores.clearConfirm')}
            </p>

            <div className="flex items-center justify-end gap-3">
              <button
                type="button"
                onClick={() => setClearConfirmId(null)}
                className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700"
              >
                {t('courses.cancel')}
              </button>
              <button
                type="button"
                onClick={handleClearScores}
                disabled={clearing}
                className="px-4 py-2 text-sm font-semibold text-white bg-amber-600 hover:bg-amber-700 disabled:bg-amber-400 rounded-lg transition-colors shadow"
              >
                {clearing ? '...' : t('scores.clear')}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* TRANSCRIPT IMPORT MODAL */}
      <TranscriptImportModal 
        isOpen={isImportModalOpen}
        onClose={() => setIsImportModalOpen(false)}
        semesterId={semesterId!}
        onParse={handleParseTranscript}
        onConfirm={handleConfirmTranscriptImport}
      />

      <ImportHistoryPanel 
        batches={importHistory} 
        onUndo={(batchId) => {
          setImportHistory(prev => prev.filter(b => b.id !== batchId));
          alert(`Đã hoàn tác batch: ${batchId}`);
        }} 
      />
    </div>
  );
};

export default CoursesPage;
