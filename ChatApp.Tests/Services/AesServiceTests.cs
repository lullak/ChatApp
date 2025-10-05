using ChatApp.Core.Services;

namespace ChatApp.Tests.Services
{
    public class AesServiceTests
    {
        [Fact]
        public void GenerateRandomKey_KeyLengthIs32AndIsNotNull()
        {
  
            var service = new AesService();

            var key = service.GenerateRandomKey();

            Assert.NotNull(key);
            Assert.Equal(32, key.Length);
        }

        [Fact]
        public void GenerateRandomKey_KeyLengthAndCanBeCalledWith16And24And32()
        {

            var service = new AesService();

            var key16 = service.GenerateRandomKey(16);
            var key24 = service.GenerateRandomKey(24);
            var key32 = service.GenerateRandomKey(32);

            Assert.NotNull(key16);
            Assert.Equal(16, key16.Length);
            Assert.NotNull(key24);
            Assert.Equal(24, key24.Length);
            Assert.NotNull(key32);
            Assert.Equal(32, key32.Length);

        }

        [Fact]
        public void GenerateRandomKey_KeyLengthOtherThan16and24and32ThrowsError()
        {
            var service = new AesService();

            Assert.Throws<ArgumentException>(() => service.GenerateRandomKey(33));
        }

        [Fact]
        public void GenerateRandomKey_KeyDoesNotMatch()
        {

            var service = new AesService();

            var key1 = service.GenerateRandomKey();
            var key2 = service.GenerateRandomKey();
            Assert.NotEqual(key1, key2);
        }

        [Fact]
        public void Encrypt_And_Decrypt_ReturnsOriginalMessage()
        {
            var service = new AesService();
            var key = service.GenerateRandomKey();
            var originalMessage = "Testing Testing 123 123";
 
            var encryptedMessage = service.Encrypt(originalMessage, key);
            var decryptedMessage = service.Decrypt(encryptedMessage, key);

            Assert.Equal(originalMessage, decryptedMessage);
        }

        [Fact]
        public void Encrypt_MultipleTimesGivesDifferentResults()
        {
            var service = new AesService();
            var key = service.GenerateRandomKey();
            var originalMessage = "Testing Testing 123 123";

            var encryptedMessage1 = service.Encrypt(originalMessage, key);
            var encryptedMessage2 = service.Encrypt(originalMessage, key);

            Assert.NotEqual(encryptedMessage1, encryptedMessage2);
        }
    }
}
