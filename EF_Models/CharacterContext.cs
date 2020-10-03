using Microsoft.EntityFrameworkCore;

namespace HitPointTracker_API.CharacterSheet
{
    public class CharacterSheetContext : DbContext
    {
        public CharacterSheetContext(DbContextOptions<CharacterSheetContext> options)
            : base(options)
        {
        }

        public DbSet<CharacterData> CharacterData { get; set; }
        public DbSet<ClassData> ClassData { get; set; }
        public DbSet<HealthData> HealthData { get; set; }
        public DbSet<StatsData> StatsData { get; set; }
        public DbSet<ItemsData> ItemsData { get; set; }
        public DbSet<DefensesData> DefensesData { get; set; }
        public DbSet<BonusesData> BonusesData { get; set; }
    }
}