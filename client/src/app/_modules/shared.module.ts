import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastrModule } from 'ngx-toastr';
import { positionElements } from 'ngx-bootstrap/positioning';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    BsDropdownModule.forRoot(),
    ToastrModule.forRoot(
      {positionClass:'toast-bottom-right'}
    )
  ],
  exports:[
    BsDropdownModule,
    ToastrModule
  ]
})
export class SharedModule { }
