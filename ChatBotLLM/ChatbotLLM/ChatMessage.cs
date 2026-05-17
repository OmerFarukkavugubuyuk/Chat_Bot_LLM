namespace ChatbotLLM
{
    public class ChatMessage
    {
        public string Text { get; set; }
        public bool IsUser { get; set; }
        public string Time { get; set; }

        public ChatMessage(string text, bool isUser)
        {
            Text = text;
            IsUser = isUser;
            Time = System.DateTime.Now.ToString("HH:mm");
        }
    }
}
