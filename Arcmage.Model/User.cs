using System;
using System.Collections.Generic;

namespace Arcmage.Model
{
    public class User
    {
        public int Id { get; set; }

        public Guid Guid { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Password2 { get; set; }

        public string Token { get; set; }

        public Role Role { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime LastLoginTime { get; set; }

        public List<Card> Cards { get; set; }

        public List<Deck> Decks { get; set; }
        

        public User()
        {
            Cards = new List<Card>();
            Decks = new List<Deck>();
        }
    }
}
