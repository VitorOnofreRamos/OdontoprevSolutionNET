using Challenge_Odontoprev_API.MachineLearning;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Challenge_Odontoprev_API.Tests.MachineLearning
{
    /// <summary>
    /// Testes simples e diretos para análise de sentimentos
    /// </summary>
    public class SimpleSentimentTests
    {
        private readonly SentimentAnalysisService _service;

        public SimpleSentimentTests()
        {
            var mockLogger = new Mock<ILogger<SentimentAnalysisService>>();
            _service = new SentimentAnalysisService(mockLogger.Object);

            // Inicializar o modelo de forma síncrona para os testes
            _service.LoadModelAsync().GetAwaiter().GetResult();
        }

        [Fact]
        public void Should_Classify_Positive_Comment()
        {
            // Arrange
            var text = "Paciente muito satisfeito com resultado";

            // Act
            var result = _service.AnalyzeSentiment(text);

            // Assert
            Assert.True(result.IsPositive);
            Assert.Equal("Positivo", result.SentimentCategory);
            Assert.True(result.Confidence > 0.5f);
        }

        [Fact]
        public void Should_Classify_Negative_Comment()
        {
            // Arrange
            var text = "Paciente com dor intensa";

            // Act
            var result = _service.AnalyzeSentiment(text);

            // Assert
            Assert.False(result.IsPositive);
            Assert.Equal("Negativo", result.SentimentCategory);
            Assert.True(result.Confidence > 0.5f);
        }

        [Fact]
        public void Should_Classify_Pain_Comments_As_Negative()
        {
            // Arrange
            var painComments = new[]
            {
                "O paciente está com dor",
                "Paciente está com dor",
                "Está doendo muito"
            };

            foreach (var text in painComments)
            {
                // Act
                var result = _service.AnalyzeSentiment(text);

                // Assert
                Assert.False(result.IsPositive);
                Assert.True(result.Confidence > 0.5f);

                if (result.IsPositive)
                {
                    throw new Xunit.Sdk.XunitException($"'{text}' deveria ser negativo");
                }
                if (result.Confidence <= 0.5f)
                {
                    throw new Xunit.Sdk.XunitException($"Baixa confiança para '{text}': {result.Confidence}");
                }
            }
        }

        [Fact]
        public void Should_Classify_Pain_Relief_As_Positive()
        {
            // Arrange
            var reliefComments = new[]
            {
                "Paciente não sente mais dor",
                "Dor completamente eliminada",
                "Sem dor após procedimento"
            };

            foreach (var text in reliefComments)
            {
                // Act
                var result = _service.AnalyzeSentiment(text);

                // Assert
                Assert.True(result.IsPositive);
                Assert.True(result.Confidence > 0.5f);

                if (!result.IsPositive)
                {
                    throw new Xunit.Sdk.XunitException($"'{text}' deveria ser positivo");
                }
                if (result.Confidence <= 0.5f)
                {
                    throw new Xunit.Sdk.XunitException($"Baixa confiança para '{text}': {result.Confidence}");
                }
            }
        }

        [Fact]
        public void Should_Throw_Exception_For_Empty_Text()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.AnalyzeSentiment(""));
            Assert.Throws<ArgumentException>(() => _service.AnalyzeSentiment(null));
            Assert.Throws<ArgumentException>(() => _service.AnalyzeSentiment("   "));
        }

        [Fact]
        public void Should_Return_Consistent_Results()
        {
            // Arrange
            var text = "Excelente resposta ao tratamento";

            // Act - Executar múltiplas vezes
            var result1 = _service.AnalyzeSentiment(text);
            var result2 = _service.AnalyzeSentiment(text);
            var result3 = _service.AnalyzeSentiment(text);

            // Assert - Resultados devem ser idênticos
            Assert.Equal(result1.IsPositive, result2.IsPositive);
            Assert.Equal(result1.IsPositive, result3.IsPositive);
            Assert.Equal(result1.SentimentCategory, result2.SentimentCategory);
            Assert.Equal(result1.SentimentCategory, result3.SentimentCategory);
        }

        [Theory]
        [InlineData("Tratamento progredindo bem", true)]
        [InlineData("Procedimento realizado com sucesso", true)]
        [InlineData("Paciente satisfeito", true)]
        [InlineData("Complicações durante procedimento", false)]
        [InlineData("Tratamento não funcionou", false)]
        [InlineData("Paciente insatisfeito", false)]
        public void Should_Classify_Various_Comments_Correctly(string text, bool expectedPositive)
        {
            // Act
            var result = _service.AnalyzeSentiment(text);

            // Assert
            Assert.Equal(expectedPositive, result.IsPositive);

            if (expectedPositive != result.IsPositive)
            {
                throw new Xunit.Sdk.XunitException(
                    $"Texto: '{text}' - Esperado: {(expectedPositive ? "Positivo" : "Negativo")}, Obtido: {result.SentimentCategory}");
            }
        }
    }
}