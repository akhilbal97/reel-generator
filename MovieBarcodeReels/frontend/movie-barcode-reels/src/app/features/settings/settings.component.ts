import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-settings',
  imports: [MatIconModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
})
export class SettingsComponent {
  readonly plannedModules = [
    'Movie Scraper',
    'AI Trivia Generation',
    'Barcode Generator',
    'YouTube Upload',
    'Instagram Upload',
    'Batch Generation',
    'Scheduling',
    'Analytics',
  ];
}
