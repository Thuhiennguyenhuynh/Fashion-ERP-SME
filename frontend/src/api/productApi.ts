// frontend/src/api/productApi.ts
import axiosClient from './axiosClient';

export interface ProductQueryParams {
  status?: string;
  categoryId?: string;
  brandId?: string;
  gender?: string;
  minPrice?: number;
  maxPrice?: number;
  keyword?: string;
  page?: number;
  pageSize?: number;
}

export const productApi = {
  getAll: (params: ProductQueryParams) => {
    return axiosClient.get('/products', { params });
  },
  getById: (id: string) => {
    return axiosClient.get(`/products/${id}`);
  }
};