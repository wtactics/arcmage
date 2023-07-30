
import { SearchOptionsBase } from "./search-options-base";
import { Status } from "./status";
import { Language } from "./language";

export class DeckSearchOptions extends SearchOptionsBase {
   exportTiles: boolean | null;
   status: Status;
   myDecks: boolean | null;
   excludeDrafts: boolean | null;
   language: Language;
}
