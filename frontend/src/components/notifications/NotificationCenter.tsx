import React, { useEffect, useState, useRef } from 'react';
import { Bell, Check, CheckSquare, Trash2, BookOpen, Target, Award, Info, X } from 'lucide-react';
import { notificationApi, NotificationDto } from '../../api/notificationApi';
import { useLanguage } from '../../contexts/LanguageContext';

export const NotificationCenter: React.FC = () => {
  const { t } = useLanguage();
  const [notifications, setNotifications] = useState<NotificationDto[]>([]);
  const [unreadCount, setUnreadCount] = useState<number>(0);
  const [isOpen, setIsOpen] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);
  const [filterUnread, setFilterUnread] = useState<boolean>(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const fetchUnreadCount = async () => {
    try {
      const res = await notificationApi.getUnreadCount();
      if (res.success) {
        setUnreadCount(res.data);
      }
    } catch (err) {
      console.error('Failed to fetch unread count:', err);
    }
  };

  const fetchNotifications = async () => {
    setLoading(true);
    try {
      const res = await notificationApi.getNotifications(1, 20, filterUnread || undefined);
      if (res.success) {
        setNotifications(res.data.items);
      }
    } catch (err) {
      console.error('Failed to fetch notifications:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUnreadCount();
    // Poll unread count every 30 seconds for real-time feel
    const interval = setInterval(fetchUnreadCount, 30000);
    return () => clearInterval(interval);
  }, []);

  useEffect(() => {
    if (isOpen) {
      fetchNotifications();
    }
  }, [isOpen, filterUnread]);

  // Close dropdown on click outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleMarkAsRead = async (id: string, e: React.MouseEvent) => {
    e.stopPropagation();
    try {
      const res = await notificationApi.markAsRead(id);
      if (res.success) {
        setNotifications(prev =>
          prev.map(n => n.id === id ? { ...n, isRead: true } : n)
        );
        fetchUnreadCount();
      }
    } catch (err) {
      console.error('Failed to mark notification as read:', err);
    }
  };

  const handleMarkAllAsRead = async () => {
    try {
      const res = await notificationApi.markAllAsRead();
      if (res.success) {
        setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
        setUnreadCount(0);
      }
    } catch (err) {
      console.error('Failed to mark all as read:', err);
    }
  };

  const handleDelete = async (id: string, e: React.MouseEvent) => {
    e.stopPropagation();
    try {
      const res = await notificationApi.deleteNotification(id);
      if (res.success) {
        setNotifications(prev => prev.filter(n => n.id !== id));
        fetchUnreadCount();
      }
    } catch (err) {
      console.error('Failed to delete notification:', err);
    }
  };

  const getIcon = (type: string) => {
    switch (type.toLowerCase()) {
      case 'academic':
        return <BookOpen className="h-5 w-5 text-blue-500" />;
      case 'goal':
        return <Target className="h-5 w-5 text-emerald-500" />;
      case 'gpamilestone':
        return <Award className="h-5 w-5 text-amber-500" />;
      default:
        return <Info className="h-5 w-5 text-gray-500" />;
    }
  };

  const formatTime = (dateStr: string) => {
    const d = new Date(dateStr);
    return d.toLocaleDateString() + ' ' + d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  return (
    <div className="relative" ref={dropdownRef}>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="relative p-2 rounded-lg text-gray-500 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800 transition-colors focus:outline-none"
        title="Notifications"
      >
        <Bell className="h-5 w-5" />
        {unreadCount > 0 && (
          <span className="absolute top-1 right-1 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white animate-pulse">
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </button>

      {isOpen && (
        <div className="absolute right-0 mt-2 w-80 sm:w-96 rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden z-50 transition-all duration-200 ease-out transform origin-top-right">
          {/* Header */}
          <div className="flex items-center justify-between px-4 py-3 border-b border-gray-100 dark:border-gray-800">
            <h3 className="font-semibold text-gray-900 dark:text-white flex items-center gap-2">
              <span>{t('notifications.title')}</span>
              {unreadCount > 0 && (
                <span className="px-2 py-0.5 rounded-full bg-brand-50 dark:bg-brand-950 text-xs font-medium text-brand-600 dark:text-brand-400">
                  {unreadCount} {t('notifications.unreadOnly')}
                </span>
              )}
            </h3>
            <div className="flex items-center gap-1">
              <button
                onClick={handleMarkAllAsRead}
                disabled={unreadCount === 0}
                className="p-1.5 rounded hover:bg-gray-100 dark:hover:bg-gray-800 text-gray-500 dark:text-gray-400 disabled:opacity-50 disabled:cursor-not-allowed"
                title={t('notifications.markAllRead')}
              >
                <CheckSquare className="h-4 w-4" />
              </button>
              <button
                onClick={() => setIsOpen(false)}
                className="p-1.5 rounded hover:bg-gray-100 dark:hover:bg-gray-800 text-gray-500 dark:text-gray-400"
              >
                <X className="h-4 w-4" />
              </button>
            </div>
          </div>

          {/* Filters */}
          <div className="flex items-center gap-2 px-4 py-2 bg-gray-50 dark:bg-gray-900/50 border-b border-gray-100 dark:border-gray-800 text-xs">
            <button
              onClick={() => setFilterUnread(false)}
              className={`px-2.5 py-1 rounded-full font-medium transition-colors ${
                !filterUnread
                  ? 'bg-brand-500 text-white'
                  : 'text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800'
              }`}
            >
              All
            </button>
            <button
              onClick={() => setFilterUnread(true)}
              className={`px-2.5 py-1 rounded-full font-medium transition-colors ${
                filterUnread
                  ? 'bg-brand-500 text-white'
                  : 'text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800'
              }`}
            >
              {t('notifications.unreadOnly')}
            </button>
          </div>

          {/* List Area */}
          <div className="max-h-[350px] overflow-y-auto divide-y divide-gray-100 dark:divide-gray-800">
            {loading && notifications.length === 0 ? (
              <div className="flex py-8 justify-center items-center">
                <div className="h-6 w-6 animate-spin rounded-full border-2 border-brand-500 border-t-transparent"></div>
              </div>
            ) : notifications.length === 0 ? (
              <div className="flex flex-col py-12 items-center justify-center text-gray-400 dark:text-gray-500">
                <Bell className="h-10 w-10 mb-2 opacity-50" />
                <span className="text-sm font-medium">{t('notifications.empty')}</span>
              </div>
            ) : (
              notifications.map((notif) => (
                <div
                  key={notif.id}
                  onClick={(e) => !notif.isRead && handleMarkAsRead(notif.id, e)}
                  className={`flex gap-3 p-4 cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors relative group ${
                    !notif.isRead ? 'bg-brand-50/20 dark:bg-brand-950/10' : ''
                  }`}
                >
                  {/* Unread dot */}
                  {!notif.isRead && (
                    <span className="absolute left-2 top-1/2 -translate-y-1/2 w-1.5 h-1.5 bg-brand-500 rounded-full" />
                  )}

                  {/* Icon */}
                  <div className="flex-shrink-0 mt-0.5">{getIcon(notif.type)}</div>

                  {/* Content */}
                  <div className="flex-1 min-w-0">
                    <p className={`text-sm text-gray-900 dark:text-white leading-tight mb-0.5 ${
                      !notif.isRead ? 'font-semibold' : 'font-medium'
                    }`}>
                      {notif.title}
                    </p>
                    <p className="text-xs text-gray-600 dark:text-gray-400 mb-1 break-words">
                      {notif.message}
                    </p>
                    <span className="text-[10px] text-gray-400 dark:text-gray-500">
                      {formatTime(notif.createdAt)}
                    </span>
                  </div>

                  {/* Quick actions on hover */}
                  <div className="flex-shrink-0 flex items-start gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                    {!notif.isRead && (
                      <button
                        onClick={(e) => handleMarkAsRead(notif.id, e)}
                        className="p-1 rounded bg-gray-100 hover:bg-brand-500 hover:text-white dark:bg-gray-800 text-gray-500 dark:text-gray-400 transition-colors"
                        title="Mark as read"
                      >
                        <Check className="h-3 w-3" />
                      </button>
                    )}
                    <button
                      onClick={(e) => handleDelete(notif.id, e)}
                      className="p-1 rounded bg-gray-100 hover:bg-red-500 hover:text-white dark:bg-gray-800 text-gray-500 dark:text-gray-400 transition-colors"
                      title="Delete"
                    >
                      <Trash2 className="h-3 w-3" />
                    </button>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
};
