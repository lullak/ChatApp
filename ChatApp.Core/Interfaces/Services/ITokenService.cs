namespace ChatApp.Core.Interfaces.Services
{
    public interface ITokenService
    {
        public string GenerateToken(string username);
    }
}