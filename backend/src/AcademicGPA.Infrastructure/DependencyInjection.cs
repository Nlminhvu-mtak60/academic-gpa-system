using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Domain.Interfaces;
using AcademicGPA.Infrastructure.Persistence;
using AcademicGPA.Infrastructure.Persistence.Repositories;
using AcademicGPA.Infrastructure.Services;
using AcademicGPA.Infrastructure.Services.TranscriptImport;
using AcademicGPA.Infrastructure.Services.TranscriptImport.Parsers;
using AcademicGPA.Application.Features.TranscriptImport.Interfaces;
using AcademicGPA.Infrastructure.AI.Gemini;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicGPA.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register PostgreSQL DB Context
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Port=5432;Database=AcademicGPA;Username=postgres;Password=postgres";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register HttpContext Accessor
        services.AddHttpContextAccessor();

        // Register Core Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IGpaCalculator, GpaCalculator>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IStatisticsService, StatisticsService>();

        // Transcript Import Services
        services.AddScoped<IUniversityDetector, UniversityDetectorService>();
        services.AddScoped<ITranscriptImporter, ExcelTranscriptImporter>();
        services.AddScoped<IGeminiVisionService, GeminiVisionService>();
        services.AddScoped<PdfTranscriptImporter>();
        services.AddScoped<ImageOcrTranscriptImporter>();
        services.AddScoped<TextTranscriptImporter>();
        services.AddScoped<IUniversityParser, GenericParser>();
        services.AddScoped<IUniversityParser, BachKhoaParser>();
        services.AddScoped<IExportService, AcademicGPA.Infrastructure.Services.Export.ExportService>();
        
        // Register transcript importers by source type
        services.AddKeyedScoped<ITranscriptImporter>("Excel", (sp, key) => sp.GetRequiredService<ExcelTranscriptImporter>());
        services.AddKeyedScoped<ITranscriptImporter>("Pdf", (sp, key) => sp.GetRequiredService<PdfTranscriptImporter>());
        services.AddKeyedScoped<ITranscriptImporter>("ImageOcr", (sp, key) => sp.GetRequiredService<ImageOcrTranscriptImporter>());
        services.AddKeyedScoped<ITranscriptImporter>("Text", (sp, key) => sp.GetRequiredService<TextTranscriptImporter>());

        services.AddScoped<IPredictionService, PredictionService>();
        services.AddScoped<IGoalPlannerService, GoalPlannerService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IUserSettingsService, UserSettingsService>();
        services.AddScoped<IAdminService, AdminService>();

        // Register HttpClient for Google Auth API Calls
        services.AddHttpClient<IGoogleAuthService, GoogleAuthService>();

        // Register HttpClient for AI Advisor Service
        services.AddHttpClient<IAiAdvisorServiceClient, AiAdvisorServiceClient>((sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var baseUrl = config["AiService:BaseUrl"] ?? "http://localhost:8000";
            var apiKey = config["AiService:ApiKey"] ?? "SuperSecretAiAdvisorApiKey123!";
            
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        });

        return services;
    }
}
