import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useLanguage } from '../contexts/LanguageContext';
import { statisticsApi, GpaTrendDto, GradeDistributionDto, CreditProgressDto, StrengthsWeaknessesDto } from '../api/statisticsApi';
import {
  TrendingUp,
  BarChart2,
  PieChart,
  Award,
  BookOpen,
  ChevronRight,
  Loader2,
  CheckCircle2,
  XCircle,
  AlertCircle,
  ThumbsUp,
  ThumbsDown,
  Info
} from 'lucide-react';

export const StatisticsPage: React.FC = () => {
  const { t, language } = useLanguage();

  const [trendData, setTrendData] = useState<GpaTrendDto[]>([]);
  const [gradeDist, setGradeDist] = useState<GradeDistributionDto | null>(null);
  const [creditProgress, setCreditProgress] = useState<CreditProgressDto | null>(null);
  const [strengthsWeaknesses, setStrengthsWeaknesses] = useState<StrengthsWeaknessesDto | null>(null);
  
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  const [hoveredSemesterIdx, setHoveredSemesterIdx] = useState<number | null>(null);
  const [hoveredYearIdx, setHoveredYearIdx] = useState<number | null>(null);

  const fetchStats = async () => {
    try {
      setLoading(true);
      setError(null);

      const [trendRes, distRes, creditRes, swRes] = await Promise.all([
        statisticsApi.getGpaTrend(),
        statisticsApi.getGradeDistribution(),
        statisticsApi.getCreditProgress(),
        statisticsApi.getStrengthsWeaknesses()
      ]);

      if (trendRes.success) setTrendData(trendRes.data || []);
      if (distRes.success) setGradeDist(distRes.data);
      if (creditRes.success) setCreditProgress(creditRes.data);
      if (swRes.success) setStrengthsWeaknesses(swRes.data);
      
    } catch (err: any) {
      console.error(err);
      setError('An error occurred while loading performance statistics.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchStats();
  }, []);

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
        <AlertCircle className="h-10 w-10 text-red-500 mx-auto mb-3" />
        <h3 className="font-extrabold text-lg">Failed to load Statistics</h3>
        <p className="mt-1 text-sm">{error}</p>
        <button
          onClick={fetchStats}
          className="mt-4 px-5 py-2.5 rounded-xl font-bold bg-red-600 hover:bg-red-700 text-white transition-colors"
        >
          Try Again
        </button>
      </div>
    );
  }

  const hasTrendData = trendData.length > 0;
  
  // Calculate Year Aggregates for Academic Year Chart
  const yearDataMap: Record<string, { sum10: number; sum4: number; count: number }> = {};
  trendData.forEach(d => {
    if (d.gpa10 !== null) {
      if (!yearDataMap[d.yearName]) {
        yearDataMap[d.yearName] = { sum10: 0, sum4: 0, count: 0 };
      }
      yearDataMap[d.yearName].sum10 += d.gpa10;
      yearDataMap[d.yearName].sum4 += d.gpa4 || 0;
      yearDataMap[d.yearName].count += 1;
    }
  });
  const yearTrend = Object.entries(yearDataMap).map(([yearName, stats]) => ({
    yearName,
    gpa10: stats.sum10 / stats.count,
    gpa4: stats.sum4 / stats.count
  }));

  // Calculations for Grade distribution percent
  const gradesList = [
    { label: 'A+', count: gradeDist?.aplus || 0 },
    { label: 'A', count: gradeDist?.a || 0 },
    { label: 'B+', count: gradeDist?.bplus || 0 },
    { label: 'B', count: gradeDist?.b || 0 },
    { label: 'C+', count: gradeDist?.cplus || 0 },
    { label: 'C', count: gradeDist?.c || 0 },
    { label: 'D+', count: gradeDist?.dplus || 0 },
    { label: 'D', count: gradeDist?.d || 0 },
    { label: 'F', count: gradeDist?.f || 0 }
  ];
  const totalGrades = gradesList.reduce((acc, curr) => acc + curr.count, 0);

  // SVG Charting dimensions
  const svgWidth = 600;
  const svgHeight = 240;
  const paddingLeft = 40;
  const paddingRight = 20;
  const paddingTop = 20;
  const paddingBottom = 40;
  
  const chartWidth = svgWidth - paddingLeft - paddingRight;
  const chartHeight = svgHeight - paddingTop - paddingBottom;

  // Scale functions helper for SVG
  const getXCoord = (index: number, total: number) => {
    if (total <= 1) return paddingLeft + chartWidth / 2;
    return paddingLeft + (index / (total - 1)) * chartWidth;
  };

  const getYCoord = (score: number | null | undefined) => {
    if (score === null || score === undefined) return paddingTop + chartHeight; // baseline
    // GPA Scale is 0.0 to 10.0
    return paddingTop + chartHeight - (score / 10.0) * chartHeight;
  };

  // Generate SVG path for line chart
  const getLinePath = (data: any[], key: string) => {
    const points = data
      .map((d, i) => {
        const val = d[key];
        if (val === null || val === undefined) return null;
        return `${getXCoord(i, data.length)},${getYCoord(val)}`;
      })
      .filter(p => p !== null);

    if (points.length === 0) return '';
    return `M ${points.join(' L ')}`;
  };

  // Generate SVG shaded area path
  const getAreaPath = (data: any[], key: string) => {
    const points = data
      .map((d, i) => {
        const val = d[key];
        if (val === null || val === undefined) return null;
        return `${getXCoord(i, data.length)},${getYCoord(val)}`;
      })
      .filter(p => p !== null);

    if (points.length === 0) return '';
    
    const firstX = getXCoord(0, data.length);
    const lastX = getXCoord(data.length - 1, data.length);
    const baselineY = paddingTop + chartHeight;

    return `M ${firstX},${baselineY} L ${points.join(' L ')} L ${lastX},${baselineY} Z`;
  };

  // Circular gauge pass rate computation
  const passedCount = creditProgress?.completedCredits || 0;
  const failedCount = creditProgress?.failedCredits || 0;
  const totalCompletedAttempted = passedCount + failedCount;
  const passRate = totalCompletedAttempted > 0 ? (passedCount / totalCompletedAttempted) * 100 : 0;
  
  const circleRadius = 40;
  const circleCircumference = 2 * Math.PI * circleRadius; // ~251.3
  const strokeDashoffset = circleCircumference - (passRate / 100) * circleCircumference;

  return (
    <div className="mx-auto max-w-6xl space-y-8 animate-in fade-in duration-500">
      
      {/* Page Header */}
      <div className="flex justify-between items-center border-b border-gray-200 dark:border-gray-800 pb-4">
        <div>
          <h1 className="text-3xl font-extrabold text-gray-900 dark:text-white tracking-tight flex items-center gap-2.5">
            <BarChart2 className="h-8 w-8 text-brand-500" />
            Performance Statistics
          </h1>
          <p className="text-gray-500 dark:text-gray-400 mt-1">
            Visual reports of your GPA trends, grade distribution, credit completions, and course strengths.
          </p>
        </div>
        <Link
          to="/dashboard"
          className="text-sm font-semibold text-brand-650 hover:text-brand-700 dark:text-brand-400 flex items-center gap-1 bg-brand-50/50 dark:bg-brand-950/20 px-4 py-2 rounded-xl transition-all"
        >
          <span>Back to Dashboard</span>
          <ChevronRight className="h-4 w-4" />
        </Link>
      </div>

      {!hasTrendData ? (
        <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-12 text-center shadow-sm">
          <Info className="h-12 w-12 text-gray-400 mx-auto mb-3" />
          <h3 className="text-lg font-bold text-gray-850 dark:text-white">No Statistics Generated</h3>
          <p className="text-sm text-gray-500 mt-1 max-w-sm mx-auto">
            Please input course grades and semesters to see statistics charts and trends.
          </p>
        </div>
      ) : (
        <>
          {/* Main Visualizations Grid */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
            
            {/* Semester GPA Trend Chart */}
            <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm space-y-4">
              <div>
                <h3 className="text-base font-extrabold text-gray-900 dark:text-white flex items-center gap-2">
                  <TrendingUp className="h-5 w-5 text-brand-500" />
                  GPA Trend by Semester
                </h3>
                <p className="text-xs text-gray-400 mt-0.5">Dual plot comparing individual semesters against rolling cumulative average.</p>
              </div>

              {/* Custom SVG Line Chart */}
              <div className="relative">
                <svg viewBox={`0 0 ${svgWidth} ${svgHeight}`} className="w-full overflow-visible">
                  <defs>
                    <linearGradient id="semesterAreaGrad" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor="#3b82f6" stopOpacity="0.25" />
                      <stop offset="100%" stopColor="#3b82f6" stopOpacity="0.0" />
                    </linearGradient>
                  </defs>

                  {/* Horizontal Grid lines */}
                  {[2.0, 4.0, 6.0, 8.0, 10.0].map(gridVal => (
                    <g key={gridVal}>
                      <line
                        x1={paddingLeft}
                        y1={getYCoord(gridVal)}
                        x2={svgWidth - paddingRight}
                        y2={getYCoord(gridVal)}
                        className="stroke-gray-100 dark:stroke-gray-800/60"
                        strokeWidth="1.5"
                        strokeDasharray="4 4"
                      />
                      <text
                        x={paddingLeft - 10}
                        y={getYCoord(gridVal) + 4}
                        className="text-[10px] font-bold fill-gray-400 text-right"
                        textAnchor="end"
                      >
                        {gridVal.toFixed(1)}
                      </text>
                    </g>
                  ))}

                  {/* Shaded Area under Semester GPA Line */}
                  <path d={getAreaPath(trendData, 'gpa10')} fill="url(#semesterAreaGrad)" />

                  {/* Cumulative GPA Trend Line */}
                  <path
                    d={getLinePath(trendData, 'cumulativeGpa10')}
                    fill="none"
                    stroke="#a855f7" // Purple
                    strokeWidth="3.5"
                    strokeDasharray="5 4"
                    className="opacity-75"
                  />

                  {/* Semester GPA Trend Line */}
                  <path
                    d={getLinePath(trendData, 'gpa10')}
                    fill="none"
                    stroke="#3b82f6" // Blue
                    strokeWidth="4"
                  />

                  {/* Interactive Circles / Data points */}
                  {trendData.map((d, idx) => {
                    if (d.gpa10 === null) return null;
                    const cx = getXCoord(idx, trendData.length);
                    const cy10 = getYCoord(d.gpa10);
                    const cyCum = getYCoord(d.cumulativeGpa10);
                    
                    return (
                      <g key={d.semesterId}>
                        {/* Semester GPA Circle */}
                        <circle
                          cx={cx}
                          cy={cy10}
                          r={hoveredSemesterIdx === idx ? "7" : "5"}
                          className="fill-blue-500 stroke-white dark:stroke-gray-900 stroke-2 cursor-pointer transition-all duration-150"
                          onMouseEnter={() => setHoveredSemesterIdx(idx)}
                          onMouseLeave={() => setHoveredSemesterIdx(null)}
                        />
                        {/* Cumulative GPA Circle */}
                        {d.cumulativeGpa10 !== null && (
                          <circle
                            cx={cx}
                            cy={cyCum}
                            r={hoveredSemesterIdx === idx ? "6" : "4.5"}
                            className="fill-purple-500 stroke-white dark:stroke-gray-900 stroke-2 cursor-pointer transition-all duration-150"
                            onMouseEnter={() => setHoveredSemesterIdx(idx)}
                            onMouseLeave={() => setHoveredSemesterIdx(null)}
                          />
                        )}
                      </g>
                    );
                  })}

                  {/* X Axis Baseline */}
                  <line
                    x1={paddingLeft}
                    y1={paddingTop + chartHeight}
                    x2={svgWidth - paddingRight}
                    y2={paddingTop + chartHeight}
                    className="stroke-gray-200 dark:stroke-gray-700"
                    strokeWidth="2"
                  />

                  {/* X Axis Labels */}
                  {trendData.map((d, idx) => {
                    const cx = getXCoord(idx, trendData.length);
                    const label = d.semesterName.length > 8 ? `${d.semesterName.substring(0, 5)}...` : d.semesterName;
                    
                    return (
                      <text
                        key={d.semesterId}
                        x={cx}
                        y={paddingTop + chartHeight + 20}
                        className="text-[10px] font-bold fill-gray-400"
                        textAnchor="middle"
                      >
                        {label}
                      </text>
                    );
                  })}
                </svg>

                {/* Legend indicator */}
                <div className="flex gap-4 text-xs font-bold justify-center pt-2 select-none">
                  <div className="flex items-center gap-1.5">
                    <div className="h-3 w-3 bg-blue-500 rounded-full" />
                    <span className="text-gray-600 dark:text-gray-400">Semester GPA</span>
                  </div>
                  <div className="flex items-center gap-1.5">
                    <div className="h-3 w-3 bg-purple-500 rounded-full border-dashed" style={{ borderStyle: 'dashed' }} />
                    <span className="text-gray-600 dark:text-gray-400">Cumulative GPA</span>
                  </div>
                </div>

                {/* Floating Interactive Tooltip */}
                {hoveredSemesterIdx !== null && trendData[hoveredSemesterIdx] && (
                  <div className="absolute top-2 left-1/2 -translate-x-1/2 bg-white/95 dark:bg-gray-900/95 border border-gray-200 dark:border-gray-800 p-3 rounded-2xl shadow-xl text-xs space-y-1.5 pointer-events-none z-10 backdrop-blur-sm transition-all animate-in fade-in zoom-in-95 duration-150">
                    <p className="font-extrabold text-gray-900 dark:text-white">
                      {trendData[hoveredSemesterIdx].semesterName} ({trendData[hoveredSemesterIdx].yearName})
                    </p>
                    <div className="grid grid-cols-2 gap-x-4 gap-y-1 text-gray-500 dark:text-gray-400">
                      <span>Semester GPA:</span>
                      <strong className="text-blue-600 dark:text-blue-400 text-right">
                        {trendData[hoveredSemesterIdx].gpa10?.toFixed(2) || '—'}
                      </strong>
                      <span>Cumulative GPA:</span>
                      <strong className="text-purple-600 dark:text-purple-400 text-right">
                        {trendData[hoveredSemesterIdx].cumulativeGpa10?.toFixed(2) || '—'}
                      </strong>
                    </div>
                  </div>
                )}
              </div>
            </div>

            {/* Academic Year GPA Trend Chart */}
            <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm space-y-4">
              <div>
                <h3 className="text-base font-extrabold text-gray-900 dark:text-white flex items-center gap-2">
                  <TrendingUp className="h-5 w-5 text-indigo-500" />
                  GPA Trend by Academic Year
                </h3>
                <p className="text-xs text-gray-400 mt-0.5">Historical average performance calculated across years.</p>
              </div>

              {/* Custom SVG Line Chart */}
              <div className="relative">
                {yearTrend.length === 0 ? (
                  <div className="h-48 flex items-center justify-center text-xs text-gray-400 italic">No academic years tracked.</div>
                ) : (
                  <>
                    <svg viewBox={`0 0 ${svgWidth} ${svgHeight}`} className="w-full overflow-visible">
                      <defs>
                        <linearGradient id="yearAreaGrad" x1="0" y1="0" x2="0" y2="1">
                          <stop offset="0%" stopColor="#4f46e5" stopOpacity="0.25" />
                          <stop offset="100%" stopColor="#4f46e5" stopOpacity="0.0" />
                        </linearGradient>
                      </defs>

                      {/* Horizontal Grid lines */}
                      {[2.0, 4.0, 6.0, 8.0, 10.0].map(gridVal => (
                        <line
                          key={gridVal}
                          x1={paddingLeft}
                          y1={getYCoord(gridVal)}
                          x2={svgWidth - paddingRight}
                          y2={getYCoord(gridVal)}
                          className="stroke-gray-100 dark:stroke-gray-800/60"
                          strokeWidth="1.5"
                          strokeDasharray="4 4"
                        />
                      ))}

                      {/* Shaded Area under Year GPA Line */}
                      <path d={getAreaPath(yearTrend, 'gpa10')} fill="url(#yearAreaGrad)" />

                      {/* Year GPA Trend Line */}
                      <path
                        d={getLinePath(yearTrend, 'gpa10')}
                        fill="none"
                        stroke="#4f46e5" // Indigo
                        strokeWidth="4"
                      />

                      {/* Interactive Circles / Data points */}
                      {yearTrend.map((d, idx) => {
                        const cx = getXCoord(idx, yearTrend.length);
                        const cy = getYCoord(d.gpa10);
                        
                        return (
                          <circle
                            key={d.yearName}
                            cx={cx}
                            cy={cy}
                            r={hoveredYearIdx === idx ? "7" : "5"}
                            className="fill-indigo-600 stroke-white dark:stroke-gray-900 stroke-2 cursor-pointer transition-all duration-150"
                            onMouseEnter={() => setHoveredYearIdx(idx)}
                            onMouseLeave={() => setHoveredYearIdx(null)}
                          />
                        );
                      })}

                      {/* X Axis Baseline */}
                      <line
                        x1={paddingLeft}
                        y1={paddingTop + chartHeight}
                        x2={svgWidth - paddingRight}
                        y2={paddingTop + chartHeight}
                        className="stroke-gray-200 dark:stroke-gray-700"
                        strokeWidth="2"
                      />

                      {/* X Axis Labels */}
                      {yearTrend.map((d, idx) => {
                        const cx = getXCoord(idx, yearTrend.length);
                        
                        return (
                          <text
                            key={d.yearName}
                            x={cx}
                            y={paddingTop + chartHeight + 20}
                            className="text-[10px] font-bold fill-gray-400"
                            textAnchor="middle"
                          >
                            {d.yearName}
                          </text>
                        );
                      })}
                    </svg>

                    <div className="flex gap-4 text-xs font-bold justify-center pt-2 select-none">
                      <div className="flex items-center gap-1.5">
                        <div className="h-3 w-3 bg-indigo-600 rounded-full" />
                        <span className="text-gray-600 dark:text-gray-400">Academic Year GPA</span>
                      </div>
                    </div>

                    {/* Floating Interactive Tooltip */}
                    {hoveredYearIdx !== null && yearTrend[hoveredYearIdx] && (
                      <div className="absolute top-2 left-1/2 -translate-x-1/2 bg-white/95 dark:bg-gray-900/95 border border-gray-200 dark:border-gray-800 p-3 rounded-2xl shadow-xl text-xs space-y-1.5 pointer-events-none z-10 backdrop-blur-sm transition-all animate-in fade-in zoom-in-95 duration-150">
                        <p className="font-extrabold text-gray-900 dark:text-white">
                          Academic Year: {yearTrend[hoveredYearIdx].yearName}
                        </p>
                        <div className="grid grid-cols-2 gap-x-4 gap-y-1 text-gray-500 dark:text-gray-400">
                          <span>Average GPA (10-scale):</span>
                          <strong className="text-indigo-600 dark:text-indigo-400 text-right font-bold">
                            {yearTrend[hoveredYearIdx].gpa10.toFixed(2)}
                          </strong>
                          <span>Average GPA (4-scale):</span>
                          <strong className="text-indigo-600 dark:text-indigo-400 text-right font-bold">
                            {yearTrend[hoveredYearIdx].gpa4.toFixed(2)}
                          </strong>
                        </div>
                      </div>
                    )}
                  </>
                )}
              </div>
            </div>

            {/* Credit Distribution Stack & Circular Progress */}
            <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm space-y-6">
              <div>
                <h3 className="text-base font-extrabold text-gray-900 dark:text-white flex items-center gap-2">
                  <PieChart className="h-5 w-5 text-emerald-500" />
                  Credit Progress & Pass/Fail Ratio
                </h3>
                <p className="text-xs text-gray-400 mt-0.5">Proportions of passed vs failed credit courses completed.</p>
              </div>

              {creditProgress && (
                <div className="flex flex-col md:flex-row items-center gap-8 justify-around py-4">
                  {/* circular gauge pass rate */}
                  <div className="relative h-32 w-32 flex items-center justify-center">
                    <svg className="h-full w-full -rotate-90">
                      {/* background circle */}
                      <circle
                        cx="64"
                        cy="64"
                        r={circleRadius}
                        className="stroke-gray-100 dark:stroke-gray-800"
                        strokeWidth="10"
                        fill="none"
                      />
                      {/* progress circle */}
                      <circle
                        cx="64"
                        cy="64"
                        r={circleRadius}
                        className="stroke-green-500 transition-all duration-1000 ease-out"
                        strokeWidth="10"
                        fill="none"
                        strokeDasharray={circleCircumference}
                        strokeDashoffset={strokeDashoffset}
                        strokeLinecap="round"
                      />
                    </svg>
                    <div className="absolute text-center select-none">
                      <span className="text-2xl font-black text-gray-900 dark:text-white">{passRate.toFixed(0)}%</span>
                      <p className="text-[10px] text-gray-400 font-bold uppercase tracking-wide">Pass Rate</p>
                    </div>
                  </div>

                  {/* Legends list */}
                  <div className="grid grid-cols-2 gap-4 w-full md:w-auto">
                    <div className="bg-gray-50 dark:bg-gray-950 p-3.5 rounded-2xl border border-gray-100 dark:border-gray-800/80">
                      <span className="text-[10px] text-green-500 font-bold uppercase block">{t('gpa.passedCredits')}</span>
                      <strong className="text-xl font-black text-gray-800 dark:text-white mt-0.5 block">{creditProgress.completedCredits}</strong>
                    </div>
                    <div className="bg-gray-50 dark:bg-gray-950 p-3.5 rounded-2xl border border-gray-100 dark:border-gray-800/80">
                      <span className="text-[10px] text-red-500 font-bold uppercase block">{t('gpa.failedCredits')}</span>
                      <strong className="text-xl font-black text-gray-800 dark:text-white mt-0.5 block">{creditProgress.failedCredits}</strong>
                    </div>
                    <div className="bg-gray-50 dark:bg-gray-950 p-3.5 rounded-2xl border border-gray-100 dark:border-gray-800/80">
                      <span className="text-[10px] text-amber-500 font-bold uppercase block">In Progress</span>
                      <strong className="text-xl font-black text-gray-800 dark:text-white mt-0.5 block">{creditProgress.inProgressCredits}</strong>
                    </div>
                    <div className="bg-gray-50 dark:bg-gray-950 p-3.5 rounded-2xl border border-gray-100 dark:border-gray-800/80">
                      <span className="text-[10px] text-gray-400 font-bold uppercase block">Remaining</span>
                      <strong className="text-xl font-black text-gray-800 dark:text-white mt-0.5 block">{creditProgress.remainingCredits}</strong>
                    </div>
                  </div>
                </div>
              )}
            </div>

            {/* Letter Grade Distribution Bar Chart */}
            <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm space-y-4">
              <div>
                <h3 className="text-base font-extrabold text-gray-900 dark:text-white flex items-center gap-2">
                  <BarChart2 className="h-5 w-5 text-amber-500" />
                  Grade Distribution Overview
                </h3>
                <p className="text-xs text-gray-400 mt-0.5">Earned letter grades count statistics (Cumulative best attempts).</p>
              </div>

              <div className="space-y-3.5">
                {gradesList.map(g => {
                  const pct = totalGrades > 0 ? (g.count / totalGrades) * 100 : 0;
                  
                  return (
                    <div key={g.label} className="flex items-center gap-4">
                      <span className="w-8 text-xs font-bold text-gray-500 dark:text-gray-400">{g.label}</span>
                      <div className="flex-1 bg-gray-100 dark:bg-gray-800 h-3 rounded-full overflow-hidden">
                        <div 
                          className="bg-brand-500 dark:bg-brand-650 h-full rounded-full transition-all duration-500" 
                          style={{ width: `${pct}%` }} 
                        />
                      </div>
                      <span className="w-6 text-xs font-extrabold text-gray-800 dark:text-gray-200 text-right">{g.count}</span>
                    </div>
                  );
                })}
              </div>
            </div>

          </div>

          {/* Strengths & Weaknesses Split lists */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
            
            {/* Strengths Courses */}
            <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm space-y-4">
              <div className="flex items-center gap-2 border-b border-gray-100 dark:border-gray-800 pb-3">
                <div className="p-1.5 rounded-lg bg-green-50 text-green-600 dark:bg-green-950/20">
                  <ThumbsUp className="h-4.5 w-4.5" />
                </div>
                <h3 className="text-base font-extrabold text-gray-900 dark:text-white">
                  Best Performing Courses (Strengths)
                </h3>
              </div>

              {strengthsWeaknesses?.strongestCourses.length === 0 ? (
                <p className="text-center py-6 text-xs text-gray-500 italic">No graded courses recorded yet.</p>
              ) : (
                <div className="space-y-3">
                  {strengthsWeaknesses?.strongestCourses.map(c => (
                    <div 
                      key={c.courseCode} 
                      className="flex items-center justify-between p-3 rounded-2xl border border-green-100 dark:border-green-950/40 bg-green-50/10 dark:bg-green-950/5 hover:scale-[1.01] transition-transform"
                    >
                      <div className="min-w-0">
                        <span className="text-[10px] font-bold text-green-600 dark:text-green-450 tracking-wider uppercase block">{c.courseCode}</span>
                        <span className="text-sm font-bold text-gray-900 dark:text-white block truncate" title={c.courseName}>
                          {c.courseName}
                        </span>
                      </div>
                      <div className="flex items-center gap-3 shrink-0">
                        <strong className="text-sm font-black text-green-700 dark:text-green-400">{c.score.toFixed(1)}</strong>
                        <span className="px-2 py-0.5 rounded-lg text-xs font-bold bg-green-500 text-white shadow-sm">
                          {c.letterGrade}
                        </span>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

            {/* Weaknesses Courses */}
            <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm space-y-4">
              <div className="flex items-center gap-2 border-b border-gray-100 dark:border-gray-800 pb-3">
                <div className="p-1.5 rounded-lg bg-red-50 text-red-600 dark:bg-red-950/20">
                  <ThumbsDown className="h-4.5 w-4.5" />
                </div>
                <h3 className="text-base font-extrabold text-gray-900 dark:text-white">
                  Weakest Performing Courses (Weaknesses)
                </h3>
              </div>

              {strengthsWeaknesses?.weakestCourses.length === 0 ? (
                <p className="text-center py-6 text-xs text-gray-500 italic">No graded courses recorded yet.</p>
              ) : (
                <div className="space-y-3">
                  {strengthsWeaknesses?.weakestCourses.map(c => (
                    <div 
                      key={c.courseCode} 
                      className="flex items-center justify-between p-3 rounded-2xl border border-red-100 dark:border-red-950/40 bg-red-50/10 dark:bg-red-950/5 hover:scale-[1.01] transition-transform"
                    >
                      <div className="min-w-0">
                        <span className="text-[10px] font-bold text-red-600 dark:text-red-450 tracking-wider uppercase block">{c.courseCode}</span>
                        <span className="text-sm font-bold text-gray-900 dark:text-white block truncate" title={c.courseName}>
                          {c.courseName}
                        </span>
                      </div>
                      <div className="flex items-center gap-3 shrink-0">
                        <strong className="text-sm font-black text-red-700 dark:text-red-400">{c.score.toFixed(1)}</strong>
                        <span className="px-2.5 py-0.5 rounded-lg text-xs font-bold bg-red-500 text-white shadow-sm">
                          {c.letterGrade}
                        </span>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

          </div>
        </>
      )}
    </div>
  );
};

export default StatisticsPage;
