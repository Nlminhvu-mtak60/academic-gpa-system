import React, { useEffect, useState } from 'react';
import { adminApi, AdminNotificationHistoryDto, AdminStudentDto } from '../api/adminApi';
import { useLanguage } from '../contexts/LanguageContext';
import { Send, Bell, History, Users, User, Info, CheckCircle, AlertTriangle } from 'lucide-react';

export const AdminNotificationPage: React.FC = () => {
  const { t } = useLanguage();
  
  // History tab pagination states
  const [history, setHistory] = useState<AdminNotificationHistoryDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [historyPage, setHistoryPage] = useState(1);
  const [loadingHistory, setLoadingHistory] = useState(true);
  
  // Direct Notification Form States
  const [students, setStudents] = useState<AdminStudentDto[]>([]);
  const [selectedStudentId, setSelectedStudentId] = useState('');
  const [directTitle, setDirectTitle] = useState('');
  const [directMessage, setDirectMessage] = useState('');
  const [directType, setDirectType] = useState('System');
  const [sendingDirect, setSendingDirect] = useState(false);
  const [directSuccess, setDirectSuccess] = useState(false);

  // Broadcast Notification Form States
  const [broadcastTitle, setBroadcastTitle] = useState('');
  const [broadcastMessage, setBroadcastMessage] = useState('');
  const [sendingBroadcast, setSendingBroadcast] = useState(false);
  const [broadcastSuccess, setBroadcastSuccess] = useState(false);

  // Toggle active tab (Send Notification vs History)
  const [activeForm, setActiveForm] = useState<'direct' | 'broadcast'>('direct');

  const fetchHistory = async () => {
    try {
      setLoadingHistory(true);
      const data = await adminApi.getNotificationHistory({
        page: historyPage,
        pageSize: 10,
      });
      setHistory(data.items);
      setTotalCount(data.totalCount);
    } catch (err) {
      console.error('Failed to load notification history:', err);
    } finally {
      setLoadingHistory(false);
    }
  };

  const fetchActiveStudents = async () => {
    try {
      const data = await adminApi.getStudents({
        page: 1,
        pageSize: 100, // Load first 100 active students for select dropdown
        isActive: true,
      });
      setStudents(data.items);
    } catch (err) {
      console.error('Failed to load active students list:', err);
    }
  };

  useEffect(() => {
    fetchHistory();
  }, [historyPage]);

  useEffect(() => {
    fetchActiveStudents();
  }, []);

  const handleSendDirect = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedStudentId || !directTitle.trim() || !directMessage.trim()) return;

    try {
      setSendingDirect(true);
      setDirectSuccess(false);
      await adminApi.sendDirectNotification({
        recipientId: selectedStudentId,
        title: directTitle.trim(),
        message: directMessage.trim(),
        type: directType,
      });
      setDirectSuccess(true);
      setDirectTitle('');
      setDirectMessage('');
      fetchHistory();
    } catch (err) {
      console.error('Failed to send direct notification:', err);
    } finally {
      setSendingDirect(false);
    }
  };

  const handleSendBroadcast = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!broadcastTitle.trim() || !broadcastMessage.trim()) return;

    try {
      setSendingBroadcast(true);
      setBroadcastSuccess(false);
      await adminApi.broadcastNotification({
        title: broadcastTitle.trim(),
        message: broadcastMessage.trim(),
      });
      setBroadcastSuccess(true);
      setBroadcastTitle('');
      setBroadcastMessage('');
      fetchHistory();
    } catch (err) {
      console.error('Failed to dispatch broadcast announcement:', err);
    } finally {
      setSendingBroadcast(false);
    }
  };

  const historyTotalPages = Math.max(Math.ceil(totalCount / 10), 1);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">{t('admin.notifications.title')}</h1>
        <p className="text-sm text-gray-500 dark:text-gray-400">Broadcast system announcements or send direct targeted alerts</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        {/* Notification Creator Panel */}
        <div className="lg:col-span-1 rounded-2xl bg-white dark:bg-gray-900 p-6 shadow-sm border border-gray-100 dark:border-gray-800 flex flex-col justify-between">
          <div className="space-y-4">
            
            {/* Form Toggle buttons */}
            <div className="flex bg-gray-50 dark:bg-gray-950 p-1 rounded-xl border border-gray-200/50 dark:border-gray-800/50">
              <button
                onClick={() => {
                  setActiveForm('direct');
                  setDirectSuccess(false);
                  setBroadcastSuccess(false);
                }}
                className={`flex-1 flex items-center justify-center gap-2 py-2 rounded-lg text-xs font-semibold transition-all ${
                  activeForm === 'direct'
                    ? 'bg-white dark:bg-gray-900 text-red-600 dark:text-red-400 shadow-sm'
                    : 'text-gray-500 hover:text-gray-900 dark:hover:text-white'
                }`}
              >
                <User className="h-4 w-4" />
                <span>Targeted alert</span>
              </button>
              <button
                onClick={() => {
                  setActiveForm('broadcast');
                  setDirectSuccess(false);
                  setBroadcastSuccess(false);
                }}
                className={`flex-1 flex items-center justify-center gap-2 py-2 rounded-lg text-xs font-semibold transition-all ${
                  activeForm === 'broadcast'
                    ? 'bg-white dark:bg-gray-900 text-red-600 dark:text-red-400 shadow-sm'
                    : 'text-gray-500 hover:text-gray-900 dark:hover:text-white'
                }`}
              >
                <Users className="h-4 w-4" />
                <span>System Broadcast</span>
              </button>
            </div>

            {/* Direct Notification Form */}
            {activeForm === 'direct' && (
              <form onSubmit={handleSendDirect} className="space-y-4">
                <div className="space-y-1">
                  <label className="text-xs font-semibold text-gray-500 uppercase">Select Target Student</label>
                  <select
                    required
                    value={selectedStudentId}
                    onChange={(e) => setSelectedStudentId(e.target.value)}
                    className="w-full border border-gray-200 dark:border-gray-800 rounded-lg bg-gray-50 dark:bg-gray-950 text-gray-900 dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-500"
                  >
                    <option value="">-- Choose recipient --</option>
                    {students.map((s) => (
                      <option key={s.id} value={s.id}>
                        {s.lastName} {s.firstName} ({s.studentCode || s.email})
                      </option>
                    ))}
                  </select>
                </div>

                <div className="grid grid-cols-1 gap-4">
                  <div className="space-y-1">
                    <label className="text-xs font-semibold text-gray-500 uppercase">Alert Category</label>
                    <select
                      value={directType}
                      onChange={(e) => setDirectType(e.target.value)}
                      className="w-full border border-gray-200 dark:border-gray-800 rounded-lg bg-gray-50 dark:bg-gray-950 text-gray-900 dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-500"
                    >
                      <option value="System">System Alert</option>
                      <option value="Academic">Academic / Scores</option>
                      <option value="Goal">Goal Target</option>
                      <option value="GpaMilestone">GPA Milestone</option>
                    </select>
                  </div>
                </div>

                <div className="space-y-1">
                  <label className="text-xs font-semibold text-gray-500 uppercase">Title</label>
                  <input
                    type="text"
                    required
                    placeholder="e.g. Profile updates pending..."
                    value={directTitle}
                    onChange={(e) => setDirectTitle(e.target.value)}
                    className="w-full border border-gray-200 dark:border-gray-800 rounded-lg bg-gray-50 dark:bg-gray-950 text-gray-900 dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-500"
                  />
                </div>

                <div className="space-y-1">
                  <label className="text-xs font-semibold text-gray-500 uppercase">Message Body</label>
                  <textarea
                    required
                    placeholder="Type details here..."
                    value={directMessage}
                    onChange={(e) => setDirectMessage(e.target.value)}
                    className="w-full border border-gray-200 dark:border-gray-800 rounded-lg bg-gray-50 dark:bg-gray-950 text-gray-900 dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-500 h-24"
                  />
                </div>

                {directSuccess && (
                  <div className="text-sm text-green-600 dark:text-green-400 flex items-center gap-1.5 font-medium">
                    <CheckCircle className="h-4.5 w-4.5" />
                    <span>Notification sent!</span>
                  </div>
                )}

                <button
                  type="submit"
                  disabled={sendingDirect}
                  className="w-full py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg text-sm font-semibold shadow-sm flex items-center justify-center gap-2 transition-colors disabled:opacity-50"
                >
                  <Send className="h-4 w-4" />
                  <span>{sendingDirect ? 'Sending...' : 'Send Alert'}</span>
                </button>
              </form>
            )}

            {/* Broadcast Notification Form */}
            {activeForm === 'broadcast' && (
              <form onSubmit={handleSendBroadcast} className="space-y-4">
                <div className="space-y-1">
                  <label className="text-xs font-semibold text-gray-500 uppercase">Announcement Title</label>
                  <input
                    type="text"
                    required
                    placeholder="e.g. System maintenance alert..."
                    value={broadcastTitle}
                    onChange={(e) => setBroadcastTitle(e.target.value)}
                    className="w-full border border-gray-200 dark:border-gray-800 rounded-lg bg-gray-50 dark:bg-gray-950 text-gray-900 dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-500"
                  />
                </div>

                <div className="space-y-1">
                  <label className="text-xs font-semibold text-gray-500 uppercase">Message Body</label>
                  <textarea
                    required
                    placeholder="This will alert ALL active students in the system..."
                    value={broadcastMessage}
                    onChange={(e) => setBroadcastMessage(e.target.value)}
                    className="w-full border border-gray-200 dark:border-gray-800 rounded-lg bg-gray-50 dark:bg-gray-950 text-gray-900 dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-500 h-28"
                  />
                </div>

                {broadcastSuccess && (
                  <div className="text-sm text-green-600 dark:text-green-400 flex items-center gap-1.5 font-medium">
                    <CheckCircle className="h-4.5 w-4.5" />
                    <span>Broadcast announcement sent!</span>
                  </div>
                )}

                <button
                  type="submit"
                  disabled={sendingBroadcast}
                  className="w-full py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg text-sm font-semibold shadow-sm flex items-center justify-center gap-2 transition-colors disabled:opacity-50"
                >
                  <Send className="h-4 w-4" />
                  <span>{sendingBroadcast ? 'Broadcasting...' : 'Broadcast to All'}</span>
                </button>
              </form>
            )}
          </div>
        </div>

        {/* History List Panel */}
        <div className="lg:col-span-2 rounded-2xl bg-white dark:bg-gray-900 p-6 shadow-sm border border-gray-100 dark:border-gray-800 flex flex-col justify-between min-h-[450px]">
          <div className="space-y-4">
            <div className="flex items-center gap-2 mb-2">
              <History className="h-5 w-5 text-gray-500" />
              <h3 className="text-lg font-bold text-gray-900 dark:text-white">{t('admin.notifications.history')}</h3>
            </div>

            <div className="space-y-3 max-h-[350px] overflow-y-auto pr-1">
              {loadingHistory ? (
                <div className="text-center py-8">
                  <div className="h-6 w-6 animate-spin rounded-full border-2 border-red-500 border-t-transparent mx-auto"></div>
                </div>
              ) : history.length === 0 ? (
                <div className="text-sm text-gray-400 text-center py-8">No notifications sent yet.</div>
              ) : (
                history.map((item) => (
                  <div key={item.id} className="border border-gray-100 dark:border-gray-800 bg-gray-50/50 dark:bg-gray-950 p-4 rounded-xl flex items-start justify-between gap-4">
                    <div className="space-y-1">
                      <div className="flex items-center gap-2">
                        {item.isBroadcast ? (
                          <span className="inline-flex px-1.5 py-0.5 rounded bg-red-100 text-red-700 dark:bg-red-950/45 dark:text-red-400 font-bold text-[9px] uppercase tracking-wider">
                            Broadcast
                          </span>
                        ) : (
                          <span className="inline-flex px-1.5 py-0.5 rounded bg-blue-100 text-blue-700 dark:bg-blue-950/45 dark:text-blue-400 font-bold text-[9px] uppercase tracking-wider">
                            Direct
                          </span>
                        )}
                        <h4 className="font-semibold text-gray-900 dark:text-white text-sm">{item.title}</h4>
                      </div>
                      <p className="text-xs text-gray-600 dark:text-gray-300">{item.message}</p>
                      
                      {!item.isBroadcast && item.recipientName && (
                        <div className="text-[10px] text-gray-400 font-medium">
                          Recipient: <span className="text-gray-500">{item.recipientName}</span>
                        </div>
                      )}
                    </div>
                    <span className="text-[10px] text-gray-400 shrink-0">
                      {new Date(item.createdAt).toLocaleDateString()}
                    </span>
                  </div>
                ))
              )}
            </div>
          </div>

          {/* History Pagination */}
          {historyTotalPages > 1 && (
            <div className="pt-4 border-t border-gray-100 dark:border-gray-800 flex justify-between items-center text-xs text-gray-500">
              <span>Total {totalCount} records</span>
              <div className="flex gap-2">
                <button
                  disabled={historyPage === 1}
                  onClick={() => setHistoryPage((prev) => Math.max(prev - 1, 1))}
                  className="px-2 py-1 border border-gray-200 dark:border-gray-800 rounded disabled:opacity-50"
                >
                  Prev
                </button>
                <span className="py-1">Page {historyPage} of {historyTotalPages}</span>
                <button
                  disabled={historyPage === historyTotalPages}
                  onClick={() => setHistoryPage((prev) => Math.min(prev + 1, historyTotalPages))}
                  className="px-2 py-1 border border-gray-200 dark:border-gray-800 rounded disabled:opacity-50"
                >
                  Next
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default AdminNotificationPage;
