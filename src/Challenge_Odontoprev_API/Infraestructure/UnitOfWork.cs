using Challenge_Odontoprev_API.Models;
using Challenge_Odontoprev_API.Repositories;

namespace Challenge_Odontoprev_API.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public _IRepository<_BaseEntity> _IRepository { get; private set; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;

        _IRepository = new _Repository<_BaseEntity>(_context);
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}