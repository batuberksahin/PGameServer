using RepositoryLibrary.Models;

namespace RepositoryLibrary;

public interface IRepository<T>
{
  Task SaveAsync(T entity);

  Task<T>              GetByIdAsync(string       id);
  Task<T>              GetByGuidAsync(Guid       id);
  Task<T>              GetByUsernameAsync(string username);
  Task<IEnumerable<T>> GetAllAsync();

  Task UpdateAsync(T      entity);
  Task DeleteAsync(string id);
}