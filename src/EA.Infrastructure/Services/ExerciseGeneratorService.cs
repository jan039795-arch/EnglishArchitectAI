using EA.Domain.Entities;
using System.Text.RegularExpressions;

namespace EA.Infrastructure.Services;

public class ExerciseGeneratorService
{
    private static readonly Random _random = new();
    private static readonly string[] _modifiers =
    {
        "What is the correct way to", "Which sentence is correct?", "Choose the best option:",
        "Identify the error in:", "Complete this sentence:", "Which phrase fits best?",
        "Select the most appropriate:", "How would you rephrase?", "What does this mean?",
        "Fill in the blank:", "Which is grammatically correct?"
    };

    public string GenerateVariation(string originalPrompt, int difficulty = 0)
    {
        // Si el prompt es muy corto, devuelve una variación simple
        if (originalPrompt.Length < 20)
            return $"{_modifiers[_random.Next(_modifiers.Length)]} \"{originalPrompt}\"";

        // Intenta parafrasear manteniendo significado
        var variation = ParaphrasePrompt(originalPrompt, difficulty);
        return variation;
    }

    private string ParaphrasePrompt(string prompt, int difficulty)
    {
        // Reemplaza palabras con sinónimos basado en dificultad
        var words = prompt.Split(' ');
        var modified = new List<string>();

        foreach (var word in words)
        {
            if (_random.Next(100) < 30 && word.Length > 4) // 30% de probabilidad para palabras largas
            {
                var synonym = GetSynonym(word.ToLower(), difficulty);
                modified.Add(synonym != word ? synonym : word);
            }
            else
            {
                modified.Add(word);
            }
        }

        return string.Join(" ", modified);
    }

    private string GetSynonym(string word, int difficulty)
    {
        // Diccionario simple de sinónimos por dificultad
        var synonyms = new Dictionary<string, Dictionary<int, string>>
        {
            { "important", new() { { 0, "important" }, { 1, "significant" }, { 2, "crucial" } } },
            { "good", new() { { 0, "good" }, { 1, "excellent" }, { 2, "outstanding" } } },
            { "bad", new() { { 0, "bad" }, { 1, "poor" }, { 2, "detrimental" } } },
            { "easy", new() { { 0, "easy" }, { 1, "simple" }, { 2, "elementary" } } },
            { "difficult", new() { { 0, "difficult" }, { 1, "challenging" }, { 2, "formidable" } } },
            { "help", new() { { 0, "help" }, { 1, "assist" }, { 2, "facilitate" } } },
            { "show", new() { { 0, "show" }, { 1, "demonstrate" }, { 2, "illustrate" } } },
            { "make", new() { { 0, "make" }, { 1, "create" }, { 2, "construct" } } },
            { "use", new() { { 0, "use" }, { 1, "employ" }, { 2, "utilize" } } },
            { "think", new() { { 0, "think" }, { 1, "consider" }, { 2, "contemplate" } } },
            { "know", new() { { 0, "know" }, { 1, "understand" }, { 2, "comprehend" } } },
            { "happy", new() { { 0, "happy" }, { 1, "cheerful" }, { 2, "delighted" } } },
            { "sad", new() { { 0, "sad" }, { 1, "sorrowful" }, { 2, "melancholic" } } },
            { "fast", new() { { 0, "fast" }, { 1, "quick" }, { 2, "swift" } } },
            { "slow", new() { { 0, "slow" }, { 1, "gradual" }, { 2, "leisurely" } } }
        };

        if (synonyms.TryGetValue(word, out var options) && options.TryGetValue(difficulty, out var synonym))
            return synonym;

        return word;
    }

    public List<string> GenerateContextualExamples(string topic, int count = 3)
    {
        var templates = new Dictionary<string, List<string>>
        {
            { "grammar", new()
            {
                $"In professional writing, one must {topic} carefully to maintain clarity.",
                $"Students often struggle with {topic} because it requires practice.",
                $"The difference between these {topic} is subtle but important.",
                $"When learning {topic}, consistency is key to mastery.",
                $"Advanced speakers understand the nuances of {topic}."
            }},
            { "vocabulary", new()
            {
                $"The term {topic} is commonly used in business contexts.",
                $"Understanding {topic} helps you express ideas more precisely.",
                $"Native speakers frequently use {topic} in everyday conversation.",
                $"The word {topic} has several related meanings in English.",
                $"Mastering {topic} elevates your communication skills."
            }},
            { "pronunciation", new()
            {
                $"The correct {topic} requires practice and listening to natives.",
                $"Pay attention to the stress pattern in {topic}.",
                $"Practicing {topic} daily will improve your fluency.",
                $"Many learners find {topic} challenging initially.",
                $"Understanding {topic} improves overall comprehension."
            }}
        };

        var examples = new List<string>();
        var topicType = topic.Length > 10 ? "grammar" : "vocabulary";

        if (templates.TryGetValue(topicType, out var templateList))
        {
            for (int i = 0; i < count; i++)
            {
                var randomTemplate = templateList[_random.Next(templateList.Count)];
                examples.Add(randomTemplate);
            }
        }

        return examples;
    }

    public string GenerateChangeLog(string originalPrompt, string newPrompt)
    {
        var changes = new List<string>();

        if (originalPrompt.Length != newPrompt.Length)
            changes.Add($"Length adjusted: {originalPrompt.Length} → {newPrompt.Length} chars");

        var originalWords = originalPrompt.Split(' ').Length;
        var newWords = newPrompt.Split(' ').Length;
        if (originalWords != newWords)
            changes.Add($"Vocabulary: {originalWords} → {newWords} words");

        if (!originalPrompt.Equals(newPrompt, StringComparison.OrdinalIgnoreCase))
            changes.Add("Content refreshed for variety");

        return changes.Count > 0 ? string.Join("; ", changes) : "Minor refresh";
    }
}
