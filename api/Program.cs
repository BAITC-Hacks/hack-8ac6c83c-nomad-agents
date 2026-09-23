using TaskForge.Api.Features.Actors;
using TaskForge.Api.Features.Health;
using TaskForge.Api.Features.Ai;
using TaskForge.Api.Features.Rating;
using TaskForge.Api.Features.Tasks;
using TaskForge.Api.Features.Catalog;
using TaskForge.Api.Infrastructure.OpenAi;
using TaskForge.Api.Infrastructure.InMemory;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:8080");

var corsOrigins = (builder.Configuration["CORS_ORIGINS"] ?? "http://localhost:5173")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(corsOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<InMemoryDataStore>();
builder.Services.AddSingleton<IActorRepository, ActorRepository>();
builder.Services.AddSingleton<ITaskRepository, TaskRepository>();
builder.Services.AddSingleton<IProposalRepository, ProposalRepository>();
builder.Services.AddSingleton<IAiLogRepository, AiLogRepository>();
builder.Services.AddSingleton<IStoreAdminRepository, StoreAdminRepository>();
builder.Services.AddSingleton<ActorGuard>();
builder.Services.AddHttpClient<ResponsesClient>(client => client.Timeout = TimeSpan.FromSeconds(20));
builder.Services.AddTransient<AnalysisService>();
builder.Services.AddSingleton<IRatingCacheRepository, RatingCacheRepository>();
builder.Services.AddTransient<RatingService>();
builder.Services.AddTransient<TaskService>();
builder.Services.AddTransient<CatalogService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "TaskForge API v1"));
}

app.UseExceptionHandler();
app.UseCors();
app.UseMiddleware<ActorResolverMiddleware>();

HealthEndpoints.MapHealthEndpoints(app);
ActorEndpoints.MapActorEndpoints(app);
app.MapTaskEndpoints();
app.MapCatalogEndpoints();

app.Run();
