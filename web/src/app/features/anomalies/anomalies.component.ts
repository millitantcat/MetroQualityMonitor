import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ApiService } from '../../core/services/api.service';
import { AnomalyDto } from '../../core/models';

@Component({
  selector: 'app-anomalies',
  standalone: true,
  imports: [
    CommonModule, RouterLink, FormsModule,
    MatProgressSpinnerModule, MatTableModule,
    MatSelectModule, MatFormFieldModule,
    MatButtonModule, MatIconModule, MatCheckboxModule,
    MatSnackBarModule, MatPaginatorModule, MatTooltipModule
  ],
  templateUrl: './anomalies.component.html',
  styleUrl: './anomalies.component.scss'
})
export class AnomaliesComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly snack = inject(MatSnackBar);

  loading = true;
  all: AnomalyDto[] = [];
  filtered: AnomalyDto[] = [];
  paged: AnomalyDto[] = [];

  severityFilter = '';
  typeFilter = '';
  yearFilter: number | '' = '';
  quarterFilter = '';
  showAcknowledged = false;

  pageSize = 20;
  pageIndex = 0;

  readonly columns = ['station', 'period', 'type', 'severity', 'score', 'actual', 'expected', 'status', 'actions'];
  readonly severities = ['', 'Low', 'Medium', 'High'];
  readonly quarters = ['', 'Q1', 'Q2', 'Q3', 'Q4'];
  readonly types = ['', 'Statistical', 'IsolationForest', 'YoYDeviation'];

  get availableYears(): number[] {
    const years = [...new Set(this.all.map(a => a.year))].sort((a, b) => b - a);
    return years;
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.api.getAnomalies().subscribe({
      next: data => {
        this.all = data;
        this.applyFilters();
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  resetFilters(): void {
    this.severityFilter = '';
    this.typeFilter = '';
    this.yearFilter = '';
    this.quarterFilter = '';
    this.showAcknowledged = false;
    this.applyFilters();
  }

  get isFiltered(): boolean {
    return !!(this.severityFilter || this.typeFilter || this.yearFilter || this.quarterFilter);
  }

  applyFilters(): void {
    this.filtered = this.all.filter(a => {
      if (this.severityFilter && a.severity !== this.severityFilter) return false;
      if (this.typeFilter && a.anomalyType !== this.typeFilter) return false;
      if (this.yearFilter && a.year !== this.yearFilter) return false;
      if (this.quarterFilter && a.quarter !== this.quarterFilter) return false;
      if (!this.showAcknowledged && a.isAcknowledged) return false;
      return true;
    });
    this.pageIndex = 0;
    this.updatePage();
  }

  updatePage(): void {
    const start = this.pageIndex * this.pageSize;
    this.paged = this.filtered.slice(start, start + this.pageSize);
  }

  onPage(e: PageEvent): void {
    this.pageIndex = e.pageIndex;
    this.pageSize = e.pageSize;
    this.updatePage();
  }

  acknowledge(a: AnomalyDto): void {
    this.api.acknowledgeAnomaly(a.id).subscribe({
      next: () => {
        a.isAcknowledged = true;
        this.applyFilters();
        this.snack.open('Аномалия подтверждена', 'OK', { duration: 2500 });
      }
    });
  }

  severityClass(s: string): string {
    return 'badge badge-' + s.toLowerCase();
  }

  typeLabel(t: string): string {
    const map: Record<string, string> = {
      Statistical: 'Z-score',
      IsolationForest: 'Isolation Forest',
      YoYDeviation: 'YoY'
    };
    return map[t] ?? t;
  }

  formatNum(n: number): string {
    if (n >= 1_000_000) return (n / 1_000_000).toFixed(1) + ' млн';
    if (n >= 1_000) return (n / 1_000).toFixed(0) + ' тыс';
    return n.toString();
  }

  get totalCount(): number { return this.filtered.length; }
  get highCount(): number { return this.all.filter(a => a.severity === 'High' && !a.isAcknowledged).length; }
  get mediumCount(): number { return this.all.filter(a => a.severity === 'Medium' && !a.isAcknowledged).length; }
}
