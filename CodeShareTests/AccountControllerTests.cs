using CodeShareBackend.Controllers;
using CodeShareBackend.IServices;
using CodeShareBackend;
using CodeShareBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CodeShareTests {

    public class AccountControllerTests {
        private readonly Mock<IAccountService> _mockAccountService;
        private readonly AccountController _controller;

        public AccountControllerTests() {
            _mockAccountService = new Mock<IAccountService>();
            _controller = new AccountController(_mockAccountService.Object);
        }

        [Fact]
        public async Task Register_SuccessfulRegistration_ReturnsOk() {
            // Arrange
            var registerRequest = new RegisterRequestCodeShare {
                UserName = "TestUser",
                Email = "test@test.com",
                Password = "TestPassword123!"
            };

            _mockAccountService
                .Setup(service => service.RegisterUser(It.IsAny<RegisterRequestCodeShare>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockAccountService
                .Setup(service => service.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new UserCodeShare { Email = "test@test.com", UserName = "TestUser" });

            _mockAccountService
                .Setup(service => service.SendConfirmationEmail(It.IsAny<string>(), It.IsAny<UserCodeShare>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User registered successfully", okResult.Value);
        }



        public async Task Register_WithValidModel_ReturnsOk() {
            // Arrange
            var registerRequest = new RegisterRequestCodeShare { Email = "test2@example.com", UserName = "test2", Password = "Password123." };
            var resultSuccess = new IdentityResult();
            _mockAccountService.Setup(s => s.RegisterUser(It.IsAny<RegisterRequestCodeShare>()))
                .ReturnsAsync(resultSuccess);
            _mockAccountService.Setup(s => s.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new UserCodeShare());
            _mockAccountService.Setup(s => s.SendConfirmationEmail(It.IsAny<string>(), It.IsAny<UserCodeShare>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User registered successfully", okResult.Value);
        }









        [Fact]
        public async Task Register_WithInvalidModel_ReturnsBadRequest() {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Required");

            // Act
            var result = await _controller.Register(new RegisterRequestCodeShare() { Email = "test@example.com", UserName = "test", Password = "" });

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        //[Fact]
        //public async Task Login_WithValidCredentials_ReturnsOkWithToken() {
        //    // Arrange
        //    var loginRequest = new LoginRequestCodeShare { Email = "test@example.com", Password = "password123" };
        //    var user = new UserCodeShare { Email = "test@example.com", EmailConfirmed = true };

        //    _mockAccountService.Setup(s => s.GetUserByEmailAsync(loginRequest.Email))
        //        .ReturnsAsync(user);
        //    _mockAccountService.Setup(s => s.LoginUser(user, loginRequest.Password))
        //        .ReturnsAsync(true);
        //    _mockAccountService.Setup(s => s.GenerateJwtToken(user))
        //        .Returns("fake-jwt-token");

        //    // Act
        //    var result = await _controller.Login(loginRequest);

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var tokenResponse = Assert.IsType<dynamic>(okResult.Value);
        //    Assert.Equal("fake-jwt-token", tokenResponse.accessToken);
        //}

        [Fact]
        public async Task Login_WithUnconfirmedEmail_ReturnsStatusCode470() {
            // Arrange
            var loginRequest = new LoginRequestCodeShare { Email = "test@example.com", Password = "password123" };
            var user = new UserCodeShare { Email = "test@example.com", EmailConfirmed = false };

            _mockAccountService.Setup(s => s.GetUserByEmailAsync(loginRequest.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(470, statusCodeResult.StatusCode);
            Assert.Equal("Email unconfirmed", statusCodeResult.Value);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized() {
            // Arrange
            var loginRequest = new LoginRequestCodeShare { Email = "test@example.com", Password = "wrongpassword" };

            _mockAccountService.Setup(s => s.GetUserByEmailAsync(loginRequest.Email))
                .ReturnsAsync((UserCodeShare)null); // Simulating user not found

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid login attempt", unauthorizedResult.Value);
        }
    }
}
