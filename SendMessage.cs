using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientCommunication
{
   public class ClientSocket
    {
       static byte[] result = new byte[1024];
        public static void StartClient(IPAddress ip, int port)
        {

            Socket socketClient = new Socket(SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint point = new IPEndPoint(ip, port);

            try
            {
                socketClient.Connect(point);
                Console.WriteLine("Successfully connected to server!");
            }
            catch
            {
                Console.WriteLine("Failed to connect to the server, please press enter to exit!");
                return;
            }
            //clientSocket accept 
            int receiveLength =   socketClient.Receive(result);

            Console.WriteLine("Receive server message:{0}", Encoding.ASCII.GetString(result, 0, receiveLength));

            // clientSocket send        
            try
            {
                    Thread.Sleep(1000);    
                    string sendMessage = "client send Message Help" + DateTime.Now;
                    socketClient.Send(Encoding.ASCII.GetBytes(sendMessage));
                    Console.WriteLine("Send message to server:{0}" + sendMessage);
                }
                catch
                {
                    socketClient.Shutdown(SocketShutdown.Both);
                    socketClient.Close();                  
                }

            Console.WriteLine("After sending, press enter to exit");
            Console.ReadLine();   

        }
}
}