using Challenge_Odontoprev_API.ML.Services;
using Challenge_Odontoprev_API.Models;

namespace Challenge_Odontoprev_API.ML.Extensions;

public static class ConsultaExtensions
{
    public static async Task<Dictionary<long, float>> AnalisarRiscoCancelamento(
        this IEnumerable<Consulta> consultas,
        ConsultaCancelamentoService mlService)
    {
        var resultados = new Dictionary<long, float>();

        foreach (var consulta in consultas)
        {
            try 
            {
                var dados = await mlService.PrepareConsultaData(consulta);
                var predicao = mlService.PredictCancelamento(dados);

                resultados.Add(consulta.Id, predicao.Probability);
            }
            catch
            {
                // Se falhar para uma consulta, continua para as próximas
                resultados.Add(consulta.Id, -1); // Valor indicado que não foi possível predizer
            }
        }

        return resultados;
    }
}