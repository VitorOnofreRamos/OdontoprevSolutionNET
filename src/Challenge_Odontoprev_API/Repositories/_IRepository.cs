using Challenge_Odontoprev_API.Models;

namespace Challenge_Odontoprev_API.Repositories;

public interface _IRepository<T> where T : _BaseEntity
{
    Task<T> GetById(long id);
    Task<IEnumerable<T>> GetAll();
    Task Insert(T entity);
    Task Update(T entity);
    Task Delete(long id);
}
