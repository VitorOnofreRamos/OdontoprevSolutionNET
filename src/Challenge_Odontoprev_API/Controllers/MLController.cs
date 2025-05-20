using Challenge_Odontoprev_API.ML.Models;
using Challenge_Odontoprev_API.ML.Services;
using Challenge_Odontoprev_API.Models;
using Challenge_Odontoprev_API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Challenge_Odontoprev_API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MLController : ControllerBase
{
    private readonly ConsultaCancelamentoService _mlService;
    private readonly _IRepository<Consulta> _consultaRepository;
    private readonly ILogger<MLController> _logger;
    public MLController(
        ConsultaCancelamentoService mlService,
        _IRepository<Consulta> consultaRepository,
        ILogger<MLController> logger)
    {
        _mlService = mlService;
        _consultaRepository = consultaRepository;
        _logger = logger;
    }
    [HttpPost("treinar")]
    [Authorize(Roles = "Admin")] // Restringe acesso a administradores
    public async Task<IActionResult> TreinarModelo()
    {
        try
        {
            bool sucesso = await _mlService.TrainModel();
            
            if (sucesso)
                return Ok(new { mensagem = "Modelo treinado com sucesso" });
            else
                return BadRequest(new { mensagem = "Não foi possível treinar o modelo. Verifique os logs." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao treinar modelo");
            return StatusCode(500, new { mensagem = "Erro interno ao treinar modelo" });
        }
    }
    [HttpGet("prever-cancelamento/{id}")]
    public async Task<IActionResult> PreverCancelamento(long id)
    {
        try
        {
            var consulta = await _consultaRepository.GetById(id);
            if (consulta == null)
                return NotFound(new { mensagem = "Consulta não encontrada" });
            // Verificar se a consulta já foi cancelada ou realizada
            if (consulta.Status == "Cancelado" || consulta.Status == "Realizado")
                return BadRequest(new { mensagem = $"Consulta já está com status {consulta.Status}" });
            // Prepara os dados para predição
            var consultaData = await _mlService.PrepareConsultaData(consulta);
            
            // Faz a predição
            var prediction = _mlService.PredictCancelamento(consultaData);
            return Ok(new
            {
                idConsulta = id,
                probabilidadeCancelamento = prediction.Probability,
                previstoCancelamento = prediction.PredictedLabel,
                score = prediction.Score
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao prever cancelamento para consulta {id}");
            return StatusCode(500, new { mensagem = "Erro ao processar previsão" });
        }
    }
}