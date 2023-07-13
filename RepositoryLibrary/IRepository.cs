namespace RepositoryLibrary;

public interface IRepository<T>
{
    Task SaveAsync(T entity);
    
    Task<T> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    
    Task UpdateAsync(T entity);
    Task DeleteAsync(string id);
}