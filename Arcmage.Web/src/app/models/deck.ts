import { Base } from "./base";
import { DeckCard } from "./deck-card";
import { Status } from "./status";

export class Deck extends Base {
  id: number;
  name: string;
  zip: string;
  txt: string;
  exportTiles: boolean;
  generatePdf: boolean;
  isAvailable: boolean;
  isEditable: boolean;
  deckCards: DeckCard[];
  totalCards: number;
  isGenerated: boolean;
  status: Status;
}
