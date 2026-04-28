import { Component, OnInit, AfterViewInit, OnDestroy, inject, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import maplibregl from 'maplibre-gl';
import { ApiService } from '../../core/services/api.service';
import { StationWithClusterDto, LineDto, VestibuleDto } from '../../core/models';

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

const OSM_STYLE: maplibregl.StyleSpecification = {
  version: 8,
  sources: {
    osm: {
      type: 'raster',
      tiles: ['https://tile.openstreetmap.org/{z}/{x}/{y}.png'],
      tileSize: 256,
      attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap contributors</a>'
    }
  },
  layers: [{ id: 'osm-tiles', type: 'raster', source: 'osm' }]
};

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatProgressSpinnerModule, MatButtonToggleModule,
    MatIconModule, MatSlideToggleModule
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

  stations:   StationWithClusterDto[] = [];
  vestibules: VestibuleDto[] = [];
  lines:      LineDto[] = [];

  clusterStats: { label: string; color: string; count: number }[] = [];
  totalAnomalies = 0;
  totalRepairs   = 0;

  readonly CLUSTER_LABELS = CLUSTER_LABELS;
  readonly CLUSTER_COLORS = CLUSTER_COLORS;

  private map: maplibregl.Map | null = null;
  private activePopup: maplibregl.Popup | null = null;
  private mapReady  = false;
  private dataReady = false;

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
        this.loading   = false;
        this.dataReady = true;
        if (this.mapReady) this.renderMarkers();
      },
      error: () => this.loading = false
    });
  }

  ngAfterViewInit(): void { this.initMap(); }

  ngOnDestroy(): void {
    this.clearLayers();
    this.map?.remove();
  }

  private initMap(): void {
    this.map = new maplibregl.Map({
      container: this.mapElRef.nativeElement,
      style: OSM_STYLE,
      center: [37.618423, 55.751244],
      zoom: 11
    });

    this.map.on('load', () => {
      this.mapReady = true;
      if (this.dataReady) this.renderMarkers();
    });
  }

  private clearLayers(): void {
    this.activePopup?.remove();
    this.activePopup = null;
    if (!this.map) return;
    if (this.map.getLayer('markers-layer')) this.map.removeLayer('markers-layer');
    if (this.map.getSource('markers'))      this.map.removeSource('markers');
  }

  renderMarkers(): void {
    if (!this.map) return;
    this.clearLayers();
    if (this.mode === 'clusters') this.renderClusterMarkers();
    else                          this.renderVestibuleMarkers();
  }

  private renderClusterMarkers(): void {
    const features = this.stations
      .filter(s => s.latitude && s.longitude)
      .map(s => {
        const label      = s.clusterLabel ?? 'Mixed';
        const baseColor  = CLUSTER_COLORS[label] ?? '#757575';
        const hasAnomaly = this.showAnomalies && (s.activeAnomalyCount ?? 0) > 0;
        const hasRepair  = this.showRepairs   && (s.activeRepairCount  ?? 0) > 0;
        return {
          type: 'Feature' as const,
          geometry: { type: 'Point' as const, coordinates: [s.longitude!, s.latitude!] },
          properties: {
            stationId:   s.stationId,
            color:       hasAnomaly ? '#e53935' : hasRepair ? '#fb8c00' : baseColor,
            radius:      hasAnomaly ? 11 : hasRepair ? 9 : 7,
            strokeColor: hasAnomaly ? '#b71c1c' : hasRepair ? '#e65100' : '#ffffff',
            strokeWidth: hasAnomaly || hasRepair ? 2.5 : 1.5,
            sortKey:     hasAnomaly ? 3 : hasRepair ? 2 : 1
          }
        };
      });

    this.map!.addSource('markers', {
      type: 'geojson',
      data: { type: 'FeatureCollection', features }
    });

    this.map!.addLayer({
      id: 'markers-layer',
      type: 'circle',
      source: 'markers',
      layout: { 'circle-sort-key': ['get', 'sortKey'] },
      paint: {
        'circle-radius':       ['get', 'radius'],
        'circle-color':        ['get', 'color'],
        'circle-stroke-color': ['get', 'strokeColor'],
        'circle-stroke-width': ['get', 'strokeWidth']
      }
    });

    this.map!.on('mouseenter', 'markers-layer', () => {
      this.map!.getCanvas().style.cursor = 'pointer';
    });
    this.map!.on('mouseleave', 'markers-layer', () => {
      this.map!.getCanvas().style.cursor = '';
    });

    this.map!.on('click', 'markers-layer', e => {
      if (!e.features?.length) return;
      const stationId = e.features[0].properties['stationId'] as number;
      const station   = this.stations.find(s => s.stationId === stationId);
      if (!station) return;

      const label     = station.clusterLabel ?? 'Mixed';
      const baseColor = CLUSTER_COLORS[label] ?? '#757575';

      this.activePopup?.remove();
      this.activePopup = new maplibregl.Popup({ offset: 12, closeButton: false })
        .setLngLat(e.lngLat)
        .setHTML(this.buildStationPopupHtml(station, baseColor, label))
        .addTo(this.map!);

      this.activePopup.on('open', () => {
        this.activePopup!.getElement()
          .querySelector('.popup-link')
          ?.addEventListener('click', ev => {
            ev.preventDefault();
            this.activePopup?.remove();
            this.router.navigate(['/stations', station.stationId]);
          });
      });
    });
  }

  private buildStationPopupHtml(s: StationWithClusterDto, baseColor: string, label: string): string {
    const anomalyBadge = this.showAnomalies && (s.activeAnomalyCount ?? 0) > 0
      ? `<div class="popup-alert popup-alert--anomaly">⚠ Аномалий: ${s.activeAnomalyCount}</div>` : '';
    const repairBadge = this.showRepairs && (s.activeRepairCount ?? 0) > 0
      ? `<div class="popup-alert popup-alert--repair">🔧 Ремонтов: ${s.activeRepairCount}</div>` : '';
    return `
      <div class="popup-title">${s.stationName}</div>
      <div class="popup-line">${s.lines.join(' · ')}</div>
      <div class="popup-cluster" style="color:${baseColor}">● ${CLUSTER_LABELS[label] ?? label}</div>
      ${anomalyBadge}${repairBadge}
      <a class="popup-link" href="#">Подробнее →</a>`;
  }

  private renderVestibuleMarkers(): void {
    const features = this.vestibules.slice(0, 1500).map(v => ({
      type: 'Feature' as const,
      geometry: { type: 'Point' as const, coordinates: [v.longitude!, v.latitude!] },
      properties: {
        name:          v.name,
        stationName:   v.stationName   ?? '',
        vestibuleType: v.vestibuleType ?? '',
        admArea:       v.admArea       ?? ''
      }
    }));

    this.map!.addSource('markers', {
      type: 'geojson',
      data: { type: 'FeatureCollection', features }
    });

    this.map!.addLayer({
      id: 'markers-layer',
      type: 'circle',
      source: 'markers',
      paint: {
        'circle-radius':       5,
        'circle-color':        '#1565c0',
        'circle-stroke-color': '#ffffff',
        'circle-stroke-width': 1.5
      }
    });

    this.map!.on('mouseenter', 'markers-layer', () => {
      this.map!.getCanvas().style.cursor = 'pointer';
    });
    this.map!.on('mouseleave', 'markers-layer', () => {
      this.map!.getCanvas().style.cursor = '';
    });

    this.map!.on('click', 'markers-layer', e => {
      if (!e.features?.length) return;
      const p = e.features[0].properties as {
        name: string; stationName: string; vestibuleType: string; admArea: string;
      };
      this.activePopup?.remove();
      this.activePopup = new maplibregl.Popup({ offset: 12, closeButton: false })
        .setLngLat(e.lngLat)
        .setHTML(`
          <div class="popup-title">${p.name}</div>
          ${p.stationName   ? `<div class="popup-line">${p.stationName}</div>`   : ''}
          ${p.vestibuleType ? `<div class="popup-line">${p.vestibuleType}</div>` : ''}
          ${p.admArea       ? `<div class="popup-line">${p.admArea}</div>`       : ''}
        `)
        .addTo(this.map!);
    });
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
