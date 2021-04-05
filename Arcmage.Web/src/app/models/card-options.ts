import { CardType } from "./card-type";
import { Faction } from "./faction";
import { Serie } from "./serie";
import { RuleSet } from "./rule-set";
import { Status } from "./status";
import { Language } from "./language";
import { License } from "./license";

export class CardOptions {
    isEditable: boolean;
    isStatusChangedAllowed: boolean;
    isRulingEditable: boolean;
    cardTypes: CardType[];
    factions: Faction[];
    series: Serie[];
    ruleSets: RuleSet[];
    statuses: Status[];
    loyalties: number[];
    languages: Language[];
    artworkLicenses: License[];
}
