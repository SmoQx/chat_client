using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebSocket_Server
{
    public class DiscoverAvaiableServers
    {
        UdpClient udpClient;
        int port;
        Action<string> callback;

        public DiscoverAvaiableServers(int port, Action<string> callback)
        {
            this.port = port;
            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            this.callback = callback;
        }

        public void Start()
        {
            Task.Run(async () => await ListenForServers());
        }

        private async Task ListenForServers()
        {
            while (true)
            {
                var result = await udpClient.ReceiveAsync();
                string message = Encoding.UTF8.GetString(result.Buffer);
                this.callback.Invoke(message);
            }
        }
    }
}
