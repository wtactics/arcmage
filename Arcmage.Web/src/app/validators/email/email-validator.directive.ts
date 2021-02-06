import { Directive, Input } from "@angular/core";
import { NG_VALIDATORS, Validator, AbstractControl } from "@angular/forms";

import { emailValidator } from "./email.validator";

@Directive({
  // tslint:disable-next-line:directive-selector
  selector: "[validEmail]",
  providers: [{ provide: NG_VALIDATORS, useExisting: EmailValidatorDirective, multi: true }]
})
export class EmailValidatorDirective implements Validator {

  validate(control: AbstractControl): { [key: string]: any } {
    return emailValidator(control);
  }
}
