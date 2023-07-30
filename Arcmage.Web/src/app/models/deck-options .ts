import { Language } from "./language";
import { Status } from "./status";

export class DeckOptions {
    isEditable: boolean;
    isStatusChangedAllowed: boolean;
    statuses: Status[];
    languages: Language[];
}
