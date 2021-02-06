import { Base } from "./base";
import { TemplateInfo } from "./template-info";

export class CardType extends Base {
    id: number;
    name: string;
    templateInfo: TemplateInfo;
}
