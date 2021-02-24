import { SearchOptionsBase } from "./search-options-base";
import { CardType } from "./card-type";
import { Faction } from "./faction";
import { Serie } from "./serie";
import { RuleSet } from "./rule-set";
import { Status } from "./status";
import { Language } from "./language";

export class CardSearchOptions extends SearchOptionsBase {
   cost: string;
   cardType: CardType;
   faction: Faction;
   serie: Serie;
   ruleSet: RuleSet;
   status: Status;
   loyalty: number | null;
   language: Language;
}