using System.Text.Json;

namespace TeamProject;

public class Authentication
{
    private const string StorageFileName = "storage.json";
    private readonly Dictionary<string, string> _storage = new();
    
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
         var storageAsList = _storage.Select(user => new Dictionary<string, string> 
        { 
            { "name", user.Key }, 
            { "password", user.Value } 
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

        var isUserInStorage = _storage.TryGetValue(name, out var storagePassword);

        if (!isUserInStorage)
        {
            return new Response(false, "User does not exist!");
        }
        
        return storagePassword == password ? 
            new Response(true, "Logged in!") : 
            new Response(false, "Credentials mismatch!");
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
        
        _storage.Add(name, password);
        SaveToFile();
        return new Response(true, "Signed up!");
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