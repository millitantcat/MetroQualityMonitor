import { Component, Input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-kpi-card',
  standalone: true,
  imports: [MatIconModule],
  template: `
    <div class="kpi-card card">
      <div class="kpi-icon" [style.background]="iconBg">
        <mat-icon [style.color]="iconColor">{{ icon }}</mat-icon>
      </div>
      <div class="kpi-body">
        <div class="kpi-value">{{ value }}</div>
        <div class="kpi-label">{{ label }}</div>
        @if (sub) {
          <div class="kpi-sub">{{ sub }}</div>
        }
      </div>
    </div>
  `,
  styles: [`
    .kpi-card {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 20px;
    }
    .kpi-icon {
      width: 48px; height: 48px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      mat-icon { font-size: 24px; width: 24px; height: 24px; }
    }
    .kpi-body { flex: 1; min-width: 0; }
    .kpi-value { font-size: 26px; font-weight: 700; color: #1a1a2e; line-height: 1.1; }
    .kpi-label { font-size: 13px; color: #757575; margin-top: 4px; }
    .kpi-sub { font-size: 11px; color: #9e9e9e; margin-top: 2px; }
  `]
})
export class KpiCardComponent {
  @Input() icon = 'info';
  @Input() iconBg = '#e8f0fe';
  @Input() iconColor = '#1565c0';
  @Input() value = '—';
  @Input() label = '';
  @Input() sub = '';
}
