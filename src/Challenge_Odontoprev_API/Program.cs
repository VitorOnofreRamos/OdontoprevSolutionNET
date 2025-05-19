// Challenge_Odontoprev_API/Program.cs
using Challenge_Odontoprev_API;
using Challenge_Odontoprev_API.Infrastructure;
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

var app = builder.Build();

// Configurar o pipeline de requisi��es HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.MapControllers();

app.Run();