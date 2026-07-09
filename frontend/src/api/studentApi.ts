import axiosInstance from './axiosInstance';

export interface StudentProfileSubDto {
  studentCode: string;
  universityName: string;
  majorName: string;
  enrollmentYear: number;
  totalRequiredCredits: number;
}

export interface StudentProfileDetailsDto {
  email: string;
  firstName: string;
  lastName: string;
  avatarUrl: string | null;
  preferredLanguage: string;
  preferredTheme: string;
  profile: StudentProfileSubDto | null;
}

export interface UpdateProfileRequest {
  studentCode: string;
  universityName: string;
  majorName: string;
  enrollmentYear: number;
  totalRequiredCredits: number;
}

export interface UpdatePreferencesRequest {
  preferredLanguage: string;
  preferredTheme: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export const studentApi = {
  getProfile: async () => {
    const response = await axiosInstance.get('/v1/students/profile');
    return response.data; // Wraps { success, data, errors, message }
  },

  updateProfile: async (data: UpdateProfileRequest) => {
    const response = await axiosInstance.put('/v1/students/profile', data);
    return response.data;
  },

  updatePreferences: async (data: UpdatePreferencesRequest) => {
    const response = await axiosInstance.put('/v1/students/preferences', data);
    return response.data;
  },

  changePassword: async (data: ChangePasswordRequest) => {
    const response = await axiosInstance.put('/v1/students/change-password', data);
    return response.data;
  },

  uploadAvatar: async (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await axiosInstance.post('/v1/students/profile/avatar', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },
};
