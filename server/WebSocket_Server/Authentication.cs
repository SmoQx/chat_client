using System.Text.Json;
using BCrypt.Net;

namespace TeamProject;

public class Authentication
{
    private const string StorageFileName = "storage.json";
    private const string LogFileName = "auth_logs.log";
    private readonly Dictionary<string, string> _storage = new();
    private readonly Dictionary<string, int> _failedAttempts = new();
    private readonly Dictionary<string, DateTime> _blockedUntil = new();
    private const int MaxFailedAttempts = 3;
    private const int BlockDurationMinutes = 1;
    
    public Authentication()
    {
        if (File.Exists(StorageFileName))
        {
            using StreamReader streamReader = new(StorageFileName);
            var jsonString = streamReader.ReadToEnd();
            var storageAsList = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(jsonString);

            if (storageAsList != null)
            {
                foreach (var item in storageAsList)
                {
                    _storage[item["name"]] = item["password"];
                    
                    if (item.TryGetValue("failed_attempts", out var failedAttemptsStr) && 
                        int.TryParse(failedAttemptsStr, out var failedAttempts))
                    {
                        _failedAttempts[item["name"]] = failedAttempts;
                    }
                    
                    if (item.TryGetValue("blocked_until", out var blockedUntilStr) && 
                        DateTime.TryParse(blockedUntilStr, out var blockedUntil))
                    {
                        _blockedUntil[item["name"]] = blockedUntil;
                    }
                }
            }

            Console.WriteLine($"Loaded {_storage.Count} users from storage");
            return;
        }
        
        var fileStream = new FileStream(StorageFileName, FileMode.Create);
        fileStream.Close();
        File.WriteAllText(StorageFileName, "[]");
        Console.WriteLine("Created new storage.json file");
    }

    private void SaveToFile()
    {
         var storageAsList = _storage.Select(user => 
         {
             var userDict = new Dictionary<string, string>
             {
                 { "name", user.Key }, 
                 { "password", user.Value }
             };
             
             if (_failedAttempts.TryGetValue(user.Key, out var attempts))
             {
                 userDict["failed_attempts"] = attempts.ToString();
             }
             
             if (_blockedUntil.TryGetValue(user.Key, out var blockedUntil))
             {
                 userDict["blocked_until"] = blockedUntil.ToString("yyyy-MM-dd HH:mm:ss");
             }
             
             return userDict;
         }).ToList();
        
        var jsonString = JsonSerializer.Serialize(storageAsList, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(StorageFileName, jsonString);
    }

    public Response SignIn(string name, string password)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
        {
            return new Response(false, "Username and password are required!");
        }

        if (IsAccountBlocked(name))
        {
            LogEvent($"BLOCKED_LOGIN_ATTEMPT: User '{name}' tried to login while account is blocked");
            return new Response(false, "Account is temporarily blocked due to too many failed login attempts!");
        }

        var isUserInStorage = _storage.TryGetValue(name, out var storagePassword);

        if (!isUserInStorage)
        {
            RecordFailedAttempt(name);
            LogEvent($"FAILED_LOGIN: User '{name}' not found");
            return new Response(false, "User does not exist!");
        }
        
        if (BCrypt.Net.BCrypt.Verify(password, storagePassword))
        {
            ResetFailedAttempts(name);
            LogEvent($"SUCCESSFUL_LOGIN: User '{name}' logged in successfully");
            return new Response(true, "Logged in!");
        }
        else
        {
            RecordFailedAttempt(name);
            LogEvent($"FAILED_LOGIN: Invalid password for user '{name}'");
            return new Response(false, "Credentials mismatch!");
        }
    }

    public Response SignUp(string name, string password)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
        {
            return new Response(false, "Username and password are required!");
        }

        if (password.Length < 3)
        {
            return new Response(false, "Password must be at least 3 characters long!");
        }

        var isUserInStorage = _storage.ContainsKey(name);

        if (isUserInStorage)
        {
            return new Response(false, "User already exists!");
        }
        
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        _storage.Add(name, hashedPassword);
        SaveToFile();
        LogEvent($"SUCCESSFUL_SIGNUP: User '{name}' registered successfully");
        return new Response(true, "Signed up!");
    }

    private bool IsAccountBlocked(string name)
    {
        if (_blockedUntil.TryGetValue(name, out var blockedUntil))
        {
            if (DateTime.Now < blockedUntil)
            {
                return true;
            }
            _blockedUntil.Remove(name);
            _failedAttempts.Remove(name);
            SaveToFile();
            LogEvent($"ACCOUNT_UNBLOCKED: User '{name}' account unblocked after timeout");
        }
        return false;
    }

    private void RecordFailedAttempt(string name)
    {
        _failedAttempts.TryGetValue(name, out var attempts);
        attempts++;
        _failedAttempts[name] = attempts;

        if (attempts >= MaxFailedAttempts)
        {
            _blockedUntil[name] = DateTime.Now.AddMinutes(BlockDurationMinutes);
            LogEvent($"ACCOUNT_BLOCKED: User '{name}' account blocked for {BlockDurationMinutes} minutes after {MaxFailedAttempts} failed attempts");
        }
        
        SaveToFile();
    }

    private void ResetFailedAttempts(string name)
    {
        _failedAttempts.Remove(name);
        _blockedUntil.Remove(name);
        SaveToFile();
    }

    private void LogEvent(string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var logEntry = $"[{timestamp}] {message}";
        
        try
        {
            File.AppendAllText(LogFileName, logEntry + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write to log file: {ex.Message}");
        }
    }
}

public class Response
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public Response(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}