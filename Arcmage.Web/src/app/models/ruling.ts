import { Base } from "./base";
import { Card } from "./card";


export class Ruling extends Base {
    id: number;
    card: Card;
    ruleText: string;
}
