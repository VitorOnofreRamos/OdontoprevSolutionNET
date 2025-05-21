//Services/HistoricoAnalysisService.cs
using Challenge_Odontoprev_API.MachineLearning;
using Challenge_Odontoprev_API.Models;
using Challenge_Odontoprev_API.Repositories;

namespace Challenge_Odontoprev_API.Services;

public interface IHistoricoAnalysisService
{
    Task<IEnumerable<HistoricoSentimentResult>> AnalyzeHistoricosByPacienteId(long pacienteId);
    Task<IEnumerable<HistoricoSentimentResult>> AnalyzeHistoricosByConsultaId(long consultaId);
    Task<Dictionary<string, int>> GetSentimentStatisticsByPaciente(long pacienteId);
}

public class HistoricoAnalysisService : IHistoricoAnalysisService
{
    private readonly _IRepository<HistoricoConsulta> _historicoRepository;
    private readonly _IRepository<Consulta> _consultaRepository;
    private readonly SentimentAnalysisService _sentimentService;
    private readonly ILogger<HistoricoAnalysisService> _logger;

    public HistoricoAnalysisService(
        _IRepository<HistoricoConsulta> historicoRepository,
        _IRepository<Consulta> consultaRepository,
        SentimentAnalysisService sentimentService,
        ILogger<HistoricoAnalysisService> logger)
    {
        _historicoRepository = historicoRepository;
        _consultaRepository = consultaRepository;
        _sentimentService = sentimentService;
        _logger = logger;
    }

    public async Task<IEnumerable<HistoricoSentimentResult>> AnalyzeHistoricosByPacienteId(long pacienteId)
    {
        try
        {
            // Obtém todas as consultas do paciente
            var consultas = (await _consultaRepository.GetAll())
                .Where(c => c.ID_Paciente == pacienteId)
                .ToList();

            if (!consultas.Any())
            {
                return Enumerable.Empty<HistoricoSentimentResult>();
            }

            // Obtém todos os históricos relacionados às consultas do paciente
            var consultasIds = consultas.Select(c => c.Id).ToList();
            var historicos = (await _historicoRepository.GetAll())
                .Where(h => consultasIds.Contains(h.ID_Consulta) && !string.IsNullOrEmpty(h.Observacoes))
                .ToList();

            // Analisa o sentimento de cada histórico
            var results = new List<HistoricoSentimentResult>();
            foreach (var historico in historicos)
            {
                var sentimentResult = _sentimentService.AnalyzeSentiment(historico.Observacoes);
                results.Add(new HistoricoSentimentResult
                {
                    HistoricoId = historico.Id,
                    ConsultaId = historico.ID_Consulta,
                    DataAtendimento = historico.Data_Atendimento,
                    MotivoConsulta = historico.Motivo_Consulta,
                    Observacoes = historico.Observacoes,
                    SentimentResult = sentimentResult
                });
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao analisar históricos do paciente ID: {pacienteId}");
            throw;
        }
    }

    public async Task<IEnumerable<HistoricoSentimentResult>> AnalyzeHistoricosByConsultaId(long consultaId)
    {
        try
        {
            // Obtém todos os históricos da consulta específica
            var historicos = (await _historicoRepository.GetAll())
                .Where(h => h.ID_Consulta == consultaId && !string.IsNullOrEmpty(h.Observacoes))
                .ToList();

            if (!historicos.Any())
            {
                return Enumerable.Empty<HistoricoSentimentResult>();
            }

            // Analisa o sentimento de cada histórico
            var results = new List<HistoricoSentimentResult>();
            foreach (var historico in historicos)
            {
                var sentimentResult = _sentimentService.AnalyzeSentiment(historico.Observacoes);
                results.Add(new HistoricoSentimentResult
                {
                    HistoricoId = historico.Id,
                    ConsultaId = historico.ID_Consulta,
                    DataAtendimento = historico.Data_Atendimento,
                    MotivoConsulta = historico.Motivo_Consulta,
                    Observacoes = historico.Observacoes,
                    SentimentResult = sentimentResult
                });
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao analisar históricos da consulta ID: {consultaId}");
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetSentimentStatisticsByPaciente(long pacienteId)
    {
        try
        {
            var historicoResults = await AnalyzeHistoricosByPacienteId(pacienteId);

            // Contagem de sentimentos positivos e negativos
            var stats = new Dictionary<string, int>
            {
                { "Positivo", historicoResults.Count(r => r.SentimentResult.IsPositive) },
                { "Negativo", historicoResults.Count(r => !r.SentimentResult.IsPositive) }
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao obter estatísticas de sentimento para o paciente ID: {pacienteId}");
            throw;
        }
    }
}

// Classe para representar o histórico com o resultado da análise de sentimento
public class HistoricoSentimentResult
{
    public long HistoricoId { get; set; }
    public long ConsultaId { get; set; }
    public DateTime DataAtendimento { get; set; }
    public string MotivoConsulta { get; set; }
    public string Observacoes { get; set; }
    public SentimentAnalysisResult SentimentResult { get; set; }
}
