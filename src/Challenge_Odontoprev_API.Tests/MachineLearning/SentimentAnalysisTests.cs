using Challenge_Odontoprev_API.MachineLearning;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Challenge_Odontoprev_API.Tests.MachineLearning
{
    /// <summary>
    /// Testes unitários para o serviço de análise de sentimentos
    /// </summary>
    public class SentimentAnalysisTests : IAsyncLifetime
    {
        private readonly SentimentAnalysisService _sentimentService;
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<SentimentAnalysisService>> _mockLogger;

        public SentimentAnalysisTests(ITestOutputHelper output)
        {
            _output = output;
            _mockLogger = new Mock<ILogger<SentimentAnalysisService>>();
            _sentimentService = new SentimentAnalysisService(_mockLogger.Object);
        }

        /// <summary>
        /// Inicialização assíncrona - treina o modelo antes dos testes
        /// </summary>
        public async Task InitializeAsync()
        {
            _output.WriteLine("Inicializando modelo para testes...");
            await _sentimentService.LoadModelAsync();
            _output.WriteLine("Modelo inicializado com sucesso!");
        }

        /// <summary>
        /// Limpeza após os testes
        /// </summary>
        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        #region Testes de Sentimentos Positivos

        [Fact]
        public void AnalyzeSentiment_PositiveComment_ShouldReturnPositive()
        {
            // Arrange
            var text = "Paciente muito satisfeito com o resultado";

            // Act
            var result = _sentimentService.AnalyzeSentiment(text);

            // Assert
            Assert.True(result.IsPositive);
            if (!result.IsPositive)
            {
                _output.WriteLine($"ERRO: Esperado Positivo, obtido {result.SentimentCategory}");
            }
            Assert.Equal("Positivo", result.SentimentCategory);
            Assert.True(result.Confidence > 0.5f, $"Confiança muito baixa: {result.Confidence}");
            Assert.Equal(text, result.Text);

            _output.WriteLine($"✅ '{text}' -> {result.SentimentCategory} (Confiança: {result.Confidence:F2})");
        }

        [Theory]
        [InlineData("Excelente resposta ao tratamento")]
        [InlineData("Paciente não sente mais dor")]
        [InlineData("Procedimento realizado com sucesso")]
        [InlineData("Cicatrização perfeita")]
        [InlineData("Resultado excepcional")]
        public void AnalyzeSentiment_MultiplePositiveComments_ShouldReturnPositive(string text)
        {
            // Act
            var result = _sentimentService.AnalyzeSentiment(text);

            // Assert
            Assert.True(result.IsPositive);
            if (!result.IsPositive)
            {
                _output.WriteLine($"ERRO: Texto '{text}' deveria ser positivo, mas foi {result.SentimentCategory}");
            }
            Assert.True(result.Confidence > 0.5f, $"Confiança muito baixa para '{text}': {result.Confidence}");

            _output.WriteLine($"✅ '{text}' -> {result.SentimentCategory} (Confiança: {result.Confidence:F2})");
        }

        #endregion

        #region Testes de Sentimentos Negativos

        [Fact]
        public void AnalyzeSentiment_NegativeComment_ShouldReturnNegative()
        {
            // Arrange
            var text = "Paciente com dor persistente e intensa";

            // Act
            var result = _sentimentService.AnalyzeSentiment(text);

            // Assert
            Assert.False(result.IsPositive);
            if (result.IsPositive)
            {
                _output.WriteLine($"ERRO: Esperado Negativo, obtido {result.SentimentCategory}");
            }
            Assert.Equal("Negativo", result.SentimentCategory);
            Assert.True(result.Confidence > 0.5f, $"Confiança muito baixa: {result.Confidence}");

            _output.WriteLine($"❌ '{text}' -> {result.SentimentCategory} (Confiança: {result.Confidence:F2})");
        }

        [Theory]
        [InlineData("O paciente está com dor")]
        [InlineData("Paciente está com dor")]
        [InlineData("Complicações graves durante procedimento")]
        [InlineData("Tratamento não funcionou")]
        [InlineData("Paciente muito insatisfeito")]
        [InlineData("Está doendo muito")]
        public void AnalyzeSentiment_MultipleNegativeComments_ShouldReturnNegative(string text)
        {
            // Act
            var result = _sentimentService.AnalyzeSentiment(text);

            // Assert
            Assert.False(result.IsPositive);
            if (result.IsPositive)
            {
                _output.WriteLine($"ERRO: Texto '{text}' deveria ser negativo, mas foi {result.SentimentCategory}");
            }
            Assert.True(result.Confidence > 0.5f, $"Confiança muito baixa para '{text}': {result.Confidence}");

            _output.WriteLine($"❌ '{text}' -> {result.SentimentCategory} (Confiança: {result.Confidence:F2})");
        }

        #endregion

        #region Testes de Casos Específicos com Dor

        [Theory]
        [InlineData("Paciente com dor", false)] // Negativo
        [InlineData("O paciente está com dor", false)] // Negativo  
        [InlineData("Paciente não sente mais dor", true)] // Positivo
        [InlineData("Dor completamente eliminada", true)] // Positivo
        [InlineData("Sem dor após procedimento", true)] // Positivo
        [InlineData("Paciente reclama de dor", false)] // Negativo
        public void AnalyzeSentiment_PainRelatedComments_ShouldClassifyCorrectly(string text, bool expectedPositive)
        {
            // Act
            var result = _sentimentService.AnalyzeSentiment(text);

            // Assert
            Assert.Equal(expectedPositive, result.IsPositive);

            // Mensagem de erro personalizada se falhar
            if (expectedPositive != result.IsPositive)
            {
                throw new Xunit.Sdk.XunitException(
                    $"Texto '{text}' deveria ser {(expectedPositive ? "Positivo" : "Negativo")}, mas foi {result.SentimentCategory}");
            }
            Assert.True(result.Confidence > 0.5f, $"Confiança muito baixa: {result.Confidence}");

            var emoji = expectedPositive ? "✅" : "❌";
            _output.WriteLine($"{emoji} '{text}' -> {result.SentimentCategory} (Confiança: {result.Confidence:F2})");
        }

        #endregion

        #region Testes de Validação de Entrada

        [Fact]
        public void AnalyzeSentiment_NullText_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _sentimentService.AnalyzeSentiment(null));
            Assert.Contains("Texto não pode ser nulo ou vazio", exception.Message);
        }

        [Fact]
        public void AnalyzeSentiment_EmptyText_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _sentimentService.AnalyzeSentiment(""));
            Assert.Contains("Texto não pode ser nulo ou vazio", exception.Message);
        }

        [Fact]
        public void AnalyzeSentiment_WhitespaceText_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _sentimentService.AnalyzeSentiment("   "));
            Assert.Contains("Texto não pode ser nulo ou vazio", exception.Message);
        }

        #endregion

        #region Testes de Confiança

        [Fact]
        public void AnalyzeSentiment_ClearPositiveStatement_ShouldHaveHighConfidence()
        {
            // Arrange
            var text = "Paciente extremamente satisfeito, resultado perfeito";

            // Act
            var result = _sentimentService.AnalyzeSentiment(text);

            // Assert
            Assert.True(result.IsPositive);
            Assert.True(result.Confidence > 0.7f, $"Confiança deveria ser alta (>0.7) para texto claro, mas foi {result.Confidence}");

            _output.WriteLine($"📊 Confiança Alta: '{text}' -> {result.SentimentCategory} (Confiança: {result.Confidence:F2})");
        }

        [Fact]
        public void AnalyzeSentiment_ClearNegativeStatement_ShouldHaveHighConfidence()
        {
            // Arrange
            var text = "Paciente com dor intensa, muito insatisfeito";

            // Act
            var result = _sentimentService.AnalyzeSentiment(text);

            // Assert
            Assert.False(result.IsPositive);
            Assert.True(result.Confidence > 0.7f);
            if (result.IsPositive || result.Confidence <= 0.7f)
            {
                _output.WriteLine($"ERRO: Deveria ser negativo com alta confiança, obtido: {result.SentimentCategory} ({result.Confidence})");
            }

            _output.WriteLine($"📊 Confiança Alta: '{text}' -> {result.SentimentCategory} (Confiança: {result.Confidence:F2})");
        }

        #endregion

        #region Testes de Robustez

        [Theory]
        [InlineData("PACIENTE MUITO SATISFEITO")] // Maiúsculas
        [InlineData("paciente muito satisfeito")] // Minúsculas
        [InlineData("  Paciente muito satisfeito  ")] // Espaços extras
        [InlineData("Paciente... muito satisfeito!")] // Pontuação
        public void AnalyzeSentiment_TextVariations_ShouldMaintainClassification(string text)
        {
            // Act
            var result = _sentimentService.AnalyzeSentiment(text);

            // Assert
            Assert.True(result.IsPositive);
            if (!result.IsPositive)
            {
                _output.WriteLine($"ERRO: Variação '{text}' deveria ser positiva");
            }
            Assert.True(result.Confidence > 0.5f);

            _output.WriteLine($"🔄 Variação: '{text}' -> {result.SentimentCategory} (Confiança: {result.Confidence:F2})");
        }

        #endregion

        #region Teste de Performance

        [Fact]
        public void AnalyzeSentiment_MultipleAnalyses_ShouldBeConsistent()
        {
            // Arrange
            var text = "Paciente satisfeito com o tratamento";
            var results = new List<SentimentAnalysisResult>();

            // Act - Analisar múltiplas vezes
            for (int i = 0; i < 5; i++)
            {
                results.Add(_sentimentService.AnalyzeSentiment(text));
            }

            // Assert - Resultados devem ser consistentes
            var firstResult = results.First();
            foreach (var result in results)
            {
                Assert.Equal(firstResult.IsPositive, result.IsPositive);
                Assert.Equal(firstResult.SentimentCategory, result.SentimentCategory);
                // Confiança pode variar ligeiramente, mas deve estar próxima
                var confidenceDiff = Math.Abs(firstResult.Confidence - result.Confidence);
                Assert.True(confidenceDiff < 0.01f);
                if (confidenceDiff >= 0.01f)
                {
                    _output.WriteLine($"ERRO: Confiança inconsistente - diferença: {confidenceDiff}");
                }
            }

            _output.WriteLine($"🔁 Consistência: '{text}' -> {firstResult.SentimentCategory} (5 execuções idênticas)");
        }

        #endregion

        #region Teste de Cobertura Geral

        [Fact]
        public void AnalyzeSentiment_ModelCoverage_ShouldClassifyVariousScenarios()
        {
            // Arrange - Cenários diversos do contexto odontológico
            var testCases = new Dictionary<string, bool>
            {
                // Positivos
                { "Tratamento concluído com sucesso", true },
                { "Paciente sem queixas", true },
                { "Resultado estético excelente", true },
                { "Rápida recuperação", true },
                
                // Negativos  
                { "Necessário refazer procedimento", false },
                { "Paciente insatisfeito", false },
                { "Complicação pós-operatória", false },
                { "Resultado abaixo do esperado", false }
            };

            int correctClassifications = 0;
            int totalTests = testCases.Count;

            // Act & Assert
            foreach (var testCase in testCases)
            {
                var result = _sentimentService.AnalyzeSentiment(testCase.Key);
                var isCorrect = result.IsPositive == testCase.Value;

                if (isCorrect) correctClassifications++;

                var status = isCorrect ? "✅" : "❌";
                var expected = testCase.Value ? "Positivo" : "Negativo";

                _output.WriteLine($"{status} '{testCase.Key}' -> Esperado: {expected}, Obtido: {result.SentimentCategory}");

                Assert.Equal(testCase.Value, result.IsPositive);
                if (testCase.Value != result.IsPositive)
                {
                    _output.WriteLine($"ERRO: Classificação incorreta para '{testCase.Key}'");
                }
            }

            var accuracy = (double)correctClassifications / totalTests;
            _output.WriteLine($"📈 Acurácia Geral: {accuracy:P1} ({correctClassifications}/{totalTests})");

            // Esperar pelo menos 80% de acurácia
            Assert.True(accuracy >= 0.8);
            if (accuracy < 0.8)
            {
                _output.WriteLine($"ERRO: Acurácia muito baixa: {accuracy:P1}");
            }
        }

        #endregion
    }
}