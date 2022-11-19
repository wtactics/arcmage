import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { Game } from 'src/app/models/game';
import { GameApiService } from 'src/app/services/api/game-api.service';

@Component({
  selector: 'app-create-game',
  templateUrl: './create-game.component.html',
  styleUrls: ['./create-game.component.scss']
})
export class CreateGameComponent implements OnInit {

  constructor(
    private gameApiService: GameApiService, 
    private router: Router,
    private titleService: Title,
    private translateService: TranslateService) { }

  names = [
    'Siege Of Wraz',
    'Battle Of Silence',
    'Battle Of Brumeg',
    'Battle Of Maidens',
    'Battle Of Steel',
    'Assault Of The Chanceless',
    'Siege Of Iviht',
    'Battle Of Eternal Fires',
    'Battle Of Greegh',
    'Battle Of Sromt',
    'Siege Of Veeht',
    'Attack Of Wruhoh',
    'Siege Of Rip',
    'Battle Of Fuig',
    'Assault Of Mad Kings',
    'War Of Upihr',
    'War Of Ilm',
    'Attack Of Gest',
    'Battle Of Kaweg',
    'Battle Of Blup',
    'Siege Of Clun',
    'Battle Of Wrupyb',
    'Battle Of Broken Laws',
    'Attack Of Differences',
    'War Of Straq',
    'Siege Of The Night',
    'Siege Of Brothers',
    'Attack Of New Orphans',
    'Attack Of The Righteous',
    'War Of New Allies',
    'War Of Fear',
    'Battle Of Chemistry',
    'War Of The Risen',
    'Battle Of Sugh',
    'Attack Of Widows',
    'War Of The Shores',
    'Battle Of Mieg',
    'Attack Of Lost Souls',
    'Assault Of Unsung Heroes',
    'Siege Of The False Prophet',
    'War Of Fuohr',
  ];

  gameName: string = null;

  ngOnInit(): void {
    this.gameName = this.getRandomName();
    this.titleService.setTitle('Aminduna - ' + this.translateService.instant('menu.play'));
    this.translateService.onLangChange.subscribe(() => {
      this.titleService.setTitle('Aminduna - ' + this.translateService.instant('menu.play'));
    });
  }

  saveGame() {

    const newGame = new Game();
    newGame.name = this.gameName;

    this.gameApiService.create$(newGame).subscribe(
      game => {
        this.router.navigate(["/invite", game.guid]);
      },
      error => {
      }

    );
  }



  getRandomName(): string {
    const randomIndex = Math.floor(Math.random() * this.names.length);
    return this.names[randomIndex];
  }

}
