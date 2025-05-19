using Auth.API.Services;
using Auth.API.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurações do banco de dados
var databaseSettings = new DatabaseSettings
{
    ConnectionString = builder.Configuration.GetSection("DatabaseSettings:ConnectionString").Value,
    DatabaseName = "OdontoprevAuth",
    UsersCollectionName = "Users"
};
builder.Services.AddSingleton(databaseSettings);

// Configurações de serviços
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<AuthService>(provider =>
{
    var userService = provider.GetRequiredService<UserService>();
    var jwtSecret = builder.Configuration["JwtSettings:Secret"];
    var jwtExpirationMinutes = int.Parse(builder.Configuration["JwtSettings:ExpirationMinutes"]);
    return new AuthService(userService, jwtSecret, jwtExpirationMinutes);
});

// Configuração do JWT
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
        ValidIssuer = "Auth.API",
        ValidateAudience = true,
        ValidAudience = "OdontoprevClients",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        // Adiciona mapeamento para ClaimTypes padrão
        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("Token inválido: " + context.Exception.ToString());
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            Console.WriteLine($"JWT Bearer: Mensagem recebida - {context.Request.Headers["Authorization"]}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"Token validado com sucesso para o usuário: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth.API", Version = "v1" });
    
    // Adiciona a configuração de segurança do JWT
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

var app = builder.Build();

// Adiciona middleware para verificar o header de autorização
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].ToString();
    if (!string.IsNullOrEmpty(authHeader))
    {
        Console.WriteLine($"Authorization header recebido: '{authHeader}'");
    }
    await next();
});

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// É importante que UseAuthentication venha antes do UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();