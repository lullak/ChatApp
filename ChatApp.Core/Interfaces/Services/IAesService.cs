namespace ChatApp.Core.Interfaces.Services
{
    public interface IAesService
    {
        byte[] GenerateRandomKey(int sizeInBytes = 32);

        string Encrypt(string plainText, byte[] key);

        string Decrypt(string encryptedMessage, byte[] key);
    }
}
