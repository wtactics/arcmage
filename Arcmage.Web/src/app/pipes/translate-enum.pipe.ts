import { Pipe, PipeTransform } from "@angular/core";

import { TranslateService } from "@ngx-translate/core";

@Pipe({ name: "translateEnum", pure: false })
export class TranslateEnumPipe implements PipeTransform {

    constructor(private translateService: TranslateService) {
    }

    transform(value: string, translationPrefix: string) {
        if (!value) {
            return null;
        }

        if (value.length === 0) {
            return value;
        }

        let propertyValue = value[0].toLowerCase();

        if (value.length > 1) {
            propertyValue = propertyValue + value.slice(1);
        }

        return this.translateService.instant(`${translationPrefix}.${propertyValue}`);
    }
}
