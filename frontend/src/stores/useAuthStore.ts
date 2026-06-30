import { create } from 'zustand';
import { authApi, normalizeAuthResponse } from '../services/api';
import type { ApiResponse, AuthResponse, LoginRequest, UserInfo } from '../services/api';

export interface User extends UserInfo {}

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isLoading: boolean;
  login: (payload: LoginRequest) => Promise<void>;
  logout: () => void;
  hasRole: (...roles: string[]) => boolean;
}

const clearStoredAuth = () => {
  localStorage.removeItem('accessToken')
  localStorage.removeItem('access_token')
  localStorage.removeItem('token')
  localStorage.removeItem('refreshToken')
  localStorage.removeItem('refresh_token')
}

const normalizeRoleKey = (value: unknown) => {
  const role = typeof value === 'string' ? value.trim() : '';
  return role.toLowerCase().replace(/^role_/, '');
}

export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  accessToken: localStorage.getItem('accessToken') || localStorage.getItem('access_token') || localStorage.getItem('token'),
  refreshToken: localStorage.getItem('refreshToken') || localStorage.getItem('refresh_token'),
  isLoading: false,

  login: async (payload) => {
    set({ isLoading: true });
    try {
      const res = (await authApi.login(payload)) as unknown as ApiResponse<AuthResponse>;
      const { accessToken, refreshToken, user } = normalizeAuthResponse(res.data);
      if (!accessToken) {
        throw new Error('Phản hồi đăng nhập không chứa access token');
      }
      localStorage.setItem('accessToken', accessToken);
      localStorage.setItem('access_token', accessToken);
      localStorage.setItem('token', accessToken);
      if (refreshToken) {
        localStorage.setItem('refreshToken', refreshToken);
        localStorage.setItem('refresh_token', refreshToken);
      }
      set({ user: user ?? null, accessToken, refreshToken, isLoading: false });
    } catch (error: unknown) {
      set({ isLoading: false });
      const msg = error instanceof Error ? error.message : 'Đăng nhập thất bại';
      throw new Error(msg, { cause: error });
    }
  },

  logout: () => {
    clearStoredAuth();
    set({ user: null, accessToken: null, refreshToken: null });
  },

  hasRole: (...roles) => {
    const currentRole = get().user?.role;
    if (!currentRole) return false;
    return roles.some((role) => normalizeRoleKey(role) === normalizeRoleKey(currentRole));
  },
}));