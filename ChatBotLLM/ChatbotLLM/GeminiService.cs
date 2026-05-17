using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace ChatbotLLM
{
    public class GeminiService
    {
        private readonly string _apiKey;

        private readonly HttpClient _httpClient;
        private readonly List<object> _conversationHistory;

        private const string SYSTEM_PROMPT = @"Sen Efe'sin. Kullanıcının en yakın arkadaşısın. 
Türkçe konuşuyorsun, tam sokak ağzıyla — abi, lan, be, ya, kanka, moruk gibi kelimeler kullanıyorsun ama seviyeli kalıyorsun, argo seviyesi orta.
Karakterin:
- Gerçek bir dost gibi davranıyorsun, sahte nezaket yok
- Gerektiğinde sıcakkanlı ve destekleyicisin, arkadaşın zor günündeyse yanında oluyorsun
- Gerektiğinde zorba ve dürüstsün — saçma bi şey söylerse 'ya ne diyorsun lan' diyebiliyorsun
- Espri yapıyorsun, hafif takılıyorsun ama incitmiyorsun
- Kısa ve öz konuşuyorsun, çok uzun cevaplar vermiyorsun
- Bazen emoji kullanıyorsun ama abartmıyorsun
- Kullanıcıya 'sen' diyorsun, resmi dil yok
- Gereksiz 'nasıl yardımcı olabilirim' tarzı yapay AI lafları kesinlikle yok
Örnek tonlama: 'ya bunu neden düşündün ki', 'haha ya aynı şeyi ben de yaşadım', 'dur dur dur, bu nasıl iş', 'abi haklısın aslında', 'ya git yat artık 😂'";

        public GeminiService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _conversationHistory = new List<object>();
        }

        public async Task<string> SendMessageAsync(string userMessage)
        {
            // Add user message to history
            _conversationHistory.Add(new
            {
                role = "user",
                parts = new[] { new { text = userMessage } }
            });

            var requestBody = new
            {
                system_instruction = new
                {
                    parts = new[] { new { text = SYSTEM_PROMPT } }
                },
                contents = _conversationHistory,
                generationConfig = new
                {
                    temperature = 0.9,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 512
                }
            };

            string json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            HttpResponseMessage response = await _httpClient.PostAsync(url, content);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API Hatası ({response.StatusCode}): {responseBody}");
            }

            using JsonDocument doc = JsonDocument.Parse(responseBody);
            string assistantText = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "...";

            // Add assistant response to history
            _conversationHistory.Add(new
            {
                role = "model",
                parts = new[] { new { text = assistantText } }
            });

            return assistantText;
        }

        public void ClearHistory()
        {
            _conversationHistory.Clear();
        }
    }
}
