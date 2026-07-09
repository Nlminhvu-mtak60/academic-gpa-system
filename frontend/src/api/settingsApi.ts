import axiosInstance from './axiosInstance';

export interface UserSettingsDto {
  preferredLanguage: string;
  preferredTheme: string;
  receiveSystem: boolean;
  receiveAcademic: boolean;
  receiveGoal: boolean;
  receiveGpaMilestone: boolean;
}

export interface UpdateUserEmailRequest {
  newEmail: string;
}

export const settingsApi = {
  getSettings: async () => {
    const response = await axiosInstance.get('/v1/settings');
    return response.data;
  },

  updateSettings: async (data: UserSettingsDto) => {
    const response = await axiosInstance.put('/v1/settings', data);
    return response.data;
  },

  updateEmail: async (newEmail: string) => {
    const response = await axiosInstance.put('/v1/settings/email', { newEmail });
    return response.data;
  }
};
