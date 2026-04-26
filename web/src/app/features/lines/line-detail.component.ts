import { Component, OnInit, inject, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { StationLiteDto, LineDetailsDto } from '../../core/models';

@Component({
  selector: 'app-line-detail',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    MatTableModule, MatSortModule, MatInputModule,
    MatFormFieldModule, MatIconModule, MatProgressSpinnerModule, MatButtonModule
  ],
  template: `
    <div class="page-container">
      <div class="breadcrumb">
        <a routerLink="/lines" class="bc-link">
          <mat-icon>arrow_back</mat-icon> Линии
        </a>
      </div>

      @if (loading) {
        <div class="spinner-center"><mat-spinner diameter="48"/></div>
      } @else if (line) {
        <div class="page-header">
          <div class="page-title-row">
            <div class="line-dot" [style.background]="'#1565c0'"></div>
            <div>
              <h1 class="page-title">{{ line.name }}</h1>
              <p class="page-sub">
                {{ line.stationCount }} станций
                @if (line.latestYear) {
                  · данные за {{ line.latestYear }} {{ line.latestQuarter }}
                }
              </p>
            </div>
          </div>
          <div class="kpi-row">
            <div class="kpi-chip">
              <span class="kpi-val">{{ formatMillions(line.totalIncoming) }}</span>
              <span class="kpi-label">въезд</span>
            </div>
            <div class="kpi-chip">
              <span class="kpi-val">{{ formatMillions(line.totalOutgoing) }}</span>
              <span class="kpi-label">выезд</span>
            </div>
          </div>
        </div>

        <div class="card">
          <div class="table-toolbar">
            <mat-form-field appearance="outline" class="search-field">
              <mat-label>Поиск станции</mat-label>
              <mat-icon matPrefix>search</mat-icon>
              <input matInput (keyup)="applyFilter($event)" placeholder="Введите название…"/>
            </mat-form-field>
          </div>
          <div class="table-wrap">
            <table mat-table [dataSource]="dataSource" matSort>
              <ng-container matColumnDef="name">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Станция</th>
                <td mat-cell *matCellDef="let row">{{ row.name }}</td>
              </ng-container>
              <ng-container matColumnDef="lines">
                <th mat-header-cell *matHeaderCellDef>Линии</th>
                <td mat-cell *matCellDef="let row">
                  @for (l of row.lines; track l) {
                    <span class="line-tag">{{ l }}</span>
                  }
                </td>
              </ng-container>
              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef></th>
                <td mat-cell *matCellDef="let row">
                  <a mat-icon-button [routerLink]="['/stations', row.id]">
                    <mat-icon>open_in_new</mat-icon>
                  </a>
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="cols"></tr>
              <tr mat-row *matRowDef="let row; columns: cols;" class="clickable-row"
                  [routerLink]="['/stations', row.id]"></tr>
              <tr class="mat-row" *matNoDataRow>
                <td class="mat-cell no-data-cell" [attr.colspan]="cols.length">Ничего не найдено</td>
              </tr>
            </table>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .page-container { padding: 24px; max-width: 1000px; margin: 0 auto; }
    .breadcrumb { margin-bottom: 16px; }
    .bc-link { display: inline-flex; align-items: center; gap: 4px; color: #1565c0;
      text-decoration: none; font-size: 14px; }
    .bc-link mat-icon { font-size: 18px; width: 18px; height: 18px; }
    .spinner-center { display: flex; justify-content: center; padding: 80px; }
    .page-header { margin-bottom: 24px; }
    .page-title-row { display: flex; align-items: center; gap: 14px; margin-bottom: 16px; }
    .line-dot { width: 20px; height: 20px; border-radius: 50%; flex-shrink: 0; }
    .page-title { font-size: 24px; font-weight: 700; margin: 0; color: #1a1a2e; }
    .page-sub { margin: 4px 0 0; color: #666; font-size: 14px; }
    .kpi-row { display: flex; gap: 16px; }
    .kpi-chip { background: #e8f0fe; border-radius: 10px; padding: 10px 20px; text-align: center; }
    .kpi-val { display: block; font-size: 20px; font-weight: 700; color: #1565c0; }
    .kpi-label { font-size: 12px; color: #666; }
    .card { background: #fff; border-radius: 12px; box-shadow: 0 2px 8px rgba(0,0,0,.06); overflow: hidden; }
    .table-toolbar { padding: 16px 16px 0; }
    .search-field { width: 100%; max-width: 360px; }
    .table-wrap { overflow-x: auto; }
    table { width: 100%; }
    .line-tag { display: inline-block; background: #f0f4ff; border-radius: 4px;
      padding: 2px 8px; font-size: 12px; margin: 2px; color: #333; }
    .no-data-cell { text-align: center; padding: 40px; color: #999; }
    .clickable-row { cursor: pointer; transition: background 0.15s; }
    .clickable-row:hover { background: #f5f7ff; }
    @media (max-width: 600px) { .page-container { padding: 12px; } }
  `]
})
export class LineDetailComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);

  loading = true;
  line: LineDetailsDto | null = null;
  cols = ['name', 'lines', 'actions'];
  dataSource = new MatTableDataSource<StationLiteDto>([]);

  @ViewChild(MatSort) set sort(s: MatSort) { this.dataSource.sort = s; }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    forkJoin({
      line: this.api.getLineDetails(id),
      stations: this.api.getLineStations(id)
    }).subscribe({
      next: data => {
        this.line = data.line as LineDetailsDto;
        this.dataSource.data = data.stations;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  applyFilter(event: Event): void {
    this.dataSource.filter = (event.target as HTMLInputElement).value.trim().toLowerCase();
  }

  formatMillions(v: number): string {
    if (v >= 1_000_000) return (v / 1_000_000).toFixed(1) + ' млн';
    if (v >= 1_000) return (v / 1_000).toFixed(0) + ' тыс';
    return v.toString();
  }
}
