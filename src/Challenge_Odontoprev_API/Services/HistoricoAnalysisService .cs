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
    Task<bool> IsModelReady();
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

    public async Task<bool> IsModelReady()
    {
        try
        {
            var testResult = _sentimentService.AnalyzeSentiment("teste");
            return testResult != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IEnumerable<HistoricoSentimentResult>> AnalyzeHistoricosByPacienteId(long pacienteId)
    {
        try
        {
            _logger.LogInformation($"Iniciando análise de históricos para paciente ID: {pacienteId}");

            // Verificar se o modelo está pronto
            if (!await IsModelReady())
            {
                throw new InvalidOperationException("O modelo de análise de sentimentos não está pronto.");
            }

            // Obtém todas as consultas do paciente
            var consultas = (await _consultaRepository.GetAll())
                .Where(c => c.ID_Paciente == pacienteId)
                .ToList();

            _logger.LogInformation($"Encontradas {consultas.Count} consultas para o paciente {pacienteId}");

            if (!consultas.Any())
            {
                _logger.LogWarning($"Nenhuma consulta encontrada para o paciente {pacienteId}");
                return Enumerable.Empty<HistoricoSentimentResult>();
            }

            // Obtém todos os históricos relacionados às consultas do paciente
            var consultasIds = consultas.Select(c => c.Id).ToList();
            var historicos = (await _historicoRepository.GetAll())
                .Where(h => consultasIds.Contains(h.ID_Consulta) && !string.IsNullOrWhiteSpace(h.Observacoes))
                .ToList();

            _logger.LogInformation($"Encontrados {historicos.Count} históricos com observações para análise");

            if (!historicos.Any())
            {
                _logger.LogWarning($"Nenhum histórico com observações encontrado para o paciente {pacienteId}");
                return Enumerable.Empty<HistoricoSentimentResult>();
            }

            // Analisa o sentimento de cada histórico
            var results = new List<HistoricoSentimentResult>();
            var processedCount = 0;

            foreach (var historico in historicos)
            {
                try
                {
                    var previewText = string.IsNullOrEmpty(historico.Observacoes) ? "" :
                                     historico.Observacoes.Length > 50 ? historico.Observacoes.Substring(0, 50) + "..." : historico.Observacoes;
                    _logger.LogDebug($"Analisando histórico ID: {historico.Id} - Texto: '{previewText}'");

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

                    processedCount++;
                    _logger.LogDebug($"Histórico {historico.Id} analisado: {sentimentResult.SentimentCategory} (Confiança: {sentimentResult.Confidence:F2})");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Erro ao analisar histórico ID: {historico.Id}");
                    // Continue com os outros históricos mesmo se um falhar
                    continue;
                }
            }

            _logger.LogInformation($"Análise concluída. {processedCount} de {historicos.Count} históricos processados com sucesso");
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
            _logger.LogInformation($"Iniciando análise de históricos para consulta ID: {consultaId}");

            // Verificar se o modelo está pronto
            if (!await IsModelReady())
            {
                throw new InvalidOperationException("O modelo de análise de sentimentos não está pronto.");
            }

            // Obtém todos os históricos da consulta específica
            var historicos = (await _historicoRepository.GetAll())
                .Where(h => h.ID_Consulta == consultaId && !string.IsNullOrWhiteSpace(h.Observacoes))
                .ToList();

            _logger.LogInformation($"Encontrados {historicos.Count} históricos com observações para a consulta {consultaId}");

            if (!historicos.Any())
            {
                _logger.LogWarning($"Nenhum histórico com observações encontrado para a consulta {consultaId}");
                return Enumerable.Empty<HistoricoSentimentResult>();
            }

            // Analisa o sentimento de cada histórico
            var results = new List<HistoricoSentimentResult>();
            var processedCount = 0;

            foreach (var historico in historicos)
            {
                try
                {
                    _logger.LogDebug($"Analisando histórico ID: {historico.Id}");

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

                    processedCount++;
                    _logger.LogDebug($"Histórico {historico.Id} analisado: {sentimentResult.SentimentCategory} (Confiança: {sentimentResult.Confidence:F2})");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Erro ao analisar histórico ID: {historico.Id}");
                    continue;
                }
            }

            _logger.LogInformation($"Análise concluída. {processedCount} de {historicos.Count} históricos processados com sucesso");
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
            _logger.LogInformation($"Obtendo estatísticas de sentimento para paciente ID: {pacienteId}");

            var historicoResults = await AnalyzeHistoricosByPacienteId(pacienteId);

            // Contagem de sentimentos positivos e negativos
            var positiveCount = historicoResults.Count(r => r.SentimentResult.IsPositive);
            var negativeCount = historicoResults.Count(r => !r.SentimentResult.IsPositive);

            var stats = new Dictionary<string, int>
            {
                { "Positivo", positiveCount },
                { "Negativo", negativeCount },
                { "Total", positiveCount + negativeCount }
            };

            _logger.LogInformation($"Estatísticas para paciente {pacienteId}: {positiveCount} positivos, {negativeCount} negativos, {stats["Total"]} total");

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