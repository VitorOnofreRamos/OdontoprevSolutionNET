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

    public float Score { get; set; }
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

            // Dados de treinamento simples
            var trainingData = new List<SentimentData>
            {
                new SentimentData { Text = "Paciente apresentou melhora significativa", Sentiment = true },
                new SentimentData { Text = "Tratamento progredindo bem", Sentiment = true },
                new SentimentData { Text = "Excelente resposta ao tratamento", Sentiment = true },
                new SentimentData { Text = "Paciente satisfeito com o resultado", Sentiment = true },
                new SentimentData { Text = "Recuperação acima do esperado", Sentiment = true },
                new SentimentData { Text = "Sem complicações pós-procedimento", Sentiment = true },
                new SentimentData { Text = "Paciente relatou ausência de dor", Sentiment = true },
                new SentimentData { Text = "Procedimento realizado com sucesso", Sentiment = true },
                new SentimentData { Text = "Bom estado de saúde bucal", Sentiment = true },
                new SentimentData { Text = "Resultado estético satisfatório", Sentiment = true },
                new SentimentData { Text = "Paciente com dor persistente", Sentiment = false },
                new SentimentData { Text = "Tratamento sem progresso significativo", Sentiment = false },
                new SentimentData { Text = "Complicações durante procedimento", Sentiment = false },
                new SentimentData { Text = "Paciente insatisfeito com resultado", Sentiment = false },
                new SentimentData { Text = "Inflamação persistente", Sentiment = false },
                new SentimentData { Text = "Dificuldade de adaptação à prótese", Sentiment = false },
                new SentimentData { Text = "Necessidade de refazer procedimento", Sentiment = false },
                new SentimentData { Text = "Paciente relatou desconforto", Sentiment = false },
                new SentimentData { Text = "Resposta negativa ao tratamento", Sentiment = false },
                new SentimentData { Text = "Infecção pós-operatória", Sentiment = false },
            };

            // Carrega os dados de treinamento
            IDataView trainingDataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            // Pipeline para processamento de dados e treinamento
            var pipeline = _mlContext.Transforms.Text
                .FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(SentimentData.Text))
                .Append(_mlContext.BinaryClassification.Trainers.FastTree(
                    numberOfLeaves: 50,
                    numberOfTrees: 50,
                    minimumExampleCountPerLeaf: 20,
                    learningRate: 0.2));

            // Treina o modelo
            _model = pipeline.Fit(trainingDataView);

            // Salva o modelo para uso futuro
            _mlContext.Model.Save(_model, trainingDataView.Schema, _modelPath);

            // Cria o motor de previsão para uso nas análises
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model);

            _logger.LogInformation("Modelo de análise de sentimentos treinado e salvo com sucesso");
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Erro ao treinar modelo de análise de sentimentos");
            throw;
        }
    }

    // Método para carregar o modelo existente ou treinar um novo se não existir
    public async Task LoadModelAsync()
    {
        try
        {
            if (File.Exists(_modelPath))
            {
                _logger.LogInformation("Carregando modelo de análise de sentimentos existente");
                _model = _mlContext.Model.Load(_modelPath, out var modelSchema);
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model);
                _logger.LogInformation("Modelo carregado com sucesso");
            }
            else
            {
                _logger.LogInformation("Modelo não encontrado. Iniciando treinamento de novo modelo");
                await TrainAndSaveModelAsync();
            }
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

            // Prepara os dados para previsão
            var sampleStatement = new SentimentData
            {
                Text = text
            };

            // Faz a previsão
            var prediction = _predictionEngine.Predict(sampleStatement);

            // Retorna o resultado formatado
            return new SentimentAnalysisResult
            {
                Text = text,
                IsPositive = prediction.Prediction,
                Confidence = Math.Abs(prediction.Score)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao analisar sentimento do texto: {text}");
            throw;
        }
    }
}