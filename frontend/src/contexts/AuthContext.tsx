import React, { createContext, useContext, useEffect, useState } from 'react';
import { authApi } from '../api/authApi';
import { setAccessToken, registerLogoutCallback } from '../api/axiosInstance';

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  avatarUrl?: string;
  preferredLanguage: string;
  preferredTheme: string;
  forcePasswordChange?: boolean;
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: Record<string, string>) => Promise<User>;
  register: (data: Record<string, string>) => Promise<void>;
  logout: () => Promise<void>;
  googleLogin: (idToken: string) => Promise<User>;
  updateUser: (updatedUser: Partial<User>) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const handleAuthResponse = (data: { accessToken: string; user: User }) => {
    setAccessToken(data.accessToken);
    setUser(data.user);
  };

  const updateUser = (updatedUser: Partial<User>) => {
    setUser((prev) => (prev ? { ...prev, ...updatedUser } : null));
  };

  const login = async (credentials: Record<string, string>) => {
    const response = await authApi.login(credentials);
    handleAuthResponse(response.data);
    return response.data.user;
  };

  const register = async (data: Record<string, string>) => {
    const response = await authApi.register(data);
    handleAuthResponse(response.data);
  };

  const logout = async () => {
    try {
      await authApi.logout();
    } finally {
      setAccessToken(null);
      setUser(null);
    }
  };

  const googleLogin = async (idToken: string) => {
    const response = await authApi.googleLogin(idToken);
    handleAuthResponse(response.data);
    return response.data.user;
  };

  // Perform silent token refresh on mount to check if user has active session
  useEffect(() => {
    const silentRefresh = async () => {
      try {
        const response = await axios.post('/api/v1/auth/refresh-token', {});
        handleAuthResponse(response.data.data);
      } catch {
        // No active session cookie or invalid token
        setAccessToken(null);
        setUser(null);
      } finally {
        setIsLoading(false);
      }
    };

    // Register automatic logout trigger on API 401 failure
    registerLogoutCallback(() => {
      setAccessToken(null);
      setUser(null);
    });

    silentRefresh();
  }, []);

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        login,
        register,
        logout,
        googleLogin,
        updateUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

// Import statement mapping to prevent build issues
import axios from 'axios';

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within an AuthProvider');
  return context;
};
