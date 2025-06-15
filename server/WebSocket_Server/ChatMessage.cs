using System.Text.Json.Serialization;

namespace ChatMessageNamespace
{
    public class ChatMessage
    {
        public string Type { get; set; }
        public string User { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}