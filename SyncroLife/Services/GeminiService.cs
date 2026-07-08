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
Analyze this image.

First, determine if the image contains food or a dish.
If it is NOT food (e.g. it is a person, an object, text, scenery, etc.) or you cannot confidently recognize the food:
Set "isFood" to false, and leave all other fields empty or 0.

If it IS food:
Set "isFood" to true and identify:
- food name in English (use transliterated English names for Vietnamese local foods, e.g. "pho bo", "banh mi", "com tam")
- calories (estimate for one serving)
- protein (estimate for one serving)
- carbs (estimate for one serving)
- fats (estimate for one serving)
- fiber (estimate for one serving)
- sodium (estimate for one serving)
- description (brief description in English)
- aiNotes (a short recommendation about this type of food in English, maximum 1-2 sentences, describing health benefits, best time to eat, or portion advice)

Return ONLY valid JSON in the following format:
{
  "isFood": true,
  "foodName": "Food name in English",
  "confidence": 0.95,
  "calories": 350,
  "protein": 15,
  "carbs": 40,
  "fats": 12,
  "fiber": 4,
  "sodium": 300,
  "description": "Short description of the food and brief instructions on how to make it.",
  "aiNotes": "A short recommendation about this food in English (1-2 sentences)."
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
                Console.WriteLine($"Gemini API Error Response: {responseContent}");
                throw new Exception("An error occurred with the system. Please try again later.");
            }

            try
            {
                var geminiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(responseContent, 
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                var jsonText = geminiResponse?.Candidates
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

                if (result == null)
                {
                    throw new Exception("Unable to parse Gemini response.");
                }

                if (!result.IsFood || string.IsNullOrWhiteSpace(result.FoodName))
                {
                    throw new Exception("No food detected in the image. Please capture or upload a valid food photo.");
                }

                return result;
            }
            catch (Exception ex) when (ex.Message != "No food detected in the image. Please capture or upload a valid food photo.")
            {
                Console.WriteLine($"Gemini Deserialization Error: {ex.Message}");
                throw new Exception("An error occurred with the system. Please try again later.");
            }
        }
    }


}