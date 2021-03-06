using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Arcmage.Configuration;
using Arcmage.DAL.Model;
using Arcmage.DAL.Utils;
using Arcmage.Model;
using Microsoft.EntityFrameworkCore;

namespace Arcmage.DAL
{
    public class Repository : IDisposable
    {

        public static string RepostioryPath;

        public static string TemplatesPath;

        public static string CardsPath;

        public static string DecksPath;

        public static string LicPath;

        public static string FontPath;

        static Repository()
        {
            try
            {
               

                RepostioryPath = Path.Combine(Settings.Current.RepositoryRootPah, "arcmage");
                TemplatesPath = Path.Combine(RepostioryPath, "CardTemplates");
                CardsPath = Path.Combine(RepostioryPath, "Cards");
                DecksPath = Path.Combine(RepostioryPath, "Decks");
                LicPath = Path.Combine(RepostioryPath, "Lic");
                FontPath = Path.Combine(RepostioryPath, "Fonts");
            }
            catch (Exception)
            {
            }
          
        }

     
     
        public string JoinUsText = "arcmage.org - join us!";

        public static void InitPaths()
        {
            
            if (!Directory.Exists(RepostioryPath))
            {
                Directory.CreateDirectory(RepostioryPath);
            }
            if (!Directory.Exists(TemplatesPath))
            {
                Directory.CreateDirectory(TemplatesPath);
            }
            if (!Directory.Exists(CardsPath))
            {
                Directory.CreateDirectory(CardsPath);
            }
            if (!Directory.Exists(LicPath))
            {
                Directory.CreateDirectory(LicPath);
            }
            
        }

