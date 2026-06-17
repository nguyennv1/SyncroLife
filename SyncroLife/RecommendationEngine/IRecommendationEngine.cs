namespace SyncroLife.AIRecommendation
{
    public interface IRecommendationEngine
    {
        Task<List<RecommendationResult>> GenerateAsync(RecommendationContext context);
    }
}
