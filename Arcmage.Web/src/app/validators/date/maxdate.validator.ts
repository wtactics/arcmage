import { AbstractControl, ValidationErrors, Validators } from "@angular/forms";

// This validation function compares a date from a form (or from a p-calendar input) to a given date
// A ValidationError is returned, in case the 'input date' is later than the compareToDate.
//
// The date to compare to can be specified using the attribute 'compareToDate'. If nothing is specified, 'today' is used as compareToDate

export function maxDateValidator(control: AbstractControl, compareToDate: Date): ValidationErrors {

    if (!control.value) {
        return null;
    }

    if (!compareToDate) {
        compareToDate = new Date();
        compareToDate.setHours(0, 0, 0, 0);
        compareToDate.setDate(compareToDate.getDate() + 1);
    } else {
        // make sure we're comparing dates with dates
        compareToDate = new Date(compareToDate);
    }

    const inputDate = new Date(control.value);

    if (inputDate > compareToDate) {
        return {error: "Inputted date is later than compareToDate"};
    }

    return null;
}

