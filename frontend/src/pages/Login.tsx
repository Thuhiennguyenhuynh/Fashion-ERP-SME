import React, { useState } from 'react';
import { Form, Input, Button, message } from 'antd';
import { UserOutlined, LockOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';

interface LoginFormValues {
  email: string;
  password: string;
}

const Login: React.FC = () => {
  const navigate = useNavigate();
  const { login, isLoading } = useAuthStore();
  const [errorMsg, setErrorMsg] = useState('');

  const onFinish = async (values: LoginFormValues) => {
    try {
      setErrorMsg('');
      await login(values);
      message.success('Đăng nhập thành công!');
      navigate('/'); // Chuyển hướng vào trang chính
    } catch (error: unknown) {
      const errorMessage =
        error instanceof Error ? error.message : 'Đăng nhập thất bại';
      setErrorMsg(errorMessage);
      message.error(errorMessage);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-100 to-slate-200 p-4">
      <div className="max-w-md w-full bg-white rounded-2xl shadow-xl p-8 animate-slide-up border border-slate-100">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-extrabold text-slate-800 tracking-tight">
            FASHION <span className="text-blue-600">ERP</span>
          </h1>
          <p className="text-slate-500 mt-2">Đăng nhập để quản trị hệ thống</p>
        </div>

        {errorMsg && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 text-red-600 rounded-lg text-sm text-center">
            {errorMsg}
          </div>
        )}

        <Form name="login_form" layout="vertical" onFinish={onFinish} size="large">
          <Form.Item
            name="email"
            rules={[
              { required: true, message: 'Vui lòng nhập Email!' },
              { type: 'email', message: 'Email không đúng định dạng!' }
            ]}
          >
            <Input 
              prefix={<UserOutlined className="text-slate-400" />} 
              placeholder="Email đăng nhập" 
              className="rounded-xl"
            />
          </Form.Item>

          <Form.Item
            name="password"
            rules={[{ required: true, message: 'Vui lòng nhập Mật khẩu!' }]}
          >
            <Input.Password 
              prefix={<LockOutlined className="text-slate-400" />} 
              placeholder="Mật khẩu" 
              className="rounded-xl"
            />
          </Form.Item>

          <Form.Item>
            <Button 
              type="primary" 
              htmlType="submit" 
              loading={isLoading}
              className="w-full bg-blue-600 hover:bg-blue-700 h-11 rounded-xl font-semibold text-base shadow-md hover:shadow-lg transition-all"
            >
              Đăng nhập
            </Button>
          </Form.Item>
        </Form>
      </div>
    </div>
  );
};

export default Login;