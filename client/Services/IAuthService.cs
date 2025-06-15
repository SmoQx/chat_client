namespace chat_client.Services
{
    public interface IAuthService
    {
        Task<bool> ValidateCredentialsAsync(string login, string password);
    }
}