        public static string GetCardPath(Guid cardGuid)
        {
            var path = Path.Combine(CardsPath, cardGuid.ToString());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static string GetDeckPath(Guid deckGuid)
        {
            var path = Path.Combine(DecksPath, deckGuid.ToString());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static string GetDeckFile(Guid deckGuid)
        {
            return Path.Combine(GetDeckPath(deckGuid), "deck.pdf");
        }

        public static string GetDeckJsonFile(Guid deckGuid)
        {
            return Path.Combine(GetDeckPath(deckGuid), "deck.json");
        }

        public static string GetDeckZipFile(Guid deckGuid)
        {
            return Path.Combine(GetDeckPath(deckGuid), "deck.zip");
        }

        public static string GetDeckTilesFile(Guid deckGuid, int tileNumber)
        {
            return Path.Combine(GetDeckPath(deckGuid), $"deck_tiles{tileNumber}.png");
        }



        public static string GetDeckFormatFile(Guid deckGuid)
        {
            return Path.Combine(GetDeckPath(deckGuid), "deck.txt");
        }

        public static string GetArtFile(Guid cardGuid)
        {
            return Path.Combine(GetCardPath(cardGuid), "art.png");
        }

     

        public static string GetOverlaySvgFile(Guid cardGuid)
        {
            return Path.Combine(GetCardPath(cardGuid), "card_overlay.svg");
        }

        public static string GetPrintSvgFile(Guid cardGuid)
        {
            return Path.Combine(GetCardPath(cardGuid), "card_print.svg");
        }

        public static string GetJsonFile(Guid cardGuid)
        {
            return Path.Combine(GetCardPath(cardGuid), "card.json");
        }

        public static string GetPngFile(Guid cardGuid)
        {
            return Path.Combine(GetCardPath(cardGuid), "card.png");
        }

        public static string GetPdfFile(Guid cardGuid)
        {
            return Path.Combine(GetCardPath(cardGuid), "card.pdf");
        }

        public static string GetRGBPdfFile(Guid cardGuid)
        {
            return Path.Combine(GetCardPath(cardGuid), "card-rgb.pdf");
        }

        public static string GetJpegFile(Guid cardGuid)
        {
            return Path.Combine(GetCardPath(cardGuid), "card.jpg");
        }

        public static string GetSvgFile(Guid cardGuid)
        {
            return Path.Combine(GetCardPath(cardGuid), "card.svg");
        }


        public static string GetBackPdfFile()
        {
            return Path.Combine(TemplatesPath, "back.pdf");
        }

        public static string GetBackJpegFile()
        {
            return Path.Combine(TemplatesPath, "back.jpeg");
        }

        public static string GetBackSvgFile()
        {
            return Path.Combine(TemplatesPath, "back.svg");
        }

        public static string GetBackPngFile()
        {
            return Path.Combine(TemplatesPath, "back.png");
        }

        public static string GetBackBorderPngFile()
        {
            return Path.Combine(TemplatesPath, "border.png");
        }



        public static string GetBackgroundPngFile(string faction, string cardType)
        {
            faction = faction.Replace(' ', '_');
            cardType = cardType.Replace(' ', '_');
            return Path.Combine(TemplatesPath, $"{faction}", $"{cardType}.png");
        }

        public static string GetTemplateFile(string faction, string cardType)
        {
            faction = faction.Replace(' ', '_');
            cardType = cardType.Replace(' ', '_');
           return Path.Combine(TemplatesPath, $"{faction}", $"{cardType}.svg");
        }

        public static string GetPrintBorderFile(string faction, string cardType, string ext)
        {
            faction = faction.Replace(' ', '_');
            cardType = cardType.Replace(' ', '_');
            return Path.Combine(TemplatesPath, $"{faction}", $"border.{ext}");
        }

        public static string GetOverlayTemplateFile(string faction, string cardType)
        {
            faction = faction.Replace(' ', '_');
            cardType = cardType.Replace(' ', '_');
            return Path.Combine(TemplatesPath, $"{faction}", $"{cardType}_overlay_plain.svg");
        }

        public DataBaseContext Context { get; private set; }

        public UserModel ServiceUser { get; private set; }
        
        public Repository()
        {
            Context = new DataBaseContext();
        }

        public Repository(Guid userGuid)
        {
            Context = new DataBaseContext();
            if (userGuid != Guid.Empty)
            {
                ServiceUser = Context.Users.Include(x=>x.Role).SingleOrDefault(x => x.Guid == userGuid);
            }
        }

        public void FillPredefinedData()
        {
            ServiceUser = Queryable.SingleOrDefault(Context.Users, x => x.Guid == PredefinedGuids.ServiceUser);
            if (ServiceUser == null)
            {
                ServiceUser = CreateServiceUser();
                FillPredefinedRoles();

                var adminRole = Context.Roles.FindByGuid(PredefinedGuids.Administrator);
                ServiceUser.Role = adminRole;
                Context.SaveChanges();

                FillPredefinedStatuses();
                FillPredefinedCartTypes();
                FillPredefinedFactions();
                FillPredefinedSeries();
                FillPredefinedRuleSets();
            }
        }

       

        #region api

        public CardModel CreateCard(string name, Guid guid)
        {
            var card = Context.Cards.FindByGuid(guid);
            if (card == null)
            {
                var serie = Context.Series.FindByGuid(PredefinedGuids.NoSerie);
                var ruleSet = Context.RuleSets.FindByGuid(PredefinedGuids.AllRuleSets);
                var status = Context.Statuses.FindByGuid(PredefinedGuids.Draft);
                var faction = Context.Factions.FindByGuid(PredefinedGuids.NoFaction);
                var cardType = Context.CardTypes.FindByGuid(PredefinedGuids.NoCardType);

                var utcNow = DateTime.UtcNow;
                card = new CardModel
                {
                    Name = name,
                    Guid = guid,
                    LastModifiedTime = utcNow,
                    CreateTime = utcNow,
                    Creator = ServiceUser,
                    LastModifiedBy = ServiceUser,
                    RuleSet = ruleSet,
                    Serie = serie,
                    Faction = faction,
                    Type = cardType,
                    Status = status,
                    Info = JoinUsText,
                };
                Context.Cards.Add(card);
                Context.SaveChanges();
            }
            return card;
        }

        public DeckModel CreateDeck(string name, Guid guid)
        {
            var deck = Context.Decks.FindByGuid(guid);
            if (deck == null)
            {
                var utcNow = DateTime.UtcNow;
                deck = new DeckModel
                {
                    Name = name,
                    Guid = guid,
                    LastModifiedTime = utcNow,
                    CreateTime = utcNow,
                    Creator = ServiceUser,
                    LastModifiedBy = ServiceUser,
                };
                Context.Decks.Add(deck);
                Context.SaveChanges();
            }
            return deck;
        }

        public DeckCardModel CreateDeckCard(DeckModel deckModel, CardModel cardModel, int quantity, Guid guid)
        {
            var deckCard = Context.DeckCards.FindByGuid(guid);
            if (deckCard == null)
            {
                var utcNow = DateTime.UtcNow;
                deckCard = new DeckCardModel
                {
                    Guid = guid,
                    Card = cardModel,
                    Deck = deckModel,
                    Quantity = quantity,
                    LastModifiedTime = utcNow,
                    CreateTime = utcNow,
                    Creator = ServiceUser,
                    LastModifiedBy = ServiceUser,
                };
                Context.DeckCards.Add(deckCard);
                Context.SaveChanges();
            }
            return deckCard;
        }

        public RulingModel CreateRuling(CardModel cardModel, string ruleText, Guid guid)
        {
            var ruling = Context.Rulings.FindByGuid(guid);
            if (ruling == null)
            {
                var utcNow = DateTime.UtcNow;
                ruling = new RulingModel
                {
                    Guid = guid,
                    Card = cardModel,
                    RuleText= ruleText,
                    LastModifiedTime = utcNow,
                    CreateTime = utcNow,
                    Creator = ServiceUser,
                    LastModifiedBy = ServiceUser,
                };
                Context.Rulings.Add(ruling);
                Context.SaveChanges();
            }
            return ruling;
        }


        public SerieModel CreateSeries(string name, Guid guid)
        {
            var serie = Context.Series.FindByGuid(guid);
            if (serie == null)
            {
                var status = Context.Statuses.FindByGuid(PredefinedGuids.Draft);
                var utcNow = DateTime.UtcNow;
                serie = new SerieModel
                {
                    Name = name,
                    Guid = guid,
                    LastModifiedTime = utcNow,
                    CreateTime = utcNow,
                    Creator = ServiceUser,
                    LastModifiedBy = ServiceUser,
                    Status = status,
                };
                Context.Series.Add(serie);
                Context.SaveChanges();
            }
            return serie;
        }

        public RuleSetModel CreateRuleSet(string name, Guid guid)
        {
            var ruleSet = Context.RuleSets.FindByGuid(guid);
            if (ruleSet == null)
            {
                var status = Context.Statuses.FindByGuid(PredefinedGuids.Draft);
                var utcNow = DateTime.UtcNow;
                ruleSet = new RuleSetModel
                {
                    Name = name,
                    Guid = guid,
                    LastModifiedTime = utcNow,
                    CreateTime = utcNow,
                    Creator = ServiceUser,
                    LastModifiedBy = ServiceUser,
                    Status = status,
                };
                Context.RuleSets.Add(ruleSet);
                Context.SaveChanges();
            }
            return ruleSet;
        }


        public FactionModel CreateFaction(string name, Guid guid)
        {
            var faction = Context.Factions.FindByGuid(guid);
            if (faction == null)
            {
                var utcNow = DateTime.UtcNow;
                faction = new FactionModel
                {
                    Name = name,
                    Guid = guid,
                    LastModifiedTime = utcNow,
                    CreateTime = utcNow,
                    Creator = ServiceUser,
                    LastModifiedBy = ServiceUser
                };
                Context.Factions.Add(faction);
                Context.SaveChanges();
            }
            return faction;
        }

        public TemplateInfoModel CreateTemplateInfoModel()
        {
            var utcNow = DateTime.UtcNow;
            var templateInfo = new TemplateInfoModel
            {
                LastModifiedTime = utcNow,
                CreateTime = utcNow,
                Creator = ServiceUser,
                LastModifiedBy = ServiceUser,
                Guid = Guid.NewGuid(),
            };
            return templateInfo;
        }

        public CardTypeModel CreateCardType(string name, Guid guid, TemplateInfoModel templateInfo)
        {
            var cardType = Context.CardTypes.FindByGuid(guid);
            if (cardType == null)
            {
                var utcNow = DateTime.UtcNow;
                cardType = new CardTypeModel
                {
                    Name = name,
                    Guid = guid,
                  
                    LastModifiedTime = utcNow,
                    CreateTime = utcNow,
                    Creator = ServiceUser,
                    LastModifiedBy = ServiceUser,

                    TemplateInfo = templateInfo
                };
               
                Context.CardTypes.Add(cardType);
                Context.SaveChanges();
            }
            return cardType;
        }

   

        public RoleModel CreateRole(string name, Guid guid)
        {
            var role = Context.Roles.FindByGuid(guid);
            if (role == null)
            {
                var utcNow = DateTime.UtcNow;
                role = new RoleModel
                {
                    Name = name,
                    Guid = guid,
                    LastModifiedTime = utcNow,
                    CreateTime = utcNow,
                    Creator = ServiceUser,
                    LastModifiedBy = ServiceUser
                };
                Context.Roles.Add(role);
                Context.SaveChanges();
            }
            return role;
        }

        public StatusModel CreateStatus(string name, Guid guid)
        {
            var status = Context.Statuses.FindByGuid(guid);
            if (status == null)
            {
                var utcNow = DateTime.UtcNow;
                status = new StatusModel
                {
                    Name = name,
                    Guid = guid,
                    LastModifiedTime = utcNow,
                    CreateTime = utcNow,
                    Creator = ServiceUser,
                    LastModifiedBy = ServiceUser
                };
                Context.Statuses.Add(status);
                Context.SaveChanges();
            }
            return status;
        }

        #endregion api

        #region predefined

        private UserModel CreateServiceUser()
        {
            var utcNow = DateTime.UtcNow;
            var administrator = new UserModel
            {
                Guid = PredefinedGuids.ServiceUser,
                Name = Settings.Current.ServiceUserName,
                Email = Settings.Current.ServiceUserEmail,
                Password = Hasher.HashPassword(GetMd5Hash(Settings.Current.ServiceUserPassword)) ,
                LastLoginTime = utcNow,
                CreateTime = utcNow,

            };
            Context.Users.Add(administrator);
            Context.SaveChanges();
            return administrator;
        }

        static string GetMd5Hash(string input)
        {
            using MD5 md5Hash = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var stringBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                stringBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return stringBuilder.ToString();
        }

        public UserModel CreateUser(string name, string email, string password, Guid newGuid)
        {
            var utcNow = DateTime.UtcNow;
            var role = Context.Roles.FindByGuid(PredefinedGuids.Contributer);
            var user = new UserModel
            {
                Guid = newGuid,
                Name = name,
                Email = email,
                Password = password,
                LastLoginTime = utcNow,
                CreateTime = utcNow,
                Role = role,
            };
            Context.Users.Add(user);
            Context.SaveChanges();
            return user;
        }


        private void FillPredefinedRoles()
        {
            CreateRole("Administrator", PredefinedGuids.Administrator);
            CreateRole("Developer", PredefinedGuids.Developer);
            CreateRole("Contributer", PredefinedGuids.Contributer);
        }

        private void FillPredefinedCartTypes()
        {
            var templateInfo = CreateTemplateInfoModel();
            templateInfo.ShowName = true;
            templateInfo.ShowType = true;
            templateInfo.ShowFaction = false;
            templateInfo.ShowAttack = false;
            templateInfo.ShowDefense = false;
            templateInfo.ShowText = false;
            templateInfo.ShowGoldCost = false;
            templateInfo.ShowLoyalty = false;
            templateInfo.ShowArt = false;
            templateInfo.ShowInfo = false;
            templateInfo.MaxTextBoxWidth = 190;
            templateInfo.MaxTextBoxHeight = 105;
            CreateCardType("None", PredefinedGuids.NoCardType, templateInfo);


            templateInfo = CreateTemplateInfoModel();
            templateInfo.ShowName = true;
            templateInfo.ShowType = true;
            templateInfo.ShowFaction = true;
            templateInfo.ShowAttack = true;
            templateInfo.ShowDefense = true;
            templateInfo.ShowText = true;
            templateInfo.ShowGoldCost = true;
            templateInfo.ShowLoyalty = true;
            templateInfo.ShowArt = true;
            templateInfo.ShowInfo = true;
            templateInfo.MaxTextBoxWidth = 190;
            templateInfo.MaxTextBoxHeight = 105;
            CreateCardType("Creature", PredefinedGuids.Creature, templateInfo);

            templateInfo = CreateTemplateInfoModel();
            templateInfo.ShowName = true;
            templateInfo.ShowType = true;
            templateInfo.ShowFaction = true;
            templateInfo.ShowAttack = false;
            templateInfo.ShowDefense = false;
            templateInfo.ShowText = true;
            templateInfo.ShowGoldCost = true;
            templateInfo.ShowLoyalty = true;
            templateInfo.ShowArt = true;
            templateInfo.ShowInfo = true;
            templateInfo.MaxTextBoxWidth = 190;
            templateInfo.MaxTextBoxHeight = 105;
            CreateCardType("Event", PredefinedGuids.Event, templateInfo);

            templateInfo = CreateTemplateInfoModel();
            templateInfo.ShowName = true;
            templateInfo.ShowType = true;
            templateInfo.ShowFaction = true;
            templateInfo.ShowAttack = false;
            templateInfo.ShowDefense = false;
            templateInfo.ShowText = true;
            templateInfo.ShowGoldCost = true;
            templateInfo.ShowLoyalty = true;
            templateInfo.ShowArt = true;
            templateInfo.ShowInfo = true;
            templateInfo.MaxTextBoxWidth = 190;
            templateInfo.MaxTextBoxHeight = 105;
            CreateCardType("Equipment", PredefinedGuids.Equipment, templateInfo);

            templateInfo = CreateTemplateInfoModel();
            templateInfo.ShowName = true;
            templateInfo.ShowType = true;
            templateInfo.ShowFaction = true;
            templateInfo.ShowAttack = false;
            templateInfo.ShowDefense = false;
            templateInfo.ShowText = true;
            templateInfo.ShowGoldCost = true;
            templateInfo.ShowLoyalty = true;
            templateInfo.ShowArt = true;
            templateInfo.ShowInfo = true;
            templateInfo.MaxTextBoxWidth = 190;
            templateInfo.MaxTextBoxHeight = 105;
            CreateCardType("Magic", PredefinedGuids.Magic, templateInfo);

            templateInfo = CreateTemplateInfoModel();
            templateInfo.ShowName = true;
            templateInfo.ShowType = true;
            templateInfo.ShowFaction = true;
            templateInfo.ShowAttack = false;
            templateInfo.ShowDefense = false;
            templateInfo.ShowText = true;
            templateInfo.ShowGoldCost = true;
            templateInfo.ShowLoyalty = true;
            templateInfo.ShowArt = true;
            templateInfo.ShowInfo = true;
            templateInfo.MaxTextBoxWidth = 190;
            templateInfo.MaxTextBoxHeight = 105;
            CreateCardType("Enchantment", PredefinedGuids.Enchantment, templateInfo);

            templateInfo = CreateTemplateInfoModel();
            templateInfo.ShowName = true;
            templateInfo.ShowType = true;
            templateInfo.ShowFaction = true;
            templateInfo.ShowAttack = false;
            templateInfo.ShowDefense = true;
            templateInfo.ShowText = true;
            templateInfo.ShowGoldCost = true;
            templateInfo.ShowLoyalty = true;
            templateInfo.ShowArt = true;
            templateInfo.ShowInfo = true;
            templateInfo.MaxTextBoxWidth = 190;
            templateInfo.MaxTextBoxHeight = 105;
            CreateCardType("City", PredefinedGuids.City, templateInfo);
           
        }

        private void FillPredefinedStatuses()
        {
            CreateStatus("Draft", PredefinedGuids.Draft);
            CreateStatus("Release Candidate", PredefinedGuids.ReleaseCandidate);
            CreateStatus("Final", PredefinedGuids.Final);
        }

        private void FillPredefinedFactions()
        {
            CreateFaction("None", PredefinedGuids.NoFaction);
            CreateFaction("Gaian", PredefinedGuids.Gaian);
            CreateFaction("Dark Legion", PredefinedGuids.DarkLegion);
            CreateFaction("Red Banner", PredefinedGuids.RedBanner);
            CreateFaction("House of Nobles", PredefinedGuids.HouseOfNobles);
            CreateFaction("The Empire", PredefinedGuids.Empire);
        }

        private void FillPredefinedSeries()
        {
            CreateSeries("None", PredefinedGuids.NoSerie);
            CreateSeries("Rebirth", PredefinedGuids.Rebirth);
            CreateSeries("Enchanted Realm", PredefinedGuids.EnchantedRealm);
            CreateSeries("New Horizons", PredefinedGuids.NewHorizons);
            CreateSeries("Shadow Waves", PredefinedGuids.ShadowWaves);

        }

        private void FillPredefinedRuleSets()
        {
            CreateRuleSet("All", PredefinedGuids.AllRuleSets);
            CreateRuleSet("Awesome Rules Concept", PredefinedGuids.AwesomeRulesConcept);
        }


        #endregion predefined

        public void Dispose()
        {
            Context.Dispose();
        }


        
    }
}
