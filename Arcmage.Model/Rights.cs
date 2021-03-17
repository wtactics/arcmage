using System;
using System.Collections.Generic;
using System.Text;

namespace Arcmage.Model
{
    public static class Rights
    {
     
        public static Guid Right = GuidUtility.Create(nameof(Right));

        public static Right GameRuntimeAdmin = new Right
        {
            Name = nameof(GameRuntimeAdmin),
            Guid = GuidUtility.Create(Right, nameof(GameRuntimeAdmin)),
        };
        

        public static Right ViewCard = new Right
        {
            Name = nameof(ViewCard),
            Guid = GuidUtility.Create(Right, nameof(ViewCard)),
        };

        public static Right CreateCard = new Right
        {
            Name = nameof(CreateCard),
            Guid = GuidUtility.Create(Right, nameof(CreateCard)),
        };

        public static Right EditCard = new Right
        {
            Name = nameof(EditCard),
            Guid = GuidUtility.Create(Right, nameof(EditCard)),
        };

        public static Right AllowOthersCardEdit = new Right
        {
            Name = nameof(AllowOthersCardEdit),
            Guid = GuidUtility.Create(Right, nameof(AllowOthersCardEdit)),
        };

        public static Right EditCardRuling = new Right
        {
            Name = nameof(EditCardRuling),
            Guid = GuidUtility.Create(Right, nameof(EditCardRuling)),
        };

        public static Right AllowCardStatusChange = new Right
        {
            Name = nameof(AllowCardStatusChange),
            Guid = GuidUtility.Create(Right, nameof(AllowCardStatusChange)),
        };

        public static Right DeleteCard = new Right
        {
            Name = nameof(DeleteCard),
            Guid = GuidUtility.Create(Right, nameof(DeleteCard)),
        };

        public static Right ViewDeck = new Right
        {
            Name = nameof(ViewDeck),
            Guid = GuidUtility.Create(Right, nameof(ViewDeck)),
        };

        public static Right CreateDeck = new Right
        {
            Name = nameof(CreateDeck),
            Guid = GuidUtility.Create(Right, nameof(CreateDeck)),
        };

        public static Right EditDeck = new Right
        {
            Name = nameof(EditDeck),
            Guid = GuidUtility.Create(Right, nameof(EditDeck)),
        };

        public static Right AllowDeckStatusChange = new Right
        {
            Name = nameof(AllowDeckStatusChange),
            Guid = GuidUtility.Create(Right, nameof(AllowDeckStatusChange)),
        };

        public static Right AllowOthersDeckEdit = new Right
        {
            Name = nameof(AllowOthersDeckEdit),
            Guid = GuidUtility.Create(Right, nameof(AllowOthersDeckEdit)),
        };

        public static Right DeleteDeck = new Right
        {
            Name = nameof(DeleteDeck),
            Guid = GuidUtility.Create(Right, nameof(DeleteDeck)),
        };

        public static Right ViewPlayer = new Right
        {
            Name = nameof(ViewPlayer),
            Guid = GuidUtility.Create(Right, nameof(ViewPlayer)),
        };

        public static Right CreatePlayer = new Right
        {
            Name = nameof(CreatePlayer),
            Guid = GuidUtility.Create(Right, nameof(CreatePlayer)),
        };

        public static Right EditPlayer = new Right
        {
            Name = nameof(EditPlayer),
            Guid = GuidUtility.Create(Right, nameof(EditPlayer)),
        };

        public static Right AllowPlayerStateChange = new Right
        {
            Name = nameof(AllowPlayerStateChange),
            Guid = GuidUtility.Create(Right, nameof(AllowPlayerStateChange)),
        };

        public static Right DeletePlayer = new Right
        {
            Name = nameof(DeletePlayer),
            Guid = GuidUtility.Create(Right, nameof(DeletePlayer)),
        };

        public static Right ViewGame = new Right
        {
            Name = nameof(ViewGame),
            Guid = GuidUtility.Create(Right, nameof(ViewGame)),
        };

        public static Right CreateGame = new Right
        {
            Name = nameof(CreateGame),
            Guid = GuidUtility.Create(Right, nameof(CreateGame)),
        };

        public static Right EditGame = new Right
        {
            Name = nameof(EditGame),
            Guid = GuidUtility.Create(Right, nameof(EditGame)),
        };

