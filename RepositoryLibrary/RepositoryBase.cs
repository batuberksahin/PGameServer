using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using RepositoryLibrary.Models;

namespace RepositoryLibrary;

public abstract class RepositoryBase<T> : IRepository<T>
{
  protected readonly IMongoCollection<T> Collection;

  protected RepositoryBase(string collectionName)
  {
    var client =
      new MongoClient("mongodb+srv://admin:faberfaber@panteon.qsbrnuv.mongodb.net/?retryWrites=true&w=majority");
    var database = client.GetDatabase("panteon");

    Collection = database.GetCollection<T>(collectionName);
  }

  public virtual async Task SaveAsync(T entity)
  {
    await Collection.InsertOneAsync(entity);
  }

  public virtual async Task<T> GetByIdAsync(string id)
  {
    var filter = Builders<T>.Filter.Eq("_id", id);
    return await Collection.Find(filter).FirstOrDefaultAsync();
  }

  public virtual async Task<T> GetByGuidAsync(Guid id)
  {
    var filter = Builders<T>.Filter.Eq("_id", id);
    return await Collection.Find(filter).FirstOrDefaultAsync();
  }

  public virtual async Task<T> GetByUsernameAsync(string username)
  {
    var filter = Builders<T>.Filter.Eq("Username", username);
    return await Collection.Find(filter).FirstOrDefaultAsync();
  }

  public virtual async Task<IEnumerable<T>> GetAllAsync()
  {
    return await Collection.Find(_ => true).ToListAsync();
  }

  public virtual async Task UpdateAsync(T entity)
  {
    var filter = Builders<T>.Filter.Eq("_id", GetGuid(entity));
    await Collection.ReplaceOneAsync(filter, entity);
  }

  public virtual async Task DeleteAsync(string id)
  {
    var filter = Builders<T>.Filter.Eq("_id", id);
    await Collection.DeleteOneAsync(filter);
  }

  protected abstract string GetId(T   entity);
  protected abstract Guid   GetGuid(T entity);
}