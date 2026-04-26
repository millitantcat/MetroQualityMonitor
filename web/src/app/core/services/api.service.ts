import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  DashboardKpiDto, SeasonalityPointDto, TopStationDto,
  LineDto, LineDetailsDto, LineFlowDto, AnomalyStatsDto,
  StationLiteDto, StationDetailsDto,
  FlowRecordDto, ForecastDto, HourlyHeatmapDto,
  AnomalyDto, VestibuleDto, StationWithClusterDto
} from '../models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api';

  // Dashboard
  getDashboardKpi(): Observable<DashboardKpiDto> {
    return this.http.get<DashboardKpiDto>(`${this.base}/dashboard/kpi`);
  }
  getTopStations(n = 10, metric = 'incoming'): Observable<TopStationDto[]> {
    return this.http.get<TopStationDto[]>(`${this.base}/dashboard/top-stations`, {
      params: new HttpParams().set('n', n).set('metric', metric)
    });
  }
  getSeasonality(): Observable<SeasonalityPointDto[]> {
    return this.http.get<SeasonalityPointDto[]>(`${this.base}/dashboard/seasonality`);
  }
  getAnomalyStats(): Observable<AnomalyStatsDto> {
    return this.http.get<AnomalyStatsDto>(`${this.base}/dashboard/anomaly-stats`);
  }
  getLinesFlow(): Observable<LineFlowDto[]> {
    return this.http.get<LineFlowDto[]>(`${this.base}/dashboard/lines-flow`);
  }

  // Lines
  getLines(): Observable<LineDto[]> {
    return this.http.get<LineDto[]>(`${this.base}/lines`);
  }
  getLineDetails(id: number): Observable<LineDetailsDto> {
    return this.http.get<LineDetailsDto>(`${this.base}/lines/${id}`);
  }
  getLineStations(id: number): Observable<StationLiteDto[]> {
    return this.http.get<StationLiteDto[]>(`${this.base}/lines/${id}/stations`);
  }

  // Stations
  getStations(): Observable<StationLiteDto[]> {
    return this.http.get<StationLiteDto[]>(`${this.base}/stations`);
  }
  getStation(id: number): Observable<StationDetailsDto> {
    return this.http.get<StationDetailsDto>(`${this.base}/stations/${id}`);
  }
  getStationFlow(id: number, fromYear?: number, toYear?: number): Observable<FlowRecordDto[]> {
    let params = new HttpParams();
    if (fromYear) params = params.set('fromYear', fromYear);
    if (toYear) params = params.set('toYear', toYear);
    return this.http.get<FlowRecordDto[]>(`${this.base}/stations/${id}/flow`, { params });
  }
  getStationForecast(id: number): Observable<ForecastDto[]> {
    return this.http.get<ForecastDto[]>(`${this.base}/stations/${id}/forecast`);
  }
  getStationHourly(id: number, dayType = 'Weekday'): Observable<HourlyHeatmapDto> {
    return this.http.get<HourlyHeatmapDto>(`${this.base}/stations/${id}/hourly`, {
      params: new HttpParams().set('dayType', dayType)
    });
  }
  getStationAnomalies(id: number): Observable<AnomalyDto[]> {
    return this.http.get<AnomalyDto[]>(`${this.base}/stations/${id}/anomalies`);
  }

  // Anomalies
  getAnomalies(severity?: string, isAcknowledged?: boolean): Observable<AnomalyDto[]> {
    let params = new HttpParams();
    if (severity) params = params.set('severity', severity);
    if (isAcknowledged !== undefined) params = params.set('isAcknowledged', isAcknowledged);
    return this.http.get<AnomalyDto[]>(`${this.base}/anomalies`, { params });
  }
  acknowledgeAnomaly(id: string): Observable<void> {
    return this.http.patch<void>(`${this.base}/anomalies/${id}/acknowledge`, null);
  }

  // Vestibules
  getVestibules(stationId?: number): Observable<VestibuleDto[]> {
    let params = new HttpParams();
    if (stationId) params = params.set('stationId', stationId);
    return this.http.get<VestibuleDto[]>(`${this.base}/vestibules`, { params });
  }

  // Clusters
  getClusters(): Observable<StationWithClusterDto[]> {
    return this.http.get<StationWithClusterDto[]>(`${this.base}/clusters`);
  }
}
