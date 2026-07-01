import { Form, Input, Button, message } from 'antd';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../stores/useAuthStore';
import type { LoginRequest } from '../../types/api';

export default function LoginPage() {
  const navigate = useNavigate();
  const login = useAuthStore((s) => s.login);
  const isLoading = useAuthStore((s) => s.isLoading);

  const onFinish = async (values: LoginRequest) => {
    try {
      await login(values);
      message.success('Đăng nhập thành công');
      navigate('/');
    } catch (error: any) {
      message.error(error?.message || 'Tài khoản hoặc mật khẩu không chính xác');
    }
  };

  return (
    <main className="min-h-screen flex items-center justify-center bg-neutral-50 font-sans">
      <div className="w-full max-w-md p-8 bg-white rounded-xl shadow-sm border border-neutral-100">
        <div className="text-center mb-10">
          <h1 className="text-2xl font-serif font-bold tracking-widest text-neutral-900 uppercase">Fashion SME</h1>
          <p className="text-sm text-neutral-500 mt-2 font-light">Đăng nhập để quản lý hệ thống ERP</p>
        </div>

        <Form name="login_form" layout="vertical" onFinish={onFinish} requiredMark={false}>
          <Form.Item label={<span className="text-sm font-medium text-neutral-700">Email</span>} name="email" rules={[{ required: true, message: 'Vui lòng nhập email!' }, { type: 'email', message: 'Email không hợp lệ!' }]}> 
            <Input size="large" placeholder="admin@fashionerp.vn" className="rounded-md border-neutral-300 hover:border-neutral-400 focus:border-neutral-900 focus:ring-0" />
          </Form.Item>

          <Form.Item label={<span className="text-sm font-medium text-neutral-700">Mật khẩu</span>} name="password" rules={[{ required: true, message: 'Vui lòng nhập mật khẩu!' }]}> 
            <Input.Password size="large" placeholder="••••••••" className="rounded-md border-neutral-300 hover:border-neutral-400 focus:border-neutral-900 focus:ring-0" />
          </Form.Item>

          <Form.Item className="mt-8 mb-0">
            <Button type="primary" htmlType="submit" loading={isLoading} className="w-full bg-neutral-900 text-white font-medium tracking-wider uppercase h-12 rounded-md hover:!bg-neutral-800 border-none shadow-none">
              Đăng nhập
            </Button>
          </Form.Item>
        </Form>
      </div>
    </main>
  );
}
