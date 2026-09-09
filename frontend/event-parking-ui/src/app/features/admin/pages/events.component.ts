import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from '../admin-api.service';

@Component({
  selector: 'app-events-admin',
  imports: [FormsModule],
  template: `
    <main class="pagex">
      <div class="head">
        <div>
          <h1>Events</h1>
          <p>Create and manage event master data.</p>
        </div>
      </div>

      <section class="card panel form-card">
        <div class="form-grid">
          <label>
            <span>Event name</span>
            <input class="input" [(ngModel)]="m.name" placeholder="Colombo Music Night 2026">
          </label>

          <label>
            <span>Venue</span>
            <select class="input" [(ngModel)]="m.venueId">
              <option [ngValue]="0">Select venue</option>
              @for (v of venues(); track v.venueId) {
                <option [ngValue]="v.venueId">{{ v.name }}</option>
              }
            </select>
          </label>

          <label>
            <span>Category</span>
            <select class="input" [(ngModel)]="m.categoryId">
              <option [ngValue]="0">Select category</option>
              @for (c of cats(); track c.categoryId) {
                <option [ngValue]="c.categoryId">{{ c.name }}</option>
              }
            </select>
          </label>

          <label>
            <span>Event date</span>
            <input class="input" [(ngModel)]="m.eventDate" type="date">
          </label>

          <label>
            <span>Start time</span>
            <input class="input" [(ngModel)]="m.startTime" type="time">
          </label>

          <label>
            <span>End time</span>
            <input class="input" [(ngModel)]="m.endTime" type="time">
          </label>

          <label>
            <span>Ticket price (Rs.)</span>
            <input class="input" [(ngModel)]="m.ticketPrice" type="number" min="0" placeholder="2500">
          </label>

          <label>
            <span>Parking fee (Rs.)</span>
            <input class="input" [(ngModel)]="m.parkingFee" type="number" min="0" placeholder="500">
          </label>

          <label>
            <span>Capacity</span>
            <input class="input" [(ngModel)]="m.capacity" type="number" min="1" placeholder="1000">
          </label>

          <div class="action-wrap">
            <span>&nbsp;</span>
            <button class="btn primary create-btn" (click)="add()" [disabled]="saving()">
              {{ saving() ? 'Creating...' : 'Create event' }}
            </button>
          </div>
        </div>

        @if (message()) {
          <div class="message" [class.error]="hasError()">{{ message() }}</div>
        }
      </section>

      <section class="card panel table-card">
        <div class="table">
          <table>
            <tr>
              <th>Event</th>
              <th>Date & time</th>
              <th>Venue</th>
              <th>Category</th>
              <th>Ticket</th>
              <th>Parking</th>
              <th>Capacity</th>
              <th></th>
            </tr>
            @for (e of rows(); track e.eventId) {
              <tr>
                <td><b>{{ e.name }}</b></td>
                <td>{{ e.eventDate }}<br><small>{{ e.startTime }} - {{ e.endTime }}</small></td>
                <td>{{ e.venueName || e.venueId }}</td>
                <td>{{ e.categoryName || e.categoryId }}</td>
                <td>Rs. {{ e.ticketPrice }}</td>
                <td>Rs. {{ e.parkingFee }}</td>
                <td>{{ e.capacity }}</td>
                <td><button class="small danger" (click)="del(e.eventId)">Delete</button></td>
              </tr>
            } @empty {
              <tr><td colspan="8" class="empty">No events yet. Create your first event above.</td></tr>
            }
          </table>
        </div>
      </section>
    </main>
  `,
  styles: [`
    .pagex{padding:28px}.head{display:flex;justify-content:space-between;gap:15px;align-items:center;margin-bottom:18px}
    .head h1{font-size:30px;margin:0}.head p{color:var(--muted);margin:4px 0}.panel{padding:18px}
    .form-card{margin-bottom:14px}.form-grid{display:grid;grid-template-columns:repeat(5,minmax(0,1fr));gap:12px}
    label,.action-wrap{display:flex;flex-direction:column;gap:6px}label span,.action-wrap span{font-size:11px;font-weight:700;color:var(--muted)}
    .create-btn{width:100%;min-height:42px}.create-btn:disabled{opacity:.65;cursor:not-allowed}
    .message{margin-top:12px;padding:10px 12px;border-radius:10px;background:#ecfdf3;color:#067647;font-size:12px}
    .message.error{background:#fff1f2;color:#be123c}.table{overflow:auto}table{width:100%;border-collapse:collapse;min-width:900px}
    th,td{text-align:left;padding:11px;border-bottom:1px solid var(--border);font-size:11px;vertical-align:top}th{color:var(--muted)}
    td small{color:var(--muted)}.small{border:0;border-radius:8px;padding:7px 9px;cursor:pointer}.danger{background:#fff1f2;color:#be123c}
    .empty{text-align:center;color:var(--muted);padding:28px}
    @media(max-width:1200px){.form-grid{grid-template-columns:repeat(3,minmax(0,1fr))}}
    @media(max-width:760px){.pagex{padding:15px}.head{align-items:flex-start;flex-direction:column}.form-grid{grid-template-columns:1fr 1fr}}
    @media(max-width:520px){.form-grid{grid-template-columns:1fr}}
  `]
})
export class EventsComponent {
  private api = inject(AdminApiService);

