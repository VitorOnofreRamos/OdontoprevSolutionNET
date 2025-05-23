using Microsoft.ML;
using Microsoft.ML.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Challenge_Odontoprev_API.MachineLearning;

// Classe para representar os dados de entrada para o modelo
public class SentimentData
{
    [LoadColumn(0)]
    public string Text { get; set; }

    [LoadColumn(1), ColumnName("Label")]
    public bool Sentiment { get; set; }
}

// Classe para representar a previsão do modelo
public class SentimentPrediction
{
    [ColumnName("PredictedLabel")]
    public bool Prediction { get; set; }

    [ColumnName("Score")]
    public float Score { get; set; }

    [ColumnName("Probability")]
    public float Probability { get; set; }
}

// Classe para o resultado da análise de sentimentos
public class SentimentAnalysisResult
{
    public string Text { get; set; }
    public bool IsPositive { get; set; }
    public float Confidence { get; set; }
    public string SentimentCategory => IsPositive ? "Positivo" : "Negativo";
}

// Classe que encapsula a lógica do modelo de ML
public class SentimentAnalysisService
{
    private readonly MLContext _mlContext;
    private ITransformer _model;
    private PredictionEngine<SentimentData, SentimentPrediction> _predictionEngine;
    private readonly string _modelPath;
    private readonly ILogger<SentimentAnalysisService> _logger;

    public SentimentAnalysisService(ILogger<SentimentAnalysisService> logger)
    {
        _mlContext = new MLContext(seed: 1);
        _modelPath = Path.Combine(AppContext.BaseDirectory, "sentiment_model.zip");
        _logger = logger;
    }

