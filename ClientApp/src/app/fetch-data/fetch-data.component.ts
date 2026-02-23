import { Component, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Subscription } from 'rxjs';
import { MatTableDataSource } from '@angular/material/table';

interface Contribution {
  projectId: number;
  days: number;
}

interface CollaborationResult {
  employee1: number;
  employee2: number;
  totalDays: number;
  contributions: Contribution[];
}

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html',
  styleUrls: ['./fetch-data.component.css']
})
export class FetchDataComponent implements OnDestroy {
  selectedFormat: string = 'yyyy-MM-dd';

  public dateFormats = [
    { value: 'yyyy-MM-dd',          label: 'ISO (yyyy-MM-dd) - 2025-02-22' },
    { value: 'yyyy-dd-MM',          label: 'ISO - 2025-22-02' },
    { value: 'dd/MM/yyyy',          label: 'European (dd/MM/yyyy) - 22/02/2025' },
    { value: 'MM/dd/yyyy',          label: 'US (MM/dd/yyyy) - 02/22/2025' },
    { value: 'dd.MM.yyyy',          label: 'Dot European (dd.MM.yyyy) - 22.02.2025' },
    { value: 'dd-MM-yyyy',          label: 'Dash European (dd-MM-yyyy) - 22-02-2025' },
    { value: 'dd MMM yyyy',         label: 'Short month name - 22 Feb 2025' },
    { value: 'dd MMMM yyyy',        label: 'Full month name - 22 February 2025' },
    { value: 'MMM dd, yyyy',        label: 'US short - Feb 22, 2025' },
    { value: 'MMMM dd, yyyy',       label: 'US full - February 22, 2025' }
  ];

  public result: CollaborationResult | null = null;

  displayedColumns: string[] = ['employee1', 'employee2', 'projectId', 'daysWorked'];
  dataSource = new MatTableDataSource<any>([]);

  private selectedFileSubject = new BehaviorSubject<File | null>(null);
  selectedFile$ = this.selectedFileSubject.asObservable();

  private fileNameSubject = new BehaviorSubject<string>('');
  fileName$ = this.fileNameSubject.asObservable();

  private subscriptions: Subscription[] = [];

  constructor(private http: HttpClient) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {
      const file = input.files[0];

      if (!file.name.toLowerCase().endsWith('.csv')) {
        this.selectedFileSubject.next(null);
        this.fileNameSubject.next('');
        return;
      }

      this.selectedFileSubject.next(file);
      this.fileNameSubject.next(file.name);
    }
  }

  uploadFile(): void {
    const file = this.selectedFileSubject.value;
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file, file.name);
    formData.append('format', this.selectedFormat);

    const sub = this.http.post<CollaborationResult>(
      '/EmployeeCollaboration/fetchLongestCollaborationWorkers',
      formData
    ).subscribe({
      next: (data: CollaborationResult) => {
        this.result = data;

        const rows = data.contributions.map((c: Contribution) => ({
          employee1: data.employee1,
          employee2: data.employee2,
          projectId: c.projectId,
          daysWorked: c.days
        }));

        this.dataSource.data = rows;

        this.selectedFileSubject.next(null);
        this.fileNameSubject.next('');
      },
      error: (err) => {
        console.error('Upload error:', err);
      }
    });

    this.subscriptions.push(sub);
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    this.selectedFileSubject.complete();
    this.fileNameSubject.complete();
  }
}