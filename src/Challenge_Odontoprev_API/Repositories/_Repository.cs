using Challenge_Odontoprev_API.Infrastructure;
using Challenge_Odontoprev_API.Models;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace Challenge_Odontoprev_API.Repositories;

public class _Repository<T> : _IRepository<T> where T : _BaseEntity
{
    protected readonly ApplicationDbContext _context;
    private readonly DbSet<T> _entities;

    public _Repository(ApplicationDbContext context)
    {
        _context = context;
        _entities = context.Set<T>();
    }

    public async Task<T> GetById(long id)
    => await _entities.FindAsync(id) ?? throw new KeyNotFoundException($"Entity with id {id} not found.");

    public async Task<IEnumerable<T>> GetAll()
        => await _entities.ToListAsync();

    public async Task Insert(T entity)
    {
        if (entity is Paciente paciente)
        {
            var sql = @"BEGIN Pkg_Procedures_CRUD_Odontoprev.Insert_Paciente(
                                :p_Nome, 
                                :p_Data_Nascimento, 
                                :p_CPF, 
                                :p_Endereco, 
                                :p_Telefone, 
                                :p_Carteirinha
                            ); END;";
            await _context.Database.ExecuteSqlRawAsync(sql,
                    new OracleParameter("p_Nome", paciente.Nome),
                    new OracleParameter("p_Data_Nascimento", paciente.Data_Nascimento),
                    new OracleParameter("p_CPF", paciente.CPF),
                    new OracleParameter("p_Endereco", paciente.Endereco),
                    new OracleParameter("p_Telefone", paciente.Telefone),
                    new OracleParameter("p_Carteirinha", paciente.Carteirinha));
        }
        else if (entity is Dentista dentista)
        {
            var sql = @"BEGIN Pkg_Procedures_CRUD_Odontoprev.Insert_Dentista(
                                :p_Nome, 
                                :p_CRO, 
                                :p_Especialidade, 
                                :p_Telefone
                            ); END;";
            await _context.Database.ExecuteSqlRawAsync(sql,
                new OracleParameter("p_Nome", dentista.Nome),
                new OracleParameter("p_CRO", dentista.CRO),
                new OracleParameter("p_Especialidade", dentista.Especialidade),
                new OracleParameter("p_Telefone", dentista.Telefone));
        }
        else if (entity is Consulta consulta)
        {
            var sql = @"BEGIN Pkg_Procedures_CRUD_Odontoprev.Insert_Consulta(
                                :p_Data_Consulta, 
                                :p_ID_Paciente, 
                                :p_ID_Dentista, 
                                :p_Status
                            ); END;";
            await _context.Database.ExecuteSqlRawAsync(sql,
                new OracleParameter("p_Data_Consulta", consulta.Data_Consulta),
                new OracleParameter("p_ID_Paciente", consulta.ID_Paciente),
                new OracleParameter("p_ID_Dentista", consulta.ID_Dentista),
                new OracleParameter("p_Status", consulta.Status));
        }
        else if (entity is HistoricoConsulta historico)
        {
            var sql = @"BEGIN Pkg_Procedures_CRUD_Odontoprev.Insert_Historico_Consulta(
                                :p_ID_Consulta, 
                                :p_Data_Atendimento, 
                                :p_Motivo_Consulta, 
                                :p_Observacoes
                            ); END;";
            await _context.Database.ExecuteSqlRawAsync(sql,
                new OracleParameter("p_ID_Consulta", historico.ID_Consulta),
                new OracleParameter("p_Data_Atendimento", historico.Data_Atendimento),
                new OracleParameter("p_Motivo_Consulta", historico.Motivo_Consulta),
                new OracleParameter("p_Observacoes", historico.Observacoes ?? (object)DBNull.Value));
        }
        else
        {
            throw new NotSupportedException("Entity type not supported for Insert.");
        }
    }

    public async Task Update(T entity)
    {
        if (entity is Paciente paciente)
        {
            var sql = @"BEGIN Pkg_Procedures_CRUD_Odontoprev.Update_Paciente(
                                :p_ID_Paciente, 
                                :p_Nome, 
                                :p_Data_Nascimento, 
                                :p_CPF, 
                                :p_Endereco, 
                                :p_Telefone, 
                                :p_Carteirinha
                            ); END;";
            await _context.Database.ExecuteSqlRawAsync(sql,
                new OracleParameter("p_ID_Paciente", paciente.Id),
                new OracleParameter("p_Nome", (object)paciente.Nome ?? DBNull.Value),
                new OracleParameter("p_Data_Nascimento", paciente.Data_Nascimento),
                new OracleParameter("p_CPF", (object)paciente.CPF ?? DBNull.Value),
                new OracleParameter("p_Endereco", (object)paciente.Endereco ?? DBNull.Value),
                new OracleParameter("p_Telefone", (object)paciente.Telefone ?? DBNull.Value),
                new OracleParameter("p_Carteirinha", paciente.Carteirinha));
        }
        else if (entity is Dentista dentista)
        {
            var sql = @"BEGIN Pkg_Procedures_CRUD_Odontoprev.Update_Dentista(
                                :p_ID_Dentista, 
                                :p_Nome, 
                                :p_CRO, 
                                :p_Especialidade, 
                                :p_Telefone
                            ); END;";
            await _context.Database.ExecuteSqlRawAsync(sql,
                new OracleParameter("p_ID_Dentista", dentista.Id),
                new OracleParameter("p_Nome", (object)dentista.Nome ?? DBNull.Value),
                new OracleParameter("p_CRO", (object)dentista.CRO ?? DBNull.Value),
                new OracleParameter("p_Especialidade", (object)dentista.Especialidade ?? DBNull.Value),
                new OracleParameter("p_Telefone", (object)dentista.Telefone ?? DBNull.Value));
        }
        else if (entity is Consulta consulta)
        {
            var sql = @"BEGIN Pkg_Procedures_CRUD_Odontoprev.Update_Consulta(
                                :p_ID_Consulta, 
                                :p_Data_Consulta, 
                                :p_ID_Paciente, 
                                :p_ID_Dentista, 
                                :p_Status
                            ); END;";
            await _context.Database.ExecuteSqlRawAsync(sql,
                new OracleParameter("p_ID_Consulta", consulta.Id),
                new OracleParameter("p_Data_Consulta", consulta.Data_Consulta),
                new OracleParameter("p_ID_Paciente", consulta.ID_Paciente),
                new OracleParameter("p_ID_Dentista", consulta.ID_Dentista),
                new OracleParameter("p_Status", (object)consulta.Status ?? DBNull.Value));
        }
        else if (entity is HistoricoConsulta historico)
        {
            var sql = @"BEGIN Pkg_Procedures_CRUD_Odontoprev.Update_Historico_Consulta(
                                :p_ID_Historico, 
                                :p_ID_Consulta, 
                                :p_Data_Atendimento, 
                                :p_Motivo_Consulta, 
                                :p_Observacoes
                            ); END;";
            await _context.Database.ExecuteSqlRawAsync(sql,
                new OracleParameter("p_ID_Historico", historico.Id),
                new OracleParameter("p_ID_Consulta", historico.ID_Consulta),
                new OracleParameter("p_Data_Atendimento", historico.Data_Atendimento),
                new OracleParameter("p_Motivo_Consulta", (object)historico.Motivo_Consulta ?? DBNull.Value),
                new OracleParameter("p_Observacoes", (object)historico.Observacoes ?? DBNull.Value));
        }
        else
        {
            throw new NotSupportedException("Entity type not supported for Update.");
        }
    }

    public async Task Delete(long id)
    {
        if (typeof(T) == typeof(Paciente))
        {
            var sql = @"BEGIN Pkg_Procedures_CRUD_Odontoprev.Delete_Paciente(:p_ID_Paciente); END;";
            await _context.Database.ExecuteSqlRawAsync(sql,
                new OracleParameter("p_ID_Paciente", id));
        }
        else if (typeof(T) == typeof(Dentista))
        {
            var sql = @"BEGIN Pkg_Procedures_CRUD_Odontoprev.Delete_Dentista(:p_ID_Dentista); END;";
            await _context.Database.ExecuteSqlRawAsync(sql,
                new OracleParameter("p_ID_Dentista", id));
        }
        else if (typeof(T) == typeof(Consulta))
        {
            var sql = @"BEGIN Pkg_Procedures_CRUD_Odontoprev.Delete_Consulta(:p_ID_Consulta); END;";
            await _context.Database.ExecuteSqlRawAsync(sql,
                new OracleParameter("p_ID_Consulta", id));
        }
        else if (typeof(T) == typeof(HistoricoConsulta))
        {
            var sql = @"BEGIN Pkg_Procedures_CRUD_Odontoprev.Delete_Historico_Consulta(:p_ID_Historico); END;";
            await _context.Database.ExecuteSqlRawAsync(sql,
                new OracleParameter("p_ID_Historico", id));
        }
        else
        {
            throw new NotSupportedException("Entity type not supported for Delete.");
        }
    }
}