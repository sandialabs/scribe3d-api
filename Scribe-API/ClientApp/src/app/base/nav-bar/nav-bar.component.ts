import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from '../../authentication/services/authentication.service';
import { User } from '../../authentication/user';
import { Router } from '@angular/router';
import { ToolsService, Tool } from '../../tools/tools.service';

@Component({
  selector: 'app-nav-bar',
  templateUrl: './nav-bar.component.html',
  styleUrls: ['./nav-bar.component.css']
})
export class NavBarComponent implements OnInit {

  user: User;
  tools: Tool[];
  constructor(private authService: AuthenticationService, private router: Router, private toolsService: ToolsService) { }

  ngOnInit(): void {
    this.authService.currentUser.subscribe((user) => this.user = user);
    this.toolsService.getTools()
      .subscribe((tools: Tool[]) => {
        this.tools = [];
        for (let i = 0; i < tools.length; i++) {
          if (tools[i].isMainTool) this.tools.push(tools[i]);
        }
      });
  }

  isLoggedIn() {
    return this.authService.isLoggedIn();
  }
  logout() {
    this.authService.logout();
    this.router.navigate(['/home']);
  }
}
