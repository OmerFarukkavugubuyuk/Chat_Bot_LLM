# 💬 ChatbotLLM — Mert ile Sohbet

Gemini API kullanan, WhatsApp tarzı arayüze sahip C# WinForms chatbot uygulaması.

## 🎨 Özellikler

- **WhatsApp stili** — sağda kullanıcı mesajları, solda AI mesajları
- **Mor/lila tema** — gradient arka plan, özel bubble tasarımı
- **Konuşma geçmişi** — Gemini her mesajda önceki konuşmayı hatırlar
- **Sokak ağzıyla konuşan AI** — yakın arkadaş hissi
- **Typing indicator** — AI yanıt üretirken "Mert yazıyor..." gösterimi
- **Sohbet sıfırlama** — tek tuşla temiz başlangıç

## 🚀 Kurulum

### Gereksinimler
- .NET 6.0 SDK (Windows)
- Visual Studio 2022 veya Rider

### Adımlar

1. Repoyu klonla:
```bash
git clone https://github.com/kullanici/ChatbotLLM.git
cd ChatbotLLM
```

2. **Gemini API Key al:**
   - [Google AI Studio](https://aistudio.google.com/app/apikey) adresine git
   - "Create API Key" butonuna tıkla
   - Ücretsiz — kredi kartı gerekmez

3. API key'i projeye ekle — `ChatForm.cs` dosyasında şu satırı bul:
```csharp
private string _apiKey = "YOUR_GEMINI_API_KEY_HERE";
```
Kendi key'inle değiştir.

4. Çalıştır:
```bash
dotnet run --project ChatbotLLM/ChatbotLLM.csproj
```

## 🏗️ Proje Yapısı

```
ChatbotLLM/
├── ChatbotLLM.sln          # Solution dosyası
└── ChatbotLLM/
    ├── Program.cs           # Giriş noktası
    ├── ChatForm.cs          # Ana form — UI ve olaylar
    ├── ChatBubble.cs        # Özel mesaj baloncuğu kontrolü
    ├── ChatMessage.cs       # Mesaj modeli
    └── GeminiService.cs     # Gemini API entegrasyonu
```

## 🤖 Nasıl Çalışır?

- `GeminiService` sınıfı Gemini 2.0 Flash modeline HTTP POST atar
- Her mesaj gönderiminde tüm konuşma geçmişi API'ye dahil edilir (multi-turn chat)
- Sistem promptuyla AI'a "Mert" karakteri verilmiştir
- `ChatBubble` kontrolü GDI+ ile elle çizilmiş özel balonlardır

## 🔑 Kullanılan Teknolojiler

- C# / .NET 6
- WinForms (masaüstü UI)
- Google Gemini API (gemini-2.0-flash)
- System.Text.Json (JSON parse)
- System.Net.Http (HTTP istekleri)

## ⌨️ Kullanım

| Kısayol | İşlev |
|---------|-------|
| `Enter` | Mesaj gönder |
| `Shift+Enter` | Yeni satır |
| 🗑 Temizle | Sohbeti sıfırla |
