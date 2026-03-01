import { Component, OnInit } from '@angular/core';
import { DashboardApiService } from '../../services/dashboard-api.service';
import {
  DepartmentMixPoint,
  FacilityPerformancePoint,
  MonthlySpendPoint,
  OperationalAlert,
  PbmDashboardResponse
} from '../../models/pbm-dashboard.models';

@Component({
  selector: 'app-pbm-dashboard',
  templateUrl: './pbm-dashboard.component.html',
  styleUrls: ['./pbm-dashboard.component.css']
})
export class PbmDashboardComponent implements OnInit {
  dashboard: PbmDashboardResponse | null = null;
  selectedRegion = 'All';
  selectedFacilityName = '';
  isLoading = true;
  errorMessage = '';

  constructor(private readonly dashboardApi: DashboardApiService) {}

  ngOnInit(): void {
    this.dashboardApi.getDashboard().subscribe({
      next: (dashboard) => {
        this.dashboard = dashboard;
        this.selectedFacilityName = dashboard.facilities[0]?.facility ?? '';
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Dashboard data is unavailable. Confirm the ASP.NET API is running.';
        this.isLoading = false;
      }
    });
  }

  get regions(): string[] {
    if (!this.dashboard) {
      return ['All'];
    }

    return ['All', ...new Set(this.dashboard.facilities.map((facility) => facility.region))];
  }

  get filteredFacilities(): FacilityPerformancePoint[] {
    if (!this.dashboard) {
      return [];
    }

    return this.selectedRegion === 'All'
      ? this.dashboard.facilities
      : this.dashboard.facilities.filter((facility) => facility.region === this.selectedRegion);
  }

  get selectedFacility(): FacilityPerformancePoint | undefined {
    return this.filteredFacilities.find((facility) => facility.facility === this.selectedFacilityName);
  }

  get monthlySpend(): MonthlySpendPoint[] {
    return this.dashboard?.monthlySpend ?? [];
  }

  get departmentMix(): DepartmentMixPoint[] {
    return (this.dashboard?.departmentMix ?? []).slice(0, 5);
  }

  get alerts(): OperationalAlert[] {
    if (!this.dashboard) {
      return [];
    }

    if (!this.selectedFacilityName) {
      return this.dashboard.alerts;
    }

    return this.dashboard.alerts.filter((alert) => alert.facility === this.selectedFacilityName || alert.severity === 'High');
  }

  get maxPaidAmount(): number {
    return Math.max(...this.monthlySpend.map((point) => point.paidAmount), 1);
  }

  trackByMonth(_: number, point: MonthlySpendPoint): string {
    return point.month;
  }

  trackByDepartment(_: number, point: DepartmentMixPoint): string {
    return point.department;
  }

  trackByFacility(_: number, point: FacilityPerformancePoint): string {
    return point.facility;
  }

  trackByAlert(_: number, point: OperationalAlert): string {
    return `${point.severity}-${point.facility}-${point.message}`;
  }

  selectFacility(facilityName: string): void {
    this.selectedFacilityName = facilityName;
  }

  onRegionChange(): void {
    const nextFacility = this.filteredFacilities[0]?.facility ?? '';
    if (!this.filteredFacilities.some((facility) => facility.facility === this.selectedFacilityName)) {
      this.selectedFacilityName = nextFacility;
    }
  }

  asCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      maximumFractionDigits: 0
    }).format(value);
  }

  severityClass(severity: string): string {
    switch (severity) {
      case 'High':
        return 'severity-high';
      case 'Medium':
        return 'severity-medium';
      default:
        return 'severity-low';
    }
  }
}
