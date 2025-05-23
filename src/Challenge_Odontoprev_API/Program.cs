//Program.cs
using Challenge_Odontoprev_API.Infrastructure;
using Challenge_Odontoprev_API.MachineLearning;
using Challenge_Odontoprev_API.Mappings;
using Challenge_Odontoprev_API.Models;
using Challenge_Odontoprev_API.Repositories;
using Challenge_Odontoprev_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Adicionar servi�os ao cont�iner
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Configurar conex�o com o banco de dados Oracle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseOracle(
        builder.Configuration.GetConnectionString("OracleConnection")
    )
);

// Configurar HTTP Client Factory
builder.Services.AddHttpClient();
;

// Configurar AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Odontoprev API",
        Version = "v1",
        Description = "API para gerenciamento de clínicas odontológicas"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var jwtSecret = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT Secret is not configured.");
}

var key = Encoding.ASCII.GetBytes(jwtSecret);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = "Auth.API", // Deve corresponder ao Issuer definido na API de autenticação
        ValidateAudience = true,
        ValidAudience = "OdontoprevClients", // Deve corresponder ao Audience definido na API de autenticação
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    };

    // Log de eventos JWT para ajudar na depuração
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            Console.WriteLine($"Token validado com sucesso para o usuário: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Falha na autenticação: {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});

// Configuração de CORS para permitir requisições da sua aplicação frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configurar servi�os de neg�cio
builder.Services.AddScoped<_IService, _Service>();

// Configurar reposit�rios
builder.Services.AddScoped(typeof(_IRepository<>), typeof(_Repository<>));
builder.Services.AddScoped<_IRepository<Paciente>, _Repository<Paciente>>();
builder.Services.AddScoped<_IRepository<Dentista>, _Repository<Dentista>>();
builder.Services.AddScoped<_IRepository<Consulta>, _Repository<Consulta>>();
builder.Services.AddScoped<_IRepository<HistoricoConsulta>, _Repository<HistoricoConsulta>>();

// Configurar Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddSingleton<SentimentAnalysisService>();
builder.Services.AddScoped<IHistoricoAnalysisService, HistoricoAnalysisService>();

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var sentimentService = scope.ServiceProvider.GetRequiredService<SentimentAnalysisService>();

        Console.WriteLine("Inicializando modelo de análise de sentimentos...");
        await sentimentService.LoadModelAsync();
        Console.WriteLine("Modelo de análise de sentimentos inicializado com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERRO CRÍTICO: Falha ao inicializar modelo de ML: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        // Não pare a aplicação, mas registre o erro
    }
});

// Configurar o pipeline de requisi��es HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Adicionar endpoint de saúde para verificar se o modelo está funcionando
app.MapGet("/health/ml", async (SentimentAnalysisService sentimentService) =>
{
    try
    {
        var testResult = sentimentService.AnalyzeSentiment("Paciente muito satisfeito");
        return Results.Ok(new
        {
            Status = "OK",
            ModelLoaded = true,
            TestResult = testResult,
            Message = "Modelo de ML funcionando corretamente"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Erro no modelo de ML: {ex.Message}");
    }
});

app.Run();