    // Método para treinar e salvar o modelo
    public async Task TrainAndSaveModelAsync()
    {
        try
        {
            _logger.LogInformation("Iniciando treinamento do modelo de análise de sentimentos");

            // Dados de treinamento expandidos e balanceados
            var trainingData = new List<SentimentData>
            {
                // Sentimentos POSITIVOS exemplos
                new SentimentData { Text = "Paciente apresentou melhora significativa", Sentiment = true },
                new SentimentData { Text = "Tratamento progredindo muito bem", Sentiment = true },
                new SentimentData { Text = "Excelente resposta ao tratamento", Sentiment = true },
                new SentimentData { Text = "Paciente muito satisfeito com o resultado", Sentiment = true },
                new SentimentData { Text = "Recuperação acima do esperado", Sentiment = true },
                new SentimentData { Text = "Sem complicações pós-procedimento", Sentiment = true },
                new SentimentData { Text = "Paciente relatou ausência de dor", Sentiment = true },
                new SentimentData { Text = "Procedimento realizado com sucesso total", Sentiment = true },
                new SentimentData { Text = "Excelente estado de saúde bucal", Sentiment = true },
                new SentimentData { Text = "Resultado estético muito satisfatório", Sentiment = true },
                new SentimentData { Text = "Paciente elogiou o atendimento", Sentiment = true },
                new SentimentData { Text = "Processo de cura normal e saudável", Sentiment = true },
                new SentimentData { Text = "Paciente demonstrou grande satisfação", Sentiment = true },
                new SentimentData { Text = "Evolução positiva do quadro clínico", Sentiment = true },
                new SentimentData { Text = "Resultados superaram as expectativas", Sentiment = true },
                new SentimentData { Text = "Paciente recomendou o tratamento", Sentiment = true },
                new SentimentData { Text = "Cicatrização perfeita", Sentiment = true },
                new SentimentData { Text = "Procedimento indolor", Sentiment = true },
                new SentimentData { Text = "Paciente voltou sorrindo", Sentiment = true },
                new SentimentData { Text = "Tratamento finalizado com êxito", Sentiment = true },
                new SentimentData { Text = "Ótima evolução clínica", Sentiment = true },
                new SentimentData { Text = "Paciente alegre e confiante", Sentiment = true },
                new SentimentData { Text = "Resultado excepcional", Sentiment = true },
                new SentimentData { Text = "Bem-estar do paciente restaurado", Sentiment = true },
                new SentimentData { Text = "Sucesso completo do procedimento", Sentiment = true },
                new SentimentData { Text = "Paciente relatou melhora logo após o início do tratamento", Sentiment = true },
                new SentimentData { Text = "Gengiva saudável e sem sinais de inflamação", Sentiment = true },
                new SentimentData { Text = "Paciente extremamente satisfeito com a consulta", Sentiment = true },
                new SentimentData { Text = "Sem dor ou desconforto relatado após o procedimento", Sentiment = true },
                new SentimentData { Text = "Avaliação clínica excelente", Sentiment = true },
                new SentimentData { Text = "Paciente com recuperação rápida e eficaz", Sentiment = true },
                new SentimentData { Text = "Resultado funcional e estético perfeito", Sentiment = true },
                new SentimentData { Text = "Paciente agradeceu pelo cuidado recebido", Sentiment = true },
                new SentimentData { Text = "Tratamento concluído antes do prazo", Sentiment = true },
                new SentimentData { Text = "Paciente com total adesão às orientações", Sentiment = true },
                new SentimentData { Text = "Melhoria visível no quadro inflamatório", Sentiment = true },
                new SentimentData { Text = "Paciente elogiou a equipe técnica", Sentiment = true },
                new SentimentData { Text = "Alta médica concedida com sucesso", Sentiment = true },
                new SentimentData { Text = "Controle total da dor obtido", Sentiment = true },
                new SentimentData { Text = "Tratamento considerado exemplar", Sentiment = true },
                new SentimentData { Text = "Paciente participou ativamente do processo de recuperação", Sentiment = true },
                new SentimentData { Text = "Boa aceitação do material restaurador", Sentiment = true },
                new SentimentData { Text = "Paciente retornou sem queixas", Sentiment = true },
                new SentimentData { Text = "Curva de recuperação excelente", Sentiment = true },
                new SentimentData { Text = "Paciente entusiasmado com os resultados", Sentiment = true },
                new SentimentData { Text = "Atendimento elogiado pela empatia", Sentiment = true },
                new SentimentData { Text = "Estabilidade clínica mantida após o procedimento", Sentiment = true },
                new SentimentData { Text = "Paciente demonstrou confiança no tratamento", Sentiment = true },
                new SentimentData { Text = "Sem efeitos colaterais observados", Sentiment = true },
                new SentimentData { Text = "Paciente relatou bem-estar geral melhorado", Sentiment = true },
                new SentimentData { Text = "Paciente demonstrou plena recuperação funcional", Sentiment = true },
                new SentimentData { Text = "Consulta finalizou sem intercorrências", Sentiment = true },
                new SentimentData { Text = "Paciente retornou para agradecer pelo excelente atendimento", Sentiment = true },
                new SentimentData { Text = "Adaptação rápida à prótese instalada", Sentiment = true },
                new SentimentData { Text = "Paciente elogiou a pontualidade da equipe", Sentiment = true },
                new SentimentData { Text = "Tratamento realizado dentro dos parâmetros ideais", Sentiment = true },
                new SentimentData { Text = "Paciente sem queixas durante acompanhamento", Sentiment = true },
                new SentimentData { Text = "Melhora contínua observada nas consultas subsequentes", Sentiment = true },
                new SentimentData { Text = "Paciente satisfeito com os esclarecimentos fornecidos", Sentiment = true },
                new SentimentData { Text = "Ótima adesão ao plano de tratamento proposto", Sentiment = true },
                new SentimentData { Text = "Paciente recuperou a confiança no sorriso", Sentiment = true },
                new SentimentData { Text = "Exame pós-operatório dentro da normalidade", Sentiment = true },
                new SentimentData { Text = "Paciente relatou boa qualidade de vida após o tratamento", Sentiment = true },
                new SentimentData { Text = "Expectativas do paciente foram plenamente atendidas", Sentiment = true },
                new SentimentData { Text = "Paciente indicou o consultório para familiares", Sentiment = true },
                new SentimentData { Text = "Sem sinais de rejeição ou inflamação", Sentiment = true },
                new SentimentData { Text = "Paciente confiante com o plano terapêutico", Sentiment = true },
                new SentimentData { Text = "Boa resposta à medicação prescrita", Sentiment = true },
                new SentimentData { Text = "Paciente saiu da consulta com boas perspectivas", Sentiment = true },
                new SentimentData { Text = "Avaliação final positiva por toda a equipe", Sentiment = true },
                new SentimentData { Text = "Paciente expressou gratidão pelo cuidado recebido", Sentiment = true },
                new SentimentData { Text = "Recuperação ocorreu dentro do tempo esperado", Sentiment = true },
                new SentimentData { Text = "Paciente demonstrou conforto durante o procedimento", Sentiment = true },
                new SentimentData { Text = "Satisfação total com o resultado obtido", Sentiment = true },
                new SentimentData { Text = "Paciente relatou sensação de alívio e confiança", Sentiment = true },
                new SentimentData { Text = "Paciente relatou sensação de alívio imediato após o procedimento", Sentiment = true },
                new SentimentData { Text = "Consulta transcorreu de forma tranquila e produtiva", Sentiment = true },
                new SentimentData { Text = "Paciente demonstrou entusiasmo com a evolução do tratamento", Sentiment = true },
                new SentimentData { Text = "Recuperação dentro do esperado, sem intercorrências", Sentiment = true },
                new SentimentData { Text = "Paciente expressou confiança na equipe médica", Sentiment = true },
                new SentimentData { Text = "Melhoria significativa na qualidade de vida relatada pelo paciente", Sentiment = true },
                new SentimentData { Text = "Paciente satisfeito com a atenção recebida durante a consulta", Sentiment = true },
                new SentimentData { Text = "Tratamento concluído com sucesso e sem complicações", Sentiment = true },
                new SentimentData { Text = "Paciente elogiou a clareza das orientações fornecidas", Sentiment = true },
                new SentimentData { Text = "Evolução clínica positiva observada nas últimas consultas", Sentiment = true },
                new SentimentData { Text = "Paciente demonstrou gratidão pelo cuidado recebido", Sentiment = true },
                new SentimentData { Text = "Procedimento realizado com excelência técnica", Sentiment = true },
                new SentimentData { Text = "Paciente relatou melhora na autoestima após o tratamento", Sentiment = true },
                new SentimentData { Text = "Consulta realizada com pontualidade e eficiência", Sentiment = true },
                new SentimentData { Text = "Paciente retornou para agradecer pelos resultados obtidos", Sentiment = true },
                new SentimentData { Text = "Adesão ao tratamento foi completa e eficaz", Sentiment = true },
                new SentimentData { Text = "Paciente relatou ausência de dor durante todo o processo", Sentiment = true },
                new SentimentData { Text = "Equipe médica recebeu elogios pela dedicação", Sentiment = true },
                new SentimentData { Text = "Paciente expressou satisfação com a estética final", Sentiment = true },
                new SentimentData { Text = "Recuperação pós-operatória ocorreu sem complicações", Sentiment = true },
                new SentimentData { Text = "Paciente demonstrou motivação para seguir as orientações", Sentiment = true },
                new SentimentData { Text = "Tratamento proporcionou melhoria funcional significativa", Sentiment = true },
                new SentimentData { Text = "Paciente relatou sensação de bem-estar após as sessões", Sentiment = true },
                new SentimentData { Text = "Consulta contribuiu para esclarecer todas as dúvidas do paciente", Sentiment = true },
                new SentimentData { Text = "Paciente recomendou o serviço a amigos e familiares", Sentiment = true },
                new SentimentData { Text = "Paciente saiu da consulta sorrindo e agradecido", Sentiment = true },
                new SentimentData { Text = "Tratamento foi bem aceito e elogiado pelo paciente", Sentiment = true },
                new SentimentData { Text = "Paciente demonstrou confiança durante todo o atendimento", Sentiment = true },
                new SentimentData { Text = "Paciente relatou melhora visível na mastigação", Sentiment = true },
                new SentimentData { Text = "Paciente elogiou a estrutura da clínica", Sentiment = true },
                new SentimentData { Text = "Recuperação superou as expectativas", Sentiment = true },
                new SentimentData { Text = "Paciente manteve acompanhamento com regularidade e disciplina", Sentiment = true },
                new SentimentData { Text = "Paciente sentiu-se acolhido desde a chegada", Sentiment = true },
                new SentimentData { Text = "Paciente agradeceu o atendimento humanizado", Sentiment = true },
                new SentimentData { Text = "Paciente expressou alegria com os primeiros resultados", Sentiment = true },
                new SentimentData { Text = "Consulta transcorreu de forma muito leve e agradável", Sentiment = true },
                new SentimentData { Text = "Paciente demonstrou empolgação com o plano de tratamento", Sentiment = true },
                new SentimentData { Text = "Paciente indicou a clínica para colegas de trabalho", Sentiment = true },
                new SentimentData { Text = "Paciente relatou estar dormindo melhor após o tratamento", Sentiment = true },
                new SentimentData { Text = "Paciente mostrou-se satisfeito com a resolução do problema", Sentiment = true },
                new SentimentData { Text = "Paciente relatou estar sem dor desde a última sessão", Sentiment = true },
                new SentimentData { Text = "Paciente sentiu-se seguro durante todo o procedimento", Sentiment = true },
                new SentimentData { Text = "Paciente demonstrou gratidão por ter sido bem orientado", Sentiment = true },
                new SentimentData { Text = "Paciente se mostrou otimista com o tratamento futuro", Sentiment = true },
                new SentimentData { Text = "Paciente relatou mais disposição após início do tratamento", Sentiment = true },
                new SentimentData { Text = "Paciente ficou satisfeito com o atendimento desde a recepção", Sentiment = true },
                new SentimentData { Text = "Paciente ficou encantado com a atenção dada aos detalhes", Sentiment = true },
                new SentimentData { Text = "Paciente mencionou que superou o medo de dentista", Sentiment = true },
                new SentimentData { Text = "Paciente relatou estar com autoestima elevada", Sentiment = true },
                new SentimentData { Text = "Paciente ficou feliz com o cuidado recebido", Sentiment = true },



                // Sentimentos NEGATIVOS - 130 exemplos
                new SentimentData { Text = "Paciente com dor persistente e intensa", Sentiment = false },
                new SentimentData { Text = "Tratamento sem progresso significativo", Sentiment = false },
                new SentimentData { Text = "Complicações graves durante procedimento", Sentiment = false },
                new SentimentData { Text = "Paciente muito insatisfeito com resultado", Sentiment = false },
                new SentimentData { Text = "Inflamação persistente e preocupante", Sentiment = false },
                new SentimentData { Text = "Dificuldade severa de adaptação à prótese", Sentiment = false },
                new SentimentData { Text = "Necessidade urgente de refazer procedimento", Sentiment = false },
                new SentimentData { Text = "Paciente relatou desconforto extremo", Sentiment = false },
                new SentimentData { Text = "Resposta muito negativa ao tratamento", Sentiment = false },
                new SentimentData { Text = "Infecção pós-operatória grave", Sentiment = false },
                new SentimentData { Text = "Paciente reclamou do atendimento", Sentiment = false },
                new SentimentData { Text = "Sangramento excessivo", Sentiment = false },
                new SentimentData { Text = "Paciente demonstrou insatisfação", Sentiment = false },
                new SentimentData { Text = "Deterioração do quadro clínico", Sentiment = false },
                new SentimentData { Text = "Resultados abaixo do esperado", Sentiment = false },
                new SentimentData { Text = "Paciente cancelou próxima consulta", Sentiment = false },
                new SentimentData { Text = "Cicatrização problemática", Sentiment = false },
                new SentimentData { Text = "Procedimento muito doloroso", Sentiment = false },
                new SentimentData { Text = "Paciente saiu contrariado", Sentiment = false },
                new SentimentData { Text = "Falha no tratamento", Sentiment = false },
                new SentimentData { Text = "Complicações sérias", Sentiment = false },
                new SentimentData { Text = "Paciente ansioso e preocupado", Sentiment = false },
                new SentimentData { Text = "Resultado insatisfatório", Sentiment = false },
                new SentimentData { Text = "Problema grave identificado", Sentiment = false },
                new SentimentData { Text = "Fracasso do procedimento", Sentiment = false },
                new SentimentData { Text = "Paciente relatou piora após o tratamento", Sentiment = false },
                new SentimentData { Text = "Gengiva com sangramento contínuo", Sentiment = false },
                new SentimentData { Text = "Paciente saiu da consulta insatisfeito", Sentiment = false },
                new SentimentData { Text = "Queixas frequentes de dor durante o procedimento", Sentiment = false },
                new SentimentData { Text = "Retorno clínico marcado por complicações", Sentiment = false },
                new SentimentData { Text = "Paciente inseguro quanto à eficácia do tratamento", Sentiment = false },
                new SentimentData { Text = "Inchaço persistente após dias do procedimento", Sentiment = false },
                new SentimentData { Text = "Paciente apresentou reação adversa", Sentiment = false },
                new SentimentData { Text = "Desconforto relatado durante a mastigação", Sentiment = false },
                new SentimentData { Text = "Paciente solicitou segunda opinião médica", Sentiment = false },
                new SentimentData { Text = "Tratamento interrompido por complicações", Sentiment = false },
                new SentimentData { Text = "Paciente demonstrou frustração com o atendimento", Sentiment = false },
                new SentimentData { Text = "Sinais de rejeição ao implante", Sentiment = false },
                new SentimentData { Text = "Paciente com dificuldades na fala após intervenção", Sentiment = false },
                new SentimentData { Text = "Consultas frequentes por causa de dor", Sentiment = false },
                new SentimentData { Text = "Paciente demonstrou medo de retornar", Sentiment = false },
                new SentimentData { Text = "Agravamento do quadro clínico pós-procedimento", Sentiment = false },
                new SentimentData { Text = "Paciente relatou sensação de formigamento constante", Sentiment = false },
                new SentimentData { Text = "Retardo na cicatrização", Sentiment = false },
                new SentimentData { Text = "Paciente não tolerou a anestesia", Sentiment = false },
                new SentimentData { Text = "Má adaptação da prótese", Sentiment = false },
                new SentimentData { Text = "Paciente expressou arrependimento com o tratamento", Sentiment = false },
                new SentimentData { Text = "Comprometimento da função mastigatória", Sentiment = false },
                new SentimentData { Text = "Paciente com infecção recorrente", Sentiment = false },
                new SentimentData { Text = "Queixa formal registrada pela família do paciente", Sentiment = false },
                new SentimentData { Text = "Paciente apresentou fortes dores após extração", Sentiment = false },
                new SentimentData { Text = "Consulta interrompida por complicações inesperadas", Sentiment = false },
                new SentimentData { Text = "Paciente relatou dormência prolongada", Sentiment = false },
                new SentimentData { Text = "Necessário reinício do tratamento devido a falhas", Sentiment = false },
                new SentimentData { Text = "Paciente expressou insegurança com o diagnóstico", Sentiment = false },
                new SentimentData { Text = "Problemas recorrentes relatados em consultas sucessivas", Sentiment = false },
                new SentimentData { Text = "Paciente não respondeu bem ao tratamento medicamentoso", Sentiment = false },
                new SentimentData { Text = "Forte sensibilidade dentária relatada", Sentiment = false },
                new SentimentData { Text = "Paciente ficou insatisfeito com a comunicação da equipe", Sentiment = false },
                new SentimentData { Text = "Reclamação registrada na ouvidoria", Sentiment = false },
                new SentimentData { Text = "Paciente precisou ser encaminhado para especialista", Sentiment = false },
                new SentimentData { Text = "Resultado final ficou aquém do esperado", Sentiment = false },
                new SentimentData { Text = "Paciente recusou continuidade do tratamento", Sentiment = false },
                new SentimentData { Text = "Desconforto persistente mesmo após ajustes", Sentiment = false },
                new SentimentData { Text = "Paciente relatou sensação de queimação", Sentiment = false },
                new SentimentData { Text = "Avaliação clínica indicou retrocesso no quadro", Sentiment = false },
                new SentimentData { Text = "Paciente exigiu reembolso pelos serviços", Sentiment = false },
                new SentimentData { Text = "Tratamento considerado ineficaz", Sentiment = false },
                new SentimentData { Text = "Paciente sentiu-se mal atendido", Sentiment = false },
                new SentimentData { Text = "Recidiva do problema odontológico", Sentiment = false },
                new SentimentData { Text = "Paciente apresentou febre no pós-operatório", Sentiment = false },
                new SentimentData { Text = "Paciente abandonou o tratamento", Sentiment = false },
                new SentimentData { Text = "Complicações exigiram intervenção de emergência", Sentiment = false },
                new SentimentData { Text = "Paciente expressou arrependimento ao final do processo", Sentiment = false },
                new SentimentData { Text = "Paciente perdeu a confiança na equipe clínica", Sentiment = false },
                new SentimentData { Text = "Paciente relatou dor intensa após o procedimento", Sentiment = false },
                new SentimentData { Text = "Consulta foi marcada por atrasos e falta de organização", Sentiment = false },
                new SentimentData { Text = "Paciente demonstrou insatisfação com os resultados obtidos", Sentiment = false },
                new SentimentData { Text = "Complicações pós-operatórias exigiram intervenção adicional", Sentiment = false },
                new SentimentData { Text = "Paciente expressou desconfiança em relação ao diagnóstico", Sentiment = false },
                new SentimentData { Text = "Evolução clínica apresentou retrocesso inesperado", Sentiment = false },
                new SentimentData { Text = "Paciente reclamou da falta de clareza nas orientações", Sentiment = false },
                new SentimentData { Text = "Tratamento não atingiu os objetivos propostos", Sentiment = false },
                new SentimentData { Text = "Paciente relatou sensação de abandono durante o processo", Sentiment = false },
                new SentimentData { Text = "Consulta foi interrompida devido a complicações", Sentiment = false },
                new SentimentData { Text = "Paciente demonstrou ansiedade em relação aos próximos passos", Sentiment = false },
                new SentimentData { Text = "Recuperação foi mais lenta do que o previsto", Sentiment = false },
                new SentimentData { Text = "Paciente expressou arrependimento por ter iniciado o tratamento", Sentiment = false },
                new SentimentData { Text = "Equipe médica não atendeu às expectativas do paciente", Sentiment = false },
                new SentimentData { Text = "Paciente relatou efeitos colaterais indesejados", Sentiment = false },
                new SentimentData { Text = "Consulta não proporcionou as respostas esperadas", Sentiment = false },
                new SentimentData { Text = "Paciente demonstrou frustração com a falta de resultados", Sentiment = false },
                new SentimentData { Text = "Tratamento foi interrompido por falta de progresso", Sentiment = false },
                new SentimentData { Text = "Paciente relatou desconforto persistente após o procedimento", Sentiment = false },
                new SentimentData { Text = "Consulta foi marcada por falhas na comunicação", Sentiment = false },
                new SentimentData { Text = "Paciente expressou insegurança quanto à continuidade do tratamento", Sentiment = false },
                new SentimentData { Text = "Recuperação apresentou complicações inesperadas", Sentiment = false },
                new SentimentData { Text = "Paciente demonstrou insatisfação com o atendimento recebido", Sentiment = false },
                new SentimentData { Text = "Tratamento não proporcionou alívio dos sintomas", Sentiment = false },
                new SentimentData { Text = "Paciente relatou sensação de negligência por parte da equipe", Sentiment = false },
                new SentimentData { Text = "Paciente reclamou de demora no atendimento", Sentiment = false },
                new SentimentData { Text = "Paciente relatou que ainda sente dor ao mastigar", Sentiment = false },
                new SentimentData { Text = "Paciente demonstrou resistência ao tratamento proposto", Sentiment = false },
                new SentimentData { Text = "Paciente não compareceu por medo do procedimento", Sentiment = false },
                new SentimentData { Text = "Paciente relatou não ter sido informado corretamente", Sentiment = false },
                new SentimentData { Text = "Paciente voltou insatisfeito com os resultados parciais", Sentiment = false },
                new SentimentData { Text = "Paciente expressou insatisfação com a abordagem clínica", Sentiment = false },
                new SentimentData { Text = "Paciente mencionou que o ambiente estava desconfortável", Sentiment = false },
                new SentimentData { Text = "Paciente reclamou de sangramento persistente", Sentiment = false },
                new SentimentData { Text = "Paciente questionou o valor cobrado pelo serviço", Sentiment = false },
                new SentimentData { Text = "Paciente abandonou o tratamento por desmotivação", Sentiment = false },
                new SentimentData { Text = "Paciente mostrou-se insatisfeito com os prazos de retorno", Sentiment = false },
                new SentimentData { Text = "Paciente expressou medo de complicações futuras", Sentiment = false },
                new SentimentData { Text = "Paciente relatou incômodo após uso do aparelho", Sentiment = false },
                new SentimentData { Text = "Paciente ficou insatisfeito com a explicação técnica", Sentiment = false },
                new SentimentData { Text = "Paciente não entendeu corretamente as instruções", Sentiment = false },
                new SentimentData { Text = "Paciente relatou piora no quadro após medicação", Sentiment = false },
                new SentimentData { Text = "Paciente reclamou do odor no ambiente clínico", Sentiment = false },
                new SentimentData { Text = "Paciente demonstrou cansaço com a frequência de consultas", Sentiment = false },
                new SentimentData { Text = "Paciente disse que não conseguiu dormir por causa da dor", Sentiment = false },
                new SentimentData { Text = "Paciente mostrou-se impaciente com o tempo de espera", Sentiment = false },
                new SentimentData { Text = "Paciente sentiu que não foi ouvido durante a consulta", Sentiment = false },
                new SentimentData { Text = "Paciente relatou falhas na comunicação com a equipe", Sentiment = false },
                new SentimentData { Text = "Paciente voltou com inflamação no local tratado", Sentiment = false },
                new SentimentData { Text = "Paciente demonstrou receio de continuar o acompanhamento", Sentiment = false },
            };

            // Carrega os dados de treinamento
            IDataView trainingDataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            // Pipeline para classificação binária com calibração de probabilidade
            var pipeline = _mlContext.Transforms.Text
                .FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(SentimentData.Text))
                .Append(_mlContext.BinaryClassification.Trainers.FastTree(
                    labelColumnName: "Label",
                    featureColumnName: "Features",
                    numberOfLeaves: 20,
                    numberOfTrees: 100,
                    minimumExampleCountPerLeaf: 5,
                    learningRate: 0.2))
                .Append(_mlContext.BinaryClassification.Calibrators.Platt(
                    labelColumnName: "Label",
                    scoreColumnName: "Score"));

            _logger.LogInformation("Iniciando treinamento do modelo...");

            // Treina o modelo
            _model = pipeline.Fit(trainingDataView);

            _logger.LogInformation("Modelo treinado com sucesso. Salvando...");

            // Salva o modelo para uso futuro
            _mlContext.Model.Save(_model, trainingDataView.Schema, _modelPath);

            // Cria o motor de previsão para uso nas análises
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model);

