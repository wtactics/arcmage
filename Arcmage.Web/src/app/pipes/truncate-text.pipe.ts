import { Pipe, PipeTransform } from "@angular/core";

@Pipe({ name: "truncateText", pure: false })
export class TruncateTextPipe implements PipeTransform {

    transform(value: string, maxChars: number) {
        if (!value) {
            return null;
        }

        if (value.length === 0) {
            return value;
        }

        if (value.length < maxChars) {
            return value;
        }

        return `${value.substr(0, maxChars)}...`;
    }
}
