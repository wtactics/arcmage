import { Directive, Input } from "@angular/core";
import { NG_VALIDATORS, Validator, AbstractControl } from "@angular/forms";
import { urlValidator } from "./url.validator";

@Directive({
  // tslint:disable-next-line:directive-selector
  selector: "[url]",
  providers: [{ provide: NG_VALIDATORS, useExisting: UrlValidatorDirective, multi: true }]
})
export class UrlValidatorDirective implements Validator {

  validate(control: AbstractControl): { [key: string]: any } {
    return urlValidator(control);
  }
}
