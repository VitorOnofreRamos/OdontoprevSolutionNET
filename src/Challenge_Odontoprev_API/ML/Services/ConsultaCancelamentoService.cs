using Microsoft.ML;
using Microsoft.ML.Data;
using Challenge_Odontoprev_API.ML.Models;
using Challenge_Odontoprev_API.Models;
using Challenge_Odontoprev_API.Repositories;
using System.Globalization;

namespace Challenge_Odontoprev_API.ML.Services;

public class ConsultaCancelamentoService
{
    private readonly MLContext _mlContext;
    private ITransformer _trainedModel;
    private readonly string _modelPath;
    private readonly _IRepository<Consulta> _consultaRepository;
    private readonly _IRepository<Paciente> _pacienteRepository;
    private readonly _IRepository<HistoricoConsulta> _historicoRepository;
    private readonly ILogger<ConsultaCancelamentoService> _logger;

    public ConsultaCancelamentoService(
        _Repository<Consulta> consultaRepository,
        _Repository<Paciente> pacienteRepository,
        _Repository<HistoricoConsulta> historicoRepository,
        ILogger<ConsultaCancelamentoService> logger)
    {
        _mlContext = new MLContext(seed: 6969);
        _modelPath = Path.Combine(AppContext.BaseDirectory, "ML/Models/consulta_cancelamento_model.zip");
        _consultaRepository = consultaRepository;
        _pacienteRepository = pacienteRepository;
        _historicoRepository = historicoRepository;
        _logger = logger;

        // Tenta carregar um modelo já existente
        if (File.Exists(_modelPath))
        {
            LoadModel();
        }
    }

