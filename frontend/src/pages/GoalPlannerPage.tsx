import React, { useEffect, useState } from 'react';
import { useLanguage } from '../contexts/LanguageContext';
import { predictionApi, FinalScorePredictionDto, ScenarioPredictionDto } from '../api/predictionApi';
import { goalApi, GoalDto, RequiredGpaDto, SimulationResultDto } from '../api/goalApi';
import { academicYearApi } from '../api/academicYearApi';
import { semesterApi } from '../api/semesterApi';
import { courseApi, CourseDto } from '../api/courseApi';
import {
  Target,
  Sparkles,
  Calculator,
  Compass,
  TrendingUp,
  Award,
  BookOpen,
  Calendar,
  AlertTriangle,
  CheckCircle2,
  XCircle,
  HelpCircle,
  PlusCircle,
  History,
  RotateCcw,
  Play,
  Loader2,
  FileText,
  Bookmark
} from 'lucide-react';

export const GoalPlannerPage: React.FC = () => {
  const { t, language } = useLanguage();

  // Active Tab: 'prediction' | 'goals' | 'simulation'
  const [activeTab, setActiveTab] = useState<'prediction' | 'goals' | 'simulation'>('prediction');

  // Loading & Error States
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  // ----------------------------------------------------
  // Tab 1: Final Exam Prediction State
  // ----------------------------------------------------
  const [predictAttendance, setPredictAttendance] = useState<number>(8.5);
  const [predictContinuous, setPredictContinuous] = useState<number>(8.0);
  const [targetGrade, setTargetGrade] = useState<string>('A');
  const [customTargetScore, setCustomTargetScore] = useState<string>('');
  const [predictionResult, setPredictionResult] = useState<FinalScorePredictionDto | null>(null);
  const [scenarios, setScenarios] = useState<ScenarioPredictionDto[]>([]);
  const [loadingPrediction, setLoadingPrediction] = useState(false);

  // ----------------------------------------------------
  // Tab 2: Goal Planner State
  // ----------------------------------------------------
  const [goals, setGoals] = useState<GoalDto[]>([]);
  const [requiredGpa, setRequiredGpa] = useState<RequiredGpaDto | null>(null);
  const [targetGpaInput, setTargetGpaInput] = useState<string>('');
  const [goalNotes, setGoalNotes] = useState<string>('');
  const [loadingRequiredGpa, setLoadingRequiredGpa] = useState(false);
  const [loadingGoalsList, setLoadingGoalsList] = useState(false);

  // ----------------------------------------------------
  // Tab 3: What-If Simulation State
  // ----------------------------------------------------
  const [currentSemesterCourses, setCurrentSemesterCourses] = useState<CourseDto[]>([]);
  const [simulatedScores, setSimulatedScores] = useState<Record<string, { attendance: number; continuous: number; final: number }>>({});
  const [simulationResult, setSimulationResult] = useState<SimulationResultDto | null>(null);
  const [loadingCourses, setLoadingCourses] = useState(false);
  const [loadingSimulation, setLoadingSimulation] = useState(false);
  const [currentSemName, setCurrentSemName] = useState<string>('');

  // Fetch initial data
  useEffect(() => {
    if (activeTab === 'goals') {
      fetchGoalsAndAnalysis();
    } else if (activeTab === 'simulation') {
      fetchCurrentSemesterCourses();
    }
  }, [activeTab]);

  // ----------------------------------------------------
  // Tab 1: Prediction Handlers
  // ----------------------------------------------------
  const handleCalculatePrediction = async () => {
    try {
      setLoadingPrediction(true);
      setError(null);
      
      const reqGrade = customTargetScore ? '' : targetGrade;
      // If custom score is specified, we can compute prediction for custom score threshold
      // Wait, our backend POST /final-score takes PredictFinalScoreCommand: AttendanceScore, ContinuousScore, TargetGrade.
      // If the user wants a custom target score, we map it. Wait, the backend PredictFinalScoreCommand validator
      // requires targetGrade to be a valid letter grade, OR does it support custom?
      // Let's check: our PredictFinalScoreCommand only takes targetGrade.
      // But we can fetch all scenarios to show them what they need! Or calculate locally if custom target.
      // Let's call the API with the selected targetGrade.
      const res = await predictionApi.predictFinalScore({
        attendanceScore: predictAttendance,
        continuousScore: predictContinuous,
        targetGrade: reqGrade || 'A' // Fallback
      });

      if (res.success) {
        setPredictionResult(res.data);
      } else {
        setError(res.message || 'Failed to calculate prediction.');
      }
    } catch (err: any) {
      console.error(err);
      setError('An error occurred during prediction calculation.');
    } finally {
      setLoadingPrediction(false);
    }
  };

  const handleShowScenarios = async () => {
    try {
      setLoadingPrediction(true);
      setError(null);
      const res = await predictionApi.getPredictionScenarios({
        attendanceScore: predictAttendance,
        continuousScore: predictContinuous
      });
      if (res.success) {
        setScenarios(res.data);
      } else {
        setError(res.message || 'Failed to load scenarios.');
      }
    } catch (err: any) {
      console.error(err);
      setError('An error occurred while fetching scenarios.');
    } finally {
      setLoadingPrediction(false);
    }
  };

  // ----------------------------------------------------
  // Tab 2: Goal Planner Handlers
  // ----------------------------------------------------
  const fetchGoalsAndAnalysis = async () => {
    try {
      setLoadingGoalsList(true);
      setLoadingRequiredGpa(true);
      setError(null);

      // Fetch goals history
      const goalsRes = await goalApi.getGoals();
      if (goalsRes.success) {
        setGoals(goalsRes.data || []);
      }

      // Fetch analysis (only works if there is an active goal)
      const analysisRes = await goalApi.getRequiredGpa();
      if (analysisRes.success) {
        setRequiredGpa(analysisRes.data);
      } else {
        setRequiredGpa(null);
      }
    } catch (err: any) {
      console.error(err);
      // Suppress 422 error since it just means "no active goal"
      setRequiredGpa(null);
    } finally {
      setLoadingGoalsList(false);
      setLoadingRequiredGpa(false);
    }
  };

  const handleCreateGoal = async (e: React.FormEvent) => {
    e.preventDefault();
    const gpa = parseFloat(targetGpaInput);
    if (isNaN(gpa) || gpa < 0 || gpa > 10) {
      setError(language === 'vi' ? 'GPA mục tiêu phải từ 0.00 đến 10.00' : 'Target GPA must be between 0.00 and 10.00');
      return;
    }

    try {
      setLoading(true);
      setError(null);
      setSuccessMsg(null);
      
      const res = await goalApi.setGoal({
        targetCumulativeGpa10: gpa,
        notes: goalNotes || null
      });

      if (res.success) {
        setSuccessMsg(language === 'vi' ? 'Đặt mục tiêu học tập thành công!' : 'Target GPA goal set successfully!');
        setTargetGpaInput('');
        setGoalNotes('');
        await fetchGoalsAndAnalysis();
      } else {
        setError(res.message || 'Failed to set GPA goal.');
      }
    } catch (err: any) {
      console.error(err);
      setError('An error occurred while setting the GPA goal.');
    } finally {
      setLoading(false);
    }
  };

  // ----------------------------------------------------
  // Tab 3: What-If Simulation Handlers
  // ----------------------------------------------------
  const fetchCurrentSemesterCourses = async () => {
    try {
      setLoadingCourses(true);
      setError(null);
      setSimulationResult(null);

      // Get academic years
      const yearsRes = await academicYearApi.getAcademicYears();
      if (!yearsRes.success || !yearsRes.data) return;

      const currentYear = yearsRes.data.find((y: any) => y.isCurrent);
      if (!currentYear) {
        setError(language === 'vi' ? 'Vui lòng đặt một Năm Học làm hiện tại trước.' : 'Please set an Academic Year as current first.');
        return;
      }

      // Get semesters
      const semestersRes = await semesterApi.getSemesters(currentYear.id);
      if (!semestersRes.success || !semestersRes.data || semestersRes.data.length === 0) {
        setError(language === 'vi' ? 'Vui lòng thêm Học Kỳ cho năm học hiện tại.' : 'Please add a Semester to the current academic year.');
        return;
      }

      // Sort descending (assuming highest sortOrder is current semester)
      const currentSemester = [...semestersRes.data].sort((a: any, b: any) => b.sortOrder - a.sortOrder)[0];
      setCurrentSemName(currentSemester.semesterName);

      // Get courses
      const coursesRes = await courseApi.getCourses(currentSemester.id);
      if (coursesRes.success) {
        const courseList = coursesRes.data || [];
        setCurrentSemesterCourses(courseList);

        // Prepopulate simulated scores with current scores (or 8.0 if missing)
        const initialSimulated: Record<string, { attendance: number; continuous: number; final: number }> = {};
        courseList.forEach((c: CourseDto) => {
          initialSimulated[c.id] = {
            attendance: c.score?.attendanceScore !== null && c.score?.attendanceScore !== undefined ? Number(c.score.attendanceScore) : 8.0,
            continuous: c.score?.continuousScore !== null && c.score?.continuousScore !== undefined ? Number(c.score.continuousScore) : 8.0,
            final: c.score?.finalExamScore !== null && c.score?.finalExamScore !== undefined ? Number(c.score.finalExamScore) : 8.0
          };
        });
        setSimulatedScores(initialSimulated);
      }
    } catch (err: any) {
      console.error(err);
      setError('An error occurred while loading semester courses.');
    } finally {
      setLoadingCourses(false);
    }
  };

  const handleSimulateScoreChange = (courseId: string, component: 'attendance' | 'continuous' | 'final', val: number) => {
    setSimulatedScores(prev => ({
      ...prev,
      [courseId]: {
        ...prev[courseId],
        [component]: val
      }
    }));
  };

  const handleRunSimulation = async () => {
    try {
      setLoadingSimulation(true);
      setError(null);

      const payload = Object.entries(simulatedScores).map(([courseId, scores]) => ({
        courseId,
        attendanceScore: scores.attendance,
        continuousScore: scores.continuous,
        finalExamScore: scores.final
      }));

      const res = await goalApi.simulateScenario({
        simulatedCourses: payload
      });

      if (res.success) {
        setSimulationResult(res.data);
      } else {
        setError(res.message || 'Failed to simulate scenario.');
      }
    } catch (err: any) {
      console.error(err);
      setError('An error occurred during what-if simulation.');
    } finally {
      setLoadingSimulation(false);
    }
  };

  const applyPreset = (averageScore: number) => {
    const updated = { ...simulatedScores };
    currentSemesterCourses.forEach((c) => {
      updated[c.id] = {
        attendance: averageScore,
        continuous: averageScore,
        final: averageScore
      };
    });
    setSimulatedScores(updated);
  };

  // Helper formatting function
  const formatGpa = (value: number | null | undefined): string => {
    if (value === null || value === undefined) return '—';
    return value.toFixed(2);
  };

  const getFeasibilityBadge = (feasibility: string) => {
    switch (feasibility.toLowerCase()) {
      case 'guaranteed':
      case 'already achieved':
        return (
          <span className="inline-flex items-center gap-1 px-3 py-1 rounded-full text-xs font-bold bg-green-500/10 border border-green-500/25 text-green-600 dark:text-green-400">
            <CheckCircle2 className="h-3.5 w-3.5" />
            {feasibility}
          </span>
        );
      case 'impossible':
      case 'not achievable':
        return (
          <span className="inline-flex items-center gap-1 px-3 py-1 rounded-full text-xs font-bold bg-red-500/10 border border-red-500/25 text-red-600 dark:text-red-400">
            <XCircle className="h-3.5 w-3.5" />
            {feasibility}
          </span>
        );
      default:
        return (
          <span className="inline-flex items-center gap-1 px-3 py-1 rounded-full text-xs font-bold bg-brand-500/10 border border-brand-500/25 text-brand-600 dark:text-brand-400">
            <Sparkles className="h-3.5 w-3.5" />
            {feasibility}
          </span>
        );
    }
  };

  const activeGoal = goals.find(g => g.isActive);

  return (
    <div className="mx-auto max-w-6xl space-y-8 animate-in fade-in duration-500 pb-12">
      
      {/* Header Banner */}
      <div className="relative overflow-hidden bg-gradient-to-br from-indigo-600 via-indigo-700 to-brand-600 text-white rounded-3xl p-8 shadow-xl shadow-indigo-500/15">
        <div className="relative z-10 space-y-2">
          <span className="bg-white/10 px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider backdrop-blur-md">
            {language === 'vi' ? 'CÔNG CỤ HỌC TẬP' : 'ACADEMIC TOOLKIT'}
          </span>
          <h1 className="text-3xl md:text-4xl font-black tracking-tight">
            {language === 'vi' ? 'Công Cụ Hoạch Định & Dự Đoán Điểm' : 'Goal Planner & Score Predictor'}
          </h1>
          <p className="text-white/80 max-w-xl text-sm">
            {language === 'vi'
              ? 'Thiết lập điểm trung bình mục tiêu tích lũy, tính toán chính xác điểm thi tối thiểu và mô phỏng kết quả học tập.'
              : 'Set target cumulative GPAs, calculate required final exam scores, and simulate grade scenarios.'}
          </p>
        </div>
        <div className="absolute right-0 bottom-0 top-0 w-1/4 opacity-15 hidden md:block select-none pointer-events-none">
          <Target className="h-full w-full p-4" />
        </div>
      </div>

      {/* Tabs Controller */}
      <div className="flex border-b border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-2 rounded-2xl gap-2 shadow-sm">
        <button
          onClick={() => setActiveTab('prediction')}
          className={`flex-1 py-3 px-4 rounded-xl font-bold text-sm transition-all flex items-center justify-center gap-2 ${
            activeTab === 'prediction'
              ? 'bg-indigo-600 text-white shadow-md shadow-indigo-600/10'
              : 'text-gray-500 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-gray-850'
          }`}
        >
          <Calculator className="h-4.5 w-4.5" />
          <span>{language === 'vi' ? 'Dự Đoán Điểm Thi' : 'Exam Score Prediction'}</span>
        </button>

        <button
          onClick={() => setActiveTab('goals')}
          className={`flex-1 py-3 px-4 rounded-xl font-bold text-sm transition-all flex items-center justify-center gap-2 ${
            activeTab === 'goals'
              ? 'bg-indigo-600 text-white shadow-md shadow-indigo-600/10'
              : 'text-gray-500 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-gray-850'
          }`}
        >
          <Compass className="h-4.5 w-4.5" />
          <span>{language === 'vi' ? 'Mục Tiêu GPA' : 'Target GPA Goals'}</span>
        </button>

        <button
          onClick={() => setActiveTab('simulation')}
          className={`flex-1 py-3 px-4 rounded-xl font-bold text-sm transition-all flex items-center justify-center gap-2 ${
            activeTab === 'simulation'
              ? 'bg-indigo-600 text-white shadow-md shadow-indigo-600/10'
              : 'text-gray-500 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-gray-850'
          }`}
        >
          <TrendingUp className="h-4.5 w-4.5" />
          <span>{language === 'vi' ? 'Giả Lập Điểm Số' : 'What-If Simulation'}</span>
        </button>
      </div>

      {/* Global Alerts */}
      {error && (
        <div className="p-4 rounded-2xl bg-red-50 border border-red-200 dark:bg-red-950/15 dark:border-red-900/40 text-red-700 dark:text-red-400 text-sm font-semibold flex items-center gap-2">
          <AlertTriangle className="h-5 w-5 flex-shrink-0" />
          <span>{error}</span>
        </div>
      )}
      {successMsg && (
        <div className="p-4 rounded-2xl bg-green-50 border border-green-200 dark:bg-green-950/15 dark:border-green-900/40 text-green-700 dark:text-green-400 text-sm font-semibold flex items-center gap-2 animate-pulse">
          <CheckCircle2 className="h-5 w-5 flex-shrink-0" />
          <span>{successMsg}</span>
        </div>
      )}

      {/* Main Tab Content */}
      <div className="space-y-8">
        
        {/* ====================================================
            TAB 1: FINAL EXAM SCORE PREDICTION
            ==================================================== */}
        {activeTab === 'prediction' && (
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            
            {/* Left Inputs Card */}
            <div className="lg:col-span-1 bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm space-y-6">
              <h2 className="text-lg font-black text-gray-900 dark:text-white flex items-center gap-2 border-b border-gray-100 dark:border-gray-800 pb-3">
                <Calculator className="text-indigo-600" />
                {language === 'vi' ? 'Nhập Điểm Thành Phần' : 'Component Scores Input'}
              </h2>

              {/* Attendance Score */}
              <div className="space-y-2">
                <div className="flex justify-between text-sm font-semibold">
                  <span className="text-gray-600 dark:text-gray-400">{t('scores.attendance')} (10%)</span>
                  <span className="text-indigo-600 dark:text-indigo-400 font-extrabold">{predictAttendance.toFixed(1)}</span>
                </div>
                <input
                  type="range"
                  min="0"
                  max="10"
                  step="0.1"
                  value={predictAttendance}
                  onChange={(e) => setPredictAttendance(parseFloat(e.target.value))}
                  className="w-full h-2 bg-gray-200 dark:bg-gray-800 rounded-lg appearance-none cursor-pointer accent-indigo-600"
                />
                <div className="flex justify-between text-[10px] text-gray-400">
                  <span>0.0</span>
                  <span>5.0</span>
                  <span>10.0</span>
                </div>
              </div>

              {/* Continuous Score */}
              <div className="space-y-2">
                <div className="flex justify-between text-sm font-semibold">
                  <span className="text-gray-600 dark:text-gray-400">{t('scores.continuous')} (30%)</span>
                  <span className="text-indigo-600 dark:text-indigo-400 font-extrabold">{predictContinuous.toFixed(1)}</span>
                </div>
                <input
                  type="range"
                  min="0"
                  max="10"
                  step="0.1"
                  value={predictContinuous}
                  onChange={(e) => setPredictContinuous(parseFloat(e.target.value))}
                  className="w-full h-2 bg-gray-200 dark:bg-gray-800 rounded-lg appearance-none cursor-pointer accent-indigo-600"
                />
                <div className="flex justify-between text-[10px] text-gray-400">
                  <span>0.0</span>
                  <span>5.0</span>
                  <span>10.0</span>
                </div>
              </div>

              {/* Target Grade Dropdown */}
              <div className="space-y-2">
                <label className="text-sm font-semibold text-gray-600 dark:text-gray-400">
                  {language === 'vi' ? 'Điểm Chữ Mục Tiêu' : 'Target Letter Grade'}
                </label>
                <select
                  value={targetGrade}
                  onChange={(e) => {
                    setTargetGrade(e.target.value);
                    setCustomTargetScore('');
                  }}
                  className="w-full bg-gray-50 border border-gray-200 dark:bg-gray-950 dark:border-gray-850 p-3 rounded-2xl font-bold text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                >
                  <option value="A+">A+ (≥ 9.0)</option>
                  <option value="A">A (≥ 8.5)</option>
                  <option value="B+">B+ (≥ 8.0)</option>
                  <option value="B">B (≥ 7.0)</option>
                  <option value="C+">C+ (≥ 6.5)</option>
                  <option value="C">C (≥ 5.5)</option>
                  <option value="D+">D+ (≥ 5.0)</option>
                  <option value="D">D (≥ 4.0)</option>
                </select>
              </div>

              {/* Action Buttons */}
              <div className="space-y-3 pt-3">
                <button
                  onClick={handleCalculatePrediction}
                  disabled={loadingPrediction}
                  className="w-full py-3 rounded-2xl bg-indigo-600 hover:bg-indigo-700 active:bg-indigo-800 text-white font-bold transition-all shadow-lg shadow-indigo-600/10 flex items-center justify-center gap-2 disabled:opacity-50"
                >
                  {loadingPrediction ? <Loader2 className="h-4 w-4 animate-spin" /> : <Play className="h-4 w-4" />}
                  <span>{language === 'vi' ? 'Dự Đoán Điểm Cần Đạt' : 'Predict Required Score'}</span>
                </button>

                <button
                  onClick={handleShowScenarios}
                  disabled={loadingPrediction}
                  className="w-full py-3 rounded-2xl bg-gray-100 hover:bg-gray-200 active:bg-gray-300 dark:bg-gray-800 dark:hover:bg-gray-700 dark:active:bg-gray-650 text-gray-850 dark:text-gray-200 font-bold transition-all flex items-center justify-center gap-2 disabled:opacity-50"
                >
                  <span>{language === 'vi' ? 'Mô Phỏng Tất Cả Học Lực' : 'View All Grade Scenarios'}</span>
                </button>
              </div>
            </div>

            {/* Right Predictions Output Card */}
            <div className="lg:col-span-2 space-y-8">
              
              {/* Single prediction result banner */}
              {predictionResult && (
                <div className="bg-gradient-to-br from-indigo-50 to-purple-50 border border-indigo-100 dark:from-indigo-950/20 dark:to-purple-950/20 dark:border-indigo-900/30 rounded-3xl p-6 space-y-4">
                  <div className="flex justify-between items-start">
                    <div className="space-y-1">
                      <span className="text-xs font-extrabold text-indigo-600 dark:text-indigo-400 uppercase tracking-wider">
                        {language === 'vi' ? 'KẾT QUẢ PHÂN TÍCH' : 'ANALYSIS OUTCOME'}
                      </span>
                      <h3 className="text-xl font-bold text-gray-900 dark:text-white">
                        {language === 'vi' ? 'Dự đoán cho điểm mục tiêu:' : 'Prediction for Target Grade:'}{' '}
                        <strong className="text-indigo-600 dark:text-indigo-400 font-black">{predictionResult.targetGrade}</strong>
                      </h3>
                    </div>
                    {getFeasibilityBadge(predictionResult.feasibility)}
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4 pt-2">
                    <div className="bg-white dark:bg-gray-900/60 p-4 rounded-2xl border border-indigo-50 dark:border-indigo-900/20">
                      <p className="text-xs text-gray-400 font-semibold uppercase">{language === 'vi' ? 'Điểm thi kết thúc tối thiểu' : 'Required Final Exam Score'}</p>
                      <h4 className="text-3xl font-black text-gray-900 dark:text-white mt-1">
                        {predictionResult.requiredFinalExamScore.toFixed(1)}
                      </h4>
                      <p className="text-[10px] text-gray-500 mt-1">
                        {language === 'vi' ? 'Trọng số: 60% tổng điểm học phần.' : 'Weighted: 60% of total course score.'}
                      </p>
                    </div>

                    <div className="bg-white dark:bg-gray-900/60 p-4 rounded-2xl border border-indigo-50 dark:border-indigo-900/20 flex flex-col justify-between">
                      <div>
                        <p className="text-xs text-gray-400 font-semibold uppercase">{language === 'vi' ? 'Ngưỡng điểm hệ 10' : 'Scale 10 Threshold'}</p>
                        <h4 className="text-lg font-bold text-gray-950 dark:text-gray-200 mt-1">
                          ≥ {predictionResult.targetScoreThreshold.toFixed(1)}
                        </h4>
                      </div>
                      <p className="text-[10px] text-gray-500">
                        {language === 'vi' ? 'Quy đổi components về bội số của 0.5.' : 'Rounds component scores to nearest 0.5.'}
                      </p>
                    </div>
                  </div>

                  <div className="p-4 rounded-2xl bg-indigo-500/10 text-indigo-750 dark:text-indigo-300 text-xs leading-relaxed font-medium">
                    💡 <strong>{language === 'vi' ? 'Lời khuyên học tập:' : 'Academic Advice:'}</strong> {predictionResult.advice}
                  </div>
                </div>
              )}

              {/* Scenarios Table */}
              {scenarios.length > 0 && (
                <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm space-y-4">
                  <h3 className="text-base font-extrabold text-gray-950 dark:text-white flex items-center gap-2">
                    <Sparkles className="text-indigo-600 h-4.5 w-4.5" />
                    {language === 'vi' ? 'Yêu Cầu Điểm Thi Theo Từng Bậc Điểm Chữ' : 'Required Final Exam Scores per Target Grade'}
                  </h3>

                  <div className="overflow-x-auto">
                    <table className="w-full text-left text-sm border-collapse">
                      <thead>
                        <tr className="border-b border-gray-100 dark:border-gray-850 text-xs text-gray-400 font-bold uppercase select-none">
                          <th className="py-3">{language === 'vi' ? 'Bậc học lực' : 'Target Grade'}</th>
                          <th className="py-3 text-center">{language === 'vi' ? 'Điểm thi tối thiểu cần đạt' : 'Required Final Score'}</th>
                          <th className="py-3 text-center">{language === 'vi' ? 'Mức độ khả thi' : 'Feasibility'}</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-gray-50 dark:divide-gray-850/50">
                        {scenarios.map((sc, idx) => (
                          <tr key={idx} className="hover:bg-gray-50/10 dark:hover:bg-gray-850/10 transition-colors">
                            <td className="py-3.5">
                              <span className="font-extrabold text-lg text-indigo-600 dark:text-indigo-400">{sc.targetGrade}</span>
                            </td>
                            <td className="py-3.5 text-center font-black text-gray-900 dark:text-white">
                              {sc.feasibility === 'Impossible' ? '—' : sc.requiredScore.toFixed(1)}
                            </td>
                            <td className="py-3.5 text-center">
                              {getFeasibilityBadge(sc.feasibility)}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              )}

              {/* Formula & Policy Card */}
              {!predictionResult && scenarios.length === 0 && (
                <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-12 text-center text-gray-500">
                  <Calculator className="h-16 w-16 text-indigo-200 dark:text-indigo-900 mx-auto mb-4" />
                  <p className="max-w-md mx-auto text-sm font-medium">
                    {language === 'vi'
                      ? 'Di chuyển thanh trượt để nhập điểm Chuyên cần và Thường xuyên, sau đó bấm nút để xem dự đoán điểm thi kết thúc tối thiểu.'
                      : 'Adjust the sliders to enter components and click the button to see your minimum required final exam scores.'}
                  </p>
                </div>
              )}
            </div>

          </div>
        )}

        {/* ====================================================
            TAB 2: GOAL SETTING & ANALYSIS
            ==================================================== */}
        {activeTab === 'goals' && (
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            
            {/* Set Goal & Details Panel */}
            <div className="lg:col-span-1 space-y-6">
              
              {/* Set Goal Form */}
              <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm space-y-4">
                <h3 className="text-base font-extrabold text-gray-900 dark:text-white flex items-center gap-2 border-b border-gray-100 dark:border-gray-800 pb-3">
                  <Compass className="text-indigo-600" />
                  {language === 'vi' ? 'Thiết Lập Mục Tiêu Mới' : 'Set New Target GPA'}
                </h3>

                <form onSubmit={handleCreateGoal} className="space-y-4">
                  <div className="space-y-1">
                    <label className="text-xs font-bold text-gray-500 uppercase tracking-wide">
                      {language === 'vi' ? 'Điểm tích lũy hệ 10 mong muốn' : 'Target Cumulative GPA (10-scale)'}
                    </label>
                    <input
                      type="number"
                      step="0.01"
                      min="0"
                      max="10"
                      required
                      placeholder="e.g. 8.50"
                      value={targetGpaInput}
                      onChange={(e) => setTargetGpaInput(e.target.value)}
                      className="w-full bg-gray-50 border border-gray-200 dark:bg-gray-950 dark:border-gray-850 p-3.5 rounded-2xl font-bold text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    />
                  </div>

                  <div className="space-y-1">
                    <label className="text-xs font-bold text-gray-500 uppercase tracking-wide">
                      {language === 'vi' ? 'Ghi chú / Động lực' : 'Goal Notes / Motivation'}
                    </label>
                    <textarea
                      maxLength={500}
                      rows={3}
                      placeholder={language === 'vi' ? 'Nhập ghi chú...' : 'Enter your motivation or plans...'}
                      value={goalNotes}
                      onChange={(e) => setGoalNotes(e.target.value)}
                      className="w-full bg-gray-50 border border-gray-200 dark:bg-gray-950 dark:border-gray-850 p-3.5 rounded-2xl text-sm text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    />
                  </div>

                  <button
                    type="submit"
                    disabled={loading}
                    className="w-full py-3.5 rounded-2xl bg-indigo-600 hover:bg-indigo-700 active:bg-indigo-800 text-white font-bold transition-all shadow-lg shadow-indigo-600/10 flex items-center justify-center gap-2 disabled:opacity-50"
                  >
                    {loading ? <Loader2 className="h-4 w-4 animate-spin" /> : <PlusCircle className="h-4 w-4" />}
                    <span>{language === 'vi' ? 'Kích Hoạt Mục Tiêu' : 'Activate GPA Goal'}</span>
                  </button>
                </form>
              </div>

              {/* Active Goal Summary Card */}
              <div className="bg-gradient-to-br from-indigo-500 via-indigo-650 to-indigo-800 text-white border border-indigo-500 rounded-3xl p-6 shadow-md relative overflow-hidden">
                <h3 className="text-base font-bold flex items-center gap-1.5 z-10 relative">
                  <Award className="h-5 w-5 text-yellow-400" />
                  {language === 'vi' ? 'Mục Tiêu Hiện Tại' : 'Current Active Goal'}
                </h3>
                
                {activeGoal ? (
                  <div className="space-y-5 mt-4 z-10 relative">
                    <div className="flex items-baseline gap-2">
                      <h4 className="text-5xl font-black">{activeGoal.targetCumulativeGpa10.toFixed(2)}</h4>
                      <span className="text-xs text-white/80 font-medium">/ 10.00</span>
                    </div>

                    <div className="space-y-1 text-xs text-white/90">
                      <p>Hệ 4 quy đổi: <strong className="text-white text-sm font-black">{activeGoal.targetCumulativeGpa4.toFixed(2)}</strong></p>
                      {activeGoal.notes && <p className="italic text-white/80 line-clamp-2 mt-1">" {activeGoal.notes} "</p>}
                      <p className="text-[10px] text-white/60 pt-2">
                        {language === 'vi' ? 'Đặt lúc: ' : 'Created: '}
                        {new Date(activeGoal.createdAt).toLocaleString(language === 'vi' ? 'vi-VN' : 'en-US')}
                      </p>
                    </div>

                    <div className="flex justify-between items-center pt-2">
                      <span className="text-xs text-white/80">Trạng thái:</span>
                      <span className={`inline-flex items-center gap-0.5 px-2.5 py-0.5 rounded-full text-[10px] font-bold ${
                        activeGoal.isAchieved 
                          ? 'bg-green-500 text-white' 
                          : 'bg-indigo-400/50 text-white'
                      }`}>
                        {activeGoal.isAchieved ? (language === 'vi' ? 'Đã đạt' : 'Achieved') : (language === 'vi' ? 'Chưa đạt' : 'Pending')}
                      </span>
                    </div>
                  </div>
                ) : (
                  <div className="text-center py-6 text-white/70 italic text-sm relative z-10">
                    {language === 'vi' ? 'Chưa thiết lập mục tiêu nào' : 'No active GPA goal set yet.'}
                  </div>
                )}
                <div className="absolute right-0 bottom-0 opacity-10 pointer-events-none select-none">
                  <Award className="h-32 w-32 translate-x-4 translate-y-4" />
                </div>
              </div>

            </div>

            {/* Analysis & Goals History */}
            <div className="lg:col-span-2 space-y-8">
              
              {/* Required GPA Analysis */}
              <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm space-y-4">
                <h3 className="text-base font-extrabold text-gray-950 dark:text-white flex items-center gap-2 border-b border-gray-100 dark:border-gray-800 pb-3">
                  <TrendingUp className="text-indigo-600 h-5 w-5" />
                  {language === 'vi' ? 'Phân Tích Khả Thi & Yêu Cầu Học Tập' : 'Feasibility & GPA Requirement Analysis'}
                </h3>

                {loadingRequiredGpa ? (
                  <div className="flex justify-center py-8">
                    <Loader2 className="h-8 w-8 animate-spin text-indigo-600" />
                  </div>
                ) : requiredGpa ? (
                  <div className="space-y-6">
                    <div className="flex justify-between items-start">
                      <p className="text-sm text-gray-500 dark:text-gray-400 font-medium">
                        {requiredGpa.message}
                      </p>
                      {getFeasibilityBadge(requiredGpa.feasibility)}
                    </div>

                    <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                      <div className="bg-gray-50 dark:bg-gray-950 p-4 rounded-2xl border border-gray-100 dark:border-gray-850">
                        <p className="text-[10px] text-gray-400 font-bold uppercase">{language === 'vi' ? 'Mục tiêu' : 'Goal GPA'}</p>
                        <h4 className="text-xl font-black text-gray-950 dark:text-white mt-1">
                          {requiredGpa.targetCumulativeGpa10.toFixed(2)}
                        </h4>
                      </div>

                      <div className="bg-gray-50 dark:bg-gray-950 p-4 rounded-2xl border border-gray-100 dark:border-gray-850">
                        <p className="text-[10px] text-gray-400 font-bold uppercase">{language === 'vi' ? 'Tích lũy hiện tại' : 'Current GPA'}</p>
                        <h4 className="text-xl font-black text-gray-950 dark:text-white mt-1">
                          {requiredGpa.currentCumulativeGpa10.toFixed(2)}
                        </h4>
                      </div>

                      <div className="bg-gray-50 dark:bg-gray-950 p-4 rounded-2xl border border-gray-100 dark:border-gray-850">
                        <p className="text-[10px] text-gray-400 font-bold uppercase">{language === 'vi' ? 'Tín chỉ còn lại' : 'Credits Left'}</p>
                        <h4 className="text-xl font-black text-gray-950 dark:text-white mt-1">
                          {requiredGpa.creditsRemaining}
                        </h4>
                        <p className="text-[9px] text-gray-500">Đã đạt: {requiredGpa.creditsCompleted}</p>
                      </div>

                      <div className="bg-indigo-50 border border-indigo-100 dark:bg-indigo-950/20 dark:border-indigo-900/35 p-4 rounded-2xl">
                        <p className="text-[10px] text-indigo-600 dark:text-indigo-400 font-bold uppercase">{language === 'vi' ? 'GPA trung bình yêu cầu' : 'Avg GPA Needed'}</p>
                        <h4 className="text-xl font-black text-indigo-650 dark:text-indigo-400 mt-1">
                          {requiredGpa.requiredRemainingGpa10.toFixed(2)}
                        </h4>
                      </div>
                    </div>
                  </div>
                ) : (
                  <div className="text-center py-8 text-gray-500 dark:text-gray-400 italic text-sm">
                    {language === 'vi' 
                      ? 'Vui lòng kích hoạt một mục tiêu GPA ở bên trái để xem phân tích học tập chi tiết.'
                      : 'Please define an active GPA goal in the settings to calculate requirements.'}
                  </div>
                )}
              </div>

              {/* Goals History List */}
              <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm space-y-4">
                <h3 className="text-base font-extrabold text-gray-950 dark:text-white flex items-center gap-2">
                  <History className="text-indigo-600 h-4.5 w-4.5" />
                  {language === 'vi' ? 'Lịch Sử Thiết Lập Mục Tiêu' : 'GPA Goals Setting History'}
                </h3>

                {loadingGoalsList ? (
                  <div className="flex justify-center py-6">
                    <Loader2 className="h-6 w-6 animate-spin text-indigo-600" />
                  </div>
                ) : goals.length === 0 ? (
                  <p className="text-center py-6 text-sm text-gray-500 italic">
                    {language === 'vi' ? 'Chưa có lịch sử thiết lập mục tiêu nào.' : 'No goal history found.'}
                  </p>
                ) : (
                  <div className="divide-y divide-gray-50 dark:divide-gray-850/50 max-h-64 overflow-y-auto pr-1">
                    {goals.map((g) => (
                      <div key={g.id} className="py-3.5 flex justify-between items-center gap-3">
                        <div className="space-y-1">
                          <div className="flex items-center gap-2">
                            <span className="text-base font-extrabold text-gray-900 dark:text-white">
                              GPA {g.targetCumulativeGpa10.toFixed(2)}
                            </span>
                            <span className="text-xs text-gray-500">
                              (Hệ 4: {g.targetCumulativeGpa4.toFixed(2)})
                            </span>
                            {g.isActive && (
                              <span className="bg-indigo-500/10 border border-indigo-500/20 text-indigo-600 dark:text-indigo-400 font-bold px-2 py-0.5 rounded-full text-[9px] uppercase">
                                Active
                              </span>
                            )}
                          </div>
                          {g.notes && <p className="text-xs text-gray-500 dark:text-gray-400 italic line-clamp-1">"{g.notes}"</p>}
                          <p className="text-[10px] text-gray-400 font-medium">
                            {new Date(g.createdAt).toLocaleDateString(language === 'vi' ? 'vi-VN' : 'en-US')}
                          </p>
                        </div>

                        <span className={`inline-flex items-center gap-0.5 px-2.5 py-0.5 rounded-full text-[10px] font-bold ${
                          g.isAchieved 
                            ? 'bg-green-50 text-green-700 dark:bg-green-950/20 dark:text-green-400 border border-green-200 dark:border-green-900/40' 
                            : 'bg-gray-50 text-gray-600 dark:bg-gray-800 dark:text-gray-400 border border-gray-200 dark:border-gray-700'
                        }`}>
                          {g.isAchieved ? (language === 'vi' ? 'Đã Đạt' : 'Achieved') : (language === 'vi' ? 'Chưa Đạt' : 'Pending')}
                        </span>
                      </div>
                    ))}
                  </div>
                )}
              </div>

            </div>

          </div>
        )}

        {/* ====================================================
            TAB 3: WHAT-IF SCENARIO SIMULATION
            ==================================================== */}
        {activeTab === 'simulation' && (
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            
            {/* Left Score Override Form */}
            <div className="lg:col-span-2 bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm space-y-6">
              
              <div className="flex justify-between items-center border-b border-gray-100 dark:border-gray-800 pb-3 flex-wrap gap-2">
                <div className="space-y-0.5">
                  <h2 className="text-lg font-black text-gray-900 dark:text-white flex items-center gap-2">
                    <TrendingUp className="text-indigo-600" />
                    {language === 'vi' ? 'Mô Phỏng Điểm Học Phần' : 'Simulate Course Scores'}
                  </h2>
                  <p className="text-xs text-gray-500">
                    {language === 'vi' ? `Học kỳ hiện tại: ${currentSemName}` : `Active Semester: ${currentSemName}`}
                  </p>
                </div>

                <div className="flex gap-2">
                  <button
                    onClick={() => applyPreset(8.0)}
                    className="px-3 py-1.5 rounded-lg text-xs font-bold bg-indigo-50 hover:bg-indigo-100 dark:bg-indigo-950/30 text-indigo-600 dark:text-indigo-400 border border-indigo-100 dark:border-indigo-900/30 transition-all"
                  >
                    Preset 8.0
                  </button>
                  <button
                    onClick={() => applyPreset(9.0)}
                    className="px-3 py-1.5 rounded-lg text-xs font-bold bg-indigo-50 hover:bg-indigo-100 dark:bg-indigo-950/30 text-indigo-600 dark:text-indigo-400 border border-indigo-100 dark:border-indigo-900/30 transition-all"
                  >
                    Preset 9.0
                  </button>
                </div>
              </div>

              {loadingCourses ? (
                <div className="flex justify-center py-12">
                  <Loader2 className="h-10 w-10 animate-spin text-indigo-600" />
                </div>
              ) : currentSemesterCourses.length === 0 ? (
                <p className="text-center py-12 text-sm text-gray-500 italic">
                  {language === 'vi' 
                    ? 'Học kỳ hiện tại chưa có học phần nào. Vui lòng thêm học phần trước.' 
                    : 'No courses registered in the active semester. Please create courses first.'}
                </p>
              ) : (
                <div className="space-y-6">
                  {currentSemesterCourses.map((course) => {
                    const sim = simulatedScores[course.id] || { attendance: 8, continuous: 8, final: 8 };
                    return (
                      <div key={course.id} className="p-4 rounded-2xl bg-gray-50 dark:bg-gray-950 border border-gray-100 dark:border-gray-850 space-y-4">
                        <div className="flex justify-between items-start gap-2">
                          <div>
                            <h4 className="font-extrabold text-sm text-gray-900 dark:text-white">{course.courseName}</h4>
                            <p className="text-[10px] text-gray-500 font-medium mt-0.5">{course.courseCode} • {course.credits} {t('gpa.credits')}</p>
                          </div>
                          {course.score?.courseScore !== undefined && course.score?.courseScore !== null && (
                            <span className="bg-gray-200 dark:bg-gray-800 text-gray-700 dark:text-gray-300 font-bold px-2 py-0.5 rounded text-[10px]">
                              {language === 'vi' ? 'Hiện tại: ' : 'Current: '}
                              {course.score.courseScore.toFixed(1)}
                            </span>
                          )}
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                          
                          {/* Attendance */}
                          <div className="space-y-1">
                            <div className="flex justify-between text-xs font-semibold">
                              <span className="text-gray-500">{t('scores.attendance')} (10%)</span>
                              <span className="text-indigo-600 dark:text-indigo-400 font-black">{sim.attendance.toFixed(1)}</span>
                            </div>
                            <input
                              type="range"
                              min="0"
                              max="10"
                              step="0.5"
                              value={sim.attendance}
                              onChange={(e) => handleSimulateScoreChange(course.id, 'attendance', parseFloat(e.target.value))}
                              className="w-full h-1.5 bg-gray-200 dark:bg-gray-800 rounded-lg appearance-none cursor-pointer accent-indigo-600"
                            />
                          </div>

                          {/* Continuous */}
                          <div className="space-y-1">
                            <div className="flex justify-between text-xs font-semibold">
                              <span className="text-gray-500">{t('scores.continuous')} (30%)</span>
                              <span className="text-indigo-600 dark:text-indigo-400 font-black">{sim.continuous.toFixed(1)}</span>
                            </div>
                            <input
                              type="range"
                              min="0"
                              max="10"
                              step="0.5"
                              value={sim.continuous}
                              onChange={(e) => handleSimulateScoreChange(course.id, 'continuous', parseFloat(e.target.value))}
                              className="w-full h-1.5 bg-gray-200 dark:bg-gray-800 rounded-lg appearance-none cursor-pointer accent-indigo-600"
                            />
                          </div>

                          {/* Final Exam */}
                          <div className="space-y-1">
                            <div className="flex justify-between text-xs font-semibold">
                              <span className="text-gray-500">{t('scores.final')} (60%)</span>
                              <span className="text-indigo-600 dark:text-indigo-400 font-black">{sim.final.toFixed(1)}</span>
                            </div>
                            <input
                              type="range"
                              min="0"
                              max="10"
                              step="0.5"
                              value={sim.final}
                              onChange={(e) => handleSimulateScoreChange(course.id, 'final', parseFloat(e.target.value))}
                              className="w-full h-1.5 bg-gray-200 dark:bg-gray-800 rounded-lg appearance-none cursor-pointer accent-indigo-600"
                            />
                          </div>

                        </div>
                      </div>
                    );
                  })}

                  <div className="flex gap-4 pt-2">
                    <button
                      onClick={handleRunSimulation}
                      disabled={loadingSimulation}
                      className="flex-1 py-3.5 rounded-2xl bg-indigo-600 hover:bg-indigo-700 active:bg-indigo-800 text-white font-bold transition-all shadow-lg shadow-indigo-600/10 flex items-center justify-center gap-2 disabled:opacity-50"
                    >
                      {loadingSimulation ? <Loader2 className="h-4 w-4 animate-spin" /> : <TrendingUp className="h-4 w-4" />}
                      <span>{language === 'vi' ? 'Chạy Giả Lập' : 'Calculate Simulated GPAs'}</span>
                    </button>

                    <button
                      onClick={fetchCurrentSemesterCourses}
                      className="px-5 py-3.5 rounded-2xl bg-gray-100 hover:bg-gray-200 text-gray-800 dark:bg-gray-800 dark:hover:bg-gray-700 dark:text-gray-200 transition-all"
                      title={language === 'vi' ? 'Đặt lại điểm' : 'Reset scores'}
                    >
                      <RotateCcw className="h-5 w-5" />
                    </button>
                  </div>
                </div>
              )}
            </div>

            {/* Right Projected Results Card */}
            <div className="lg:col-span-1 space-y-6">
              
              {/* Projected GPAs Panel */}
              <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm space-y-6">
                <h3 className="text-base font-extrabold text-gray-950 dark:text-white flex items-center gap-2 border-b border-gray-100 dark:border-gray-800 pb-3">
                  <Award className="text-indigo-600 h-5 w-5" />
                  {language === 'vi' ? 'GPA Dự Kiến' : 'Projected GPAs Summary'}
                </h3>

                {simulationResult ? (
                  <div className="space-y-6">
                    
                    {/* Simulated Semester GPA */}
                    <div className="p-4 bg-gray-50 dark:bg-gray-950 rounded-2xl border border-gray-100 dark:border-gray-850">
                      <p className="text-xs text-gray-400 font-bold uppercase">{language === 'vi' ? 'GPA học kỳ dự kiến' : 'Projected Semester GPA'}</p>
                      <h4 className="text-3xl font-black text-gray-900 dark:text-white mt-1">
                        {formatGpa(simulationResult.simulatedSemesterGpa10)}
                      </h4>
                      <p className="text-[10px] text-gray-500 mt-1">
                        {language === 'vi' ? 'Chỉ tính trên học kỳ hiện tại.' : 'Based only on simulated semester courses.'}
                      </p>
                    </div>

                    {/* Simulated Cumulative GPA */}
                    <div className="p-4 bg-gray-50 dark:bg-gray-950 rounded-2xl border border-gray-100 dark:border-gray-850">
                      <p className="text-xs text-gray-400 font-bold uppercase">{language === 'vi' ? 'GPA tích lũy dự kiến' : 'Projected Cumulative GPA'}</p>
                      <h4 className="text-3xl font-black text-indigo-600 dark:text-indigo-400 mt-1">
                        {formatGpa(simulationResult.simulatedCumulativeGpa10)}
                      </h4>
                      <p className="text-[10px] text-gray-500 mt-1">
                        {language === 'vi' ? 'Cập nhật từ lịch sử tốt nhất của bạn.' : 'Updates using your best-attempt history.'}
                      </p>
                    </div>

                    {/* Target Variance */}
                    {simulationResult.targetVariance !== null && (
                      <div className={`p-4 rounded-2xl text-xs flex justify-between items-center ${
                        simulationResult.targetVariance >= 0
                          ? 'bg-green-500/10 border border-green-500/20 text-green-750 dark:text-green-400'
                          : 'bg-amber-500/10 border border-amber-500/20 text-amber-750 dark:text-amber-400'
                      }`}>
                        <div className="space-y-0.5">
                          <p className="font-bold">{language === 'vi' ? 'Độ lệch mục tiêu' : 'Variance from Target Goal'}</p>
                          <p className="text-[10px] opacity-75">
                            {language === 'vi' ? `Mục tiêu hoạt động: ${activeGoal?.targetCumulativeGpa10.toFixed(2)}` : `Active target: ${activeGoal?.targetCumulativeGpa10.toFixed(2)}`}
                          </p>
                        </div>
                        <span className="text-lg font-black">
                          {simulationResult.targetVariance >= 0 ? '+' : ''}{simulationResult.targetVariance.toFixed(2)}
                        </span>
                      </div>
                    )}

                  </div>
                ) : (
                  <div className="text-center py-12 text-gray-400 dark:text-gray-500 italic text-sm">
                    {language === 'vi'
                      ? 'Điều chỉnh điểm số mô phỏng và bấm nút chạy giả lập để tính toán GPA dự kiến.'
                      : 'Change the slider values and run the simulation to check your projected GPAs.'}
                  </div>
                )}
              </div>

              {/* Notice simulation disclaimer */}
              <div className="bg-gray-50 dark:bg-gray-950 p-4 rounded-2xl border border-gray-100 dark:border-gray-850 text-[10px] text-gray-400 leading-relaxed">
                📢 <strong>Note:</strong> What-if calculations are simulated in-memory and will **not** modify your permanent academic database.
              </div>

            </div>

          </div>
        )}

      </div>

    </div>
  );
};

export default GoalPlannerPage;
