namespace SyncroLife.Interfaces.Services;

using SyncroLife.DTOs.FoodAnalysis;

public interface IGeminiService
{
    Task<GeminiFoodResult> AnalyzeFoodAsync(byte[] imageBytes);
}
