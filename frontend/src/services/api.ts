import axios from 'axios';

// Khởi tạo instance kết nối đến C# Backend (URL lấy từ file .env)
const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'https://localhost:7034/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor: Tự động gắn token vào header trước khi gửi request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor: Xử lý lỗi trả về (ví dụ token hết hạn)
api.interceptors.response.use(
  (response) => response.data, // Chỉ lấy phần data từ axios response
  async (error) => {
    if (error.response?.status === 401) {
      // Token hết hạn, xử lý logout hoặc gọi API refresh token ở đây
      localStorage.removeItem('accessToken');
      window.location.href = '/login';
    }
    return Promise.reject(error.response?.data || error);
  }
);

export default api;