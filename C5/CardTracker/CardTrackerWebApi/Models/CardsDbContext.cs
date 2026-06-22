using Microsoft.EntityFrameworkCore;

namespace CardTrackerWebApi.Models;
public class CardsDbContext(DbContextOptions<CardsDbContext> options) 
    : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Deck> Decks { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<ActionCard> ActionCards { get; set; }
    public DbSet<CreatureCard> CreatureCards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CardsDbContext).Assembly);
    }
}