using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ClientCommunication
{
    public class ClientSocket
    {
        static byte[] result = new byte[1024];

        public static string SendMessage(IPAddress ip, int port, string user_name, string message)
        {
            Socket socketClient = new Socket(SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint point = new IPEndPoint(ip, port);

            try
            {
                socketClient.Connect(point);
                Console.WriteLine("Pomyślnie połączono z serwerem!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Nie udało się połączyć z serwerem! Szczegóły: " + ex.Message);
                Console.ReadLine();
                return "Error";
            }

            // Odebranie początkowej wiadomości od serwera

            // Wysyłanie wiadomości

            // Utworzenie obiektu do wysłania jako JSON
            var jsonMessage = new
            {
                username = user_name,
                message = message,
                timestamp = DateTime.Now
            };

            // Serializacja obiektu do formatu JSON
            string json = JsonSerializer.Serialize(jsonMessage);
            byte[] buffer = Encoding.ASCII.GetBytes(json);

            try
            {
                // Wysłanie wiadomości w postaci JSON do serwera
                socketClient.Send(buffer);
                Console.WriteLine("Wysłano wiadomość: " + json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd wysyłania wiadomości: " + ex.Message);
            }


            Console.WriteLine("Połączenie zakończone. Naciśnij Enter, aby wyjść.");
            Console.ReadLine();

            int receiveLength = socketClient.Receive(result);
            Console.WriteLine("Wiadomość od serwera: {0}", Encoding.ASCII.GetString(result, 0, receiveLength));

            // Zamknięcie połączenia
            socketClient.Shutdown(SocketShutdown.Both);
            socketClient.Close();
            
            string return_result = "";

            if(receiveLength > 0){
                return_result = Encoding.ASCII.GetString(result, 0, receiveLength);
            }

            return return_result;
        }
    }
}
