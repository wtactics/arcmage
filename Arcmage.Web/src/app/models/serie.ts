import { Base } from "./base";
import { Status } from "./status";
import { Card } from "./card";

export class Serie extends Base {
    id: number;
    name: string;
    cards: Card[];
    status: Status;
}