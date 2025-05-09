using Challenge_Odontoprev_API.Models;
using Challenge_Odontoprev_API.Repositories;

namespace Challenge_Odontoprev_API.Infrastructure;

public interface IUnitOfWork : IDisposable
{
    _IRepository<_BaseEntity> _IRepository { get; }

    Task<int> CompleteAsync();
}