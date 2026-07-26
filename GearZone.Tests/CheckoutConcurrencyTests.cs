using GearZone.Domain.Entities;
using GearZone.Infrastructure;
using GearZone.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Tests;

public class CheckoutConcurrencyTests
{
    [Fact]
    public async Task ClearPurchasedCartItems_IsIdempotent_WhenItemWasAlreadyDeleted()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new ApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();
        await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF;");

        var cartItemId = Guid.NewGuid();
        db.CartItems.Add(new CartItem
        {
            Id = cartItemId,
            CartId = Guid.NewGuid(),
            VariantId = Guid.NewGuid(),
            Quantity = 1
        });
        await db.SaveChangesAsync();

        var repository = new CartItemRepository(db);

        await repository.DeleteRangeByIdsAsync(new[] { cartItemId });
        await repository.DeleteRangeByIdsAsync(new[] { cartItemId });

        // Simulate a stale tracked delete left by work performed before the
        // payment-persistence boundary. Clearing the tracker prevents that stale
        // command from joining the payment SaveChanges batch.
        db.CartItems.Remove(db.CartItems.Local.Single(x => x.Id == cartItemId));
        new UnitOfWork(db).ClearTrackedEntities();
        await db.SaveChangesAsync();

        Assert.Empty(db.ChangeTracker.Entries());
        Assert.False(await db.CartItems.AsNoTracking().AnyAsync(x => x.Id == cartItemId));
    }
}
