namespace Arcmage.Game.Api.GameRuntime
{
    public enum GameActionType
    {
        JoinGame,
        LeaveGame,
        LoadDeck,
        StartGame,

        ShuffleList,
        UpdateList,

        DrawCard,
        DiscardCard,
        DeckCard,
        PlayCard,
        RemoveCard,

        ChangeCardState,
        ChangePlayerStats,

        FlipCoin,
        DiceRoll,

        ChangeCurtainState

    }
}
