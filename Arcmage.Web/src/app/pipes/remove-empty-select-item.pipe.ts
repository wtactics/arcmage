import { Pipe, PipeTransform } from "@angular/core";
import { SelectItem } from "primeng/api";

@Pipe({ name: "removeEmpty" })
export class RemoveEmptySelectItemPipe implements PipeTransform {

    transform(selectItems: SelectItem[]): SelectItem[] {
        if (!selectItems) {
            return selectItems;
        }

        return selectItems.filter(item => item.value);
    }
}
