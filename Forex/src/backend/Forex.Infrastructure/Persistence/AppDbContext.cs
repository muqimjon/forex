namespace Forex.Infrastructure.Persistence;

using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Account> Accounts { get; set; } = default!;

    public async Task<bool> SaveAsync(CancellationToken cancellation)
        => await SaveChangesAsync(cancellation) > 0;
}
