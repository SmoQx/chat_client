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
            // Pobranie nazwy użytkownika.
            string username;
            do
            {
                Console.Write("Podaj nazwę użytkownika: ");
                username = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(username));

            // Inicjalizacja tokenu autoryzacyjnego (Niech grupy od serwera i autoryzacji się dogadają czy ma być np. stała wartość, czy odczyt z pliku/konfiguracji)
            string authToken = "SECRET_TOKEN";

            // Utworzenie połączenia z serwerem.
            Socket socketClient = new Socket(SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endpoint = new IPEndPoint(ip, port);

            try
            {
                socketClient.Connect(endpoint);
                Console.WriteLine("Pomyślnie połączono z serwerem!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Nie udało się połączyć z serwerem! Szczegóły: " + ex.Message);
                Console.ReadLine();
                return;
            }

            // Odebranie początkowej wiadomości od serwera.
            int received = socketClient.Receive(result);
            Console.WriteLine("Wiadomość od serwera: {0}", Encoding.ASCII.GetString(result, 0, received));

            // Weryfikacja tokenu autoryzacyjnego w tle.
            if (!CheckAuthorizationToken(socketClient, authToken))
            {
                Console.WriteLine("Token autoryzacyjny jest niepoprawny. Zamykanie połączenia.");
                socketClient.Shutdown(SocketShutdown.Both);
                socketClient.Close();
                return;
            }
            Console.WriteLine("Token autoryzacyjny zaakceptowany.");

            // Główna pętla wysyłania wiadomości.
            while (true)
            {
                Console.Write("Wpisz wiadomość (lub wpisz 'exit', aby zakończyć): ");
                string userMessage = Console.ReadLine();

                if (userMessage.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;

                // Utworzenie obiektu JSON z nazwą użytkownika, wiadomością oraz znacznikiem czasu.
                var jsonMessage = new
                {
                    username = username,
                    message = userMessage,
                    timestamp = DateTime.Now
                };

                // Serializacja obiektu do JSON.
                string json = JsonSerializer.Serialize(jsonMessage);
                byte[] buffer = Encoding.ASCII.GetBytes(json);

                try
                {
                    // Wysłanie wiadomości w formacie JSON do serwera.
                    socketClient.Send(buffer);
                    Console.WriteLine("Wysłano wiadomość: " + json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Błąd wysyłania wiadomości: " + ex.Message);
                    break;
                }
            }

            // Zamknięcie połączenia.
            socketClient.Shutdown(SocketShutdown.Both);
            socketClient.Close();

            Console.WriteLine("Połączenie zakończone. Naciśnij Enter, aby wyjść.");
            Console.ReadLine();
        }
        // Funkcja sprawdzająca token autoryzacyjny.
        // Token jest przesyłany w formacie JSON do serwera, a następnie oczekuje na wartość True.
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
