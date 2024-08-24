using CodeShareBackend.Data;
using CodeShareBackend.Helpers;
using CodeShareBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CodeShareBackend.Controllers
{
    [ApiController]
    [Route("account")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<UserCodeShare> _signInManager;
        private readonly UserManager<UserCodeShare> _userManager;
        private readonly IConfiguration _configuration;

        public AccountController(ApplicationDbContext context, SignInManager<UserCodeShare> signInManager,
            UserManager<UserCodeShare> userManager, IConfiguration configuration)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestCodeShare model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new UserCodeShare { UserName = model.UserName, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
                return Ok("User registered successfully");

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return BadRequest(ModelState);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestCodeShare model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user!.UserName!, model.Password, true, false);

                if (result.Succeeded)
                {
                    user = await _userManager.FindByEmailAsync(model.Email);
                    var token = JwtTokenGenerator.GenerateToken(user!.Email!, user!.UserName!, new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])), _configuration["Jwt:Issuer"], _configuration["Jwt:Audience"]);
                    return Ok(new { accessToken = token });
                }
                else if (result.IsLockedOut)
                {
                    return Unauthorized("User account locked out");
                }
                else
                {
                    return Unauthorized("Unsucceeded");
                }

            }

            return Unauthorized("Invalid login attempt");
        }



        [HttpDelete("{UniqueId}")]
        [Authorize]
        public async Task<IActionResult> DeleteSnipet(string UniqueId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null)
                return Unauthorized("User not authenticated");

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return NotFound("User not found");

            var snippet = await _context.CodeSnippets.SingleOrDefaultAsync(s => s.UniqueId == UniqueId);

            if (snippet == null)
            {
                return NotFound("Snippet not found");
            }

            if (snippet.UserId != user.Id)
            {
                return Unauthorized("Snippet does not belong to user");
            }

            _context.CodeSnippets.Remove(snippet);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAccountInfo()
        {
            //User? user = await _userManager.GetUserAsync(User);
            //Console.WriteLine(user?.Email + user?.Id);
            var email = User.FindFirstValue(ClaimTypes.Email);
            //var userName = User.FindFirstValue(ClaimTypes.Name);
            //var idToken = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Log claims for debugging purposes
            Console.WriteLine($"Email: {email}");
            //Console.WriteLine($"UserName: {userName}");
            //Console.WriteLine($"IdToken: {idToken}");

            if (email == null)
                return Unauthorized("User not authenticated");

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return NotFound("User not found");

            Console.WriteLine(user.Email + user.Id);

            var accountInfo = await _context.Users
                .Where(u => u.Id == user.Id)
                .Include(u => u.CodeSnippets)
                .Select(u =>
                new
                {
                    u.Id,
                    u.Email,
                    u.UserName,
                    u.EmailConfirmed,
                    // code snippets with max 20 chars
                    CodeSnippets = u.CodeSnippets.Select(cs => new
                    {
                        cs.UniqueId,
                        Code = cs.Code == null ? string.Empty : (cs.Code.Length > 20 ? cs.Code.Substring(0, 20) : cs.Code)
                    }).ToArray()
                })
                .FirstOrDefaultAsync();

            Console.WriteLine(accountInfo);
            return Ok(accountInfo);
        }

        [HttpGet("snippet")]
        [Authorize]
        public async Task<IActionResult> GetSnippets()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null)
                return Unauthorized("User not authenticated");

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return NotFound("User not found");

            if (user == null)
            {
                return Unauthorized("User not found in token");
            }

            var snippets = await _context.CodeSnippets.Where(s => s.UserId == user.Id).ToListAsync();
            return Ok(snippets);
        }

        public async Task<IActionResult> ChangeNickname(string newNickname)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null)
                return Unauthorized("User not authenticated");

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return NotFound("User not found");

            user.UserName = newNickname;
            await _userManager.UpdateAsync(user);

            return Ok();
        }

    }
}
