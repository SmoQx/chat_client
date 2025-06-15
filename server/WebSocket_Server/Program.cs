using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebSocket_Server;

public class TestService : WebSocketBehavior
{
    private string userId;

    protected override void OnMessage(MessageEventArgs e)
    {
        Console.WriteLine("Received from client: " + e.Data);

        userId = ExtractUserId(e.Data);
        Console.WriteLine("User ID: " + userId);

        Send("Data from server");
    }

    private string ExtractUserId(string message)
    {
        return "dummyUserId";
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

        ws.AddWebSocketService<TestService>("/test");
        ws.Start();
        Console.ReadKey(true);
        ws.Stop();
    }
}