using WebSocketSharp;
using WebSocketSharp.Server;
using System.Text.Json;
using ChatMessageNamespace;
using TeamProject;
using System.Threading.Tasks;
using System;

namespace WebSocket_Server;

public class ChatService : WebSocketBehavior
{
    private static readonly Authentication _auth = new();

    private string? userId;

    protected override void OnMessage(MessageEventArgs e)
    {
        Console.WriteLine("Received from client: " + e.Data);

        try
        {
            using var doc = JsonDocument.Parse(e.Data);
            var root = doc.RootElement;

            string type = root.GetProperty("type").GetString();

            switch (type)
            {
                case "signup":
                    {
                        string name = root.GetProperty("name").GetString();
                        string password = root.GetProperty("password").GetString();
                        var response = _auth.SignUp(name, password);
                        Send(JsonSerializer.Serialize(response));
                        break;
                    }
                case "signin":
                    {
                        string name = root.GetProperty("name").GetString();
                        string password = root.GetProperty("password").GetString();
                        var response = _auth.SignIn(name, password);
                        if (response.Success)
                        {
                            userId = name;
                            Console.WriteLine($"User signed in: {userId}");
                        }
                        Send(JsonSerializer.Serialize(response));
                        break;
                    }
                case "message":
                    {
                        var chatMsg = JsonSerializer.Deserialize<ChatMessage>(e.Data); // wiadomość od klienta
                        Console.WriteLine($"[{chatMsg.Timestamp}] {chatMsg.User}: {chatMsg.Content}");
                        Send("Message received");
                        break;
                    }
                default:
                    Send("Unknown type.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Send($"Error: {ex.Message}");
        }
    }

    protected override void OnError(WebSocketSharp.ErrorEventArgs e)
    {
        Console.WriteLine("Error: " + e.Message);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var ws = new WebSocketServer("ws://localhost:8081");
        ws.AddWebSocketService<ChatService>("/chat");
        ws.Start();
        Console.WriteLine("WebSocket server started on ws://localhost:8081/chat");
        Console.ReadKey(true);
        ws.Stop();
    }
}








