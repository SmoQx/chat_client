using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebSocketSharp;
using ChatMessageNamespace;

namespace ClientCommunication
{
    public class ClientSocket
    {
        public static async Task<string> SendMessage(string ip, int port, string user_name, string message)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = "127.0.0.1";
            }

            var tcs = new TaskCompletionSource<string>();

            using var ws = new WebSocket("ws://localhost:8081/chat");

            ws.OnMessage += (sender, e) =>
            {
                Console.WriteLine("Server response: " + e.Data);
                tcs.TrySetResult(e.Data); // Complete the task with server's response
            };

            ws.OnError += (sender, e) =>
            {
                Console.WriteLine("WebSocket error: " + e.Message);
                tcs.TrySetException(new Exception(e.Message));
            };

            ws.OnOpen += (sender, e) =>
            {
                // Construct message JSON
                var chatMessage = new
                {
                    type = "message",
                    user = user_name,
                    content = message,
                    timestamp = DateTime.Now
                };

                string json = JsonSerializer.Serialize(chatMessage);
                ws.Send(json);
            };

            ws.Connect();

            return await tcs.Task;
        }

        public static async Task<bool> CheckAuthorizationToken(string username, string password)
        {
            var tcs = new TaskCompletionSource<string>();
            using var ws = new WebSocket("ws://localhost:8081/chat");

            ws.OnMessage += (sender, e) =>
            {
                tcs.TrySetResult(e.Data);
            };

            ws.OnError += (sender, e) =>
            {
                tcs.TrySetException(new Exception(e.Message));
            };

            ws.OnOpen += (sender, e) =>
            {
                var signin = new
                {
                    type = "signin",
                    name = username,
                    password = password,
                    timestamp = DateTime.Now
                };
                string json = JsonSerializer.Serialize(signin);
                ws.Send(json);
            };

            ws.Connect();

            try
            {
                var responseJson = await tcs.Task;  // Wait for server response
                var response = JsonSerializer.Deserialize<Response>(responseJson);
                return response != null && response.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during authorization: {ex.Message}");
                return false;
            }
            finally
            {
                if (ws.IsAlive)
                    ws.Close();
            }
        }
    }
}
