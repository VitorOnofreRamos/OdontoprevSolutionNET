using Challenge_Odontoprev_API.Models;
using Challenge_Odontoprev_API.Repositories;

namespace Challenge_Odontoprev_API.Services;

public class _Service : _IService
{
    private readonly _IRepository<Paciente> _pacienteRepository;
    private readonly _IRepository<Dentista> _dentistaRepository;
    private readonly _IRepository<Consulta> _consultaRepository;
    private readonly _IRepository<HistoricoConsulta> _historicoConsultaRepository;

    public _Service(
        _IRepository<Paciente> pacienteRepository, 
        _IRepository<Dentista> dentistaRepository, 
        _IRepository<Consulta> consultaRepository, 
        _IRepository<HistoricoConsulta> historicoConsultaRepository)
    {
        _pacienteRepository = pacienteRepository;
        _dentistaRepository = dentistaRepository;
        _consultaRepository = consultaRepository;
        _historicoConsultaRepository = historicoConsultaRepository;
    }

    public async Task<bool> IsConsultaAgendada(string status)
    {
        var consultas = await _consultaRepository.GetAll();
        return consultas.Any(c => c.Status == status);
    }

    public async Task<IEnumerable<Consulta>> FindConsultasInTimePeriod(DateTime dataInicio, DateTime dataFim)
    {
        var consultas = await _consultaRepository.GetAll();
        return consultas.Where(c => c.Data_Consulta >= dataInicio && c.Data_Consulta <= dataFim);
    }

    public async Task<IEnumerable<HistoricoConsulta>> FindHistoricoConsultasByPacienteId(long id)
    {
        var historicoConsultas = await _historicoConsultaRepository.GetAll();
        return historicoConsultas.Where(hc => hc.Id == id);
    }
}
