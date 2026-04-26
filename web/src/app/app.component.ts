import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet, RouterLink, RouterLinkActive,
    MatSidenavModule, MatToolbarModule, MatListModule,
    MatIconModule, MatButtonModule, MatTooltipModule
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  private readonly bp = inject(BreakpointObserver);

  isMobile = toSignal(
    this.bp.observe([Breakpoints.XSmall, Breakpoints.Small]).pipe(
      map(r => r.matches)
    ),
    { initialValue: false }
  );

  sidenavOpen = true;

  readonly navItems = [
    { label: 'Дашборд',   icon: 'dashboard',    route: '/dashboard' },
    { label: 'Карта',     icon: 'map',           route: '/map' },
    { label: 'Линии',     icon: 'linear_scale',  route: '/lines' },
    { label: 'Станции',   icon: 'place',         route: '/stations' },
    { label: 'Аномалии',  icon: 'warning_amber', route: '/anomalies' },
  ];

  toggleSidenav(): void {
    this.sidenavOpen = !this.sidenavOpen;
  }
}