  readonly rows = signal<any[]>([]);
  readonly venues = signal<any[]>([]);
  readonly cats = signal<any[]>([]);
  readonly saving = signal(false);
  readonly message = signal('');
  readonly hasError = signal(false);

  m: any = this.newModel();

  constructor() {
    this.load();
    this.api.venues().subscribe(x => this.venues.set(x ?? []));
    this.api.categories().subscribe(x => this.cats.set(x ?? []));
  }

  private newModel() {
    return {
      name: '',
      venueId: 0,
      categoryId: 0,
      eventDate: '',
      startTime: '18:00',
      endTime: '21:00',
      ticketPrice: 2500,
      parkingFee: 500,
      capacity: 1000
    };
  }

  load() {
    this.api.events().subscribe({
      next: rows => this.rows.set(rows ?? []),
      error: () => {
        this.rows.set([]);
        this.hasError.set(true);
        this.message.set('Could not load events from the API.');
      }
    });
  }

  add() {
    this.message.set('');
    this.hasError.set(false);

    if (!this.m.name?.trim() || !this.m.venueId || !this.m.categoryId || !this.m.eventDate || !this.m.startTime || !this.m.endTime) {
      this.hasError.set(true);
      this.message.set('Please complete event name, venue, category, date, start time and end time.');
      return;
    }
    if (+this.m.capacity < 1) {
      this.hasError.set(true);
      this.message.set('Capacity must be at least 1.');
      return;
    }
    if (this.m.endTime <= this.m.startTime) {
      this.hasError.set(true);
      this.message.set('End time must be later than start time.');
      return;
    }

    const body = {
      name: this.m.name.trim(),
      venueId: +this.m.venueId,
      categoryId: +this.m.categoryId,
      eventDate: this.m.eventDate,
      startTime: this.apiTime(this.m.startTime),
      endTime: this.apiTime(this.m.endTime),
      ticketPrice: +this.m.ticketPrice || 0,
      parkingFee: +this.m.parkingFee || 0,
      capacity: +this.m.capacity
    };

    this.saving.set(true);
    this.api.addEvent(body).subscribe({
      next: () => {
        this.saving.set(false);
        this.message.set('Event created successfully.');
        this.m = this.newModel();
        this.load();
      },
      error: err => {
        this.saving.set(false);
        this.hasError.set(true);
        this.message.set(err?.error?.message || err?.error?.title || 'Event could not be created. Check the fields and try again.');
      }
    });
  }

  del(id: number) {
    if (confirm('Delete event?')) {
      this.api.delEvent(id).subscribe({
        next: () => this.load(),
        error: () => {
          this.hasError.set(true);
          this.message.set('Event could not be deleted.');
        }
      });
    }
  }

  private apiTime(value: string) {
    return value?.length === 5 ? `${value}:00` : value;
  }
}
