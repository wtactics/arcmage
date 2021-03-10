using System;
using System.Collections.Generic;

namespace Arcmage.Model
{
    public static class Routes
    {
        // Remark : Do not auto-format the routes or we lose the outline
        private const string Root = "api/";

        public const string Roles = Root + "Roles";
        public const string Users = Root + "Users";
        public const string Factions = Root + "Factions";
        public const string Series = Root + "Series";
        public const string CardTypes = Root + "CardTypes";
        public const string Statuses = Root + "Statuses";
        public const string Cards = Root + "Cards";
        public const string Decks = Root + "Decks";
        public const string DeckCards = Root + "DeckCards";
        public const string Rulings = Root + "Rulings";

        public const string CardOptions = Root + "CardOptions";
        public const string DeckOptions = Root + "DeckOptions";
        public const string SettingsOptions = Root + "SettingsOptions";

        public const string CardSearchOptions = Root + "CardSearch";
        public const string DeckSearchOptions = Root + "DeckSearch";
        public const string UserSearchOptions = Root + "UserSearch";
        

        public const string Game = Root + "Games";
        public const string GameSearchOptions = Root + "GameSearch";

        public const string FileUpload = Root + "FileUpload";
        public const string Login = Root + "Login";

        public const string Platform = Root + "Platform";


        public static Dictionary<Type, string> RouteMapping { get; }
        static Routes()
        {
            RouteMapping = new Dictionary<Type, string>
            {
                {typeof(Role), Roles},
                {typeof(User), Users},
                {typeof(Faction), Factions},
                {typeof(Serie), Series},
                {typeof(CardType), CardTypes},
                {typeof(Status), Statuses},
                {typeof(Card), Cards},
                {typeof(Deck), Decks},
                {typeof(DeckCard), DeckCards},
                {typeof(Ruling), Rulings},
                {typeof(Game), Game},

                {typeof(CardOptions), CardOptions},
                {typeof(DeckOptions), DeckOptions},
                {typeof(SettingsOptions), SettingsOptions},


                {typeof(CardSearchOptions), CardSearchOptions},
                {typeof(DeckSearchOptions), DeckSearchOptions},
                {typeof(UserSearchOptions), UserSearchOptions},
                {typeof(GameSearchOptions), GameSearchOptions},

                {typeof(Login), Login},

            };
        }
    }
}
