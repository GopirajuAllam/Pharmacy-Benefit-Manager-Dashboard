import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PbmDashboardResponse } from '../models/pbm-dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardApiService {
  private readonly endpoint = '/api/dashboard/pbm';

  constructor(private readonly http: HttpClient) {}

  getDashboard(): Observable<PbmDashboardResponse> {
    return this.http.get<PbmDashboardResponse>(this.endpoint);
  }
}
