using ChatApp.Core.Interfaces.Repos;
using ChatApp.Core.Interfaces.Services;
using ChatApp.Core.Models;
using ChatApp.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace ChatApp.Tests.Hubs
{
    public class ChatHubTests
    {
        // Mocktestet har tagits fram i samband med Ai
        private static (ChatHub hub, Mock<IChatRepo> repoMock, Mock<IClientProxy> clientProxy, Mock<ISingleClientProxy> singleClientProxy, Mock<IHubCallerClients> clientsMock) CreateHub(string? username = null)
        {
            var repoMock = new Mock<IChatRepo>();
            var clientProxy = new Mock<IClientProxy>(); 
            var singleClientProxy = new Mock<ISingleClientProxy>(); 
            var clientsMock = new Mock<IHubCallerClients>();
            clientsMock.Setup(c => c.All).Returns(clientProxy.Object);

            var logger = new Mock<ILogger<ChatHub>>().Object;
            var aesMock = new Mock<IAesKeyService>().Object;

            var hub = new ChatHub(repoMock.Object, logger, aesMock)
            {
                Clients = clientsMock.Object
            };

            var ctxMock = new Mock<HubCallerContext>();
            ctxMock.SetupGet(c => c.ConnectionId).Returns("conn-1");
            if (username is not null)
            {
                var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }, "test");
                ctxMock.SetupGet(c => c.User).Returns(new ClaimsPrincipal(identity));
            }
            else
            {
                ctxMock.SetupGet(c => c.User).Returns((ClaimsPrincipal?)null);
            }

            hub.Context = ctxMock.Object;
            return (hub, repoMock, clientProxy, singleClientProxy, clientsMock);
        }

        [Fact]
        public async Task SendMessageAll_WithUsernameSavesAndSendMessageAll()
        {
            var (hub, repoMock, clientProxy, _, _) = CreateHub("Dennis");

            ChatMessage? saved = null;
            repoMock.Setup(r => r.SaveMessageAsync(It.IsAny<ChatMessage>()))
                .Callback<ChatMessage>(m => saved = m)
                .Returns(Task.CompletedTask);

            await hub.SendMessageAll("test");

            repoMock.Verify(r => r.SaveMessageAsync(It.IsAny<ChatMessage>()), Times.Once);
            Assert.NotNull(saved);
            Assert.Equal("Dennis", saved!.Username);
            Assert.Equal("test", saved.Message);

            clientProxy.Verify(p =>
                p.SendCoreAsync("ReceiveMessage",
                    It.Is<object[]>(a => (string)a[0] == "Dennis" && (string)a[1] == "test"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SendMessageAll_WithoutUsernameDoesNotSaveOrSendMessageAll()
        {
            var (hub, repoMock, clientProxy, _, _) = CreateHub(null);

            await hub.SendMessageAll("nope");

            repoMock.Verify(r => r.SaveMessageAsync(It.IsAny<ChatMessage>()), Times.Never);
            clientProxy.Verify(p =>
                p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task OnConnectedAsync_AddsUserAndSendsKeyAndUpdateUserList()
        {
            var (hub, _, clientProxy, singleClientProxy, clientsMock) = CreateHub("Dennis");

            clientsMock.Setup(c => c.Client("conn-1")).Returns(singleClientProxy.Object);

            // Act
            await hub.OnConnectedAsync();

            Assert.True(ChatHub.IsUserOnline("Dennis"));

            singleClientProxy.Verify(p =>
                p.SendCoreAsync("GeneralChatKey",
                    It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            clientProxy.Verify(p =>
                p.SendCoreAsync("UpdateUserList",
                    It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task OnDisconnectedAsync_RemovesConnectionAndUpdatesUserList()
        {
            var (hub, _, clientProxy, singleClientProxy, clientsMock) = CreateHub("Dennis");

            clientsMock.Setup(c => c.Client("conn-1")).Returns(singleClientProxy.Object);

            await hub.OnConnectedAsync();
            Assert.True(ChatHub.IsUserOnline("Dennis"));

            await hub.OnDisconnectedAsync(null);

            Assert.False(ChatHub.IsUserOnline("Dennis"));

            clientProxy.Verify(p =>
                p.SendCoreAsync("UpdateUserList",
                    It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }
    }
}
