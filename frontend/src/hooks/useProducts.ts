import { useQuery } from '@tanstack/react-query';
import { productApi } from '../api/productApi';

export function useProducts(keyword?: string) {
  return useQuery({
    queryKey: ['products', keyword],
    queryFn: () => productApi.getAll({ keyword, page: 1, pageSize: 50 }),
  });
}
