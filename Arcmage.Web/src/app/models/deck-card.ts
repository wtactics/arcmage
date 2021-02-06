import { Base } from "./base";
import { Deck } from "./deck";
import { Card } from "./card";


export class DeckCard extends Base {
    id: number;
    quantity: number;
    card: Card;
    deck: Deck;
}
