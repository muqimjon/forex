namespace Forex.Infrastructure.Persistence;

using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class AppDbContext : DbContext, IAppDbContext
{
    public DbSet<User> Users { get; set; } = default!;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellation)
        => await base.SaveChangesAsync(cancellation);
}
