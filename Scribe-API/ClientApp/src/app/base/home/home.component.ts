import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  scribeImagePath = '/assets/Home/scribe_trains.PNG';
  traceImagePath = '/assets/Home/trace_Lonepine2.JPG';
  prepImagePath = '/assets/Home/prep_ttx.png';
  constructor() { }

  ngOnInit(): void {
  }

}
