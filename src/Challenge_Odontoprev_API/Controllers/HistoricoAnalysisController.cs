using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Challenge_Odontoprev_API.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Challenge_Odontoprev_API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class HistoricoAnalysisController : ControllerBase
{
    private readonly IHistoricoAnalysisService _historicoAnalysisService;
    private readonly ILogger<HistoricoAnalysisController> _logger;

    public HistoricoAnalysisController(
            IHistoricoAnalysisService historicoAnalysisService,
            ILogger<HistoricoAnalysisController> logger)
    {
        _historicoAnalysisService = historicoAnalysisService;
        _logger = logger;
    }

    [HttpGet("paciente/{id}")]
    public async Task<IActionResult> GetAnalysisByPaciente(long id)
    {
        try
        {
            var results = await _historicoAnalysisService.AnalyzeHistoricosByPacienteId(id);
            if (!results.Any())
            {
                return NotFound($"Não foram encontrados históricos para análise do paciente com ID {id}");
            }
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao analisar históricos do paciente ID: {id}");
            return StatusCode(500, "Ocorreu um erro ao processar a análise de históricos.");
        }
    }

    [HttpGet("consulta/{id}")]
    public async Task<IActionResult> GetAnalysisByConsulta(long id)
    {
        try
        {
            var results = await _historicoAnalysisService.AnalyzeHistoricosByConsultaId(id);
            if (!results.Any())
            {
                return NotFound($"Não foram encontrados históricos para análise da consulta com ID {id}");
            }
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao analisar históricos da consulta ID: {id}");
            return StatusCode(500, "Ocorreu um erro ao processar a análise de históricos.");
        }
    }

    [HttpGet("estatisticas/paciente/{id}")]
    public async Task<IActionResult> GetStatisticsByPaciente(long id)
    {
        try
        {
            var statistics = await _historicoAnalysisService.GetSentimentStatisticsByPaciente(id);
            if (statistics["Positivo"] == 0 && statistics["Negativo"] == 0)
            {
                return NotFound($"Não foram encontrados históricos para análise do paciente com ID {id}");
            }
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao obter estatísticas de sentimento para o paciente ID: {id}");
            return StatusCode(500, "Ocorreu um erro ao processar as estatísticas de análise.");
        }
    }
}
