import React, { useEffect, useState } from 'react';
import { useLanguage } from '../contexts/LanguageContext';
import { gpaApi, CumulativeGpaDto, GpaClassificationDto, AcademicYearGpaDto, SemesterGpaDto } from '../api/gpaApi';
import { academicYearApi, AcademicYearDto } from '../api/academicYearApi';
import { semesterApi, SemesterDto } from '../api/semesterApi';
import { courseApi, CourseDto } from '../api/courseApi';
import {
  GraduationCap,
  Award,
  Calendar,
  BookOpen,
  ChevronDown,
  ChevronUp,
  Loader2,
  CheckCircle2,
  XCircle,
  TrendingUp,
  Sparkles,
  BookMarked,
  Info
} from 'lucide-react';
import { ExportPanel } from '../components/ExportPanel';

export const GpaPage: React.FC = () => {
  const { t, language } = useLanguage();
  const summaryRef = React.useRef<HTMLDivElement>(null);

  // Cumulative data states
  const [cumulativeGpa, setCumulativeGpa] = useState<CumulativeGpaDto | null>(null);
  const [classification, setClassification] = useState<GpaClassificationDto | null>(null);
  const [loadingCumulative, setLoadingCumulative] = useState(true);

  // Academic years states
  const [academicYears, setAcademicYears] = useState<AcademicYearDto[]>([]);
  const [selectedYearId, setSelectedYearId] = useState<string>('');
  const [selectedYearGpa, setSelectedYearGpa] = useState<AcademicYearGpaDto | null>(null);
  const [loadingYears, setLoadingYears] = useState(true);
  const [loadingYearDetails, setLoadingYearDetails] = useState(false);

  // Semesters & Courses nested states
  const [semesters, setSemesters] = useState<SemesterDto[]>([]);
  const [semesterGpas, setSemesterGpas] = useState<Record<string, SemesterGpaDto>>({});
  const [semesterCourses, setSemesterCourses] = useState<Record<string, CourseDto[]>>({});
  const [expandedSemesters, setExpandedSemesters] = useState<Record<string, boolean>>({});

  const [error, setError] = useState<string | null>(null);

  // Formatting helper for GPA scores to handle empty/null states
  const formatGpa = (value: number | null | undefined): string => {
    if (value === null || value === undefined) return '—';
    return value.toFixed(2);
  };

  // Fetch cumulative aggregates & academic years list on load
  useEffect(() => {
    const initPageData = async () => {
      try {
        setLoadingCumulative(true);
        setLoadingYears(true);
        setError(null);

        // Fetch cumulative GPA & classification in parallel
        const [cumulRes, classRes, yearsRes] = await Promise.all([
          gpaApi.getCumulativeGpa(),
          gpaApi.getGpaClassification(),
          academicYearApi.getAcademicYears()
        ]);

        if (cumulRes.success) {
          setCumulativeGpa(cumulRes.data);
        }
        if (classRes.success) {
          setClassification(classRes.data);
        }
        if (yearsRes.success) {
          const yearsList = yearsRes.data || [];
          setAcademicYears(yearsList);
          
          // Select default academic year: "current" if available, else first in list
          if (yearsList.length > 0) {
            const currentYear = yearsList.find(y => y.isCurrent);
            setSelectedYearId(currentYear ? currentYear.id : yearsList[0].id);
          }
        }
      } catch (err: any) {
        console.error('Failed to initialize GPA data:', err);
        setError('Failed to load cumulative academic records.');
      } finally {
        setLoadingCumulative(false);
        setLoadingYears(false);
      }
    };

    initPageData();
  }, []);

  // Fetch detailed year GPA & nested semester data whenever active Academic Year changes
  useEffect(() => {
    if (!selectedYearId) return;

    const fetchYearData = async () => {
      try {
        setLoadingYearDetails(true);
        
        // 1. Fetch Year GPA & Year's Semesters
        const [yearGpaRes, semestersRes] = await Promise.all([
          gpaApi.getAcademicYearGpa(selectedYearId),
          semesterApi.getSemesters(selectedYearId)
        ]);

        if (yearGpaRes.success) {
          setSelectedYearGpa(yearGpaRes.data);
        }

        if (semestersRes.success) {
          const semesterList = semestersRes.data || [];
          setSemesters(semesterList);

          // Initialize accordions as collapsed
          const initialExpanded: Record<string, boolean> = {};
          semesterList.forEach(sem => {
            initialExpanded[sem.id] = false;
          });
          setExpandedSemesters(initialExpanded);

          // 2. Fetch all Semester GPAs & courses in parallel
          const gpaPromises = semesterList.map(sem => gpaApi.getSemesterGpa(sem.id));
          const coursesPromises = semesterList.map(sem => courseApi.getCourses(sem.id));

          const gpaResults = await Promise.all(gpaPromises);
          const coursesResults = await Promise.all(coursesPromises);

          const tempGpas: Record<string, SemesterGpaDto> = {};
          const tempCourses: Record<string, CourseDto[]> = {};

          semesterList.forEach((sem, idx) => {
            const gpaRes = gpaResults[idx];
            const courseRes = coursesResults[idx];

            if (gpaRes.success && gpaRes.data) {
              tempGpas[sem.id] = gpaRes.data;
            }
            if (courseRes.success && courseRes.data) {
              tempCourses[sem.id] = courseRes.data;
            }
          });

          setSemesterGpas(tempGpas);
          setSemesterCourses(tempCourses);
        }
      } catch (err) {
        console.error('Failed to load academic year details:', err);
      } finally {
        setLoadingYearDetails(false);
      }
    };

    fetchYearData();
  }, [selectedYearId]);

  // Toggle Semester Card Expand/Collapse
  const toggleSemester = (semesterId: string) => {
    setExpandedSemesters(prev => ({
      ...prev,
      [semesterId]: !prev[semesterId]
    }));
  };

  // Determine styling for Academic Classification badge
  const getClassificationStyles = (classEn: string | null | undefined) => {
    const base = 'px-3.5 py-1.5 rounded-full text-xs font-bold border shadow-sm transition-all duration-300';
    if (!classEn) return `${base} bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300 border-gray-200 dark:border-gray-700`;

    switch (classEn.toLowerCase()) {
      case 'excellent':
        return `${base} bg-purple-50 text-purple-700 dark:bg-purple-950/30 dark:text-purple-300 border-purple-200 dark:border-purple-800/60 animate-pulse`;
      case 'very good':
        return `${base} bg-indigo-50 text-indigo-700 dark:bg-indigo-950/30 dark:text-indigo-300 border-indigo-200 dark:border-indigo-800/60`;
      case 'good':
        return `${base} bg-emerald-50 text-emerald-700 dark:bg-emerald-950/30 dark:text-emerald-300 border-emerald-200 dark:border-emerald-800/60`;
      case 'average good':
        return `${base} bg-amber-50 text-amber-700 dark:bg-amber-950/30 dark:text-amber-300 border-amber-200 dark:border-amber-800/60`;
      case 'average':
        return `${base} bg-orange-50 text-orange-700 dark:bg-orange-950/30 dark:text-orange-300 border-orange-200 dark:border-orange-800/60`;
      case 'weak':
        return `${base} bg-red-50 text-red-700 dark:bg-red-950/30 dark:text-red-300 border-red-200 dark:border-red-800/60`;
      default:
        return `${base} bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300 border-gray-200 dark:border-gray-700`;
    }
  };

  const getPassFailBadge = (isPass: boolean | null | undefined, score: number | null | undefined) => {
    if (score === null || score === undefined) {
      return (
        <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-semibold bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400">
          —
        </span>
      );
    }
    
    return isPass ? (
      <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-bold bg-green-50 text-green-700 border border-green-200 dark:bg-green-950/30 dark:text-green-300 dark:border-green-800/60">
        <CheckCircle2 className="h-3 w-3" />
        {t('scores.pass')}
      </span>
    ) : (
      <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-bold bg-red-50 text-red-700 border border-red-200 dark:bg-red-950/30 dark:text-red-300 dark:border-red-800/60">
        <XCircle className="h-3 w-3" />
        {t('scores.fail')}
      </span>
    );
  };

  if (loadingCumulative || loadingYears) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Loader2 className="h-10 w-10 animate-spin text-brand-500" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-2xl p-6 dark:bg-red-950/20 dark:border-red-900/50 text-red-700 dark:text-red-300 max-w-2xl mx-auto mt-8">
        <div className="flex items-center gap-3">
          <XCircle className="h-6 w-6" />
          <span className="font-bold text-lg">Error occurred</span>
        </div>
        <p className="mt-2 text-sm">{error}</p>
      </div>
    );
  }

  // If there are no academic years at all, display empty state
  const hasAcademicRecords = academicYears.length > 0;

  const handleExportPdf = async () => {
    // Call backend API for PDF export
    const response = await fetch('http://localhost:5000/api/v1/export/pdf', {
      headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
    });
    if (!response.ok) throw new Error('Không thể xuất PDF từ máy chủ.');
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `GPA_Report.pdf`;
    a.click();
  };

  const handleExportExcel = async () => {
    // Call backend API for Excel export
    const response = await fetch('http://localhost:5000/api/v1/export/excel', {
      headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
    });
    if (!response.ok) throw new Error('Không thể xuất Excel từ máy chủ.');
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `GPA_Report.xlsx`;
    a.click();
  };

  return (
    <div className="mx-auto max-w-6xl space-y-8 animate-in fade-in duration-500">
      {/* Page Header */}
      <div>
        <h1 className="text-3xl font-extrabold text-gray-900 dark:text-white tracking-tight flex items-center gap-2.5">
          <GraduationCap className="h-8 w-8 text-brand-500" />
          {t('gpa.title')}
        </h1>
        <p className="text-gray-500 dark:text-gray-400 mt-1">
          {t('gpa.subtitle')}
        </p>
      </div>

      {!hasAcademicRecords ? (
        <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-12 text-center shadow-sm">
          <div className="flex justify-center mb-4">
            <div className="p-4 bg-brand-50 rounded-2xl dark:bg-brand-950/30 text-brand-500">
              <BookMarked className="h-12 w-12" />
            </div>
          </div>
          <h3 className="text-xl font-bold text-gray-800 dark:text-white mb-2">
            No academic data setup yet
          </h3>
          <p className="text-gray-500 dark:text-gray-400 max-w-md mx-auto mb-6">
            Please add an Academic Year and semesters to begin tracking courses, inputting scores, and calculating GPAs.
          </p>
        </div>
      ) : (
        <>
          <div ref={summaryRef} className="bg-transparent space-y-8 pb-4">
            {/* Cumulative GPA and Credit Progress Dashboard */}
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Cumulative GPA Card */}
            <div className="lg:col-span-2 bg-gradient-to-br from-brand-500 via-brand-600 to-indigo-700 text-white rounded-3xl p-6 shadow-xl shadow-brand-500/10 flex flex-col justify-between min-h-[220px] transition-all duration-300 hover:scale-[1.01]">
              <div className="flex justify-between items-start">
                <div>
                  <span className="text-white/80 text-xs font-bold uppercase tracking-wider bg-white/10 px-3 py-1 rounded-full backdrop-blur-md">
                    {t('gpa.cumulative')}
                  </span>
                  <div className="flex items-baseline gap-4 mt-4">
                    <h2 className="text-5xl font-black tracking-tight select-none">
                      {formatGpa(cumulativeGpa?.cumulativeGpa10)}
                    </h2>
                    <span className="text-white/70 text-sm font-medium">/ 10.0</span>
                  </div>
                  <div className="flex items-center gap-1.5 mt-1 text-white/90">
                    <span className="text-2xl font-bold">{formatGpa(cumulativeGpa?.cumulativeGpa4)}</span>
                    <span className="text-white/70 text-xs">/ 4.0 ({t('gpa.average4')})</span>
                  </div>
                </div>

                {classification && (
                  <div className="flex flex-col items-end gap-1.5">
                    <span className="text-white/60 text-xs font-semibold uppercase">{t('gpa.academicClassification')}</span>
                    <span className={getClassificationStyles(classification.classificationEn)}>
                      {language === 'vi' ? classification.classificationVn : classification.classificationEn}
                    </span>
                  </div>
                )}
              </div>

              {/* Progress Bar of Completed Credits */}
              {cumulativeGpa && (
                <div className="space-y-2 mt-6 pt-4 border-t border-white/10">
                  <div className="flex justify-between items-center text-xs font-bold text-white/90">
                    <span className="flex items-center gap-1">
                      <BookOpen className="h-3.5 w-3.5" />
                      {t('gpa.credits.completed')}: {cumulativeGpa.totalCreditsCompleted} / {cumulativeGpa.totalCreditsRequired}
                    </span>
                    <span>{cumulativeGpa.completionPercentage}%</span>
                  </div>
                  <div className="w-full bg-white/20 h-2.5 rounded-full overflow-hidden">
                    <div
                      className="bg-white h-full rounded-full transition-all duration-1000 ease-out"
                      style={{ width: `${Math.min(cumulativeGpa.completionPercentage, 100)}%` }}
                    />
                  </div>
                </div>
              )}
            </div>

            {/* General Information or Tips Panel */}
            <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm flex flex-col justify-between">
              <div>
                <h3 className="text-lg font-extrabold text-gray-900 dark:text-white flex items-center gap-2">
                  <Sparkles className="h-5 w-5 text-yellow-500" />
                  Academic Insights
                </h3>
                <div className="mt-4 space-y-3.5 text-sm text-gray-600 dark:text-gray-400">
                  <div className="flex gap-2.5 items-start">
                    <div className="p-1.5 rounded-lg bg-brand-50 text-brand-500 dark:bg-brand-950/30 mt-0.5">
                      <TrendingUp className="h-4 w-4" />
                    </div>
                    <p>
                      GPA updates dynamically as component scores are recorded. Failures ($Score &lt; 4.0$) are factored in until successfully retaken.
                    </p>
                  </div>
                  <div className="flex gap-2.5 items-start">
                    <div className="p-1.5 rounded-lg bg-brand-50 text-brand-500 dark:bg-brand-950/30 mt-0.5">
                      <Info className="h-4 w-4" />
                    </div>
                    <p>
                      <strong>Retake Policy:</strong> For cumulative averages, the system automatically deduplicates and keeps ONLY your highest attempt.
                    </p>
                  </div>
                </div>
              </div>

              {classification && classification.cumulativeGpa10 !== null && (
                <div className="mt-4 bg-gray-50 dark:bg-gray-950 p-3.5 rounded-2xl border border-gray-100 dark:border-gray-800 text-xs flex justify-between items-center text-gray-500 dark:text-gray-400">
                  <span>Current Class threshold:</span>
                  <span className="font-semibold text-gray-800 dark:text-gray-200">
                    &ge; {classification.minimumThresholdGpa10.toFixed(1)}
                  </span>
                </div>
              )}
            </div>
          </div>

          <ExportPanel
            targetRef={summaryRef}
            onExportPdf={handleExportPdf}
            onExportExcel={handleExportExcel}
          />

          {/* Academic Year Navigation & Details Section */}
          <div className="space-y-6">
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 border-b border-gray-200 dark:border-gray-800 pb-3">
              <h2 className="text-xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
                <Calendar className="h-5 w-5 text-gray-400" />
                {t('gpa.yearSelect')}
              </h2>

              {/* Horizontal pills list to choose Academic Year */}
              <div className="flex flex-wrap gap-2">
                {academicYears.map(yr => (
                  <button
                    key={yr.id}
                    onClick={() => setSelectedYearId(yr.id)}
                    className={`px-4 py-2 rounded-xl text-sm font-semibold transition-all border ${
                      selectedYearId === yr.id
                        ? 'bg-brand-500 text-white border-brand-500 shadow-md shadow-brand-500/10'
                        : 'bg-white dark:bg-gray-900 text-gray-700 dark:text-gray-300 border-gray-200 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800'
                    }`}
                  >
                    {yr.yearName} {yr.isCurrent && `(${t('academicyears.current')})`}
                  </button>
                ))}
              </div>
            </div>

            {loadingYearDetails ? (
              <div className="flex py-12 justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-brand-500" />
              </div>
            ) : (
              <div className="grid grid-cols-1 lg:grid-cols-4 gap-6 items-start">
                
                {/* Left Side: Selected Year Aggregates Summary */}
                <div className="lg:col-span-1 bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-5 shadow-sm space-y-5">
                  <div className="border-b border-gray-100 dark:border-gray-800 pb-3">
                    <h3 className="text-base font-extrabold text-gray-900 dark:text-white">
                      {t('gpa.year')}
                    </h3>
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                      Averages and credits overview for the selected academic year.
                    </p>
                  </div>

                  <div className="space-y-4">
                    {/* GPA 10-scale */}
                    <div>
                      <span className="text-xs text-gray-400 font-medium uppercase tracking-wider">{t('gpa.average10')}</span>
                      <p className="text-3xl font-extrabold text-gray-900 dark:text-white mt-0.5">
                        {formatGpa(selectedYearGpa?.gpa10)}
                      </p>
                    </div>

                    {/* GPA 4-scale */}
                    <div>
                      <span className="text-xs text-gray-400 font-medium uppercase tracking-wider">{t('gpa.average4')}</span>
                      <p className="text-xl font-bold text-gray-850 dark:text-gray-200 mt-0.5">
                        {formatGpa(selectedYearGpa?.gpa4)}
                      </p>
                    </div>

                    {/* Credits split details */}
                    <div className="grid grid-cols-2 gap-3.5 pt-4 border-t border-gray-100 dark:border-gray-800">
                      <div>
                        <span className="text-xs text-green-500 font-bold block">{t('gpa.passedCredits')}</span>
                        <span className="text-lg font-bold text-gray-850 dark:text-gray-200">{selectedYearGpa?.passedCredits || 0}</span>
                      </div>
                      <div>
                        <span className="text-xs text-red-500 font-bold block">{t('gpa.failedCredits')}</span>
                        <span className="text-lg font-bold text-gray-850 dark:text-gray-200">{selectedYearGpa?.failedCredits || 0}</span>
                      </div>
                    </div>

                    <div className="pt-2">
                      <span className="text-xs text-gray-400 font-semibold block">Total Registered Credits</span>
                      <span className="text-sm font-bold text-gray-800 dark:text-gray-200">{selectedYearGpa?.totalCredits || 0}</span>
                    </div>
                  </div>
                </div>

                {/* Right Side: Nested Semesters List Accordions */}
                <div className="lg:col-span-3 space-y-4">
                  {semesters.length === 0 ? (
                    <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-8 text-center text-gray-500 dark:text-gray-400 shadow-sm">
                      {t('semesters.empty')}
                    </div>
                  ) : (
                    semesters.map(sem => {
                      const gpaDetails = semesterGpas[sem.id];
                      const coursesList = semesterCourses[sem.id] || [];
                      const isExpanded = !!expandedSemesters[sem.id];

                      return (
                        <div
                          key={sem.id}
                          className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-2xl shadow-sm overflow-hidden transition-all duration-300"
                        >
                          {/* Accordion Trigger Header */}
                          <div
                            onClick={() => toggleSemester(sem.id)}
                            className="flex items-center justify-between p-5 cursor-pointer select-none hover:bg-gray-50/50 dark:hover:bg-gray-850/50 transition-colors"
                          >
                            <div className="flex flex-wrap items-center gap-x-6 gap-y-2">
                              <h4 className="font-extrabold text-gray-900 dark:text-white text-base">
                                {sem.semesterName}
                              </h4>
                              
                              <div className="flex items-center gap-4 text-sm">
                                <div>
                                  <span className="text-gray-400 text-xs font-semibold mr-1.5 uppercase">GPA:</span>
                                  <span className="font-bold text-brand-600 dark:text-brand-400">
                                    {formatGpa(gpaDetails?.gpa10)}
                                  </span>
                                  <span className="text-gray-400 text-xs font-medium ml-1">({formatGpa(gpaDetails?.gpa4)}/4)</span>
                                </div>
                                <div className="text-gray-400 dark:text-gray-500 font-semibold text-xs border-l border-gray-200 dark:border-gray-800 pl-4">
                                  {sem.completedCredits || gpaDetails?.passedCredits || 0} Credits Passed
                                </div>
                              </div>
                            </div>

                            <div className="text-gray-400 dark:text-gray-500">
                              {isExpanded ? <ChevronUp className="h-5 w-5" /> : <ChevronDown className="h-5 w-5" />}
                            </div>
                          </div>

                          {/* Accordion Collapsible Panel */}
                          {isExpanded && (
                            <div className="border-t border-gray-150 dark:border-gray-800/80 bg-gray-50/30 dark:bg-gray-950/20 p-5 animate-in slide-in-from-top-2 duration-200">
                              {coursesList.length === 0 ? (
                                <p className="text-center py-4 text-sm text-gray-500 dark:text-gray-400 italic">
                                  {t('courses.empty')}
                                </p>
                              ) : (
                                <div className="overflow-x-auto rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900">
                                  {/* Responsive Courses Table */}
                                  <table className="w-full text-left text-sm border-collapse">
                                    <thead>
                                      <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-250 dark:border-gray-700/80 text-xs text-gray-500 dark:text-gray-400 font-bold uppercase select-none">
                                        <th className="px-4 py-3.5">{t('courses.courseCode')}</th>
                                        <th className="px-4 py-3.5">{t('courses.courseName')}</th>
                                        <th className="px-4 py-3.5 text-center">{t('courses.credits')}</th>
                                        <th className="px-4 py-3.5 text-center">{t('scores.courseScore')} (10)</th>
                                        <th className="px-4 py-3.5 text-center">{t('scores.letterGrade')}</th>
                                        <th className="px-4 py-3.5 text-center">{t('scores.gpa4')}</th>
                                        <th className="px-4 py-3.5 text-center">{t('scores.status')}</th>
                                      </tr>
                                    </thead>
                                    <tbody className="divide-y divide-gray-150 dark:divide-gray-800/60">
                                      {coursesList.map(c => {
                                        const score = c.score;
                                        
                                        return (
                                          <tr key={c.id} className="hover:bg-gray-50/30 dark:hover:bg-gray-850/20 transition-colors">
                                            <td className="px-4 py-3 font-semibold text-gray-700 dark:text-gray-300">
                                              {c.courseCode}
                                            </td>
                                            <td className="px-4 py-3">
                                              <div className="flex flex-col">
                                                <span className="font-semibold text-gray-900 dark:text-white">{c.courseName}</span>
                                                {c.isRetake && (
                                                  <span className="mt-0.5 inline-flex self-start px-2 py-0.5 rounded text-[10px] font-bold bg-amber-50 text-amber-700 border border-amber-200 dark:bg-amber-950/20 dark:text-amber-400 dark:border-amber-900/60 uppercase tracking-wide">
                                                    {t('courses.isRetake')}
                                                  </span>
                                                )}
                                              </div>
                                            </td>
                                            <td className="px-4 py-3 text-center font-bold text-gray-800 dark:text-gray-250">
                                              {c.credits}
                                            </td>
                                            <td className="px-4 py-3 text-center font-extrabold text-gray-900 dark:text-white">
                                              {score?.courseScore !== undefined && score?.courseScore !== null 
                                                ? score.courseScore.toFixed(1) 
                                                : <span className="text-gray-400">—</span>}
                                            </td>
                                            <td className="px-4 py-3 text-center font-bold text-brand-600 dark:text-brand-400">
                                              {score?.letterGrade || <span className="text-gray-400">—</span>}
                                            </td>
                                            <td className="px-4 py-3 text-center font-bold text-gray-800 dark:text-gray-200">
                                              {score?.gpa4Value !== undefined && score?.gpa4Value !== null 
                                                ? score.gpa4Value.toFixed(2) 
                                                : <span className="text-gray-400">—</span>}
                                            </td>
                                            <td className="px-4 py-3 text-center">
                                              {getPassFailBadge(score?.isPass, score?.courseScore)}
                                            </td>
                                          </tr>
                                        );
                                      })}
                                    </tbody>
                                  </table>
                                </div>
                              )}
                            </div>
                          )}
                        </div>
                      );
                    })
                  )}
                </div>

              </div>
            )}
          </div>
          </div>
        </>
      )}
    </div>
  );
};

export default GpaPage;
