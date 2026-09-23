using System.Text.Json;
namespace TaskForge.Api.Infrastructure.OpenAi;

public static class Schemas
{
    public static JsonElement Analysis { get; } = JsonDocument.Parse(
        """
{
  "type": "object",
  "additionalProperties": false,
  "required": ["title", "missingFields", "extracted", "questions", "suggestions"],
  "properties": {
    "title": { "type": "string", "description": "Short neutral title from the draft wording; empty if unclear" },
    "missingFields": { "type": "array", "items": { "type": "string", "enum": ["context","need","users","data","constraints","expectedResult","successCriteria","contact","interactionFormat"] } },
    "extracted": {
      "type": "array",
      "items": {
        "type": "object", "additionalProperties": false,
        "required": ["fieldKey", "value", "evidence"],
        "properties": {
          "fieldKey": { "type": "string", "enum": ["context","need","users","data","constraints","expectedResult","successCriteria","contact","interactionFormat"] },
          "value":    { "type": "string" },
          "evidence": { "type": "string" }
        }
      }
    },
    "questions": {
      "type": "array",
      "items": {
        "type": "object", "additionalProperties": false,
        "required": ["fieldKey", "question", "chips"],
        "properties": {
          "fieldKey": { "type": "string", "enum": ["context","need","users","data","constraints","expectedResult","successCriteria","contact","interactionFormat"] },
          "question": { "type": "string" },
          "chips":    { "type": "array", "items": { "type": "string" } }
        }
      }
    },
    "suggestions": {
      "type": "array",
      "items": {
        "type": "object", "additionalProperties": false,
        "required": ["fieldKey", "action"],
        "properties": {
          "fieldKey": { "type": "string", "enum": ["context","need","users","data","constraints","expectedResult","successCriteria","contact","interactionFormat"] },
          "action":   { "type": "string" }
        }
      }
    }
  }
}
""").RootElement.Clone();
}
