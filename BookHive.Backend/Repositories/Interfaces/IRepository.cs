namespace BookHive.Backend.Repositories;

public interface IRepository<TEntity> where TEntity : class
{
    void Add(TEntity entity);
    void Remove(TEntity entity);
    Task SaveAsync();
}
