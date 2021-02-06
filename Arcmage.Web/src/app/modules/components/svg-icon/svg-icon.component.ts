import { Component, Input } from "@angular/core";

@Component({
  // tslint:disable-next-line:component-selector
  selector: "svg-icon",
  templateUrl: "./svg-icon.component.html",
  styleUrls: ["./svg-icon.component.css"]
})
export class SvgIconComponent {

  /**
   * Base path for SVG sprite
   */
  public basePath = "/assets";

  /**
   * Icon size
   */
  @Input() size = 16;

  /**
   * Symbol ID value from SVG sprite
   */
  @Input() name: string;

  constructor() {}
}
