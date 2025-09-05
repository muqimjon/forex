namespace Forex.Application.Commons.Interfaces;

using Forex.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Account> Accounts { get; }

    Task<bool> SaveAsync(CancellationToken cancellation);
}
