using Google.Cloud.Firestore;
using TaskForge.Api.Infrastructure.Firestore;
using TaskForge.Api.Infrastructure.OpenAi;
using TaskForge.Api.Features.Actors;
using TaskForge.Api.Features.Tasks;
using TaskForge.Api.Features.Ai;
using TaskForge.Api.Features.Rating;
using TaskForge.Api.Features.Catalog;
using TaskForge.Api.Features.Proposals;
using TaskForge.Api.Features.Admin;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var config = builder.Configuration;
var gcpProjectId = config["GCP_PROJECT_ID"] ?? "taskforge-local";
var corsOrigins = config["CORS_ORIGINS"] ?? "http://localhost:5173";
var aiMode = config["AI_MODE"] ?? "live";

// Services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries);
        policy
            .WithOrigins(origins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddOpenApi();

// Firestore
builder.Services.AddSingleton(FirestoreDb.Create(gcpProjectId));

// Repositories
builder.Services.AddScoped<BusinessRepository>();
builder.Services.AddScoped<TeamRepository>();
builder.Services.AddScoped<TaskRepository>();
builder.Services.AddScoped<ProposalRepository>();
builder.Services.AddScoped<AiLogRepository>();

// OpenAI
builder.Services.AddHttpClient<ResponsesClient>();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();

// Health
app.MapGet("/api/health", () => new { status = "ok", aiMode = config["AI_MODE"] ?? "live", firestore = "ok" })
    .WithName("GetHealth")
    .WithOpenApi();

// Feature endpoints
ActorsEndpoints.MapActorEndpoints(app);
TasksEndpoints.MapTaskEndpoints(app);
AiEndpoints.MapAiEndpoints(app);
RatingEndpoints.MapRatingEndpoints(app);
CatalogEndpoints.MapCatalogEndpoints(app);
ProposalsEndpoints.MapProposalEndpoints(app);
AdminEndpoints.MapAdminEndpoints(app);

app.Run();
