using CodeShareBackend.Models;
using Microsoft.AspNetCore.Identity;

namespace CodeShareBackend.IServices
{
    public interface IAccountService
    {
        Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
        Task<bool> DeleteSnippetAsync(string uniqueId, string userId);
        Task<string> GenerateJwtToken(UserCodeShare user);
        Task<object> GetAccountInfoAsync(string userId);
        Task<List<CodeSnippet>> GetSnippetsByUserIdAsync(string userId);
        Task<UserCodeShare?> GetUserByEmailAsync(string email);
        Task<UserCodeShare?> GetUserByIdAsync(string userId);
        Task<SignInResult?> LoginUser(UserCodeShare user, string password);
        Task<IdentityResult> RegisterUser(RegisterRequestCodeShare model);
        Task<bool> SendConfirmationEmail(string email, UserCodeShare user);
        Task<bool> UpdateUserNameAsync(string userId, string newUsername);
    }
}