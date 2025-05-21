using Challenge_Odontoprev_API.MachineLearning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Challenge_Odontoprev_API.Controllers;


[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SentimentAnalysisController : ControllerBase
{
    private readonly SentimentAnalysisService _sentimentService;
    private readonly ILogger<SentimentAnalysisController> _logger;

    public SentimentAnalysisController(
            SentimentAnalysisService sentimentService,
            ILogger<SentimentAnalysisController> logger)
    {
        _sentimentService = sentimentService;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public ActionResult<SentimentAnalysisResult> AnalyzeText([FromBody] TextAnalysisRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("O texto para análise não pode estar vazio.");
            }

            var result = _sentimentService.AnalyzeSentiment(request.Text);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao analisar sentimento do texto");
            return StatusCode(500, "Ocorreu um erro ao processar a análise de sentimentos.");
        }
    }

    [HttpPost("train")]
    [Authorize(Roles = "Admin")] // Restringe o treinamento apenas para administradores
    public async Task<ActionResult> TrainModel()
    {
        try
        {
            await _sentimentService.TrainAndSaveModelAsync();
            return Ok("Modelo treinado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao treinar modelo de análise de sentimentos");
            return StatusCode(500, "Ocorreu um erro ao treinar o modelo.");
        }
    }

    // DTO para solicitação de análise de texto
    public class TextAnalysisRequest
    {
        public string Text { get; set; }
    }
}
