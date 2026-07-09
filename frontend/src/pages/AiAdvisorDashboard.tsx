import React, { useState, useEffect, useRef } from 'react';
import { useLanguage } from '../contexts/LanguageContext';
import { useAuth } from '../contexts/AuthContext';
import { aiAdvisorApi, ConversationDto, ConversationMessageDto } from '../api/aiAdvisorApi';
import { statisticsApi, StrengthsWeaknessesDto, CreditProgressDto } from '../api/statisticsApi';
import { dashboardApi, DashboardSummaryDto } from '../api/dashboardApi';
import {
  Sparkles,
  Send,
  Brain,
  Trash2,
  Plus,
  Target,
  ArrowRight,
  Loader2,
  AlertTriangle,
  CheckCircle2,
  XCircle,
  TrendingUp,
  Award,
  BookOpen,
  Clock,
  AlertCircle,
  Info
} from 'lucide-react';

export const AiAdvisorDashboard: React.FC = () => {
  const { t, language } = useLanguage();
  const { user } = useAuth();

  // Conversations states
  const [conversations, setConversations] = useState<ConversationDto[]>([]);
  const [activeConversation, setActiveConversation] = useState<ConversationDto | null>(null);
  const [messages, setMessages] = useState<ConversationMessageDto[]>([]);
  
  // Loading states
  const [loadingConversations, setLoadingConversations] = useState(false);
  const [loadingMessages, setLoadingMessages] = useState(false);
  const [loadingInsights, setLoadingInsights] = useState(false);
  const [sendingMessage, setSendingMessage] = useState(false);
  const [startingConversation, setStartingConversation] = useState(false);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  // Academic insights states
  const [dashboardSummary, setDashboardSummary] = useState<DashboardSummaryDto | null>(null);
  const [strengthsWeaknesses, setStrengthsWeaknesses] = useState<StrengthsWeaknessesDto | null>(null);
  const [creditProgress, setCreditProgress] = useState<CreditProgressDto | null>(null);

  // Chat message input
  const [inputText, setInputText] = useState('');
  
  // Custom target GPA calculator state
  const [simTargetGpa, setSimTargetGpa] = useState<number>(8.0);

  // Error/Alert states
  const [chatError, setChatError] = useState<string | null>(null);
  const [insightsError, setInsightsError] = useState<string | null>(null);

  const messagesEndRef = useRef<HTMLDivElement>(null);

  // Load all initial data on mount
  useEffect(() => {
    fetchAcademicInsights();
    fetchConversations();
  }, []);

  // Sync simulator target to current active goal target if available
  useEffect(() => {
    if (dashboardSummary?.goalProgress?.targetGpa10) {
      setSimTargetGpa(dashboardSummary.goalProgress.targetGpa10);
    }
  }, [dashboardSummary]);

  // Load messages when active conversation changes
  useEffect(() => {
    if (activeConversation) {
      fetchMessages(activeConversation.id);
    } else {
      setMessages([]);
    }
  }, [activeConversation]);

  // Scroll chat window to bottom
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const fetchAcademicInsights = async () => {
    try {
      setLoadingInsights(true);
      setInsightsError(null);
      const [summaryRes, swRes, cpRes] = await Promise.all([
        dashboardApi.getSummary(),
        statisticsApi.getStrengthsWeaknesses(),
        statisticsApi.getCreditProgress()
      ]);

      if (summaryRes.success) setDashboardSummary(summaryRes.data);
      if (swRes.success) setStrengthsWeaknesses(swRes.data);
      if (cpRes.success) setCreditProgress(cpRes.data);
    } catch (err: any) {
      console.error(err);
      setInsightsError(t('aiAdvisor.errLoadInsights') || 'Failed to load academic insights.');
    } finally {
      setLoadingInsights(false);
    }
  };

  const fetchConversations = async () => {
    try {
      setLoadingConversations(true);
      const res = await aiAdvisorApi.getConversations(1, 50);
      if (res.success) {
        setConversations(res.data || []);
        if (res.data && res.data.length > 0 && !activeConversation) {
          setActiveConversation(res.data[0]);
        }
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoadingConversations(false);
    }
  };

  const fetchMessages = async (id: string) => {
    try {
      setLoadingMessages(true);
      setChatError(null);
      const res = await aiAdvisorApi.getMessages(id);
      if (res.success) {
        setMessages(res.data || []);
      }
    } catch (err: any) {
      console.error(err);
      setChatError(t('aiAdvisor.errLoadMessages') || 'Failed to load chat history.');
    } finally {
      setLoadingMessages(false);
    }
  };

  const handleStartConversation = async () => {
    if (conversations.length >= 50) {
      setChatError(t('aiAdvisor.threadLimitExceeded') || 'Maximum limit of 50 conversations reached.');
      return;
    }

    try {
      setStartingConversation(true);
      setChatError(null);
      const defaultTitle = language === 'vi' 
        ? `Tư vấn ngày ${new Date().toLocaleDateString('vi-VN')}` 
        : `Advisor Session - ${new Date().toLocaleDateString('en-US')}`;
      
      const res = await aiAdvisorApi.startConversation(defaultTitle);
      if (res.success) {
        setConversations(prev => [res.data, ...prev]);
        setActiveConversation(res.data);
      }
    } catch (err: any) {
      console.error(err);
      setChatError(err.response?.data?.message || t('aiAdvisor.errStartThread') || 'Failed to start conversation.');
    } finally {
      setStartingConversation(false);
    }
  };

  const handleDeleteConversation = async (id: string, e: React.MouseEvent) => {
    e.stopPropagation();
    if (!window.confirm(t('aiAdvisor.deleteConfirm') || 'Are you sure you want to delete this conversation?')) {
      return;
    }

    try {
      setDeletingId(id);
      await aiAdvisorApi.deleteConversation(id);
      setConversations(prev => prev.filter(c => c.id !== id));
      if (activeConversation?.id === id) {
        const remaining = conversations.filter(c => c.id !== id);
        setActiveConversation(remaining.length > 0 ? remaining[0] : null);
      }
    } catch (err: any) {
      console.error(err);
      setChatError(t('aiAdvisor.errDeleteThread') || 'Failed to delete conversation.');
    } finally {
      setDeletingId(null);
    }
  };

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!inputText.trim() || !activeConversation || sendingMessage) return;

    const messageText = inputText.trim();
    setInputText('');
    setChatError(null);

    // Append user message immediately locally for interactive feedback
    const tempUserMsg: ConversationMessageDto = {
      role: 'user',
      content: messageText,
      createdAt: new Date().toISOString()
    };
    setMessages(prev => [...prev, tempUserMsg]);
    setSendingMessage(true);

    try {
      const res = await aiAdvisorApi.sendMessage(activeConversation.id, messageText);
      if (res.success) {
        const tempAssistantMsg: ConversationMessageDto = {
          role: 'assistant',
          content: res.data.reply,
          createdAt: res.data.createdAt
        };
        setMessages(prev => [...prev, tempAssistantMsg]);
      }
    } catch (err: any) {
      console.error(err);
      // Remove the unsent user message or keep it and show error
      setChatError(err.response?.data?.message || err.message || t('aiAdvisor.errSendMessage') || 'Failed to send message.');
    } finally {
      setSendingMessage(false);
    }
  };

  // Custom markdown simple renderer for safety and compatibility
  const renderMarkdown = (text: string) => {
    const lines = text.split('\n');
    let inList = false;
    const listItems: React.ReactNode[] = [];
    const renderedElements: React.ReactNode[] = [];

    const parseInline = (line: string) => {
      const parts = line.split('**');
      return parts.map((part, i) => {
        if (i % 2 === 1) {
          return <strong key={i} className="font-extrabold text-gray-900 dark:text-white">{part}</strong>;
        }
        return part;
      });
    };

    lines.forEach((line, index) => {
      const trimmed = line.trim();
      if (trimmed.startsWith('###')) {
        if (inList) {
          renderedElements.push(<ul key={`list-${index}`} className="list-disc pl-5 my-2 space-y-1">{[...listItems]}</ul>);
          listItems.length = 0;
          inList = false;
        }
        const titleText = trimmed.replace(/^###\s*/, '');
        renderedElements.push(
          <h4 key={index} className="text-base font-black text-indigo-750 dark:text-brand-400 mt-4 mb-2 first:mt-0 border-b border-gray-150 dark:border-gray-800 pb-1">
            {parseInline(titleText)}
          </h4>
        );
      } else if (trimmed.startsWith('-')) {
        inList = true;
        const itemText = trimmed.replace(/^-\s*/, '');
        listItems.push(<li key={index} className="text-sm text-gray-700 dark:text-gray-300 ml-1 leading-relaxed">{parseInline(itemText)}</li>);
      } else if (trimmed === '') {
        if (inList) {
          renderedElements.push(<ul key={`list-${index}`} className="list-disc pl-5 my-2 space-y-1">{[...listItems]}</ul>);
          listItems.length = 0;
          inList = false;
        }
      } else {
        if (inList) {
          renderedElements.push(<ul key={`list-${index}`} className="list-disc pl-5 my-2 space-y-1">{[...listItems]}</ul>);
          listItems.length = 0;
          inList = false;
        }
        renderedElements.push(
          <p key={index} className="text-sm text-gray-700 dark:text-gray-300 my-1.5 leading-relaxed">
            {parseInline(line)}
          </p>
        );
      }
    });

    if (inList && listItems.length > 0) {
      renderedElements.push(<ul key={`list-end`} className="list-disc pl-5 my-2 space-y-1">{[...listItems]}</ul>);
    }

    return <div className="space-y-1">{renderedElements}</div>;
  };

  // Live Calculator formulas
  const totalRequired = creditProgress?.totalRequiredCredits || dashboardSummary?.performanceSummary?.totalCreditsRequired || 120;
  const completed = creditProgress?.completedCredits || dashboardSummary?.performanceSummary?.totalCreditsCompleted || 0;
  const remaining = Math.max(totalRequired - completed, 0);
  const currentGPA = dashboardSummary?.performanceSummary?.cumulativeGpa10 || 0;

  let calculatedRemainingGpa: number | null = null;
  let feasibility = 'Achievable';
  let adviceMessage = '';

  if (remaining > 0) {
    calculatedRemainingGpa = ((simTargetGpa * totalRequired) - (currentGPA * completed)) / remaining;
    
    if (calculatedRemainingGpa <= currentGPA) {
      feasibility = 'Already Achieved';
      adviceMessage = language === 'vi'
        ? 'Bạn đã đạt được điểm trung bình này rồi! Hãy nâng mục tiêu lên để bứt phá.'
        : 'You have already achieved this cumulative GPA target! Set a higher target to challenge yourself.';
    } else if (calculatedRemainingGpa > 10.0) {
      feasibility = 'Impossible';
      adviceMessage = language === 'vi'
        ? `Không thể đạt mục tiêu này vì điểm GPA yêu cầu (${calculatedRemainingGpa.toFixed(2)}) vượt quá 10.0.`
        : `Impossible target. Required remaining GPA is ${calculatedRemainingGpa.toFixed(2)}, exceeding the 10.0 limit.`;
    } else {
      feasibility = 'Achievable';
      adviceMessage = language === 'vi'
        ? `Có thể đạt được. Bạn cần duy trì mức GPA trung bình là ${calculatedRemainingGpa.toFixed(2)} trong các tín chỉ còn lại.`
        : `Achievable. You need to average ${calculatedRemainingGpa.toFixed(2)} GPA across remaining semesters.`;
    }
  } else {
    feasibility = currentGPA >= simTargetGpa ? 'Already Achieved' : 'Impossible';
    adviceMessage = currentGPA >= simTargetGpa
      ? (language === 'vi' ? 'Bạn đã hoàn tất chương trình học và đạt mục tiêu!' : 'Program completed and target achieved!')
      : (language === 'vi' ? 'Bạn đã hoàn tất chương trình học và không thể thay đổi GPA nữa.' : 'Program completed, GPA is locked.');
  }

  const getFeasibilityBadge = (val: string) => {
    switch (val) {
      case 'Already Achieved':
      case 'Achieved':
        return (
          <span className="inline-flex items-center gap-1 px-3 py-1 rounded-full text-xs font-bold bg-green-50 text-green-700 border border-green-200 dark:bg-green-950/20 dark:text-green-400 dark:border-green-900/40">
            <CheckCircle2 className="h-3.5 w-3.5" />
            {language === 'vi' ? 'Đã Đạt' : 'Achieved'}
          </span>
        );
      case 'Impossible':
        return (
          <span className="inline-flex items-center gap-1 px-3 py-1 rounded-full text-xs font-bold bg-red-50 text-red-700 border border-red-200 dark:bg-red-950/20 dark:text-red-400 dark:border-red-900/40">
            <XCircle className="h-3.5 w-3.5" />
            {language === 'vi' ? 'Không Khả Thi' : 'Impossible'}
          </span>
        );
      default:
        return (
          <span className="inline-flex items-center gap-1 px-3 py-1 rounded-full text-xs font-bold bg-brand-50 text-brand-700 border border-brand-200 dark:bg-brand-950/20 dark:text-brand-400 dark:border-brand-900/40">
            <Sparkles className="h-3.5 w-3.5" />
            {language === 'vi' ? 'Khả Thi' : 'Achievable'}
          </span>
        );
    }
  };

  return (
    <div className="mx-auto max-w-7xl space-y-8 animate-in fade-in duration-500 pb-12">
      
      {/* Disclaimer / Banner Section */}
      <div className="relative overflow-hidden bg-gradient-to-br from-indigo-700 via-brand-600 to-indigo-900 text-white rounded-3xl p-6 md:p-8 shadow-xl shadow-indigo-500/10">
        <div className="relative z-10 flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
          <div className="space-y-2">
            <span className="inline-flex items-center gap-1 bg-white/10 px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider backdrop-blur-md">
              <Brain className="h-3.5 w-3.5 text-yellow-400" />
              AI Advisor
            </span>
            <h1 className="text-2xl md:text-3xl font-black tracking-tight">
              {language === 'vi' ? 'Cố Vấn Học Tập AI Của Bạn' : 'Your Academic AI Advisor'}
            </h1>
            <p className="text-white/80 max-w-xl text-xs md:text-sm">
              {language === 'vi'
                ? 'Nhận phân tích học tập dựa trên dữ liệu, dự báo kết quả mục tiêu và đề xuất chiến lược học tập hiệu quả nhất.'
                : 'Get data-driven academic analyses, target forecast estimates, and optimized study strategies.'}
            </p>
          </div>
          
          {/* Warning badge */}
          <div className="bg-amber-500/15 border border-amber-500/30 text-amber-300 rounded-2xl p-4 max-w-xs flex gap-2 text-xs backdrop-blur-md">
            <AlertTriangle className="h-5 w-5 flex-shrink-0 text-amber-400" />
            <div>
              <span className="font-bold block mb-0.5">{language === 'vi' ? 'Tuyên bố miễn trừ' : 'Disclaimer'}</span>
              <span>{language === 'vi' 
                ? 'Dự báo chỉ là ước tính. Khuyến nghị chỉ mang tính chất tham khảo học tập.' 
                : 'Predictions are estimates. Recommendations are advisory only.'}</span>
            </div>
          </div>
        </div>
        <div className="absolute right-0 bottom-0 top-0 w-1/4 opacity-10 hidden lg:block select-none pointer-events-none">
          <Brain className="h-full w-full p-4" />
        </div>
      </div>

      {/* Main Split Layout Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
        
        {/* LEFT COLUMN: ACADEMIC INSIGHTS & CALCULATOR (7/12 cols) */}
        <div className="lg:col-span-7 space-y-8">
          
          {/* Academic Insights Box */}
          <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm space-y-6">
            <div className="flex justify-between items-center border-b border-gray-100 dark:border-gray-800 pb-3">
              <h2 className="text-lg font-black text-gray-900 dark:text-white flex items-center gap-2">
                <TrendingUp className="text-indigo-600 h-5 w-5" />
                {language === 'vi' ? 'Báo Cáo Phân Tích Kết Quả' : 'Academic Insights'}
              </h2>
              {loadingInsights && <Loader2 className="h-4 w-4 animate-spin text-brand-500" />}
            </div>

            {insightsError && (
              <div className="p-4 rounded-xl bg-red-50 text-red-700 text-xs font-semibold flex items-center gap-2">
                <AlertCircle className="h-4 w-4" />
                <span>{insightsError}</span>
              </div>
            )}

            {/* GPA & Credit stats grids */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              
              {/* GPA Metric Display */}
              <div className="bg-gray-50 dark:bg-gray-950 p-5 rounded-2xl border border-gray-100 dark:border-gray-850 flex flex-col justify-between">
                <div>
                  <p className="text-xs text-gray-400 font-bold uppercase tracking-wider">{t('gpa.cumulative')}</p>
                  <h3 className="text-4xl font-black text-brand-600 dark:text-brand-400 mt-2">
                    {dashboardSummary?.performanceSummary?.cumulativeGpa10?.toFixed(2) || '—'}
                  </h3>
                  <p className="text-xs text-gray-500 mt-1">
                    {language === 'vi' ? 'Điểm Hệ 4.0: ' : '4.0 Scale: '}
                    <strong className="text-gray-850 dark:text-gray-200 font-bold">
                      {dashboardSummary?.performanceSummary?.cumulativeGpa4?.toFixed(2) || '—'}
                    </strong>
                  </p>
                </div>
                <div className="mt-4 pt-3 border-t border-gray-100 dark:border-gray-800 flex justify-between items-center text-xs">
                  <span className="text-gray-400">{t('gpa.classification')}:</span>
                  <span className="font-bold text-indigo-600 dark:text-indigo-400">
                    {dashboardSummary?.performanceSummary?.classificationVn || '—'}
                  </span>
                </div>
              </div>

              {/* Credits Metric Display */}
              <div className="bg-gray-50 dark:bg-gray-950 p-5 rounded-2xl border border-gray-100 dark:border-gray-850 flex flex-col justify-between">
                <div>
                  <p className="text-xs text-gray-400 font-bold uppercase tracking-wider">{t('gpa.progress')}</p>
                  <h3 className="text-2xl font-black text-gray-900 dark:text-white mt-2">
                    {completed} / {totalRequired} {language === 'vi' ? 'Tín chỉ' : 'Credits'}
                  </h3>
                  
                  {/* Visual Progress Bar */}
                  <div className="w-full bg-gray-200 dark:bg-gray-800 h-2 rounded-full mt-3 overflow-hidden">
                    <div 
                      className="bg-brand-500 h-full rounded-full transition-all duration-500" 
                      style={{ width: `${Math.min((completed / (totalRequired || 1)) * 100, 100)}%` }}
                    />
                  </div>
                </div>
                
                <div className="mt-4 pt-3 border-t border-gray-100 dark:border-gray-800 flex justify-between items-center text-xs text-gray-500">
                  <span>{language === 'vi' ? 'Còn lại: ' : 'Remaining: '}<strong>{remaining}</strong></span>
                  <span>{language === 'vi' ? 'Đã đạt: ' : 'Passed: '}<strong>{creditProgress?.completedCredits || 0}</strong></span>
                </div>
              </div>

            </div>

            {/* Strengths & Weaknesses Courses lists */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 pt-2">
              
              {/* Strongest */}
              <div className="space-y-3">
                <h4 className="text-sm font-bold text-green-600 dark:text-green-400 flex items-center gap-1.5 uppercase tracking-wide">
                  <span className="h-2 w-2 rounded-full bg-green-500" />
                  {language === 'vi' ? 'Học phần thế mạnh' : 'Strongest Courses'}
                </h4>
                {strengthsWeaknesses?.strongestCourses && strengthsWeaknesses.strongestCourses.length > 0 ? (
                  <div className="space-y-2">
                    {strengthsWeaknesses.strongestCourses.slice(0, 3).map((c, i) => (
                      <div key={i} className="flex justify-between items-center p-3 bg-green-50/20 border border-green-200/30 rounded-xl">
                        <div className="min-w-0">
                          <p className="text-xs font-bold text-gray-800 dark:text-white truncate" title={c.courseName}>{c.courseName}</p>
                          <span className="text-[10px] text-gray-400">{c.courseCode}</span>
                        </div>
                        <span className="text-xs font-extrabold text-green-600 bg-green-50 px-2 py-0.5 rounded-md dark:bg-green-950/20 dark:text-green-400 border border-green-200/30">
                          {c.score.toFixed(1)} ({c.letterGrade})
                        </span>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-xs text-gray-400 italic py-2">{language === 'vi' ? 'Chưa có thông tin học phần.' : 'No course records found.'}</p>
                )}
              </div>

              {/* Weakest / Focus Needed */}
              <div className="space-y-3">
                <h4 className="text-sm font-bold text-red-600 dark:text-red-400 flex items-center gap-1.5 uppercase tracking-wide">
                  <span className="h-2 w-2 rounded-full bg-red-500" />
                  {language === 'vi' ? 'Cần tập trung cải thiện' : 'Needs Improvement'}
                </h4>
                {strengthsWeaknesses?.weakestCourses && strengthsWeaknesses.weakestCourses.length > 0 ? (
                  <div className="space-y-2">
                    {strengthsWeaknesses.weakestCourses.slice(0, 3).map((c, i) => (
                      <div key={i} className="flex justify-between items-center p-3 bg-red-50/20 border border-red-200/30 rounded-xl">
                        <div className="min-w-0">
                          <p className="text-xs font-bold text-gray-800 dark:text-white truncate" title={c.courseName}>{c.courseName}</p>
                          <span className="text-[10px] text-gray-400">{c.courseCode}</span>
                        </div>
                        <span className="text-xs font-extrabold text-red-600 bg-red-50 px-2 py-0.5 rounded-md dark:bg-red-950/20 dark:text-red-400 border border-red-200/30">
                          {c.score.toFixed(1)} ({c.letterGrade})
                        </span>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-xs text-gray-400 italic py-2">{language === 'vi' ? 'Chưa có thông tin học phần cần cải thiện.' : 'No critical areas identified.'}</p>
                )}
              </div>

            </div>

          </div>

          {/* Interactive Goal Planner Feasibility Simulator */}
          <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl p-6 shadow-sm space-y-6">
            <h2 className="text-lg font-black text-gray-900 dark:text-white flex items-center gap-2 border-b border-gray-100 dark:border-gray-800 pb-3">
              <Target className="text-indigo-600 h-5 w-5" />
              {language === 'vi' ? 'Trình Mô Phỏng Khả Thi Điểm Số' : 'GPA Target Feasibility Simulator'}
            </h2>

            <div className="space-y-6">
              
              {/* Target GPA slider input */}
              <div className="space-y-3">
                <div className="flex justify-between items-center">
                  <label className="text-sm font-bold text-gray-600 dark:text-gray-400">
                    {language === 'vi' ? 'Mục tiêu GPA mong muốn (Hệ 10):' : 'Desired Cumulative GPA (10-scale):'}
                  </label>
                  <div className="flex items-center gap-2">
                    <input
                      type="number"
                      min="0.0"
                      max="10.0"
                      step="0.05"
                      value={simTargetGpa}
                      onChange={(e) => setSimTargetGpa(Math.min(10.0, Math.max(0.0, parseFloat(e.target.value) || 0)))}
                      className="w-20 p-1.5 text-center font-black text-brand-600 dark:text-brand-400 bg-gray-50 border border-gray-200 dark:bg-gray-950 dark:border-gray-800 rounded-xl focus:outline-none focus:ring-2 focus:ring-brand-500"
                    />
                  </div>
                </div>

                <input
                  type="range"
                  min="4.0"
                  max="10.0"
                  step="0.05"
                  value={simTargetGpa}
                  onChange={(e) => setSimTargetGpa(parseFloat(e.target.value))}
                  className="w-full h-2 bg-gray-200 dark:bg-gray-850 rounded-lg appearance-none cursor-pointer accent-brand-500"
                />
                <div className="flex justify-between text-[10px] text-gray-400 font-bold select-none">
                  <span>4.0</span>
                  <span>6.0</span>
                  <span>8.0</span>
                  <span>10.0</span>
                </div>
              </div>

              {/* Calculator Output Display */}
              <div className="p-5 rounded-2xl bg-indigo-50 border border-indigo-100 dark:bg-indigo-950/20 dark:border-indigo-900/35 space-y-4">
                <div className="flex justify-between items-start">
                  <div>
                    <span className="text-[10px] font-extrabold uppercase text-indigo-600 dark:text-indigo-400 tracking-wider">
                      {language === 'vi' ? 'MỨC GPA YÊU CẦU' : 'REQUIRED GPA PER REMAINING CREDITS'}
                    </span>
                    <h3 className="text-3xl font-black text-indigo-950 dark:text-white mt-1">
                      {calculatedRemainingGpa !== null ? calculatedRemainingGpa.toFixed(2) : '—'}
                    </h3>
                  </div>
                  {getFeasibilityBadge(feasibility)}
                </div>

                <div className="flex gap-2 text-xs font-semibold leading-relaxed text-indigo-850 dark:text-indigo-300">
                  <Info className="h-4 w-4 text-indigo-500 flex-shrink-0 mt-0.5" />
                  <p>{adviceMessage}</p>
                </div>
              </div>

            </div>

          </div>

        </div>

        {/* RIGHT COLUMN: ADVISOR CHAT CONVERSATION WINDOW (5/12 cols) */}
        <div className="lg:col-span-5 h-[760px] flex flex-col bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-3xl overflow-hidden shadow-sm">
          
          {/* Header Panel with thread controls */}
          <div className="p-4 bg-gray-50 border-b border-gray-200 dark:bg-gray-900/60 dark:border-gray-800 flex justify-between items-center">
            <div className="flex items-center gap-2">
              <Brain className="h-5 w-5 text-brand-500 animate-pulse" />
              <span className="font-extrabold text-sm text-gray-900 dark:text-white">
                {language === 'vi' ? 'Trò Chuyện Với Cố Vấn' : 'Advisor Chat'}
              </span>
            </div>

            {/* Sidebar toggle or Actions */}
            <div className="flex gap-2">
              <button
                onClick={handleStartConversation}
                disabled={startingConversation || conversations.length >= 50}
                className="inline-flex items-center gap-1 px-3 py-1.5 rounded-xl bg-brand-500 hover:bg-brand-600 active:bg-brand-700 text-white text-xs font-bold transition-all disabled:opacity-50 shadow-md shadow-brand-500/10"
                title={t('aiAdvisor.startChat')}
              >
                {startingConversation ? (
                  <Loader2 className="h-3 w-3 animate-spin" />
                ) : (
                  <Plus className="h-3.5 w-3.5" />
                )}
                <span>New</span>
              </button>
            </div>
          </div>

          {/* Conversations sidebar and active dialogue panel split */}
          <div className="flex-1 flex overflow-hidden">
            
            {/* Left sidebar list (smaller sidebar inside card) */}
            <div className="w-1/3 border-r border-gray-150 dark:border-gray-800 overflow-y-auto flex flex-col bg-gray-50/50 dark:bg-gray-950/20">
              {loadingConversations ? (
                <div className="flex justify-center p-6">
                  <Loader2 className="h-5 w-5 animate-spin text-brand-500" />
                </div>
              ) : conversations.length === 0 ? (
                <div className="p-4 text-center text-xs text-gray-400 italic">
                  {t('aiAdvisor.noConversations') || 'No conversations'}
                </div>
              ) : (
                <div className="divide-y divide-gray-100 dark:divide-gray-850/50">
                  {conversations.map((c) => {
                    const isActive = activeConversation?.id === c.id;
                    const isDeleting = deletingId === c.id;
                    return (
                      <div
                        key={c.id}
                        onClick={() => !isDeleting && setActiveConversation(c)}
                        className={`p-3 cursor-pointer text-left transition-colors relative group select-none ${
                          isActive 
                            ? 'bg-white dark:bg-gray-900 border-l-2 border-brand-500 font-bold' 
                            : 'hover:bg-gray-100/50 dark:hover:bg-gray-850/20'
                        }`}
                      >
                        <div className="pr-4 space-y-1">
                          <p className={`text-xs text-gray-700 dark:text-gray-300 truncate ${isActive ? 'font-bold' : ''}`}>
                            {c.title}
                          </p>
                          <span className="text-[9px] text-gray-400 block font-medium">
                            {new Date(c.createdAt).toLocaleDateString(language === 'vi' ? 'vi' : 'en')}
                          </span>
                        </div>
                        
                        <button
                          onClick={(e) => handleDeleteConversation(c.id, e)}
                          disabled={isDeleting}
                          className="absolute right-2 top-3 opacity-0 group-hover:opacity-100 text-gray-400 hover:text-red-500 transition-opacity p-1 rounded-md"
                          title="Delete thread"
                        >
                          {isDeleting ? (
                            <Loader2 className="h-3 w-3 animate-spin" />
                          ) : (
                            <Trash2 className="h-3 w-3" />
                          )}
                        </button>
                      </div>
                    );
                  })}
                </div>
              )}
            </div>

            {/* Right Chat Dialogue Screen */}
            <div className="w-2/3 flex flex-col h-full overflow-hidden bg-white dark:bg-gray-900">
              
              {/* Message scroll log area */}
              <div className="flex-1 overflow-y-auto p-4 space-y-4">
                {chatError && (
                  <div className="p-3.5 rounded-2xl bg-red-50 border border-red-200 dark:bg-red-950/10 dark:border-red-900/40 text-red-700 dark:text-red-400 text-xs font-semibold flex items-center gap-2 select-none">
                    <AlertCircle className="h-4.5 w-4.5 flex-shrink-0" />
                    <span>{chatError}</span>
                  </div>
                )}

                {!activeConversation ? (
                  <div className="h-full flex flex-col items-center justify-center text-center p-6 space-y-4">
                    <div className="p-3 bg-brand-50 dark:bg-brand-950/30 rounded-2xl text-brand-500">
                      <Brain className="h-8 w-8" />
                    </div>
                    <div>
                      <h4 className="text-sm font-bold text-gray-800 dark:text-white">
                        {language === 'vi' ? 'Bắt Đầu Tư Vấn' : 'Academic AI Chat'}
                      </h4>
                      <p className="text-xs text-gray-400 mt-1 max-w-[200px] leading-relaxed">
                        {language === 'vi'
                          ? 'Bấm nút "New" để tạo cuộc hội thoại mới với Cố vấn.'
                          : 'Click the "New" button to start consulting your academic records.'}
                      </p>
                    </div>
                  </div>
                ) : loadingMessages ? (
                  <div className="h-full flex items-center justify-center">
                    <Loader2 className="h-8 w-8 animate-spin text-brand-500" />
                  </div>
                ) : messages.length === 0 ? (
                  <div className="h-full flex flex-col items-center justify-center text-center p-4 text-gray-400 space-y-2">
                    <Sparkles className="h-8 w-8 text-yellow-500" />
                    <p className="text-xs italic max-w-[200px] leading-relaxed">
                      {language === 'vi' 
                        ? 'Hỏi cố vấn về điểm số, lộ trình học tập, môn học cần tập trung, hoặc đề xuất cải thiện GPA.' 
                        : 'Ask your advisor about grades, target plans, weak points, or credit loads.'}
                    </p>
                  </div>
                ) : (
                  <div className="space-y-4">
                    {messages.map((m, i) => {
                      const isUser = m.role === 'user';
                      return (
                        <div
                          key={i}
                          className={`flex ${isUser ? 'justify-end' : 'justify-start'}`}
                        >
                          <div
                            className={`max-w-[90%] rounded-2xl p-3.5 text-left border ${
                              isUser
                                ? 'bg-indigo-650 text-white border-indigo-700 shadow-md shadow-indigo-600/5'
                                : 'bg-gray-50 text-gray-800 border-gray-150 dark:bg-gray-950/50 dark:text-gray-200 dark:border-gray-800/80 shadow-sm'
                            }`}
                          >
                            {!isUser ? (
                              renderMarkdown(m.content)
                            ) : (
                              <p className="text-sm font-semibold whitespace-pre-wrap leading-relaxed">{m.content}</p>
                            )}
                            <span className={`text-[8px] mt-1.5 block select-none ${isUser ? 'text-white/60 text-right' : 'text-gray-400 text-left'}`}>
                              {new Date(m.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                            </span>
                          </div>
                        </div>
                      );
                    })}
                    {sendingMessage && (
                      <div className="flex justify-start">
                        <div className="bg-gray-50 border border-gray-150 dark:bg-gray-950/50 dark:border-gray-800/80 rounded-2xl p-4 flex items-center gap-2.5">
                          <Loader2 className="h-4 w-4 animate-spin text-brand-500" />
                          <span className="text-xs text-gray-500 italic select-none">
                            {language === 'vi' ? 'Cố vấn đang suy nghĩ...' : 'Advisor is analyzing...'}
                          </span>
                        </div>
                      </div>
                    )}
                    <div ref={messagesEndRef} />
                  </div>
                )}
              </div>

              {/* Chat Input form area */}
              {activeConversation && (
                <div className="p-3 border-t border-gray-150 dark:border-gray-850">
                  <form onSubmit={handleSendMessage} className="flex gap-2">
                    <input
                      type="text"
                      disabled={sendingMessage || loadingMessages}
                      value={inputText}
                      onChange={(e) => setInputText(e.target.value)}
                      placeholder={language === 'vi' ? 'Nhập câu hỏi ở đây...' : 'Ask advisor here...'}
                      className="flex-1 bg-gray-50 border border-gray-200 dark:bg-gray-950 dark:border-gray-850 px-4 py-2.5 rounded-2xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 text-gray-900 dark:text-white disabled:opacity-50"
                    />
                    <button
                      type="submit"
                      disabled={!inputText.trim() || sendingMessage || loadingMessages}
                      className="p-2.5 rounded-2xl bg-indigo-600 hover:bg-indigo-700 text-white disabled:opacity-50 shadow-md shadow-indigo-600/10 flex items-center justify-center transition-colors"
                    >
                      <Send className="h-4.5 w-4.5" />
                    </button>
                  </form>
                  
                  {/* Limits and security notice info bar */}
                  <div className="flex justify-between items-center text-[9px] text-gray-400 mt-2 px-1 select-none font-medium">
                    <span className="flex items-center gap-0.5">
                      <Clock className="h-3 w-3" />
                      {language === 'vi' ? 'Giới hạn: 20 tin nhắn/giờ' : 'Limit: 20 msgs/hour'}
                    </span>
                    <span>
                      {language === 'vi' ? 'Quyền riêng tư được bảo vệ' : 'Anonymized & Secured'}
                    </span>
                  </div>
                </div>
              )}

            </div>

          </div>

        </div>

      </div>

    </div>
  );
};

export default AiAdvisorDashboard;
