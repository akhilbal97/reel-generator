import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./layout/shell-layout/shell-layout.component').then((m) => m.ShellLayoutComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      {
        path: 'movies',
        loadComponent: () =>
          import('./features/movie-list/movie-list.component').then((m) => m.MovieListComponent),
      },
      {
        path: 'movies/new',
        loadComponent: () =>
          import('./features/movie-editor/movie-editor.component').then((m) => m.MovieEditorComponent),
      },
      {
        path: 'movies/:id/edit',
        loadComponent: () =>
          import('./features/movie-editor/movie-editor.component').then((m) => m.MovieEditorComponent),
      },
      {
        path: 'videos',
        loadComponent: () =>
          import('./features/video-library/video-library.component').then((m) => m.VideoLibraryComponent),
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./features/settings/settings.component').then((m) => m.SettingsComponent),
      },
    ],
  },
  { path: '**', redirectTo: 'dashboard' },
];
