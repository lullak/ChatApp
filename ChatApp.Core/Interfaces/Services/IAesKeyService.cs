namespace ChatApp.Core.Interfaces.Services
{
    public interface IAesKeyService
    {
            byte[] GenerateRandomKey(int sizeInBytes = 32);
    }
}
