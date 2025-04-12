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
    }
}
