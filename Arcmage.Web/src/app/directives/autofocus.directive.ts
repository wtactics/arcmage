import { Directive, AfterViewInit, ElementRef } from "@angular/core";

@Directive({
  selector: "[appAutofocus]"
})
export class AutofocusDirective implements AfterViewInit {

  private primeNgElementsPrefix = "P-";

  constructor(private element: ElementRef) {
  }

  ngAfterViewInit(): void {
    if (this.element) {
      setTimeout(() => {

        if (this.element.nativeElement.type) {

          // native element has a known type (like 'input' or 'textarea' )
          this.element.nativeElement.focus();
        }
        else {
          if (this.element.nativeElement.nodeName.toUpperCase().startsWith(this.primeNgElementsPrefix)) {

            // we're dealing with a primeNg component; set the focus on its input element (in case it exists)
            const inputElement = this.element.nativeElement.querySelector("input");
            if (inputElement) {
              inputElement.focus();
            }
          }
        }

      }, 250);
    }
  }
}
