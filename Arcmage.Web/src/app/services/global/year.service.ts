import { Injectable } from "@angular/core";
import { SelectItem } from "primeng";

@Injectable()
export class YearService {
  getYearRange(startYear, endYear): SelectItem[] {
    let years = [];
    let descending = startYear > endYear;
    let currentYear = startYear;
    if (descending) {
      while (currentYear >= endYear) {
        years.push({ label: currentYear, value: currentYear });
        currentYear--;
      }
    } else {
      while (currentYear <= endYear) {
        years.push({ label: currentYear, value: currentYear });
        currentYear++;
      }
    }
    return years;
  }
}
