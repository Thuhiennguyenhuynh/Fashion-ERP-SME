import { message } from 'antd';
import type { ApiResponse } from '../services/api';

export function handleApiError(error: unknown, fallback = 'Có lỗi xảy ra') {
  const axiosErr = error as { response?: { data?: ApiResponse<null> } };
  const msg = axiosErr?.response?.data?.message ?? fallback;
  const errors = axiosErr?.response?.data?.errors;
  if (errors?.length) {
    errors.forEach((e) => message.error(e));
  } else {
    message.error(msg);
  }
}
