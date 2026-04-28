import {
  Component, OnInit, AfterViewInit, OnDestroy, inject,
  ViewChild, ElementRef, ChangeDetectorRef
} from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { FormsModule } from '@angular/forms';
import { KpiCardComponent } from '../../shared/components/kpi-card/kpi-card.component';
import { ApiService } from '../../core/services/api.service';
import {
  StationDetailsDto, FlowRecordDto, ForecastDto,
  HourlyHeatmapDto, AnomalyDto
} from '../../core/models';
import { forkJoin } from 'rxjs';
import Chart from 'chart.js/auto';

@Component({
  selector: 'app-station-details',
  standalone: true,
  imports: [
    CommonModule, RouterLink, FormsModule,
    MatProgressSpinnerModule, MatButtonToggleModule,
    MatButtonModule, MatIconModule, MatTableModule, MatChipsModule,
    KpiCardComponent
  ],
  templateUrl: './station-details.component.html',
  styleUrl: './station-details.component.scss'
})
export class StationDetailsComponent implements OnInit, AfterViewInit, OnDestroy {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly cdr = inject(ChangeDetectorRef);

  @ViewChild('flowCanvas') flowCanvasRef!: ElementRef<HTMLCanvasElement>;

  loading = true;
  stationId = 0;
  station: StationDetailsDto | null = null;
  flow: FlowRecordDto[] = [];
  forecasts: ForecastDto[] = [];
  hourly: HourlyHeatmapDto | null = null;
  anomalies: AnomalyDto[] = [];

  dayType = 'Weekday';
  hourlyLoading = false;

  private flowChart?: Chart;

  readonly dayTypes = [
    { value: 'Weekday', label: 'Будни' },
    { value: 'Saturday', label: 'Суббота' },
    { value: 'Sunday', label: 'Воскресенье' }
  ];

  readonly heatHours = Array.from({ length: 19 }, (_, i) => i + 5); // 5..23

  ngOnInit(): void {
    this.stationId = +this.route.snapshot.paramMap.get('id')!;
    this.loadData();
  }

  ngAfterViewInit(): void {}

  ngOnDestroy(): void {
    this.flowChart?.destroy();
  }

  private loadData(): void {
    this.loading = true;
    forkJoin({
      station: this.api.getStation(this.stationId),
      flow: this.api.getStationFlow(this.stationId),
      forecasts: this.api.getStationForecast(this.stationId),
      hourly: this.api.getStationHourly(this.stationId, this.dayType),
      anomalies: this.api.getStationAnomalies(this.stationId)
    }).subscribe({
      next: data => {
        this.station = data.station;
        this.flow = data.flow;
        this.forecasts = data.forecasts;
        this.hourly = data.hourly;
        this.anomalies = data.anomalies;
        this.loading = false;
        this.cdr.detectChanges();
        this.buildFlowChart();
      },
      error: () => this.loading = false
    });
  }

  onDayTypeChange(): void {
    this.hourlyLoading = true;
    this.api.getStationHourly(this.stationId, this.dayType).subscribe({
      next: data => { this.hourly = data; this.hourlyLoading = false; },
      error: () => this.hourlyLoading = false
    });
  }

  getSlot(hour: number) {
    return this.hourly?.slots.find(s => s.hour === hour);
  }

  heatColor(share: number, max: number): string {
    if (!share || !max) return '#f5f7fa';
    const t = Math.min(share / max, 1);
    const r = Math.round(21 + t * (229 - 21));
    const g = Math.round(101 + t * (57 - 101));
    const b = Math.round(192 + t * (53 - 192));
    return `rgb(${r},${g},${b})`;
  }

  get maxInShare(): number {
    if (!this.hourly) return 0;
    return Math.max(...this.hourly.slots.map(s => s.incomingShare));
  }
  get maxOutShare(): number {
    if (!this.hourly) return 0;
    return Math.max(...this.hourly.slots.map(s => s.outgoingShare));
  }

  get yoyLabel(): string {
    if (this.station?.yoyGrowth == null) return '—';
    const v = this.station.yoyGrowth * 100;
    return (v >= 0 ? '+' : '') + v.toFixed(1) + '%';
  }

  get yoyColor(): string {
    if (this.station?.yoyGrowth == null) return '#757575';
    return this.station.yoyGrowth >= 0 ? '#2e7d32' : '#c62828';
  }

