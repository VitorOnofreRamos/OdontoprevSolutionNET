// Challenge_Odontoprev_API/Program.cs
using Challenge_Odontoprev_API.Auth;
using Challenge_Odontoprev_API.Infrastructure;
using Challenge_Odontoprev_API.Mappings;
using Challenge_Odontoprev_API.Models;
using Challenge_Odontoprev_API.Repositories;
using Challenge_Odontoprev_API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços ao contêiner
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Configurar conexão com o banco de dados Oracle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseOracle(
        builder.Configuration.GetConnectionString("OracleConnection")
    )
);

// Configurar HTTP Client Factory
builder.Services.AddHttpClient();

// Configurar serviço de autenticação
builder.Services.AddScoped<AuthService>();

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

    // Configurar o Swagger para usar o JWT Bearer token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            Array.Empty<string>()
        }
    });
});

// Configurar serviços de negócio
builder.Services.AddScoped<_IService, _Service>();

// Configurar repositórios
builder.Services.AddScoped(typeof(_IRepository<>), typeof(_Repository<>));
builder.Services.AddScoped<_IRepository<Paciente>, _Repository<Paciente>>();
builder.Services.AddScoped<_IRepository<Dentista>, _Repository<Dentista>>();
builder.Services.AddScoped<_IRepository<Consulta>, _Repository<Consulta>>();
builder.Services.AddScoped<_IRepository<HistoricoConsulta>, _Repository<HistoricoConsulta>>();

// Configurar Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

// Configurar o pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Usar middleware de autenticação JWT personalizado antes do middleware de autorização
app.UseJwtMiddleware();

app.UseAuthorization();

app.MapControllers();

app.Run();