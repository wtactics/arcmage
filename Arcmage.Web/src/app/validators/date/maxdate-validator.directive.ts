import { Directive, Input } from "@angular/core";
import { NG_VALIDATORS, Validator, AbstractControl } from "@angular/forms";
import { maxDateValidator } from "./maxdate.validator";


@Directive({
  // tslint:disable-next-line:directive-selector
  selector: "[validateMaxDate]",
  providers: [{ provide: NG_VALIDATORS, useExisting: MaxDateValidatorDirective, multi: true }]
})
export class MaxDateValidatorDirective implements Validator {

  // tslint:disable-next-line:no-input-rename
  @Input("compareToDate") compareToDate: Date;
  validate(control: AbstractControl): { [key: string]: any } {
    return maxDateValidator(control, this.compareToDate);
  }
}
