import {
  productApi as productApiClient,
  variantApi as variantApiClient,
  inventoryApi as inventoryApiClient,
} from '../services/api';

export const productApi = productApiClient;
export const variantApi = variantApiClient;
export const inventoryApi = inventoryApiClient;
export default productApi;
