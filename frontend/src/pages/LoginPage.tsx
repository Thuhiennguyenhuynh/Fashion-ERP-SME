import React, { useState } from 'react';
import { Form, Input, Button, message } from 'antd'; // Đã import thêm Button từ antd
import { useNavigate } from 'react-router-dom';
import axiosClient from '../api/axiosClient';

export default function LoginPage() {
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const onFinish = async (values: any) => {
    // In ra console để đảm bảo hàm này thực sự được gọi và không bị reload trang
    console.log("Dữ liệu gửi đi:", values); 
    setLoading(true);
    
    try {
      // Gọi API đăng nhập từ Backend
      const res: any = await axiosClient.post('/auth/login', values);
      
      console.log("Phản hồi từ Server:", res); // In ra để kiểm tra token
      
      if (res?.accessToken) {
        // Lưu token vào localStorage
        localStorage.setItem('access_token', res.accessToken);
        localStorage.setItem('refresh_token', res.refreshToken);
        
        message.success('Đăng nhập thành công');
        // Chuyển hướng về trang chủ (Dashboard)
        navigate('/');
      } else {
        message.error('Không nhận được Token từ server');
      }
    } catch (error: any) {
      console.error("Lỗi đăng nhập:", error);
      message.error(error?.message || 'Tài khoản hoặc mật khẩu không chính xác');
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="min-h-screen flex items-center justify-center bg-neutral-50 font-sans">
      <div className="w-full max-w-md p-8 bg-white rounded-xl shadow-sm border border-neutral-100">
        <div className="text-center mb-10">
          <h1 className="text-2xl font-serif font-bold tracking-widest text-neutral-900 uppercase">
            Fashion SME
          </h1>
          <p className="text-sm text-neutral-500 mt-2 font-light">
            Đăng nhập để quản lý hệ thống ERP
          </p>
        </div>

        <Form
          name="login_form"
          layout="vertical"
          onFinish={onFinish}
          requiredMark={false}
        >
          <Form.Item
            label={<span className="text-sm font-medium text-neutral-700">Email</span>}
            name="email"
            rules={[
              { required: true, message: 'Vui lòng nhập email!' },
              { type: 'email', message: 'Email không hợp lệ!' }
            ]}
          >
            <Input 
              size="large" 
              placeholder="admin@fashionerp.vn" 
              className="rounded-md border-neutral-300 hover:border-neutral-400 focus:border-neutral-900 focus:ring-0"
            />
          </Form.Item>

          <Form.Item
            label={<span className="text-sm font-medium text-neutral-700">Mật khẩu</span>}
            name="password"
            rules={[{ required: true, message: 'Vui lòng nhập mật khẩu!' }]}
          >
            <Input.Password 
              size="large" 
              placeholder="••••••••" 
              className="rounded-md border-neutral-300 hover:border-neutral-400 focus:border-neutral-900 focus:ring-0"
            />
          </Form.Item>

          <Form.Item className="mt-8 mb-0">
            {/* ĐÃ SỬA: Sử dụng Button của Ant Design thay vì button HTML */}
            <Button
              type="primary"
              htmlType="submit"
              loading={loading}
              className="w-full bg-neutral-900 text-white font-medium tracking-wider uppercase h-12 rounded-md hover:!bg-neutral-800 border-none shadow-none"
            >
              Đăng nhập
            </Button>
          </Form.Item>
        </Form>
      </div>
    </main>
  );
}