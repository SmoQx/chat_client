using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocket_Server
{
    public class UdpData
    {
        public string serverChatName { get; set; }
        public int serverChatPort { get; set; }
        public Guid ID { get; set; } = Guid.NewGuid();
    }
}
