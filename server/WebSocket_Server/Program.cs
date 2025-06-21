
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Text.Json;
using ChatMessageNamespace;
using TeamProject;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Collections.Generic;
using UdpServerPropagation;

namespace WebSocket_Server;

public class ChatService : WebSocketBehavior
{
    private static readonly Authentication _auth = new();

    private static readonly ConcurrentDictionary<string, ChatService> _activeConnections = new();

    private string? userId;

    private static readonly string HistoryFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chat_history.json");

    private static readonly object FileLock = new object();

    protected override void OnOpen()
    {
        Console.WriteLine($"Client connected: {ID}");
        EnsureHistoryFileExists();
    }

    protected override void OnClose(CloseEventArgs e)
    {
        if (!string.IsNullOrEmpty(userId))
        {
            _activeConnections.TryRemove(userId, out _);
            Console.WriteLine($"User disconnected: {userId}");

            BroadcastSystemMessage($"{userId} opuścił chat");
        }
    }

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
                            _activeConnections[userId] = this;
                            Console.WriteLine($"User signed in: {userId}");

                            BroadcastSystemMessage($"{userId} dołączył do chatu");
                            SendChatHistory();
                        }

                        Send(JsonSerializer.Serialize(response));
                        break;
                    }
                case "message":
                    {
                        if (string.IsNullOrEmpty(userId))
                        {
                            Send("Error: You must be signed in to send messages.");
                            Console.WriteLine("Received message from unsigned user");
                            break;
                        }

                        string messageUser = root.GetProperty("user").GetString() ?? "";
                        string messageContent = root.GetProperty("content").GetString() ?? "";
                        DateTime messageTimestamp = root.GetProperty("timestamp").GetDateTime();

                        Console.WriteLine($"[{messageTimestamp}] {messageUser}: {messageContent}");

                        var chatMsg = new ChatMessage
                        {
                            Type = "message",
                            User = messageUser,
                            Content = messageContent,
                            Timestamp = messageTimestamp
                        };
                        SaveMessageToHistory(chatMsg);
                        BroadcastMessage(chatMsg);
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
            Console.WriteLine($"Error processing message: {ex.Message}");
        }
    }

    protected override void OnError(WebSocketSharp.ErrorEventArgs e)
    {
        Console.WriteLine("Error: " + e.Message);
    }

    private void EnsureHistoryFileExists()
    {
        lock (FileLock)
        {
            if (!File.Exists(HistoryFilePath))
            {
                try
                {
                    File.WriteAllText(HistoryFilePath, JsonSerializer.Serialize(new List<ChatMessage>(), new JsonSerializerOptions { WriteIndented = true }));
                    Console.WriteLine($"History file created in: {HistoryFilePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating history file: {ex.Message}");
                }
            }
        }
    }

    private void SaveMessageToHistory(ChatMessage message)
    {
        lock (FileLock)
        {
            List<ChatMessage> history;
            try
            {
                if (File.Exists(HistoryFilePath))
                {
                    var json = File.ReadAllText(HistoryFilePath);
                    history = JsonSerializer.Deserialize<List<ChatMessage>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ChatMessage>();
                }
                else
                {
                    history = new List<ChatMessage>();
                }

                history.Add(message);
                File.WriteAllText(HistoryFilePath, JsonSerializer.Serialize(history, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }
    }

    private void SendChatHistory()
    {
        lock (FileLock)
        {
                if (File.Exists(HistoryFilePath))
                {
                    var json = File.ReadAllText(HistoryFilePath);
                    Console.WriteLine($"History file read in {json}");
                    var history = JsonSerializer.Deserialize<List<ChatMessage>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (history != null && history.Count > 0)
                    {
                        foreach (var msg in history)
                        {
                            var messageJson = JsonSerializer.Serialize(new
                            {
                                type = "chat_message",
                                user = msg.User,
                                content = msg.Content,
                                timestamp = msg.Timestamp
                            });
                            Send(messageJson);
                        }
                    }
                }
        }
    }


    private void BroadcastMessage(ChatMessage message)
    {
        var messageJson = JsonSerializer.Serialize(new
        {
            type = "chat_message",
            user = message.User,
            content = message.Content,
            timestamp = message.Timestamp
        });

        BroadcastToAll(messageJson);
    }

    private void BroadcastSystemMessage(string content)
    {
        var systemMessage = JsonSerializer.Serialize(new
        {
            type = "system_message",
            user = "System",
            content = content,
            timestamp = DateTime.Now
        });

        BroadcastToAll(systemMessage);
    }

    private void BroadcastToAll(string message)
    {
        var connectionsToRemove = new List<string>();

        foreach (var connection in _activeConnections)
        {
            try
            {
                connection.Value.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to {connection.Key}: {ex.Message}");
                connectionsToRemove.Add(connection.Key);
            }
        }

        foreach (var connectionId in connectionsToRemove)
        {
            _activeConnections.TryRemove(connectionId, out _);
        }
    }
}

public class Program
{

    public static void Main(string[] args)
    {
        var udpPropagation = new ServerPropagationProcess(10000, "ChatServer", 8081);
        udpPropagation.Start();
        var ws = new WebSocketServer("ws://server:8081");
        ws.AddWebSocketService<ChatService>("/chat");
        Console.WriteLine("WebSocket server started on ws://localhost:8081/chat");
        ws.Start();
        Thread.Sleep(Timeout.Infinite); 
        ws.Stop();
    }


}
