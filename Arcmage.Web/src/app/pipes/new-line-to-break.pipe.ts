import { Pipe, PipeTransform } from "@angular/core";

@Pipe({ name: "newLineToBreak", pure: false })
export class NewLineToBreakPipe implements PipeTransform {

    transform(value: string): string {
        if (!value) {
            return "";
        }

        return value.replace(/\r\n|\r|\n/g, "<br/>");
    }
}
