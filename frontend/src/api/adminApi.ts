import axiosInstance from './axiosInstance';

export interface AdminStudentDto {
  id: string;
  studentCode: string;
  firstName: string;
  lastName: string;
  email: string;
  universityName: string;
  majorName: string;
  isActive: boolean;
  cumulativeGpa10: number | null;
  cumulativeGpa4: number | null;
  registrationDate: string;
  lastLoginAt: string | null;
}

export interface AdminStudentDetailDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  studentCode: string | null;
  universityName: string | null;
  majorName: string | null;
  enrollmentYear: number | null;
  totalRequiredCredits: number | null;
  cumulativeGpa10: number | null;
  cumulativeGpa4: number | null;
}

export interface UserStatsDto {
  totalStudents: number;
  activeStudents: number;
  lockedAccounts: number;
}

export interface AcademicOverviewDto {
  systemAverageGpa10: number | null;
  systemAverageGpa4: number | null;
  totalCreditsEarned: number;
}

export interface GpaDistributionDto {
  excellent: number;
  veryGood: number;
  good: number;
  average: number;
  belowAverage: number;
  fail: number;
}

export interface AdminStatisticsDto {
  userStats: UserStatsDto;
  academicOverview: AcademicOverviewDto;
  gpaDistribution: GpaDistributionDto;
}

export interface AdminNotificationHistoryDto {
  id: string;
  title: string;
  message: string;
  isBroadcast: boolean;
  recipientName: string | null;
  createdAt: string;
}

export interface AdminUserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  isActive: boolean;
  lastLoginAt: string | null;
}

export interface UserActivityLogDto {
  id: string;
  userId: string;
  userEmail: string;
  activity: string;
  ipAddress: string;
  timestamp: string;
}

export interface EditStudentInfoRequest {
  firstName: string;
  lastName: string;
  studentCode: string;
  universityName: string;
  majorName: string;
  enrollmentYear: number;
  totalRequiredCredits: number;
}

export interface SendDirectNotificationRequest {
  recipientId: string;
  title: string;
  message: string;
  type: string;
}

export interface BroadcastNotificationRequest {
  title: string;
  message: string;
}

export const adminApi = {
  getStatistics: async (): Promise<AdminStatisticsDto> => {
    const response = await axiosInstance.get('/v1/admin/statistics');
    return response.data.data;
  },

  getStudents: async (params: {
    page: number;
    pageSize: number;
    search?: string;
    isActive?: boolean;
    sortBy?: string;
    sortOrder?: string;
  }): Promise<{ items: AdminStudentDto[]; totalCount: number }> => {
    const response = await axiosInstance.get('/v1/admin/students', { params });
    return response.data.data;
  },

  getStudentDetails: async (id: string): Promise<AdminStudentDetailDto> => {
    const response = await axiosInstance.get(`/v1/admin/students/${id}`);
    return response.data.data;
  },

  editStudentInfo: async (id: string, data: EditStudentInfoRequest): Promise<void> => {
    await axiosInstance.put(`/v1/admin/students/${id}`, data);
  },

  lockStudent: async (id: string, reason: string): Promise<void> => {
    await axiosInstance.put(`/v1/admin/students/${id}/lock`, { reason });
  },

  unlockStudent: async (id: string): Promise<void> => {
    await axiosInstance.put(`/v1/admin/students/${id}/unlock`);
  },

  deleteStudent: async (id: string): Promise<void> => {
    await axiosInstance.delete(`/v1/admin/students/${id}`);
  },

  resetPassword: async (id: string): Promise<string> => {
    const response = await axiosInstance.post(`/v1/admin/students/${id}/reset-password`);
    return response.data.data.temporaryPassword;
  },

  getUsers: async (params: {
    page: number;
    pageSize: number;
    search?: string;
  }): Promise<{ items: AdminUserDto[]; totalCount: number }> => {
    const response = await axiosInstance.get('/v1/admin/users', { params });
    return response.data.data;
  },

  getActivityLogs: async (params: {
    page: number;
    pageSize: number;
    userId?: string;
  }): Promise<{ items: UserActivityLogDto[]; totalCount: number }> => {
    const response = await axiosInstance.get('/v1/admin/activity-logs', { params });
    return response.data.data;
  },

  sendDirectNotification: async (data: SendDirectNotificationRequest): Promise<void> => {
    await axiosInstance.post('/v1/admin/notifications', data);
  },

  broadcastNotification: async (data: BroadcastNotificationRequest): Promise<void> => {
    await axiosInstance.post('/v1/admin/notifications/broadcast', data);
  },

  getNotificationHistory: async (params: {
    page: number;
    pageSize: number;
  }): Promise<{ items: AdminNotificationHistoryDto[]; totalCount: number }> => {
    const response = await axiosInstance.get('/v1/admin/notifications/history', { params });
    return response.data.data;
  },
};
