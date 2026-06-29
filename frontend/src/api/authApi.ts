import axiosClient from './axiosClient';
import { User } from '../store/useAuthStore';

// Tham chiếu từ LoginRequestDto (C#)
export interface LoginPayload {
  email: string;
  password: string;
}

// Tham chiếu từ AuthResponseDto (C#)
export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

// C# bọc mọi response trong class ApiResponse<T>
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

export const authApi = {
  login: (payload: LoginPayload): Promise<ApiResponse<AuthResponse>> => {
    return axiosClient.post('/Auth/login', payload);
  },
  
  logout: (): Promise<ApiResponse<any>> => {
    return axiosClient.post('/Auth/logout');
  },
  
  getMe: (): Promise<ApiResponse<any>> => {
    return axiosClient.get('/Auth/me');
  }
};