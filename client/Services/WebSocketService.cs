using System;
using System.Text.Json;
using System.Threading.Tasks;
using WebSocketSharp;
using ChatMessageNamespace;
using TeamProject;

namespace chat_client.Services
{
    public class WebSocketService : IDisposable
    {
        private WebSocket? _webSocket;
        private bool _isConnected = false;
        private TaskCompletionSource<bool>? _signInTcs;
        private string? _lastSignInErrorMessage;

        public event Action<ChatMessage>? OnMessageReceived;
        public event Action<string>? OnSystemMessage;
        public event Action? OnConnected;
        public event Action? OnDisconnected;
        public event Action<string>? OnError;

        public bool IsConnected => _isConnected;
        public string? LastSignInErrorMessage => _lastSignInErrorMessage;

        public async Task<bool> ConnectAsync()
        {
            try
            {
                Console.WriteLine($"WebSocketService.ConnectAsync: IsConnected={_isConnected}");

                if (_webSocket != null && _webSocket.IsAlive)
                {
                    Console.WriteLine("WebSocketService: Already connected, returning true");
                    return true;
                }

                Console.WriteLine("WebSocketService: Creating new WebSocket connection");
                _webSocket = new WebSocket("ws://server:8081/chat");

                _webSocket.OnOpen += (sender, e) =>
                {
                    _isConnected = true;
                    Console.WriteLine("WebSocket connected");
                    OnConnected?.Invoke();
                };

                _webSocket.OnMessage += (sender, e) =>
                {
                    Console.WriteLine($"Received: {e.Data}");
                    HandleIncomingMessage(e.Data);
                };

                _webSocket.OnClose += (sender, e) =>
                {
                    _isConnected = false;
                    Console.WriteLine($"WebSocket disconnected: {e.Reason}");
                    OnDisconnected?.Invoke();
                };

                _webSocket.OnError += (sender, e) =>
                {
                    _isConnected = false;
                    Console.WriteLine($"WebSocket error: {e.Message}");
                    OnError?.Invoke(e.Message);
                };

                _webSocket.Connect();

                // Wait a bit for connection to establish
                await Task.Delay(500);

                return _isConnected;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to WebSocket: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        public async Task<bool> SignInAsync(string username, string password)
        {
            if (!_isConnected || _webSocket == null)
            {
                return false;
            }

            _lastSignInErrorMessage = null;
            _signInTcs = new TaskCompletionSource<bool>();

            var signInMessage = new
            {
                type = "signin",
                name = username,
                password = password,
                timestamp = DateTime.Now
            };

            var json = JsonSerializer.Serialize(signInMessage);
            _webSocket.Send(json);

            try
            {
                // Wait for the response with timeout
                var timeoutTask = Task.Delay(5000); // 5 second timeout
                var completedTask = await Task.WhenAny(_signInTcs.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    Console.WriteLine("SignIn timeout");
                    return false;
                }

                return await _signInTcs.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignIn error: {ex.Message}");
                return false;
            }
            finally
            {
                _signInTcs = null;
            }
        }

        public void SendMessage(string username, string content)
        {
            if (!_isConnected || _webSocket == null)
            {
                OnError?.Invoke("Nie po��czono z serwerem");
                return;
            }

            var message = new
            {
                type = "message",
                user = username,
                content = content,
                timestamp = DateTime.Now
            };

            var json = JsonSerializer.Serialize(message);
            _webSocket.Send(json);
        }

        private void HandleIncomingMessage(string jsonData)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonData);
                var root = doc.RootElement;

                // Check if this is a sign-in response first
                if (_signInTcs != null && root.TryGetProperty("Success", out var successProperty))
                {
                    // This is a Response object (sign-in response)
                    var response = JsonSerializer.Deserialize<Response>(jsonData);
                    Console.WriteLine($"SignIn response: Success={response?.Success}, Message={response?.Message}");
                    
                    if (response?.Success == false)
                    {
                        _lastSignInErrorMessage = response.Message;
                    }
                    
                    _signInTcs.SetResult(response?.Success ?? false);
                    return;
                }

                // Handle regular chat messages
                if (root.TryGetProperty("type", out var typeProperty))
                {
                    string messageType = typeProperty.GetString() ?? "";

                    switch (messageType)
                    {
                        case "chat_message":
                            var user = root.GetProperty("user").GetString() ?? "";
                            var content = root.GetProperty("content").GetString() ?? "";
                            var timestamp = root.GetProperty("timestamp").GetDateTime();

                            var chatMessage = new ChatMessage
                            {
                                User = user,
                                Content = content,
                                Timestamp = timestamp
                            };

                            OnMessageReceived?.Invoke(chatMessage);
                            break;

                        case "system_message":
                            var systemContent = root.GetProperty("content").GetString() ?? "";
                            OnSystemMessage?.Invoke(systemContent);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing incoming message: {ex.Message}");
                Console.WriteLine($"Raw message: {jsonData}");
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_webSocket != null && _webSocket.IsAlive)
                {
                    _webSocket.Close();
                }
                _isConnected = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disconnecting: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Disconnect();
            _webSocket = null;
        }
    }
}
