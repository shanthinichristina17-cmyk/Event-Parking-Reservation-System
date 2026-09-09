import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from '../admin-api.service';

@Component({
  selector:'app-categories',
  imports:[FormsModule],
  template:`
  <main class="pagex categories-page">
    <div class="head">
      <div><span class="eyebrow">EVENT ORGANIZATION</span><h1>Categories</h1><p>Create clear event groups so customers can discover and filter events quickly.</p></div>
      <div class="count-box"><small>Total categories</small><b>{{rows().length}}</b></div>
    </div>

    <section class="card create-box">
      <div class="create-copy"><span>+</span><div><b>Add new category</b><small>Use a clear title such as Concert, Sports, Workshop or Conference.</small></div></div>
      <div class="create-form">
        <input class="input" [(ngModel)]="name" (keyup.enter)="add()" placeholder="Enter category title">
        <button class="btn primary" (click)="add()">+ Add category</button>
      </div>
    </section>

    <div class="cards">
      @for(c of rows();track c.categoryId){
        <article class="card category-card">
          <div class="category-top">
            <span class="cat-icon">{{icon(c.name)}}</span>
            <span class="id-chip">ID {{c.categoryId}}</span>
          </div>
          <h3>{{c.name}}</h3>
          <p>Event category used for customer browsing and event classification.</p>
          <div class="category-actions">
            <span class="active-chip">● Active</span>
            <button class="delete-btn" (click)="del(c.categoryId)">Delete</button>
          </div>
        </article>
      }
      @empty{<div class="card empty">No categories found. Add your first category above.</div>}
    </div>
  </main>
  `,
  styles:[`
    .eyebrow{font-size:7px;font-weight:950;letter-spacing:1.5px;color:#12927d}.count-box{min-width:115px;padding:11px 14px;border:1.5px solid #8fd2c4;border-radius:12px;background:#ffffffdb;display:flex;flex-direction:column}.count-box small{font-size:7px;color:#71858a}.count-box b{font-size:20px;color:#0c5956}
    .create-box{padding:16px;margin-bottom:14px;display:grid;grid-template-columns:1fr minmax(350px,.9fr);align-items:center;gap:15px}.create-copy{display:flex;gap:10px;align-items:center}.create-copy>span{width:36px;height:36px;border-radius:11px;display:grid;place-items:center;background:#e6f8f3;color:#0b8f79;font-size:18px;font-weight:900}.create-copy div{display:flex;flex-direction:column}.create-copy b{font-size:13px}.create-copy small{font-size:10px;color:#748a8f;margin-top:2px}.create-form{display:grid;grid-template-columns:1fr auto;gap:8px}
    .cards{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:13px}.category-card{padding:16px;min-height:155px;display:flex;flex-direction:column}.category-top{display:flex;justify-content:space-between;align-items:center}.cat-icon{width:35px;height:35px;border-radius:11px;display:grid;place-items:center;background:linear-gradient(135deg,#0b6b63,#22b69a);color:white;font-size:9px;font-weight:950}.id-chip{font-size:9px;color:#627a80;background:#edf7f5;border:1px solid #c4e4de;border-radius:99px;padding:5px 7px}.category-card h3{font-size:16px!important;margin:12px 0 4px!important;font-weight:900}.category-card p{font-size:10px;line-height:1.5;color:#74888d;margin:0 0 15px}.category-actions{margin-top:auto;display:flex;align-items:center;justify-content:space-between;border-top:1px solid #d8ebe6;padding-top:10px}.active-chip{font-size:9px;font-weight:850;color:#14814a}.delete-btn{border:1px solid #f2b7b7;background:#fff2f2;color:#c92c2c;border-radius:8px;padding:8px 13px;font-size:10px;font-weight:900;cursor:pointer}.delete-btn:hover{background:#d83232;color:white}
    .empty{grid-column:1/-1;padding:35px;text-align:center;color:#73878c}
    @media(max-width:950px){.cards{grid-template-columns:1fr 1fr}.create-box{grid-template-columns:1fr}}
    @media(max-width:580px){.cards,.create-form{grid-template-columns:1fr}}
  `]
})
export class CategoriesComponent{
  private api=inject(AdminApiService);
  readonly rows=signal<any[]>([]);
  name='';
  constructor(){this.load()}
  load(){this.api.categories().subscribe(x=>this.rows.set(x))}
  add(){const n=this.name.trim();if(!n)return;this.api.addCategory(n).subscribe(()=>{this.name='';this.load()})}
  del(id:number){if(confirm('Delete this category?'))this.api.delCategory(id).subscribe(()=>this.load())}
  icon(name:any){const n=String(name||'').trim();return (n.charAt(0)||'C').toUpperCase()}
}
