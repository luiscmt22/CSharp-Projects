using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardTrackerWebApi.Models.Configuration;

public class DeckConfiguration : IEntityTypeConfiguration<Deck>
{
    public void Configure(EntityTypeBuilder<Deck> builder)
    {
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(d => d.UserId);

        builder.HasMany<Card>()
            .WithMany(c => c.Decks)
            .UsingEntity<CardDeck>();
    }
}
