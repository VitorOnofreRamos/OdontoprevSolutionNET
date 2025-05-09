using Challenge_Odontoprev_API.Models;

namespace Challenge_Odontoprev_API.Services;

public interface _IService
{
    Task<bool> IsConsultaAgendada(string status);
    Task<IEnumerable<Consulta>> FindConsultasInTimePeriod(DateTime dataInicio, DateTime dataFim);
    Task<IEnumerable<HistoricoConsulta>> FindHistoricoConsultasByPacienteId(long id);
}
