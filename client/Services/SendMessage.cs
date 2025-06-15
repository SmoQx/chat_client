using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ClientCommunication
{
    public class ClientSocket
    {
        public static async Task<string> SendMessage(string ip, int port, string user_name, string message)
        {
            if (ip == "")
            {
                ip = "127.0.0.1";
            }
            var ipaddress = IPAddress.Parse(ip);
            using Socket socketClient = new Socket(SocketType.Stream, ProtocolType.Tcp);
            var point = new IPEndPoint(ipaddress, port);

            try
            {
                await socketClient.ConnectAsync(point);
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }

            var jsonMessage = new
            {
                username = user_name,
                message = message,
                timestamp = DateTime.Now
            };

            string json = JsonSerializer.Serialize(jsonMessage);
            byte[] buffer = Encoding.ASCII.GetBytes(json);

            try
            {
                socketClient.Send(buffer);
            }
            catch (Exception ex)
            {
                return $"Send error: {ex.Message}";
            }

            var result = new byte[1024];
            int receiveLength = socketClient.Receive(result);

            socketClient.Shutdown(SocketShutdown.Both);
            socketClient.Close();

            return receiveLength > 0 ? Encoding.ASCII.GetString(result, 0, receiveLength) : "No response";
        }

        private static bool CheckAuthorizationToken(Socket socket, string token)
        {
            var tokenObject = new
            {
                token = token
            };

            string jsonToken = JsonSerializer.Serialize(tokenObject);
            byte[] tokenBuffer = Encoding.ASCII.GetBytes(jsonToken);

            try
            {
                // Wysłanie tokenu do serwera.
                socket.Send(tokenBuffer);


                // Odebranie odpowiedzi od serwera.
                byte[] authResponseBuffer = new byte[1024];
                int responseLength = socket.Receive(authResponseBuffer);
                string response = Encoding.ASCII.GetString(authResponseBuffer, 0, responseLength);

                // Jeżeli serwer zwróci "True" (bez względu na wielkość liter), token jest uznawany za poprawny.
                return response.Trim().Equals("True", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd podczas weryfikacji tokenu: " + ex.Message);
                return false;
            }
        }
    }
}
