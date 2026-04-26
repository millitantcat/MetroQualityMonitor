import { Component, OnInit, AfterViewInit, OnDestroy, inject, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { StationWithClusterDto, LineDto, VestibuleDto } from '../../core/models';
import { forkJoin } from 'rxjs';
import * as L from 'leaflet';

type MapMode = 'clusters' | 'vestibules';

const CLUSTER_COLORS: Record<string, string> = {
  Central:     '#e53935',
  Transfer:    '#fb8c00',
  Residential: '#43a047',
  Mixed:       '#757575'
};

const CLUSTER_LABELS: Record<string, string> = {
  Central:     'Центральные',
  Transfer:    'Пересадочные',
  Residential: 'Спальные',
  Mixed:       'Смешанные'
};

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatProgressSpinnerModule, MatButtonToggleModule,
    MatSelectModule, MatFormFieldModule, MatIconModule,
    MatSlideToggleModule
  ],
  templateUrl: './map.component.html',
  styleUrl: './map.component.scss'
})
export class MapComponent implements OnInit, AfterViewInit, OnDestroy {
  private readonly api    = inject(ApiService);
  private readonly router = inject(Router);

  @ViewChild('mapEl') mapElRef!: ElementRef<HTMLDivElement>;

  loading  = true;
  mode: MapMode = 'clusters';
  showAnomalies = true;
  showRepairs   = true;
  selectedLineId: number | null = null;

  stations:   StationWithClusterDto[] = [];
  vestibules: VestibuleDto[] = [];
  lines:      LineDto[] = [];

  clusterStats: { label: string; color: string; count: number }[] = [];
  totalAnomalies = 0;
  totalRepairs   = 0;

  private map?: L.Map;
  private markers: (L.CircleMarker | L.Marker)[] = [];

  readonly CLUSTER_LABELS = CLUSTER_LABELS;
  readonly CLUSTER_COLORS = CLUSTER_COLORS;

  ngOnInit(): void {
    forkJoin({
      stations:   this.api.getClusters(),
      vestibules: this.api.getVestibules(),
      lines:      this.api.getLines()
    }).subscribe({
      next: data => {
        this.stations   = data.stations;
        this.vestibules = data.vestibules.filter(v => v.latitude && v.longitude);
        this.lines      = data.lines;
        this.totalAnomalies = data.stations.reduce((s, x) => s + (x.activeAnomalyCount ?? 0), 0);
        this.totalRepairs   = data.stations.reduce((s, x) => s + (x.activeRepairCount  ?? 0), 0);
        this.buildClusterStats();
        this.loading = false;
        setTimeout(() => this.renderMarkers(), 0);
      },
      error: () => this.loading = false
    });
  }

  ngAfterViewInit(): void { this.initMap(); }
  ngOnDestroy(): void { this.map?.remove(); }

  private initMap(): void {
    this.map = L.map(this.mapElRef.nativeElement, {
      center: [55.751244, 37.618423],
      zoom: 11,
      zoomControl: true
    });
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors',
      maxZoom: 19
    }).addTo(this.map);
  }

  private clearMarkers(): void {
    this.markers.forEach(m => m.remove());
    this.markers = [];
  }

  renderMarkers(): void {
    if (!this.map) return;
    this.clearMarkers();
    if (this.mode === 'clusters') this.renderClusterMarkers();
    else                          this.renderVestibuleMarkers();
  }

  private renderClusterMarkers(): void {
    for (const s of this.stations) {
      if (!s.latitude || !s.longitude) continue;

      const label      = s.clusterLabel ?? 'Mixed';
      const baseColor  = CLUSTER_COLORS[label] ?? '#757575';
      const hasAnomaly = this.showAnomalies && (s.activeAnomalyCount ?? 0) > 0;
      const hasRepair  = this.showRepairs   && (s.activeRepairCount  ?? 0) > 0;

      const fillColor   = hasAnomaly ? '#e53935' : hasRepair ? '#fb8c00' : baseColor;
      const radius      = hasAnomaly ? 11 : hasRepair ? 9 : 7;
      const weight      = hasAnomaly || hasRepair ? 2.5 : 1.5;
      const borderColor = hasAnomaly ? '#b71c1c' : hasRepair ? '#e65100' : '#fff';

      const m = L.circleMarker([s.latitude, s.longitude], {
        radius,
        fillColor,
        color:       borderColor,
        weight,
        opacity:     1,
        fillOpacity: hasAnomaly ? 0.95 : 0.85
      });

      const anomalyBadge = hasAnomaly
        ? `<div class="popup-alert popup-alert--anomaly">⚠ Аномалий: ${s.activeAnomalyCount}</div>` : '';
      const repairBadge = hasRepair
        ? `<div class="popup-alert popup-alert--repair">🔧 Ремонтов эскалаторов: ${s.activeRepairCount}</div>` : '';
      const clusterBadge = `<div class="popup-cluster" style="color:${baseColor}">● ${CLUSTER_LABELS[label] ?? label}</div>`;

      m.bindPopup(`
        <div class="map-popup">
          <div class="popup-title">${s.stationName}</div>
          <div class="popup-line">${s.lines.join(' · ')}</div>
          ${clusterBadge}
          ${anomalyBadge}${repairBadge}
          <a href="/stations/${s.stationId}" class="popup-link">Подробнее →</a>
        </div>
      `, { maxWidth: 240 });

      m.on('click', () => this.router.navigate(['/stations', s.stationId]));
      m.addTo(this.map!);
      this.markers.push(m);
    }
  }

  private renderVestibuleMarkers(): void {
    for (const v of this.vestibules.slice(0, 3000)) {
      const m = L.circleMarker([v.latitude!, v.longitude!], {
        radius: 5,
        fillColor: '#1565c0',
        color: '#fff',
        weight: 1.5,
        opacity: 1,
        fillOpacity: 0.78
      });

      m.bindPopup(`
        <div class="map-popup">
          <div class="popup-title">${v.name}</div>
          ${v.stationName ? `<div class="popup-line">${v.stationName}</div>` : ''}
          ${v.vestibuleType ? `<div class="popup-line">${v.vestibuleType}</div>` : ''}
          ${v.admArea ? `<div class="popup-line">${v.admArea}</div>` : ''}
        </div>
      `, { maxWidth: 220 });

      m.addTo(this.map!);
      this.markers.push(m);
    }
  }

  onModeChange(): void { this.renderMarkers(); }
  onFilterChange(): void { this.renderMarkers(); }

  private buildClusterStats(): void {
    const counts: Record<string, number> = {};
    for (const s of this.stations) {
      const l = s.clusterLabel ?? 'Mixed';
      counts[l] = (counts[l] ?? 0) + 1;
    }
    this.clusterStats = Object.entries(CLUSTER_COLORS).map(([label, color]) => ({
      label: CLUSTER_LABELS[label],
      color,
      count: counts[label] ?? 0
    }));
  }
}
