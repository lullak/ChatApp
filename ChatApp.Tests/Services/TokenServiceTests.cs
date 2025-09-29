using ChatApp.Core.Interfaces.Services;
using ChatApp.Core.Services;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChatApp.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly IConfiguration _mockConfiguration;
        private readonly ITokenService _tokenService;

        public TokenServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"Jwt:Key", "7f6387aa-14ce-40c0-b43c-ed7bfa1cc76a7f6387aa-14ce-40c0-b43c-ed7bfa1cc76a"},
                {"Jwt:Issuer", "testIssuer"},
                {"Jwt:Audience", "testAudience"}
            };
            _mockConfiguration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
            _tokenService = new TokenService(_mockConfiguration);
        }
        [Fact]
        public void GenerateToken_ShouldCreateValidToken()
        {
            // Arrange
            const string testUsername = "testUsername";

            // Act
            var tokenString = _tokenService.GenerateToken(testUsername);

            // Assert 
            Assert.NotNull(tokenString);

            var handler = new JwtSecurityTokenHandler();
            var decodedToken = handler.ReadJwtToken(tokenString);

            var subjectClaim = decodedToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            var nameClaim = decodedToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);

            Assert.NotNull(subjectClaim);
            Assert.Equal(testUsername, subjectClaim.Value);

            Assert.NotNull(nameClaim);
            Assert.Equal(testUsername, nameClaim.Value);
        }
    }
}
