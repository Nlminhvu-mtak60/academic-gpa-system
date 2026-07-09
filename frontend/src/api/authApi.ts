import axiosInstance from './axiosInstance';

export const authApi = {
  register: async (data: Record<string, string>) => {
    const response = await axiosInstance.post('/v1/auth/register', data);
    return response.data;
  },

  login: async (data: Record<string, string>) => {
    const response = await axiosInstance.post('/v1/auth/login', data);
    return response.data;
  },

  logout: async () => {
    const response = await axiosInstance.post('/v1/auth/logout');
    return response.data;
  },

  forgotPassword: async (data: Record<string, string>) => {
    const response = await axiosInstance.post('/v1/auth/forgot-password', data);
    return response.data;
  },

  resetPassword: async (data: Record<string, string>) => {
    const response = await axiosInstance.post('/v1/auth/reset-password', data);
    return response.data;
  },

  googleLogin: async (idToken: string) => {
    const response = await axiosInstance.post('/v1/auth/google-login', { idToken });
    return response.data;
  }
};
