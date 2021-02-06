using System;
using System.Collections.Generic;
using System.Linq;

namespace Arcmage.Game.Api.GameRuntime
{
    public static class GameExtensions
    {
        #region game

        public static GamePlayer GetPlayer(this Game game, Guid playerGuid)
        {
            return game?.Players.FirstOrDefault(x => x.PlayerGuid == playerGuid);
        }

        public static GameList GetList(this Game game, Guid playerGuid, ListType kind)
        {
            var player = GetPlayer(game, playerGuid);
            if (player == null) return new GameList();
            switch (kind)
            {
                case ListType.Deck:
                    return player.Deck;
                case ListType.Graveyard:
                    return player.Graveyard;
                case ListType.Hand:
                    return player.Hand;
                case ListType.Play:
                    return player.Play;
                case ListType.Removed:
                    return player.Removed;
            }
            return new GameList();
        }

        #endregion game


        #region lists

        [ThreadStatic]
        private static Random _instance;


        private static Random Random
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Random();
                }
                return _instance;
            }
        }


        public static void Shuffle<T>(this IList<T> list)
        {
            for (var i = 0; i < list.Count; i++)
                list.Swap(i, Random.Next(i, list.Count));
        }

        public static void Move<T>(this IList<T> list, int oldIndex, int newIndex)
        {
            if (list == null) return;
            if (0 > oldIndex || oldIndex >= list.Count) return;
            if (0 > newIndex || newIndex >= list.Count) return;
            var temp = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex,temp);
            
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            if(list == null) return;
            if (0 > i || i >= list.Count ) return;
            if (0 > j || j >= list.Count) return;
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        public static void Swap<T>(this IList<T> list, T item1, T item2)
        {
            var i = list.IndexOf(item1);
            var j = list.IndexOf(item2);
            Swap(list,i,j);
        }

        public static void Push<T>(this IList<T> list, T item)
        {
            if (item!= null) list?.Add(item);
        }

        public static T Pop<T>(this IList<T> list) where T : class
        {
            var last = list?.LastOrDefault();
            if (last != null) list?.Remove(last);
            return last;
        }
        #endregion lists
    }
}