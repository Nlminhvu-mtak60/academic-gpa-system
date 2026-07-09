import axiosInstance from './axiosInstance';

export interface NotificationDto {
  id: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}

export interface NotificationsListDto {
  items: NotificationDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const notificationApi = {
  getNotifications: async (page = 1, pageSize = 10, unreadOnly?: boolean) => {
    const response = await axiosInstance.get('/v1/notifications', {
      params: { page, pageSize, unreadOnly }
    });
    return response.data;
  },

  getUnreadCount: async () => {
    const response = await axiosInstance.get('/v1/notifications/unread-count');
    return response.data;
  },

  markAsRead: async (id: string) => {
    const response = await axiosInstance.put(`/v1/notifications/${id}/read`);
    return response.data;
  },

  markAllAsRead: async () => {
    const response = await axiosInstance.put('/v1/notifications/read-all');
    return response.data;
  },

  deleteNotification: async (id: string) => {
    const response = await axiosInstance.delete(`/v1/notifications/${id}`);
    return response.data;
  }
};
