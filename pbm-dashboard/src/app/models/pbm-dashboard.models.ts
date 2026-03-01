export interface PbmDashboardResponse {
  dashboardTitle: string;
  generatedUtc: string;
  summary: DashboardSummary;
  monthlySpend: MonthlySpendPoint[];
  departmentMix: DepartmentMixPoint[];
  facilities: FacilityPerformancePoint[];
  alerts: OperationalAlert[];
}

export interface DashboardSummary {
  activeFacilities: number;
  activePatients: number;
  claimsToday: number;
  paidAmountMonthToDate: number;
  genericDispenseRate: number;
  priorAuthorizationApprovalRate: number;
  averageTurnaroundHours: number;
  pendingUrgentAuthorizations: number;
}

export interface MonthlySpendPoint {
  month: string;
  paidAmount: number;
  claimsProcessed: number;
  deniedClaims: number;
}

export interface DepartmentMixPoint {
  department: string;
  claimsProcessed: number;
  paidAmount: number;
}

export interface FacilityPerformancePoint {
  facility: string;
  region: string;
  claimsProcessed: number;
  approvalRate: number;
  averageTurnaroundHours: number;
  paidAmount: number;
  pendingAuthorizations: number;
}

export interface OperationalAlert {
  severity: 'High' | 'Medium' | 'Low' | string;
  facility: string;
  message: string;
}
