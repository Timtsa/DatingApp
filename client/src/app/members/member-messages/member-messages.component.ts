import { ChangeDetectionStrategy, Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/_models/message';
import { AccountService } from 'src/app/_services/account.service';
import { MessageService } from 'src/app/_services/message.service';


@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {

  @Input() userName:string;
  @Input() messages: Message[];
  @ViewChild('messageForm') messageForm :NgForm
  messageContent:string;

  constructor(public messageService:MessageService) { }

  ngOnInit(): void {    
  }

  sendMessage(){
    this.messageService.sendMessage(this.userName,this.messageContent)
    .then(()=>this.messageForm.reset());
  }

  

}
