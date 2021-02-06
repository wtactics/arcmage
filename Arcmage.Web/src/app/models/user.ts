import { Role } from "./role";
import { Card } from "./card";
import { Deck } from "./deck";

export class User {
    id: number;
    guid: string;
    name: string;
    email: string;
    password: string;
    passowrd2: string;
    token: string;
    role: Role;
    createTime: any;
    lastLoginTime: any;
    cards: Card[];
    decks: Deck[];

}