        public static Right DeleteGame = new Right
        {
            Name = nameof(DeleteGame),
            Guid = GuidUtility.Create(Right, nameof(DeleteGame)),
        };

        public static Right CreateCardType = new Right
        {
            Name = nameof(CreateCardType),
            Guid = GuidUtility.Create(Right, nameof(CreateCardType)),
        };

        public static Right EditCardType = new Right
        {
            Name = nameof(EditCardType),
            Guid = GuidUtility.Create(Right, nameof(EditCardType)),
        };

        public static Right CreateFaction = new Right
        {
            Name = nameof(CreateFaction),
            Guid = GuidUtility.Create(Right, nameof(CreateFaction)),
        };

        public static Right EditFaction = new Right
        {
            Name = nameof(EditFaction),
            Guid = GuidUtility.Create(Right, nameof(EditFaction)),
        };

        public static Right CreateSerie = new Right
        {
            Name = nameof(CreateSerie),
            Guid = GuidUtility.Create(Right, nameof(CreateSerie)),
        };

        public static Right EditSerie = new Right
        {
            Name = nameof(EditSerie),
            Guid = GuidUtility.Create(Right, nameof(EditSerie)),
        };

        public static Right CreateStatus = new Right
        {
            Name = nameof(CreateStatus),
            Guid = GuidUtility.Create(Right, nameof(CreateStatus)),
        };

        public static Right EditStatus = new Right
        {
            Name = nameof(EditStatus),
            Guid = GuidUtility.Create(Right, nameof(EditStatus)),
        };


        public static List<Right> DefaultRights = new List<Right>
        {
            Rights.ViewCard,
            Rights.ViewDeck,
            Rights.ViewGame,
        };


        public static List<Right> ServiceUserRights = new List<Right>
        {
            Rights.GameRuntimeAdmin,
            Rights.ViewPlayer,
            Rights.ViewCard,
            Rights.ViewDeck,
            Rights.ViewGame,
            Rights.EditPlayer,
            Rights.EditCard,
            Rights.EditCardRuling,
            Rights.AllowCardStatusChange,
            Rights.CreateCard,
            Rights.AllowOthersCardEdit,
            Rights.AllowOthersDeckEdit,
            Rights.CreateCardType,
            Rights.EditCardType,
            Rights.AllowDeckStatusChange,
            Rights.EditDeck,
            Rights.CreateDeck,
            Rights.CreateFaction,
            Rights.EditFaction,
            Rights.CreateSerie,
            Rights.EditSerie,
            Rights.CreateStatus,
            Rights.EditStatus,
            Rights.AllowPlayerStateChange,
        };

        public static List<Right> AdministratorRights = new List<Right>
        {
            Rights.GameRuntimeAdmin,
            Rights.ViewPlayer,
            Rights.ViewCard,
            Rights.ViewDeck,
            Rights.ViewGame,
            Rights.EditPlayer,
            Rights.EditCard,
            Rights.EditCardRuling,
            Rights.AllowCardStatusChange,
            Rights.CreateCard,
            Rights.AllowOthersCardEdit,
            Rights.AllowOthersDeckEdit,
            Rights.CreateCardType,
            Rights.EditCardType,
            Rights.AllowDeckStatusChange,
            Rights.EditDeck,
            Rights.CreateDeck,
            Rights.CreateFaction,
            Rights.EditFaction,
            Rights.CreateSerie,
            Rights.EditSerie,
            Rights.CreateStatus,
            Rights.EditStatus,
            Rights.AllowPlayerStateChange,
        };

        public static List<Right> DeveloperRights = new List<Right>
        {
            Rights.ViewCard,
            Rights.ViewDeck,
            Rights.ViewGame,
            Rights.EditCard,
            Rights.EditCardRuling,
            Rights.CreateCard,
            Rights.AllowOthersCardEdit,
            Rights.AllowOthersDeckEdit,
            Rights.AllowDeckStatusChange,
            Rights.EditDeck,
            Rights.CreateDeck,
        };

        public static List<Right> ContributerRights = new List<Right>
        {
            Rights.ViewCard,
            Rights.ViewDeck,
            Rights.ViewGame,
            Rights.EditCard,
            Rights.CreateCard,
            Rights.EditDeck,
            Rights.CreateDeck,
        };
    }
}