            _logger.LogInformation("Modelo de análise de sentimentos treinado e salvo com sucesso");

            // Teste básico para verificar se o modelo está funcionando
            await TestModelAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao treinar modelo de análise de sentimentos");
            throw;
        }
    }

    // Método para testar o modelo após o treinamento
    private async Task TestModelAsync()
    {
        try
        {
            var testCases = new[]
            {
                "Paciente muito satisfeito com resultado",
                "Complicações graves durante procedimento",
                "Excelente resposta ao tratamento",
                "Paciente com dor intensa"
            };

            _logger.LogInformation("Testando modelo treinado:");

            foreach (var testCase in testCases)
            {
                var result = AnalyzeSentiment(testCase);
                _logger.LogInformation($"Teste: '{testCase}' -> {result.SentimentCategory} (Confiança: {result.Confidence:F2})");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao testar modelo");
        }
    }

    // Método para carregar o modelo existente ou treinar um novo se não existir
    public async Task LoadModelAsync()
    {
        try
        {
            // FORÇAR RETREINAMENTO PARA CORRIGIR O MODELO
            // Remover essa condição após confirmar que está funcionando
            if (File.Exists(_modelPath))
            {
                _logger.LogInformation("Removendo modelo antigo para forçar retreinamento...");
                File.Delete(_modelPath);
            }

            _logger.LogInformation("Iniciando treinamento de novo modelo");
            await TrainAndSaveModelAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar modelo de análise de sentimentos");
            throw;
        }
    }

    // Método para analisar o sentimento de um texto
    public SentimentAnalysisResult AnalyzeSentiment(string text)
    {
        try
        {
            if (_predictionEngine == null)
            {
                throw new InvalidOperationException("O modelo de análise de sentimentos não foi carregado.");
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Texto não pode ser nulo ou vazio", nameof(text));
            }

            // Prepara os dados para previsão
            var sampleStatement = new SentimentData
            {
                Text = text.Trim()
            };

            // Faz a previsão
            var prediction = _predictionEngine.Predict(sampleStatement);

            // Para classificação binária, a confiança é baseada na probabilidade
            float confidence = Math.Max(prediction.Probability, 1 - prediction.Probability);

            // Log para debug
            _logger.LogDebug($"Análise: '{text}' -> Predição: {prediction.Prediction}, Score: {prediction.Score:F4}, Probabilidade: {prediction.Probability:F4}, Confiança: {confidence:F4}");

            // Retorna o resultado formatado
            return new SentimentAnalysisResult
            {
                Text = text,
                IsPositive = prediction.Prediction,
                Confidence = confidence
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao analisar sentimento do texto: {text}");
            throw;
        }
    }
}