namespace  LLMThinkTank.Services;

public class HumanNameService
{
    private static readonly string[] FirstNames =
    [
        "Avery", "Blake", "Casey", "Drew", "Elliot", "Emerson", "Finley", "Harper", "Hayden", "Jordan",
        "Kai", "Logan", "Morgan", "Parker", "Quinn", "Reese", "Riley", "Rowan", "Sawyer", "Skyler",
        "Sydney", "Taylor", "Teagan", "Dakota", "Cameron", "Jules", "Sasha", "Robin", "Jamie", "Alex"
    ];

    private readonly Random _rng = new();

    public string NextFirstName() => FirstNames[_rng.Next(FirstNames.Length)];

    public string NextDisplayName(string llmName) => $"{NextFirstName()} ({llmName})";
}