    public async Task<bool> TrainModel()
    {
        try
        {
            // Obter dados de treinamento
            var trainingData = await PrepareTrainingData();
            
            if (trainingData.Count() < 50)
            {
                _logger.LogWarning("Dados insuficientes para treinamento. Mínimo 50 registros necessários.");
                return false;
            }

            // Converter para IDataView
            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            // Pipeline de treinamento
            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding(
                    outputColumnName: "DiaDaSemanaEncoded", 
                    inputColumnName: "DiaDaSemana")
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(
                    outputColumnName: "HoraDoDiaEncoded", 
                    inputColumnName: "HoraDoDia"))
                .Append(_mlContext.Transforms.Concatenate(
                    "Features", 
                    "DiasAteConsulta", "IdadeEmAnos", "NumeroConsultasPrevias", 
                    "NumeroCancelamentosPrevios", "DiaDaSemanaEncoded", "HoraDoDiaEncoded",
                    "JaFezTratamentoAnterior", "PrimeiraConsulta", "FoiFeriado"))
                .Append(_mlContext.BinaryClassification.Trainers.FastTree(
                    labelColumnName: "Cancelado",
                    featureColumnName: "Features",
                    numberOfLeaves: 20,
                    numberOfTrees: 100,
                    minimumExampleCountPerLeaf: 10));

            // Treinar modelo
            _trainedModel = pipeline.Fit(dataView);

            // Avaliar modelo
            var predictions = _trainedModel.Transform(dataView);
            var metrics = _mlContext.BinaryClassification.Evaluate(predictions);
            _logger.LogInformation($"Acurácia do modelo: {metrics.Accuracy:P2}");
            _logger.LogInformation($"AUC: {metrics.AreaUnderRocCurve:P2}");
            _logger.LogInformation($"F1 Score: {metrics.F1Score:P2}");
            
            // Salvar modelo
            _mlContext.Model.Save(_trainedModel, dataView.Schema, _modelPath);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao treinar modelo de ML");
            return false;
        }
    }

    public ConsultaCancelamentoPrediction PredictCancelamento(ConsultaCancelamentoData consultaData)
    {
        // Verifica se o modelo está carregado
        if (_trainedModel == null)
        {
            if (!LoadModel())
            {
                throw new InvalidOperationException("Modelo de ML não está disponível. Execute o treinamento primeiro.");
            }
        }

        // Criar engine de predição
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<ConsultaCancelamentoData, ConsultaCancelamentoPrediction>(_trainedModel);
        
        // Fazer predição
        return predictionEngine.Predict(consultaData);
    }

    public async Task<ConsultaCancelamentoData> PrepareConsultaData(Consulta consulta)
    {
        try
        {
            var paciente = await _pacienteRepository.GetById(consulta.ID_Paciente);
            var historicoConsultas = await _historicoRepository.GetAll();
            var consultas = await _consultaRepository.GetAll();

            var consultasPaciente = consultas.Where(c => c.ID_Paciente == consulta.ID_Paciente).ToList();
            var historicoPaciente = historicoConsultas
                .Where(h => consultasPaciente.Any(c => c.Id == h.ID_Consulta))
                .ToList();

            var dataConsulta = consulta.Data_Consulta;
            var dataNascimento = paciente.Data_Nascimento;
            var idade = CalcularIdade(dataNascimento, dataConsulta);
            
            var diasAteConsulta = (dataConsulta - DateTime.Now).Days;
            var numeroConsultasPrevias = consultasPaciente.Count;
            var numeroCancelamentosPrevios = consultasPaciente.Count(c => c.Status == "Cancelado");
            var jaFezTratamento = historicoPaciente.Any();
            var primeiraConsulta = numeroConsultasPrevias == 0;

            // Verificar se é feriado (implementação simplificada)
            var foiFeriado = EhFeriado(dataConsulta);

            return new ConsultaCancelamentoData
            {
                DiasAteConsulta = diasAteConsulta,
                IdadeEmAnos = idade,
                NumeroConsultasPrevias = numeroConsultasPrevias,
                NumeroCancelamentosPrevios = numeroCancelamentosPrevios,
                HoraDoDia = dataConsulta.Hour,
                DiaDaSemana = (float)dataConsulta.DayOfWeek,
                JaFezTratamentoAnterior = jaFezTratamento,
                PrimeiraConsulta = primeiraConsulta,
                FoiFeriado = foiFeriado,
                Cancelado = false // Valor padrão para predição
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao preparar dados da consulta para ML");
            throw;
        }
    }

    private bool LoadModel()
    {
        try
        {
            _trainedModel = _mlContext.Model.Load(_modelPath, out _);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar modelo de ML");
            return false;
        }
    }

    private async Task<IEnumerable<ConsultaCancelamentoData>> PrepareTrainingData()
    {
        // Obter todas as consultas e pacientes
        var consultas = await _consultaRepository.GetAll();
        var pacientes = (await _pacienteRepository.GetAll()).ToDictionary(p => p.Id);
        var historicoConsultas = await _historicoRepository.GetAll();
        
        var trainingData = new List<ConsultaCancelamentoData>();
        
        foreach (var consulta in consultas)
        {
            // Pular consultas sem status definido ou futuras
            if (string.IsNullOrEmpty(consulta.Status) || consulta.Data_Consulta > DateTime.Now)
                continue;

            if (!pacientes.TryGetValue(consulta.ID_Paciente, out var paciente))
                continue;

            var consultasPaciente = consultas
                .Where(c => c.ID_Paciente == consulta.ID_Paciente && c.Id != consulta.Id)
                .ToList();
            
            var dataConsulta = consulta.Data_Consulta;
            var dataNascimento = paciente.Data_Nascimento;
            var idade = CalcularIdade(dataNascimento, dataConsulta);
            
            var numeroConsultasPrevias = consultasPaciente.Count;
            var numeroCancelamentosPrevios = consultasPaciente.Count(c => c.Status == "Cancelado");
            
            var historicoPaciente = historicoConsultas
                .Where(h => consultasPaciente.Any(c => c.Id == h.ID_Consulta))
                .ToList();
            
            var jaFezTratamento = historicoPaciente.Any();
            var primeiraConsulta = numeroConsultasPrevias == 0;
            var foiFeriado = EhFeriado(dataConsulta);
            var cancelado = consulta.Status == "Cancelado";
            
            // Criar dados de treinamento
            var trainingItem = new ConsultaCancelamentoData
            {
                DiasAteConsulta = 7, // Valor padrão para consultas passadas
                IdadeEmAnos = idade,
                NumeroConsultasPrevias = numeroConsultasPrevias,
                NumeroCancelamentosPrevios = numeroCancelamentosPrevios,
                HoraDoDia = dataConsulta.Hour,
                DiaDaSemana = (float)dataConsulta.DayOfWeek,
                JaFezTratamentoAnterior = jaFezTratamento,
                PrimeiraConsulta = primeiraConsulta,
                FoiFeriado = foiFeriado,
                Cancelado = cancelado
            };
            
            trainingData.Add(trainingItem);
        }
        
        return trainingData;
    }
    
    private float CalcularIdade(DateTime dataNascimento, DateTime dataReferencia)
    {
        int idade = dataReferencia.Year - dataNascimento.Year;
        
        if (dataReferencia.Month < dataNascimento.Month || 
            (dataReferencia.Month == dataNascimento.Month && 
             dataReferencia.Day < dataNascimento.Day))
        {
            idade--;
        }
        
        return idade;
    }
    
    private bool EhFeriado(DateTime data)
    {
        // Implementação simplificada para verificar feriados
        // Feriados nacionais fixos
        var feriados = new List<(int mes, int dia)>
        {
            (1, 1),   // Ano Novo
            (4, 21),  // Tiradentes
            (5, 1),   // Dia do Trabalho
            (9, 7),   // Independência
            (10, 12), // Nossa Senhora Aparecida
            (11, 2),  // Finados
            (11, 15), // Proclamação da República
            (12, 25), // Natal
        };
        
        return feriados.Any(f => f.mes == data.Month && f.dia == data.Day);
    }
}