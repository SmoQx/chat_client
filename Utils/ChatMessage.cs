using System.Text.Json.Serialization;

namespace ChatMessageNamespace
{
    public struct ChatMessage
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
