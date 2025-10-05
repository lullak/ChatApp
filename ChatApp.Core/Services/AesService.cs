using ChatApp.Core.Interfaces.Services;
using System.Security.Cryptography;
using System.Text;

namespace ChatApp.Core.Services
{

    public class AesService : IAesService
    {
        private const int TagSizeInBytes = 16;
        private const int IvSizeInBytes = 12;
        public string Decrypt(string encryptedMessage, byte[] key)
        {
            var parts = encryptedMessage.Split(':');
            if (parts.Length != 3 || parts[0] != "ENC")
            {
                throw new ArgumentException("Invalid encrypted message format.", nameof(encryptedMessage));
            }

            var iv = Convert.FromBase64String(parts[1]);
            var combinedCiphertext = Convert.FromBase64String(parts[2]);

            var tag = new byte[TagSizeInBytes];
            var ciphertext = new byte[combinedCiphertext.Length - tag.Length];

            Buffer.BlockCopy(combinedCiphertext, 0, ciphertext, 0, ciphertext.Length);
            Buffer.BlockCopy(combinedCiphertext, ciphertext.Length, tag, 0, tag.Length);

            var decryptedPlainTextBytes = new byte[ciphertext.Length];
            using (var aesGcm = new AesGcm(key, TagSizeInBytes))
            {
                aesGcm.Decrypt(iv, ciphertext, tag, decryptedPlainTextBytes);
            }

            return Encoding.UTF8.GetString(decryptedPlainTextBytes);
        }

        public string Encrypt(string plainText, byte[] key)
        {
            var iv = new byte[IvSizeInBytes];
            RandomNumberGenerator.Fill(iv);

            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherTextBytes = new byte[plainTextBytes.Length];
            var tag = new byte[TagSizeInBytes];

            using (var aesGcm = new AesGcm(key, TagSizeInBytes))
            {
                aesGcm.Encrypt(iv, plainTextBytes, cipherTextBytes, tag);
            }

            var combinedCiphertext = new byte[cipherTextBytes.Length + tag.Length];
            Buffer.BlockCopy(cipherTextBytes, 0, combinedCiphertext, 0, cipherTextBytes.Length);
            Buffer.BlockCopy(tag, 0, combinedCiphertext, cipherTextBytes.Length, tag.Length);

            var ivBase64 = Convert.ToBase64String(iv);
            var combinedCiphertextBase64 = Convert.ToBase64String(combinedCiphertext);

            return $"ENC:{ivBase64}:{combinedCiphertextBase64}";
        }

        public byte[] GenerateRandomKey(int sizeInBytes = 32)
        {
            if (sizeInBytes != 16 && sizeInBytes != 24 && sizeInBytes != 32)
                throw new ArgumentException("Key size must be 16, 24 or 32 bytes.", nameof(sizeInBytes));

            return RandomNumberGenerator.GetBytes(sizeInBytes);
        }

    }
}