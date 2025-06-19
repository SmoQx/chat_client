using System;

namespace YourUserNamespace
{
    public class User
    {
        public string UserName { get; set; } // Default value

        // You can add methods to set or get the username if necessary
        public void SetUserName(string username)
        {
            UserName = username;
            Console.WriteLine($"Przyjęto zmiane nazwy na {UserName}");
        }

        public bool CheckUserName()
        {
            if (!string.IsNullOrWhiteSpace(UserName))
            {
                Console.WriteLine($"Autoryzowano dla {UserName}");
                return true;
            }
            else
            {
                Console.WriteLine($"Brak autoryzacji - użytkownik niezalogowany");
                return false;
            }
        }
    }
}