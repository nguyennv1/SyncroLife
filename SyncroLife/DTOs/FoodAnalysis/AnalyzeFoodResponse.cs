namespace SyncroLife.DTOs.FoodAnalysis
{
    public class AnalyzeFoodResponse
    {
        public Guid AnalysisId { get; set; }

        public string FoodName { get; set; } = string.Empty;

        public decimal Confidence { get; set; }

        public int Calories { get; set; }

        public decimal Protein { get; set; }

        public decimal Carbs { get; set; }

        public decimal Fats { get; set; }

        public decimal Fiber { get; set; }

        public decimal Sodium { get; set; }

        public string Description { get; set; } = string.Empty;

        public string AiNotes { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}