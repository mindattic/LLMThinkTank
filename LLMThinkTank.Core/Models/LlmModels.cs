namespace LLMThinkTank.Core.Models;

public class LlmModel
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Avatar { get; init; } = "";
    public string Personality { get; init; } = "";
}

public class SharedTurn
{
    public string ModelId { get; set; } = "";
    public string ModelName { get; set; } = "";
    public string Text { get; set; } = "";
    public int Round { get; set; }
}

public class ConversationMessage
{
    public string ModelId { get; set; } = "";
    public string Text { get; set; } = "";
    public int Round { get; set; }
    public bool IsError { get; set; }
}
