// frontend/src/api/axiosClient.ts
import axios from 'axios';

const axiosClient = axios.create({
  baseURL: 'https://localhost:7034/api',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor xử lý dữ liệu trước khi gửi lên Server
axiosClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('access_token');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Interceptor xử lý dữ liệu trả về từ Server
axiosClient.interceptors.response.use(
  (response) => {
    if (response.data && response.data.success) {
      return response.data.data;
    }
    return response.data;
  },
  async (error) => {
    const originalRequest = error.config;
    
    // ĐÃ SỬA CHỖ NÀY: Thêm điều kiện KHÔNG chặn lỗi 401 nếu đang gọi API /auth/login
    if (
      error.response?.status === 401 && 
      !originalRequest._retry && 
      !originalRequest.url?.includes('/auth/login')
    ) {
      originalRequest._retry = true;
      try {
        const refreshToken = localStorage.getItem('refresh_token');
        const accessToken = localStorage.getItem('access_token');
        
        const res: any = await axios.post('https://localhost:7034/api/auth/refresh', {
          accessToken,
          refreshToken
        });
        
        if (res.data?.success) {
          localStorage.setItem('access_token', res.data.data.accessToken);
          localStorage.setItem('refresh_token', res.data.data.refreshToken);
          // Gắn lại token mới và gọi lại request bị lỗi ban đầu
          originalRequest.headers.Authorization = `Bearer ${res.data.data.accessToken}`;
          return axiosClient(originalRequest);
        }
      } catch (refreshError) {
        localStorage.clear();
        window.location.href = '/login'; // Chuyển hướng khi refresh token thật sự thất bại
      }
    }
    
    // Trả về lỗi thẳng ra ngoài để LoginPage.tsx có thể bắt được và hiện thông báo đỏ
    return Promise.reject(error.response?.data || error);
  }
);

export default axiosClient;