using CodeShareBackend.Data;
using CodeShareBackend.Helpers;
using CodeShareBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

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
            {
                await SendConfirmationEmail(model.Email, user);

                return Ok("User registered successfully");
            }
            else
            {
                foreach (var error in result.Errors)
                    return error.Code switch
                    {
                        "DuplicateUserName" => StatusCode(450,"Username already taken"),
                        "DuplicateEmail" => StatusCode(452, "Email already taken"),
                        _ => StatusCode(454, error.Code)
                    };
            }

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
                if (user.EmailConfirmed == false)
                {
                    return StatusCode(470);
                }

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

        [HttpPatch("nickname")]
        [Authorize]
        public async Task<IActionResult> ChangeNickname([FromForm] string newUsername)
        {
            if (string.IsNullOrWhiteSpace(newUsername))
                return BadRequest("Invalid username");

            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null)
                return Unauthorized("User not authenticated");

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return NotFound("User not found");

            user.UserName = newUsername;
            await _userManager.UpdateAsync(user);

            return Ok();
        }

        //[HttpPost("send-confirmation-email")]
        //[Authorize]
        //public async Task<IActionResult> SendConfirmationEmail()
        //{
        //    var email = User.FindFirstValue(ClaimTypes.Email);
        //    if (email == null)
        //        return Unauthorized("User not authenticated");

        //    var user = await _userManager.FindByEmailAsync(email);

        //    if (user == null)
        //        return NotFound("User not found");

        //    if (user.EmailConfirmed)
        //        return BadRequest("Email already confirmed");

        //    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //    var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);

        //    // Send email with confirmation link
        //    Console.WriteLine(confirmationLink);

        //    return Ok();
        //}

        private async Task SendConfirmationEmail(string? email, UserCodeShare? user)
        {
            //Generate the Token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            //Build the Email Confirmation Link which must include the Callback URL
            var ConfirmationLink = Url.Action("ConfirmEmail", "Account",
            new { userId = user.Id, token }, protocol: Request.Scheme);

            Console.WriteLine($"Please confirm your account by <a href='{ConfirmationLink!}'>clicking here</a>.");

            //Send the Confirmation Email to the User Email Id
            // await emailSender.SendEmailAsync(email, "Confirm Your Email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(ConfirmationLink)}'>clicking here</a>.", true);
        }

        [AllowAnonymous]
        [HttpGet("confirm")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (token == null)
            {
                return BadRequest("token == null");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Not found user");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded ? Redirect("http://localhost:3000/account/confirmedEmail") : BadRequest("Error");
        }

    }
}
