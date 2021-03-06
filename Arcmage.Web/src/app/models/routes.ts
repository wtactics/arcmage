export class Routes  {

    public static routeMap: Map<string, string>;

    static initialize() {
        this.routeMap = new Map();
        this.routeMap.set("Role", "api/Roles");
        this.routeMap.set("User", "api/Users");
        this.routeMap.set("Faction", "api/Factions");
        this.routeMap.set("Serie", "api/Series");
        this.routeMap.set("CardType", "api/CardTypes");
        this.routeMap.set("Status", "api/Statuses");
        this.routeMap.set("Card", "api/Cards");
        this.routeMap.set("Deck", "api/Decks");
        this.routeMap.set("DeckCard", "api/DeckCards");
        this.routeMap.set("Ruling", "api/Rulings");
        this.routeMap.set("Game", "api/Games");

        this.routeMap.set("CardOptions", "api/CardOptions");
        this.routeMap.set("DeckOptions", "api/DeckOptions");
        this.routeMap.set("SettingsOptions", "api/SettingsOptions");

        this.routeMap.set("CardSearch", "api/CardSearch");
        this.routeMap.set("DeckSearch", "api/DeckSearch");
        this.routeMap.set("GameSearch", "api/GameSearch");

        this.routeMap.set("Login", "api/Login");
        this.routeMap.set("FileUpload", "api/FileUpload");
    }
}

Routes.initialize();

