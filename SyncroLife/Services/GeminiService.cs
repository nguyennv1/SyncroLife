using Microsoft.Extensions.Options;
using SyncroLife.DTOs.FoodAnalysis;
using SyncroLife.Helpers;
using SyncroLife.Interfaces.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace SyncroLife.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;

        public GeminiService(
            HttpClient httpClient,
            IOptions<GeminiSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<GeminiFoodResult> AnalyzeFoodAsync(byte[] imageBytes)
        {
            var base64Image = Convert.ToBase64String(imageBytes);

            var prompt = """
Analyze this food image.

Identify:
- food name
- calories
- protein
- carbs
- fats
- fiber
- sodium

Estimate nutrition values for one serving.

Return ONLY valid JSON.

{
  "foodName":"",
  "confidence":0,
  "calories":0,
  "protein":0,
  "carbs":0,
  "fats":0,
  "fiber":0,
  "sodium":0,
  "description":"",
  "aiNotes":""
}
""";

            return await CallGeminiAsync(base64Image, prompt);
        }

        private async Task<GeminiFoodResult> CallGeminiAsync(
    string base64Image,
    string prompt)
        {
            var requestBody = new
            {
                contents = new[]
                {
            new
            {
                parts = new object[]
                {
                    new
                    {
                        text = prompt
                    },
                    new
                    {
                        inline_data = new
                        {
                            mime_type = "image/jpeg",
                            data = base64Image
                        }
                    }
                }
            }
        }
            };

            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";

            var response =
                await _httpClient.PostAsJsonAsync(
                    url,
                    requestBody);

            var responseContent =
                await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(responseContent);
            }

            var geminiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(responseContent, 
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            var jsonText =geminiResponse?.Candidates
                .FirstOrDefault()?.Content.Parts
                .FirstOrDefault()?.Text;

            jsonText = jsonText?
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            if (string.IsNullOrWhiteSpace(jsonText))
            {
                throw new Exception("Gemini returned empty response.");
            }

            var result = JsonSerializer.Deserialize<GeminiFoodResult>(jsonText,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return result ?? throw new Exception("Unable to parse Gemini response.");
        }
    }


}