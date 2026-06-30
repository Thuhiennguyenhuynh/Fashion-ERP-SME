// src/types/shift.ts
export interface Register {
  id: string;
  name: string;          
  location?: string;
  isActive: boolean;
}

export interface OpenShiftPayload {
  registerId: string;
  openingCash: number;   
  note?: string;
}

export interface CloseShiftPayload {
  shiftId: string;
  actualCash: number;    
  note?: string;
}

export interface ShiftSummary {
  totalOrders: number;
  totalRevenue: number;
  cashRevenue: number;
  transferRevenue: number;
  cardRevenue: number;
  totalReturns: number;
  cashExpenses: number;      
  cashIn: number;             
  cashOut: number;             
}

export interface Shift {
  id: string;
  registerId: string;
  registerName: string;
  openedBy: string;            
  openedAt: string;            
  closedBy?: string;
  closedAt?: string;
  status: 'Open' | 'Closed';
  openingCash: number;
  expectedCash: number;        
  actualCash?: number;
  cashDifference?: number;     
  note?: string;
  summary: ShiftSummary;
  staffInvolved: { id: string; fullName: string; orderCount: number }[]; 
}

export interface ShiftQueryParams {
  page?: number;
  pageSize?: number;
  registerId?: string;
  status?: 'Open' | 'Closed';
  from?: string;
  to?: string;
}