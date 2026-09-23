namespace TaskForge.Api.Features.Tasks;

public static class TasksEndpoints
{
    public static void MapTaskEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tasks");

        group.MapPost("", CreateTask)
            .WithName("CreateTask")
            .WithOpenApi();

        group.MapGet("mine", GetMyTasks)
            .WithName("GetMyTasks")
            .WithOpenApi();

        group.MapGet("{id}", GetTask)
            .WithName("GetTask")
            .WithOpenApi();

        group.MapPost("{id}/analyze", Analyze)
            .WithName("AnalyzeTask")
            .WithOpenApi();

        group.MapPut("{id}/answers", UpdateAnswers)
            .WithName("UpdateAnswers")
            .WithOpenApi();

        group.MapPut("{id}/fields", UpdateFields)
            .WithName("UpdateFields")
            .WithOpenApi();

        group.MapPost("{id}/confirm", Confirm)
            .WithName("ConfirmTask")
            .WithOpenApi();

        group.MapPost("{id}/publish", Publish)
            .WithName("PublishTask")
            .WithOpenApi();
    }

    private static async Task<TaskDto> CreateTask(CreateTaskRequest request) => new(
        Id: Guid.NewGuid().ToString(),
        BusinessId: "",
        Status: "editing",
        Fields: new TaskFieldsDto(),
        ProposalCount: 0,
        CreatedAt: DateTime.UtcNow,
        UpdatedAt: DateTime.UtcNow,
        PublishedAt: null
    );

    private static async Task<TaskSummaryDto[]> GetMyTasks() => Array.Empty<TaskSummaryDto>();

    private static async Task<TaskDto> GetTask(string id) => new(
        Id: id,
        BusinessId: "",
        Status: "editing",
        Fields: new TaskFieldsDto(),
        ProposalCount: 0,
        CreatedAt: DateTime.UtcNow,
        UpdatedAt: DateTime.UtcNow,
        PublishedAt: null
    );

    private static async Task<object> Analyze(string id) => new { };

    private static async Task<TaskDto> UpdateAnswers(string id, AnswersRequest request) => new(
        Id: id,
        BusinessId: "",
        Status: "editing",
        Fields: new TaskFieldsDto(),
        ProposalCount: 0,
        CreatedAt: DateTime.UtcNow,
        UpdatedAt: DateTime.UtcNow,
        PublishedAt: null
    );

    private static async Task<TaskDto> UpdateFields(string id, UpdateFieldsRequest request) => new(
        Id: id,
        BusinessId: "",
        Status: "editing",
        Fields: new TaskFieldsDto(),
        ProposalCount: 0,
        CreatedAt: DateTime.UtcNow,
        UpdatedAt: DateTime.UtcNow,
        PublishedAt: null
    );

    private static async Task<object> Confirm(string id) => new { };

    private static async Task<TaskDto> Publish(string id) => new(
        Id: id,
        BusinessId: "",
        Status: "published",
        Fields: new TaskFieldsDto(),
        ProposalCount: 0,
        CreatedAt: DateTime.UtcNow,
        UpdatedAt: DateTime.UtcNow,
        PublishedAt: DateTime.UtcNow
    );
}
