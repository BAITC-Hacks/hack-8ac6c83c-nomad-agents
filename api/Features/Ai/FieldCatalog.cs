using TaskForge.Api.Domain;

namespace TaskForge.Api.Features.Ai;

internal sealed record AnalysisField(string Key, string Meaning, string Question, Func<TaskFields, string> Read);

internal static class FieldCatalog
{
    internal static readonly AnalysisField[] All =
    [
        new("need", "What must change", "What exactly should change after the students' work?", f => f.Need),
        new("context", "What is happening now", "What is happening now and why is it a problem?", f => f.Context),
        new("data", "Available data, examples, sources", "What data, examples or sources can you give the team?", f => f.Data),
        new("expectedResult", "Concrete deliverable of the student team", "What concrete result do you expect from the team?", f => f.ExpectedResult),
        new("successCriteria", "Measurable acceptance signs", "How will you measure that the solution is accepted?", f => f.SuccessCriteria),
        new("users", "Who the solution is for", "Who will use the solution?", f => f.Users),
        new("constraints", "Deadlines, tech, access limits", "Are there deadlines, required technologies or access limits?", f => f.Constraints),
        new("interactionFormat", "Consultation format, feedback procedure", "How and how often can the team consult with you and get feedback?", f => f.InteractionFormat),
        new("contact", "Contact person/channel", "Who is the contact person and how to reach them?", f => f.Contact)
    ];

    internal static bool Known(string? key) => All.Any(f => f.Key == key);
}
