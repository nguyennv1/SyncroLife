namespace SyncroLife.DTOs.FoodAnalysis
{
    public class GeminiApiResponse
    {
        public List<GeminiCandidate> Candidates { get; set; } = new();
    }

    public class GeminiCandidate
    {
        public GeminiContent Content { get; set; } = new();
    }

    public class GeminiContent
    {
        public List<GeminiPart> Parts { get; set; } = new();
    }

    public class GeminiPart
    {
        public string Text { get; set; } = string.Empty;
    }
}