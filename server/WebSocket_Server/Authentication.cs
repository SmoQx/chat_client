using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TeamProject;

public class UserRecord
{
    public string Name { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public int FailedAttempts { get; set; } = 0;
    public bool IsLocked { get; set; } = false;
    public DateTime? LockTime { get; set; } = null;
}

public class Authentication
{
    private const string StorageFileName = "storage.json";
    private readonly Dictionary<string, UserRecord> _users = new();

    public Authentication()
    {
        if (File.Exists(StorageFileName))
        {
            var jsonString = File.ReadAllText(StorageFileName);
            var users = JsonSerializer.Deserialize<List<UserRecord>>(jsonString);
            if (users != null)
            {
                foreach (var user in users)
                    _users[user.Name] = user;
            }
        }
        else
        {
            File.WriteAllText(StorageFileName, "[]");
        }
    }

    private void SaveToFile()
    {
        var users = _users.Values.ToList();
        var jsonString = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(StorageFileName, jsonString);
    }

    private string HashPassword(string password)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public Response SignIn(string name, string password)
    {
        if (!_users.TryGetValue(name, out var user))
            return new Response(false, "User does not exist!");

        if (user.IsLocked && user.LockTime.HasValue && DateTime.Now < user.LockTime.Value.AddMinutes(30))
            return new Response(false, "Account locked. Try again later.");

        if (user.PasswordHash == HashPassword(password))
        {
            user.FailedAttempts = 0;
            user.IsLocked = false;
            user.LockTime = null;
            SaveToFile();
            LogEvent(name, "Login successful");
            return new Response(true, "Logged in!");
        }

        user.FailedAttempts++;
        if (user.FailedAttempts >= 3)
        {
            user.IsLocked = true;
            user.LockTime = DateTime.Now;
            SaveToFile();
            LogEvent(name, "Account locked due to failed attempts");
            return new Response(false, "Account locked after 3 failed attempts.");
        }
        SaveToFile();
        LogEvent(name, "Failed login attempt");
        return new Response(false, "Credentials mismatch!");
    }

    public Response SignUp(string name, string password)
    {
        if (_users.ContainsKey(name))
            return new Response(false, "User already exists!");

        var user = new UserRecord
        {
            Name = name,
            PasswordHash = HashPassword(password)
        };
        _users[name] = user;
        SaveToFile();
        LogEvent(name, "Signed up");
        return new Response(true, "Signed up!");
    }

    private void LogEvent(string user, string message)
    {
        File.AppendAllText("login_events.log", $"{DateTime.Now}: {user} - {message}{Environment.NewLine}");
    }
}

public class Response(bool success, string message)
{
    public bool Success { get; set; } = success;
    public string Message { get; set; } = message;
}
