import { Role } from "./role";
import { Card } from "./card";
import { Deck } from "./deck";
import { Right } from "./right";

export class User {
    id: number;
    guid: string;
    name: string;
    email: string;
    password: string;
    password2: string;
    isVerified: boolean;
    isDisabled: boolean;
    token: string;
    role: Role;
    createTime: any;
    lastLoginTime: any;
    cards: Card[];
    decks: Deck[];
    rights: Right[];
}