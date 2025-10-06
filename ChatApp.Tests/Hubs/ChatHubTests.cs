using ChatApp.Core.Interfaces.Repos;
using ChatApp.Core.Interfaces.Services;
using ChatApp.Core.Models;
using ChatApp.Hubs;
using Ganss.Xss;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace ChatApp.Tests.Hubs
{
    public class ChatHubTests
    {
        private readonly Mock<IChatRepo> _mockChatRepo;
        private readonly Mock<ILogger<ChatHub>> _mockLogger;
        private readonly Mock<IAesService> _mockAesService;
        private readonly Mock<IHtmlSanitizer> _mockSanitizer;
        private readonly Mock<IHubCallerClients> _mockClients;
        private readonly Mock<IClientProxy> _mockClientProxy;
        private readonly ChatHub _sut;

        public ChatHubTests()
        {
            _mockChatRepo = new Mock<IChatRepo>();
            _mockLogger = new Mock<ILogger<ChatHub>>();
            _mockAesService = new Mock<IAesService>();
            _mockSanitizer = new Mock<IHtmlSanitizer>();
            _mockClients = new Mock<IHubCallerClients>();
            _mockClientProxy = new Mock<IClientProxy>();
            _mockClients.Setup(clients => clients.All).Returns(_mockClientProxy.Object);

            _sut = new ChatHub(_mockChatRepo.Object, _mockLogger.Object, _mockAesService.Object, _mockSanitizer.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testusername")
            }));
            
            var mockContext = new Mock<HubCallerContext>();
            mockContext.Setup(c => c.User).Returns(user);

            _sut.Clients = _mockClients.Object;
            _sut.Context = mockContext.Object;
        }

        [Fact]
        public async Task SendMessageAll_DecryptAndSanitizeAndSaveAndEncryptAndBroadcastMessage()
        {

            var encryptedMessage = "encryptedMessage";
            var decryptedMessage = "decryptedMessage";
            var sanitizedMessage = "sanitizedMessage";
            var username = "testusername";

            _mockAesService.Setup(aes => aes.Decrypt(encryptedMessage, It.IsAny<byte[]>())).Returns(decryptedMessage);
            _mockSanitizer.Setup(s => s.Sanitize(decryptedMessage, It.IsAny<string>(), It.IsAny<AngleSharp.IMarkupFormatter>()))
              .Returns(sanitizedMessage);
            _mockAesService.Setup(aes => aes.Encrypt(sanitizedMessage, It.IsAny<byte[]>())).Returns("encryptedMessage");

            await _sut.SendMessageAll(encryptedMessage);

            _mockAesService.Verify(aes => aes.Decrypt(encryptedMessage, It.IsAny<byte[]>()), Times.Once);
            _mockSanitizer.Verify(s => s.Sanitize(decryptedMessage, It.IsAny<string>(), It.IsAny<AngleSharp.IMarkupFormatter>()), Times.Once);
            _mockChatRepo.Verify(repo => repo.SaveMessageAsync(It.Is<ChatMessage>(msg => msg.Username == username && msg.Message == sanitizedMessage)), Times.Once);
            _mockAesService.Verify(aes => aes.Encrypt(sanitizedMessage, It.IsAny<byte[]>()), Times.Once);
            _mockClientProxy.Verify(p => p.SendCoreAsync("ReceiveMessage", 
                It.Is<object[]>(o => o[0].ToString() == username && o[1].ToString() == encryptedMessage),
                It.IsAny<CancellationToken>()), Times.Once);


        }


    }
}