  categoryLabel(c: string | null): string {
    const map: Record<string, string> = {
      Central: 'Центральная', Transfer: 'Пересадочная',
      Residential: 'Спальный район', Mixed: 'Смешанная'
    };
    return c ? (map[c] ?? c) : '—';
  }

  severityClass(s: string): string {
    return 'badge badge-' + s.toLowerCase();
  }

  formatNum(n: number): string {
    if (n >= 1_000_000) return (n / 1_000_000).toFixed(1) + ' млн';
    if (n >= 1_000) return (n / 1_000).toFixed(0) + ' тыс';
    return n.toString();
  }

  private buildFlowChart(): void {
    if (!this.flowCanvasRef) return;

    const anomalyPeriods = new Set(this.anomalies.map(a => `${a.year}-${a.quarter}`));

    const actualLabels = this.flow.map(r => `${r.year} ${r.quarter}`);
    const actualIn = this.flow.map(r => r.incomingPassengers);

    const forecastLabels = this.forecasts.map(f => `${f.year} ${f.quarter}`);
    const forecastIn = this.forecasts.map(f => f.predictedIncoming);
    const ciLower = this.forecasts.map(f => f.confidenceLowerIncoming ?? f.predictedIncoming);
    const ciUpper = this.forecasts.map(f => f.confidenceUpperIncoming ?? f.predictedIncoming);

    const allLabels = [...actualLabels, ...forecastLabels];

    const actualData = [...actualIn, ...new Array(forecastLabels.length).fill(null)];
    const forecastData = [...new Array(actualLabels.length - 1).fill(null), actualIn[actualIn.length - 1], ...forecastIn];
    const lowerData = [...new Array(actualLabels.length - 1).fill(null), actualIn[actualIn.length - 1], ...ciLower];
    const upperData = [...new Array(actualLabels.length - 1).fill(null), actualIn[actualIn.length - 1], ...ciUpper];

    const anomalyPoints = this.flow.map(r => {
      return anomalyPeriods.has(`${r.year}-${r.quarter}`) ? r.incomingPassengers : null;
    });
    const anomalyData = [...anomalyPoints, ...new Array(forecastLabels.length).fill(null)];

    this.flowChart = new Chart(this.flowCanvasRef.nativeElement, {
      type: 'line',
      data: {
        labels: allLabels,
        datasets: [
          {
            label: 'Фактические данные',
            data: actualData,
            borderColor: '#1565c0',
            backgroundColor: 'rgba(21,101,192,0)',
            borderWidth: 2.5,
            pointRadius: 3,
            pointBackgroundColor: '#1565c0',
            tension: 0.3,
            spanGaps: false
          },
          {
            label: this.forecasts[0]?.modelName === 'SARIMA' ? 'Прогноз (SARIMA)' : 'Прогноз (Seasonal Naive)',
            data: forecastData,
            borderColor: '#1565c0',
            borderDash: [6, 4],
            backgroundColor: 'rgba(0,0,0,0)',
            borderWidth: 2,
            pointRadius: 3,
            pointBackgroundColor: '#1565c0',
            tension: 0.3,
            spanGaps: false
          },
          {
            label: 'ДИ верхняя',
            data: upperData,
            borderColor: 'rgba(21,101,192,0)',
            backgroundColor: 'rgba(21,101,192,0.1)',
            fill: '+1',
            pointRadius: 0,
            borderWidth: 0,
            spanGaps: false
          },
          {
            label: 'ДИ нижняя',
            data: lowerData,
            borderColor: 'rgba(21,101,192,0)',
            backgroundColor: 'rgba(21,101,192,0.1)',
            fill: false,
            pointRadius: 0,
            borderWidth: 0,
            spanGaps: false
          },
          {
            label: 'Аномалии',
            data: anomalyData,
            borderColor: 'transparent',
            backgroundColor: '#e53935',
            pointRadius: 7,
            pointStyle: 'circle',
            showLine: false,
            spanGaps: false
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            labels: {
              filter: item => !['ДИ верхняя', 'ДИ нижняя'].includes(item.text),
              boxWidth: 16,
              font: { size: 12 }
            }
          },
          tooltip: { mode: 'index', intersect: false }
        },
        scales: {
          x: { grid: { color: '#f0f0f0' }, ticks: { font: { size: 11 }, maxRotation: 45 } },
          y: {
            grid: { color: '#f0f0f0' },
            ticks: {
              font: { size: 11 },
              callback: v => this.formatNum(Number(v))
            }
          }
        }
      }
    });
  }
}
