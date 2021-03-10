import { Role } from "./role";
import { SearchOptionsBase } from "./search-options-base";

export class UserSearchOptions extends SearchOptionsBase {
   role: Role;
   isVerified: boolean | null;
   isDisabled: boolean | null;
}
