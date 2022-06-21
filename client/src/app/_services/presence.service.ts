import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl=environment.hubUrl;
  private hubConnection:HubConnection;
  private onlineUsersSorce = new BehaviorSubject<string[]>([]);
  onlineUsers$ =this.onlineUsersSorce.asObservable();

  constructor(private toastr:ToastrService, private router: Router) { }

  createHubConnection(user:User){
    this.hubConnection=new HubConnectionBuilder()
    .withUrl(this.hubUrl+'presence',{
      accessTokenFactory:()=>user.token
    })
    .withAutomaticReconnect()
    .build();

    this.hubConnection
    .start()
    .catch(error=>console.log(error));

    this.hubConnection.on('UserIsOnline',username=>{
     this.onlineUsers$.pipe(take(1)).subscribe(usernames=>
      this.onlineUsersSorce.next([...usernames, username]))
    })

    this.hubConnection.on('UserIsOffline',username=>{
      this.onlineUsers$.pipe(take(1)).subscribe(usernames=>
        this.onlineUsersSorce.next([...usernames.filter(x=>x !==username)]))
      })
    
    this.hubConnection.on("GetOnlineUsers", (userNemes : string[])=>{
      this.onlineUsersSorce.next(userNemes);
    })

    this.hubConnection.on("NewMessageRecived", ({userName, knownAs})=>{
      this.toastr.info(knownAs + "has sent you message!")
      .onTap
      .pipe(take(1))
      .subscribe(()=>{this.router.navigateByUrl('/members/'+userName + '?tab=3')})
    })
  }



  stopHubConnection(){
    this.hubConnection.stop().catch(error=>console.log(error));
  }
}
