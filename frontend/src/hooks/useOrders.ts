import { useQuery } from '@tanstack/react-query';
import { orderApi } from '../api/orderApi';

export function useOrders() {
  return useQuery({
    queryKey: ['orders'],
    queryFn: () => orderApi.getAll({ page: 1, pageSize: 20 }),
  });
}
