import { Component, OnInit, inject, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ApiService } from '../../core/services/api.service';
import { StationLiteDto } from '../../core/models';

@Component({
  selector: 'app-stations',
  standalone: true,
  imports: [
    CommonModule, RouterLink, FormsModule,
    MatTableModule, MatSortModule, MatPaginatorModule,
    MatInputModule, MatFormFieldModule, MatSelectModule,
    MatIconModule, MatProgressSpinnerModule, MatButtonModule, MatTooltipModule
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div class="page-title-row">
          <mat-icon class="page-icon">place</mat-icon>
          <div>
            <h1 class="page-title">Станции метро</h1>
            <p class="page-sub">Справочник станций с фильтрацией по линиям</p>
          </div>
        </div>
      </div>

      @if (loading) {
        <div class="spinner-center"><mat-spinner diameter="48"/></div>
      } @else {
        <div class="summary-chips">
          <span class="chip-stat"><mat-icon>place</mat-icon> {{ stations.length }} станций</span>
          <span class="chip-stat"><mat-icon>route</mat-icon> {{ allLines.length }} линий</span>
        </div>

        <div class="card">
          <div class="table-toolbar">
            <mat-form-field appearance="outline" class="search-field">
              <mat-label>Поиск станции</mat-label>
              <mat-icon matPrefix>search</mat-icon>
              <input matInput #searchInput (keyup)="applyFilter()" placeholder="Введите название…"/>
            </mat-form-field>
            <mat-form-field appearance="outline" class="filter-field">
              <mat-label>Линия</mat-label>
              <mat-select [(ngModel)]="selectedLine" (selectionChange)="applyFilter()">
                <mat-option value="">Все линии</mat-option>
                @for (line of allLines; track line) {
                  <mat-option [value]="line">{{ line }}</mat-option>
                }
              </mat-select>
            </mat-form-field>
          </div>

          <div class="table-wrap">
            <table mat-table [dataSource]="dataSource" matSort>

              <ng-container matColumnDef="name">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Станция</th>
                <td mat-cell *matCellDef="let row">
                  <span class="station-name">{{ row.name }}</span>
                </td>
              </ng-container>

              <ng-container matColumnDef="lines">
                <th mat-header-cell *matHeaderCellDef>Линии</th>
                <td mat-cell *matCellDef="let row">
                  <div class="line-tags">
                    @for (l of row.lines; track l) {
                      <span class="line-tag">{{ l }}</span>
                    }
                  </div>
                </td>
              </ng-container>

              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef></th>
                <td mat-cell *matCellDef="let row">
                  <a mat-icon-button [routerLink]="['/stations', row.id]" matTooltip="Открыть детали">
                    <mat-icon>open_in_new</mat-icon>
                  </a>
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns;" class="clickable-row"
                  [routerLink]="['/stations', row.id]"></tr>
              <tr class="mat-row" *matNoDataRow>
                <td class="mat-cell no-data-cell" [attr.colspan]="displayedColumns.length">
                  Станции не найдены
                </td>
              </tr>
            </table>
          </div>

          <mat-paginator [pageSizeOptions]="[20, 50, 100]" pageSize="20" showFirstLastButtons/>
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
    .chip-stat { display: flex; align-items: center; gap: 6px; background: #fff;
      border: 1px solid #e0e0e0; border-radius: 20px; padding: 6px 14px; font-size: 14px; color: #333; }
    .chip-stat mat-icon { font-size: 18px; width: 18px; height: 18px; color: #1565c0; }
    .card { background: #fff; border-radius: 12px; box-shadow: 0 2px 8px rgba(0,0,0,.06); overflow: hidden; }
    .table-toolbar { padding: 16px; display: flex; gap: 16px; flex-wrap: wrap; }
    .search-field { flex: 1; min-width: 200px; max-width: 360px; }
    .filter-field { width: 220px; }
    .table-wrap { overflow-x: auto; }
    table { width: 100%; }
    .station-name { font-weight: 500; }
    .line-tags { display: flex; flex-wrap: wrap; gap: 4px; }
    .line-tag { background: #f0f4ff; border-radius: 4px; padding: 2px 8px;
      font-size: 12px; color: #333; white-space: nowrap; }
    .no-data-cell { text-align: center; padding: 40px; color: #999; }
    .clickable-row { cursor: pointer; transition: background 0.15s; }
    .clickable-row:hover { background: #f5f7ff; }
    @media (max-width: 600px) {
      .page-container { padding: 12px; }
      .page-title { font-size: 20px; }
      .filter-field { width: 100%; }
    }
  `]
})
export class StationsComponent implements OnInit {
  private readonly api = inject(ApiService);

  loading = true;
  stations: StationLiteDto[] = [];
  allLines: string[] = [];
  selectedLine = '';

  displayedColumns = ['name', 'lines', 'actions'];
  dataSource = new MatTableDataSource<StationLiteDto>([]);

  @ViewChild(MatSort) set sort(s: MatSort) { this.dataSource.sort = s; }
  @ViewChild(MatPaginator) set paginator(p: MatPaginator) { this.dataSource.paginator = p; }

  ngOnInit(): void {
    this.api.getStations().subscribe({
      next: data => {
        this.stations = data;
        const lineSet = new Set<string>();
        data.forEach(s => s.lines.forEach(l => lineSet.add(l)));
        this.allLines = Array.from(lineSet).sort();
        this.dataSource.data = data;
        this.dataSource.filterPredicate = (row, filter) => {
          const f = JSON.parse(filter) as { text: string; line: string };
          const nameMatch = row.name.toLowerCase().includes(f.text);
          const lineMatch = !f.line || row.lines.includes(f.line);
          return nameMatch && lineMatch;
        };
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  applyFilter(): void {
    const searchEl = document.querySelector('input[placeholder="Введите название…"]') as HTMLInputElement;
    const text = searchEl?.value?.trim().toLowerCase() ?? '';
    this.dataSource.filter = JSON.stringify({ text, line: this.selectedLine });
    this.dataSource.paginator?.firstPage();
  }
}
