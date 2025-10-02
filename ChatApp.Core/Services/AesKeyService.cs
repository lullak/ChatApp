using ChatApp.Core.Interfaces.Services;
using System.Security.Cryptography;

namespace ChatApp.Core.Services
{

    public class AesKeyService : IAesKeyService
    {
        public byte[] GenerateRandomKey(int sizeInBytes = 32)
        {
            if (sizeInBytes != 16 && sizeInBytes != 24 && sizeInBytes != 32)
                throw new ArgumentException("Key size must be 16, 24 or 32 bytes.", nameof(sizeInBytes));

            var key = new byte[sizeInBytes];
            RandomNumberGenerator.Fill(key);
            return key;
        }

    }
}