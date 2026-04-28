import { Component, OnInit, inject, ElementRef, ViewChild, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { KpiCardComponent } from '../../shared/components/kpi-card/kpi-card.component';
import { ApiService } from '../../core/services/api.service';
import {
  DashboardKpiDto, SeasonalityPointDto, TopStationDto,
  AnomalyDto, AnomalyStatsDto, LineFlowDto
} from '../../core/models';
import { forkJoin } from 'rxjs';
import Chart from 'chart.js/auto';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    MatProgressSpinnerModule, MatButtonModule, MatIconModule,
    MatTableModule, MatChipsModule, MatDividerModule,
    KpiCardComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly api = inject(ApiService);
  private readonly cdr = inject(ChangeDetectorRef);

  @ViewChild('flowChart')    flowChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('topChart')     topChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('linesChart')   linesChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('severityChart') severityChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('typeChart')    typeChartRef!: ElementRef<HTMLCanvasElement>;

  loading = true;
  kpi: DashboardKpiDto | null = null;
  seasonality: SeasonalityPointDto[] = [];
  topStations: TopStationDto[] = [];
  recentAnomalies: AnomalyDto[] = [];
  anomalyStats: AnomalyStatsDto | null = null;
  linesFlow: LineFlowDto[] = [];

  private charts: Chart[] = [];

  ngOnInit(): void {
    forkJoin({
      kpi:          this.api.getDashboardKpi(),
      seasonality:  this.api.getSeasonality(),
      top:          this.api.getTopStations(10, 'incoming'),
      anomalies:    this.api.getAnomalies(undefined, false),
      anomalyStats: this.api.getAnomalyStats(),
      linesFlow:    this.api.getLinesFlow()
    }).subscribe({
      next: data => {
        this.kpi            = data.kpi;
        this.seasonality    = data.seasonality;
        this.topStations    = data.top;
        this.recentAnomalies = data.anomalies.slice(0, 6);
        this.anomalyStats   = data.anomalyStats;
        this.linesFlow      = data.linesFlow;
        this.loading        = false;
        this.cdr.detectChanges();
        this.buildCharts();
      },
      error: () => this.loading = false
    });
  }

  ngOnDestroy(): void {
    this.charts.forEach(c => c.destroy());
  }

  get totalPassengersFormatted(): string {
    return this.kpi ? this.formatMillions(this.kpi.totalPassengersLastQuarter) : '—';
  }

  formatMillions(n: number): string {
    if (n >= 1_000_000) return (n / 1_000_000).toFixed(1) + ' млн';
    if (n >= 1_000)     return (n / 1_000).toFixed(0) + ' тыс';
    return n.toString();
  }

  severityClass(s: string): string { return 'badge badge-' + s.toLowerCase(); }
  typeLabel(t: string): string {
    return t === 'Statistical' ? 'Z-score' : t === 'IsolationForest' ? 'Isolation Forest' : t;
  }

  private buildCharts(): void {
    this.buildFlowChart();
    this.buildTopChart();
    this.buildLinesChart();
    this.buildSeverityChart();
    this.buildTypeChart();
  }

  private reg(c: Chart): void { this.charts.push(c); }

  private buildFlowChart(): void {
    if (!this.flowChartRef?.nativeElement || !this.seasonality.length) return;
    const labels   = this.seasonality.map(p => `${p.year} ${p.quarter}`);
    const incoming = this.seasonality.map(p => +(p.totalIncoming / 1_000_000).toFixed(2));
    const outgoing = this.seasonality.map(p => +(p.totalOutgoing / 1_000_000).toFixed(2));

    this.reg(new Chart(this.flowChartRef.nativeElement, {
      type: 'line',
      data: {
        labels,
        datasets: [
          {
            label: 'Входящие (млн)',
            data: incoming,
            borderColor: '#1565c0',
            backgroundColor: 'rgba(21,101,192,.10)',
            fill: true, tension: 0.4,
            pointRadius: 3, pointBackgroundColor: '#1565c0', borderWidth: 2
          },
          {
            label: 'Исходящие (млн)',
            data: outgoing,
            borderColor: '#43a047',
            backgroundColor: 'rgba(67,160,71,.07)',
            fill: true, tension: 0.4,
            pointRadius: 3, pointBackgroundColor: '#43a047', borderWidth: 2
          }
        ]
      },
      options: {
        responsive: true, maintainAspectRatio: false,
        plugins: {
          legend: { position: 'top', labels: { boxWidth: 12, font: { size: 12 } } },
          tooltip: { mode: 'index', intersect: false }
        },
        scales: {
          x: { grid: { color: '#f5f5f5' }, ticks: { font: { size: 11 }, maxRotation: 45 } },
          y: { grid: { color: '#f5f5f5' }, ticks: { font: { size: 11 } }, beginAtZero: false }
        }
      }
    }));
  }

  private buildTopChart(): void {
    if (!this.topChartRef?.nativeElement || !this.topStations.length) return;
    const labels = this.topStations.map(s => s.stationName);
    const values = this.topStations.map(s => Math.round(s.value / 1_000));

    this.reg(new Chart(this.topChartRef.nativeElement, {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label: 'Пассажиры (тыс)',
          data: values,
          backgroundColor: labels.map((_, i) => `hsla(${220 - i * 10}, 72%, ${52 + i * 2}%, 0.88)`),
          borderRadius: 6, borderSkipped: false
        }]
      },
      options: {
        indexAxis: 'y', responsive: true, maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: { callbacks: { label: ctx => ` ${Number(ctx.parsed.x ?? 0).toLocaleString('ru')} тыс` } }
        },
        scales: {
          x: { grid: { color: '#f5f5f5' }, ticks: { font: { size: 11 } } },
          y: { grid: { display: false }, ticks: { font: { size: 12 } } }
        }
      }
    }));
  }

  private buildLinesChart(): void {
    if (!this.linesChartRef?.nativeElement || !this.linesFlow.length) return;
    const top = this.linesFlow.slice(0, 10);
    const labels = top.map(l => l.lineName.length > 20 ? l.lineName.slice(0, 18) + '…' : l.lineName);

    this.reg(new Chart(this.linesChartRef.nativeElement, {
      type: 'bar',
      data: {
        labels,
        datasets: [
          {
            label: 'Входящие (млн)',
            data: top.map(l => +(l.totalIncoming / 1_000_000).toFixed(2)),
            backgroundColor: 'rgba(21,101,192,0.80)',
            borderRadius: 5
          },
          {
            label: 'Исходящие (млн)',
            data: top.map(l => +(l.totalOutgoing / 1_000_000).toFixed(2)),
            backgroundColor: 'rgba(67,160,71,0.75)',
            borderRadius: 5
          }
        ]
      },
      options: {
        responsive: true, maintainAspectRatio: false,
        plugins: {
          legend: { position: 'top', labels: { boxWidth: 12, font: { size: 12 } } },
          tooltip: { mode: 'index', intersect: false }
        },
        scales: {
          x: { grid: { display: false }, ticks: { font: { size: 11 }, maxRotation: 30 } },
          y: { grid: { color: '#f5f5f5' }, ticks: { font: { size: 11 } } }
        }
      }
    }));
  }

  private buildSeverityChart(): void {
    if (!this.severityChartRef?.nativeElement || !this.anomalyStats?.bySeverity.length) return;
    const data = this.anomalyStats.bySeverity;
    const COLORS: Record<string, string> = {
      High: '#e53935', Medium: '#fb8c00', Low: '#43a047'
    };

    this.reg(new Chart(this.severityChartRef.nativeElement, {
      type: 'doughnut',
      data: {
        labels: data.map(d => d.label),
        datasets: [{
          data: data.map(d => d.count),
          backgroundColor: data.map(d => COLORS[d.label] ?? '#9e9e9e'),
          borderWidth: 2, borderColor: '#fff'
        }]
      },
      options: {
        responsive: true, maintainAspectRatio: false,
        plugins: {
          legend: { position: 'bottom', labels: { boxWidth: 12, font: { size: 12 } } },
          tooltip: { callbacks: { label: ctx => ` ${ctx.label}: ${ctx.parsed}` } }
        },
        cutout: '62%'
      }
    }));
  }

  private buildTypeChart(): void {
    if (!this.typeChartRef?.nativeElement || !this.anomalyStats?.byType.length) return;
    const data = this.anomalyStats.byType;
    const COLORS = ['#5c6bc0', '#26a69a', '#ab47bc'];

    this.reg(new Chart(this.typeChartRef.nativeElement, {
      type: 'doughnut',
      data: {
        labels: data.map(d => this.typeLabel(d.label)),
        datasets: [{
          data: data.map(d => d.count),
          backgroundColor: COLORS.slice(0, data.length),
          borderWidth: 2, borderColor: '#fff'
        }]
      },
      options: {
        responsive: true, maintainAspectRatio: false,
        plugins: {
          legend: { position: 'bottom', labels: { boxWidth: 12, font: { size: 12 } } }
        },
        cutout: '62%'
      }
    }));
  }
}
