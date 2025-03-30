namespace chat_client.Services
{
    public class AuthService : IAuthService
    {
        public async Task<bool> ValidateCredentialsAsync(string login, string password)
        {
            await Task.Delay(100);
            return login == "admin" && password == "admin";
        }
    }
}