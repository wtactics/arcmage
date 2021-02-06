import { SearchOptionsBase } from "./search-options-base";

export class ResultList<T> {
    searchOptionsBase: SearchOptionsBase;
    items: T[];
    totalItems: number;
}