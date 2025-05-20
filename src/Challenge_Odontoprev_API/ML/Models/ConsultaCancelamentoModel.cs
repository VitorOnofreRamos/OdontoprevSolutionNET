namespace Challenge_Odontoprev_API.ML.Models;

public class ConsultaCancelamentoData
{
    public float DiasAteConsulta { get; set; }              // Dias entre agendamento e consulta
    public float IdadeEmAnos { get; set; }                  // Idade do paciente
    public float NumeroConsultasPrevias { get; set; }       // Quantidade de consultas prévias do paciente
    public float NumeroCancelamentosPrevios { get; set; }   // Cancelamentos prévios
    public float HoraDoDia { get; set; }                    // Hora do dia (0-23)
    public float DiaDaSemana { get; set; }                  // Dia da semana (0-6, começando no domingo)
    public bool JaFezTratamentoAnterior { get; set; }       // Se já fez algum tratamento
    public bool PrimeiraConsulta { get; set; }              // Se é primeira consulta do paciente
    public bool FoiFeriado { get; set; }                    // Se o dia era feriado
    public bool Cancelado { get; set; }                     // Label - se foi cancelado (para treino)
}

public class ConsultaCancelamentoPrediction
{
    public bool PredictedLabel { get; set; }
    public float Probability { get; set; }
    public float Score { get; set; }
}