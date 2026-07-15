using System.Text;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using AuthService.Configuration;
using AuthService.Infrastructure;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// JWT
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt")
);

var jwtOptions = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing."
    );

// DynamoDB
builder.Services.Configure<DynamoDbOptions>(
    builder.Configuration.GetSection("DynamoDb")
);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("UnityWebClient", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtOptions.Key
                        )
                    )
            };
    });

builder.Services.AddAuthorization();

// DynamoDB client
builder.Services.AddSingleton<IAmazonDynamoDB>(
    serviceProvider =>
    {
        var options = serviceProvider
            .GetRequiredService<
                Microsoft.Extensions.Options.IOptions<
                    DynamoDbOptions
                >
            >()
            .Value;

        if (options.UseLocalMode)
        {
            var credentials = new BasicAWSCredentials(
                "local",
                "local"
            );

            return new AmazonDynamoDBClient(
                credentials,
                new AmazonDynamoDBConfig
                {
                    ServiceURL = options.ServiceUrl
                }
            );
        }

        // En AWS usará el IAM Role de la instancia EC2.
        return new AmazonDynamoDBClient();
    }
);

// Repositories and services
builder.Services.AddScoped<
    IUserRepository,
    DynamoDbUserRepository
>();

builder.Services.AddScoped<
    IAuthService,
    AuthService.Services.AuthService
>();

builder.Services.AddSingleton<
    IJwtTokenService,
    JwtTokenService
>();

builder.Services.AddSingleton<
    IPasswordHasher<UserAccount>,
    PasswordHasher<UserAccount>
>();

builder.Services.AddSingleton<
    DynamoDbTableInitializer
>();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Crear la tabla local si todavía no existe.
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider
        .GetRequiredService<
            DynamoDbTableInitializer
        >();

    await initializer.InitializeAsync();
}

app.UseCors("UnityWebClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.Run();