import { Component, OnInit, inject, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ApiService } from '../../core/services/api.service';
import { LineDto } from '../../core/models';

@Component({
  selector: 'app-lines',
  standalone: true,
  imports: [
    CommonModule, RouterLink, FormsModule,
    MatTableModule, MatSortModule, MatInputModule,
    MatFormFieldModule, MatIconModule, MatProgressSpinnerModule,
    MatChipsModule, MatButtonModule, MatTooltipModule
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div class="page-title-row">
          <mat-icon class="page-icon">linear_scale</mat-icon>
          <div>
            <h1 class="page-title">Линии метро</h1>
            <p class="page-sub">Справочник линий с показателями пассажиропотока</p>
          </div>
        </div>
      </div>

      @if (loading) {
        <div class="spinner-center"><mat-spinner diameter="48"/></div>
      } @else {
        <div class="summary-chips">
          <span class="chip-stat">
            <mat-icon>route</mat-icon> {{ lines.length }} линий
          </span>
          <span class="chip-stat">
            <mat-icon>place</mat-icon> {{ totalStations }} станций
          </span>
          <span class="chip-stat">
            <mat-icon>people</mat-icon> {{ formatMillions(totalPassengers) }} пассажиров
          </span>
        </div>

        <div class="card">
          <div class="table-toolbar">
            <mat-form-field appearance="outline" class="search-field">
              <mat-label>Поиск по линии</mat-label>
              <mat-icon matPrefix>search</mat-icon>
              <input matInput (keyup)="applyFilter($event)" placeholder="Введите название…"/>
            </mat-form-field>
          </div>

          <div class="table-wrap">
            <table mat-table [dataSource]="dataSource" matSort class="lines-table">

              <ng-container matColumnDef="name">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Линия</th>
                <td mat-cell *matCellDef="let row">
                  <div class="line-name-cell">
                    <div class="line-dot" [style.background]="lineColor(row.name)"></div>
                    <span>{{ row.name }}</span>
                  </div>
                </td>
              </ng-container>

              <ng-container matColumnDef="stationCount">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Станций</th>
                <td mat-cell *matCellDef="let row">{{ row.stationCount }}</td>
              </ng-container>

              <ng-container matColumnDef="totalIncoming">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Въезд (послед. кв.)</th>
                <td mat-cell *matCellDef="let row">{{ formatMillions(row.totalIncoming) }}</td>
              </ng-container>

              <ng-container matColumnDef="totalOutgoing">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Выезд (послед. кв.)</th>
                <td mat-cell *matCellDef="let row">{{ formatMillions(row.totalOutgoing) }}</td>
              </ng-container>

              <ng-container matColumnDef="period">
                <th mat-header-cell *matHeaderCellDef>Период</th>
                <td mat-cell *matCellDef="let row">
                  @if (row.latestYear) {
                    <span class="period-badge">{{ row.latestYear }} {{ row.latestQuarter }}</span>
                  } @else {
                    <span class="no-data">Нет данных</span>
                  }
                </td>
              </ng-container>

              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef></th>
                <td mat-cell *matCellDef="let row">
                  <a mat-icon-button [routerLink]="['/lines', row.id]" matTooltip="Станции линии">
                    <mat-icon>arrow_forward</mat-icon>
                  </a>
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns;" class="clickable-row"
                  [routerLink]="['/lines', row.id]"></tr>

              <tr class="mat-row" *matNoDataRow>
                <td class="mat-cell no-data-cell" [attr.colspan]="displayedColumns.length">
                  Ничего не найдено
                </td>
              </tr>
            </table>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .page-container { padding: 24px; max-width: 1200px; margin: 0 auto; }
    .page-header { margin-bottom: 24px; }
    .page-title-row { display: flex; align-items: center; gap: 16px; }
    .page-icon { font-size: 36px; width: 36px; height: 36px; color: #1565c0; }
    .page-title { font-size: 24px; font-weight: 700; margin: 0; color: #1a1a2e; }
    .page-sub { margin: 4px 0 0; color: #666; font-size: 14px; }
    .spinner-center { display: flex; justify-content: center; padding: 80px; }
    .summary-chips { display: flex; gap: 12px; flex-wrap: wrap; margin-bottom: 20px; }
    .chip-stat { display: flex; align-items: center; gap: 6px; background: #fff; border: 1px solid #e0e0e0;
      border-radius: 20px; padding: 6px 14px; font-size: 14px; color: #333; }
    .chip-stat mat-icon { font-size: 18px; width: 18px; height: 18px; color: #1565c0; }
    .card { background: #fff; border-radius: 12px; box-shadow: 0 2px 8px rgba(0,0,0,.06); overflow: hidden; }
    .table-toolbar { padding: 16px 16px 0; }
    .search-field { width: 100%; max-width: 360px; }
    .table-wrap { overflow-x: auto; }
    .lines-table { width: 100%; }
    .line-name-cell { display: flex; align-items: center; gap: 10px; }
    .line-dot { width: 12px; height: 12px; border-radius: 50%; flex-shrink: 0; }
    .period-badge { background: #e8f0fe; color: #1565c0; border-radius: 4px; padding: 2px 8px; font-size: 12px; }
    .no-data { color: #bbb; font-size: 13px; }
    .no-data-cell { text-align: center; padding: 40px; color: #999; }
    .clickable-row { cursor: pointer; transition: background 0.15s; }
    .clickable-row:hover { background: #f5f7ff; }
    @media (max-width: 600px) {
      .page-container { padding: 12px; }
      .page-title { font-size: 20px; }
    }
  `]
})
export class LinesComponent implements OnInit {
  private readonly api = inject(ApiService);

  loading = true;
  lines: LineDto[] = [];
  totalStations = 0;
  totalPassengers = 0;

  displayedColumns = ['name', 'stationCount', 'totalIncoming', 'totalOutgoing', 'period', 'actions'];
  dataSource = new MatTableDataSource<LineDto>([]);

  @ViewChild(MatSort) set sort(s: MatSort) {
    this.dataSource.sort = s;
  }

  ngOnInit(): void {
    this.api.getLines().subscribe({
      next: data => {
        this.lines = data;
        this.totalStations = data.reduce((s, l) => s + l.stationCount, 0);
        this.totalPassengers = data.reduce((s, l) => s + l.totalIncoming, 0);
        this.dataSource.data = data;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  applyFilter(event: Event): void {
    const v = (event.target as HTMLInputElement).value;
    this.dataSource.filter = v.trim().toLowerCase();
  }

  formatMillions(v: number): string {
    if (v >= 1_000_000) return (v / 1_000_000).toFixed(1) + ' млн';
    if (v >= 1_000) return (v / 1_000).toFixed(0) + ' тыс';
    return v.toString();
  }

  lineColor(name: string): string {
    const map: Record<string, string> = {
      'Сокольническая': '#e53935', 'Замоскворецкая': '#43a047',
      'Арбатско-Покровская': '#1565c0', 'Филёвская': '#29b6f6',
      'Кольцевая': '#8b4513', 'Калужско-Рижская': '#ff8c00',
      'Таганско-Краснопресненская': '#9c27b0', 'Калининская': '#ffeb3b',
      'Серпуховско-Тимирязевская': '#9e9e9e', 'Люблинско-Дмитровская': '#4db6ac',
      'Большая кольцевая': '#00897b', 'Бутовская': '#c8e6c9',
      'Некрасовская': '#ff69b4', 'Троицкая': '#ff4081',
    };
    for (const [k, v] of Object.entries(map)) {
      if (name.includes(k.split(' ')[0])) return v;
    }
    return '#757575';
  }
}
