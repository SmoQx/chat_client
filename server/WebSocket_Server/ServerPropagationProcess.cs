using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using WebSocket_Server;

namespace UdpServerPropagation
{
    public class ServerPropagationProcess
    {
        string serverName = "ChatServer";
        int port;
        UdpClient udpClient;
        int interval = 2000;
        UdpData udpData;

        public ServerPropagationProcess(int port, string serverName = "ChatServer", int chatPort = 2137)
        {
            this.port = port;
            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;

            udpData = new UdpData()
            {
                serverChatName = serverName,
                serverChatPort = chatPort
            };
        }

        public void Start()
        {
            Task.Run(async () => await DiscoverMe());
        }

        private async Task DiscoverMe()
        {
            while (true)
            {
                string json = JsonSerializer.Serialize(udpData, new JsonSerializerOptions() { WriteIndented = true });
                byte[] datagram = Encoding.UTF8.GetBytes(json);
                await udpClient.SendAsync(datagram, datagram.Length, new IPEndPoint(IPAddress.Broadcast, port));
                await Task.Delay(interval);
            }
        }
    }
}
