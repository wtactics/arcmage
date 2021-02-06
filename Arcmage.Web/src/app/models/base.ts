import { User } from "./user";


export abstract class Base {

    guid: string;
    creator: User;
    createTime: any;
    lastModifiedBy: User;
    lastModifiedTime: any;

    constructor() {
    }

}
