import axios from 'axios';
import { useAuthStore } from '../store/useAuthStore'; // Giả định bạn dùng Zustand

const axiosClient = axios.create({
  baseURL: 'http://localhost:5038/api', // Port của ASP.NET Core backend
  headers: {
    'Content-Type': 'application/json',
  },
});

// Thêm token vào mỗi request
axiosClient.interceptors.request.use(
  (config) => {
    const token = useAuthStore.getState().accessToken;
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Xử lý response & Auto refresh token
axiosClient.interceptors.response.use(
  (response) => response.data, // Backend của bạn bọc data trong ApiResponse
  async (error) => {
    const originalRequest = error.config;
    
    // Bắt lỗi 401 Unauthorized và thử refresh token
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      try {
        const { accessToken, refreshToken } = useAuthStore.getState();
        const res = await axios.post('http://localhost:5038/api/auth/refresh', {
          accessToken,
          refreshToken,
        });
        
        const newAccessToken = res.data.data.accessToken;
        useAuthStore.getState().setTokens(newAccessToken, res.data.data.refreshToken);
        
        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
        return axiosClient(originalRequest);
      } catch (refreshError) {
        useAuthStore.getState().logout();
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }
    return Promise.reject(error);
  }
);

export default axiosClient;