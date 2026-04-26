export interface DashboardKpiDto {
  totalPassengersLastQuarter: number;
  stationCount: number;
  activeAnomalyCount: number;
  activeRepairCount: number;
  latestQuarter: string | null;
  latestYear: number | null;
}

export interface SeasonalityPointDto {
  year: number;
  quarter: string;
  totalIncoming: number;
  totalOutgoing: number;
}

export interface TopStationDto {
  stationId: number;
  stationName: string;
  lines: string[];
  value: number;
}

export interface LineDto {
  id: number;
  name: string;
  stationCount: number;
  totalIncoming: number;
  totalOutgoing: number;
  latestQuarter: string | null;
  latestYear: number | null;
}

export interface StationLiteDto {
  id: number;
  name: string;
  lines: string[];
}

export interface StationDetailsDto {
  id: number;
  name: string;
  lines: string[];
  category: string | null;
  vestibuleCount: number;
  activeRepairCount: number;
  latestIncoming: number | null;
  latestOutgoing: number | null;
  yoyGrowth: number | null;
}

export interface FlowRecordDto {
  year: number;
  quarter: string;
  incomingPassengers: number;
  outgoingPassengers: number;
}

export interface ForecastDto {
  id: string;
  year: number;
  quarter: string;
  predictedIncoming: number;
  predictedOutgoing: number;
  confidenceLowerIncoming: number | null;
  confidenceUpperIncoming: number | null;
  confidenceLowerOutgoing: number | null;
  confidenceUpperOutgoing: number | null;
  modelName: string;
  modelVersion: string;
}

export interface HourlySlotDto {
  hour: number;
  incomingShare: number;
  outgoingShare: number;
  estimatedIncoming: number | null;
  estimatedOutgoing: number | null;
}

export interface HourlyHeatmapDto {
  stationId: number;
  dayType: string;
  stationCategory: string;
  slots: HourlySlotDto[];
}

export interface AnomalyDto {
  id: string;
  stationId: number;
  stationName: string;
  year: number;
  quarter: string;
  anomalyType: string;
  severity: string;
  score: number;
  actualValue: number;
  expectedValue: number | null;
  description: string | null;
  isAcknowledged: boolean;
  acknowledgedDateTimeUtc: string | null;
  createDateTimeUtc: string;
}

export interface VestibuleDto {
  id: number;
  name: string;
  stationId: number | null;
  stationName: string | null;
  longitude: number | null;
  latitude: number | null;
  vestibuleType: string | null;
  admArea: string | null;
  district: string | null;
}

export interface StationWithClusterDto {
  stationId: number;
  stationName: string;
  lines: string[];
  latitude: number | null;
  longitude: number | null;
  clusterLabel: string | null;
  clusterId: number | null;
  activeAnomalyCount: number;
  activeRepairCount: number;
}

export interface AnomalyCountItem {
  label: string;
  count: number;
}

export interface AnomalyStatsDto {
  bySeverity: AnomalyCountItem[];
  byType: AnomalyCountItem[];
  totalActive: number;
}

export interface LineFlowDto {
  lineId: number;
  lineName: string;
  totalIncoming: number;
  totalOutgoing: number;
  stationCount: number;
}

export interface LineDetailsDto {
  id: number;
  name: string;
  stationCount: number;
  totalIncoming: number;
  totalOutgoing: number;
  latestQuarter: string | null;
  latestYear: number | null;
}
