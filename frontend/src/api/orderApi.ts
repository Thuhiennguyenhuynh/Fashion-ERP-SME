import {
  orderApi as orderApiClient,
  customerApi as customerApiClient,
  promotionApi as promotionApiClient,
  dashboardApi as dashboardApiClient,
  reportApi as reportApiClient,
} from '../services/api';

export const orderApi = orderApiClient;
export const customerApi = customerApiClient;
export const promotionApi = promotionApiClient;
export const dashboardApi = dashboardApiClient;
export const reportApi = reportApiClient;
export default orderApi;
