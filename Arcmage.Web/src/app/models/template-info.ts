import { Base } from "./base";

export class TemplateInfo extends Base {
    id: number;
    showName: boolean;
    showType: boolean;
    showGoldCost: boolean;
    showLoyalty: boolean;
    showText: boolean;
    showAttack: boolean;
    showDefense: boolean;
    showDiscipline: boolean;
    showArt: boolean;
    showInfo: boolean;
    maxTextBoxWidth: number;
    maxTextBoxHeight: number;
}
