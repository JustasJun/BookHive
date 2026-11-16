using System.Linq.Expressions;
using BookHive.Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace BookHive.Backend.Repositories;

public abstract class Repository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    protected readonly BookHiveDbContext _context;

    protected Repository(BookHiveDbContext context)
    {
        _context = context;
    }

    protected DbSet<TEntity> Entities => _context.Set<TEntity>();

    public void Add(TEntity entity)
    {
        Entities.Add(entity);
    }

    public void Remove(TEntity entity)
    {
        Entities.Remove(entity);
    }

    public virtual async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    protected async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate)
    {
        return await Entities.AnyAsync(predicate);
    }

    protected async Task<TEntity?> FindFirstAsync(
        Expression<Func<TEntity, bool>> predicate)
    {
        return await Entities.FirstOrDefaultAsync(predicate);
    }

    // generic method: the result type is chosen by the caller
    protected async Task<List<TResult>> SelectAsync<TResult>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> selector)
    {
        return await Entities.Where(filter).Select(selector).ToListAsync();
    }
}