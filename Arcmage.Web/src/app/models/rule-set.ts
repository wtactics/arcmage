import { Base } from "./base";
import { Status } from "./status";

export class RuleSet extends Base {
    id: number;
    name: string;
    status: Status;
}