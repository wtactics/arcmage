import { Deck } from "./deck";

export class Game {
    guid: string;
    name: string;
    canJoin: boolean;
    createTime: any;
    selectedDeck: Deck;
}