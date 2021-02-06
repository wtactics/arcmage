using System.ComponentModel;
using Arcmage.Configuration;
using Arcmage.DAL.Model;
using Microsoft.EntityFrameworkCore;

namespace Arcmage.DAL
{
    public class DataBaseContext : DbContext
    {

        public DataBaseContext()
        {
        }

        public DbSet<UserModel> Users { get; set; }

        public DbSet<RoleModel> Roles { get; set; }

        public DbSet<CardModel> Cards { get; set; }

        public DbSet<CardTypeModel> CardTypes { get; set; }

        public DbSet<TemplateInfoModel> TemplateInfoModels { get; set; }

        public DbSet<FactionModel> Factions { get; set; }
        
        public DbSet<SerieModel> Series { get; set; }

        public DbSet<RuleSetModel> RuleSets { get; set; }

        public DbSet<StatusModel> Statuses { get; set; }

        public DbSet<DeckModel> Decks { get; set; }

        public DbSet<DeckCardModel> DeckCards { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Settings.Current.ArcmageConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserModel>().ToTable("UserModels");
            modelBuilder.Entity<UserModel>().HasKey(x => x.UserId);
            modelBuilder.Entity<UserModel>().HasOne(x => x.Role).WithMany();
            modelBuilder.Entity<UserModel>().HasMany(x => x.Cards).WithOne(x=>x.Creator);
            modelBuilder.Entity<UserModel>().HasMany(x => x.Decks).WithOne(x=>x.Creator);


            AddDefaults<RoleModel>(modelBuilder);
            AddDefaults<TemplateInfoModel>(modelBuilder);
            AddDefaults<CardTypeModel>(modelBuilder);
            AddDefaults<FactionModel>(modelBuilder);
            AddDefaults<StatusModel>(modelBuilder);
            AddDefaults<SerieModel>(modelBuilder);
            AddDefaults<RuleSetModel>(modelBuilder);
            AddDefaults<CardModel>(modelBuilder);
            AddDefaults<DeckModel>(modelBuilder);
            AddDefaults<DeckCardModel>(modelBuilder);


            modelBuilder.Entity<CardTypeModel>().HasOne(x => x.TemplateInfo);
            modelBuilder.Entity<SerieModel>().HasOne(x => x.Status);
            modelBuilder.Entity<RuleSetModel>().HasOne(x => x.Status);

            modelBuilder.Entity<CardModel>().HasOne(x=>x.Type).WithMany();
            modelBuilder.Entity<CardModel>().HasOne(x => x.Faction).WithMany();
            modelBuilder.Entity<CardModel>().HasOne(x => x.RuleSet).WithMany();
            modelBuilder.Entity<CardModel>().HasOne(x => x.Status).WithMany();
            modelBuilder.Entity<CardModel>().HasOne(x => x.Serie).WithMany(x=>x.Cards);

            modelBuilder.Entity<DeckModel>().HasOne(x => x.Status).WithMany().IsRequired(false);

            modelBuilder.Entity<DeckCardModel>().HasOne(x => x.Deck).WithMany(x=>x.DeckCards);
            modelBuilder.Entity<DeckCardModel>().HasOne(x => x.Card).WithMany();

        }

        private void AddDefaults<T>(ModelBuilder modelBuilder) where T : ModelBase
        {
            var className = typeof(T).Name;

            var tableName = $"{className}s";
            modelBuilder.Entity<T>().ToTable(tableName);


            var key = className.Substring(0, className.Length - "Model".Length) + "Id";
            modelBuilder.Entity<T>().HasKey(key);

            modelBuilder.Entity<T>()
                .HasOne(x => x.Creator)
                .WithMany().OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<T>()
                .HasOne(x => x.LastModifiedBy)
                .WithMany().OnDelete(DeleteBehavior.NoAction);
        }
    }
}
