import { AbstractControl, ValidationErrors, Validators } from "@angular/forms";

export function urlValidator(control: AbstractControl): ValidationErrors {
    if (!control.value) {
        return null;
    }

    const result = control.value.match("https?://.+");
    const isValid = result && result.length > 0;

    return !isValid ? { invalidUrl: { value: control.value } } : null;
}
