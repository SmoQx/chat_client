using System.Text.Json.Serialization;

namespace ChatMessageNamespace
{
    public class ChatMessage
    {
        public string User { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public class Response
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
