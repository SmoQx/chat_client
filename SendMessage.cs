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

        public static void StartClient(IPAddress ip, int port)
        {
            // Nazwa użytkownika
            string name;
            do
            {
                Console.Write("Podaj nazwę użytkownika: ");
                name = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(name));

            // Utworzenie połączenia z serwerem
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
                return;
            }

            // Odebranie początkowej wiadomości od serwera
            int receiveLength = socketClient.Receive(result);
            Console.WriteLine("Wiadomość od serwera: {0}", Encoding.ASCII.GetString(result, 0, receiveLength));

            // Wysyłanie wiadomości
            while (true)
            {
                Console.Write("Wpisz wiadomość (lub wpisz 'exit' aby zakończyć): ");
                string userMessage = Console.ReadLine();

                if (userMessage.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;

                // Utworzenie obiektu do wysłania jako JSON
                var jsonMessage = new
                {
                    username = name,
                    message = userMessage,
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
                    break;
                }
            }

            // Zamknięcie połączenia
            socketClient.Shutdown(SocketShutdown.Both);
            socketClient.Close();

            Console.WriteLine("Połączenie zakończone. Naciśnij Enter, aby wyjść.");
            Console.ReadLine();
        }
    }
}
