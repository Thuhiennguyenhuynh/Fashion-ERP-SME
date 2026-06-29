import { create } from 'zustand';
import api from '../services/api';

interface User {
  id: string;
  email: string;
  role: string;
  fullName?: string;
  avatarUrl?: string;
}

interface LoginPayload {
  email: string;
  password: string;
}

interface LoginResponse {
  success: boolean;
  data?: {
    accessToken: string;
    user: User;
  };
  message?: string;
}

interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  isLoading: boolean;
  login: (payload: LoginPayload) => Promise<void>;
  logout: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  isAuthenticated: !!localStorage.getItem('accessToken'),
  user: null,
  isLoading: false,

  login: async (payload) => {
    set({ isLoading: true });
    try {
      // Gọi API login đến C# Backend
      const response = await api.post<LoginResponse>('/auth/login', payload);
      const data = response as unknown as LoginResponse;

      if (data.success && data.data) {
        const { accessToken, user } = data.data;
        localStorage.setItem('accessToken', accessToken);
        set({ isAuthenticated: true, user, isLoading: false });
        return;
      }

      set({ isLoading: false });
      throw new Error(data.message || 'Đăng nhập thất bại');
    } catch (error: unknown) {
      set({ isLoading: false });
      const errorMessage =
        error instanceof Error
          ? error.message
          : 'Đăng nhập thất bại';
      throw new Error(errorMessage, { cause: error });
    }
  },

  logout: () => {
    localStorage.removeItem('accessToken');
    set({ isAuthenticated: false, user: null });
  },
}));