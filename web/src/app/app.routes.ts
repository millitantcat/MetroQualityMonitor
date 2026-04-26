import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'map',
    loadComponent: () => import('./features/map/map.component').then(m => m.MapComponent)
  },
  {
    path: 'lines',
    loadComponent: () => import('./features/lines/lines.component').then(m => m.LinesComponent)
  },
  {
    path: 'lines/:id',
    loadComponent: () => import('./features/lines/line-detail.component').then(m => m.LineDetailComponent)
  },
  {
    path: 'stations',
    loadComponent: () => import('./features/stations/stations.component').then(m => m.StationsComponent)
  },
  {
    path: 'stations/:id',
    loadComponent: () => import('./features/station-details/station-details.component').then(m => m.StationDetailsComponent)
  },
  {
    path: 'anomalies',
    loadComponent: () => import('./features/anomalies/anomalies.component').then(m => m.AnomaliesComponent)
  },
  { path: '**', redirectTo: 'dashboard' }
];
