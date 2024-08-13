using CodeShareBackend.Data;
using CodeShareBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CodeShareBackend.Controllers
{
    [ApiController]
    [Route("account")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AccountController(ApplicationDbContext context, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestCodeShare model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User { UserName = model.Email, Email = model.Email };
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

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
                return Ok("Login successful");

            if (result.IsLockedOut)
                return Unauthorized("User account locked out");

            return Unauthorized("Invalid login attempt");
        }


        [HttpDelete("{UniqueId}")]
        public async Task<IActionResult> DeleteSnipet(string UniqueId)
        {
            User? user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized("User not found in token");
            }

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
        public async Task<IActionResult> GetAccountInfo()
        {
            User? user = await _userManager.GetUserAsync(User);
            //Console.WriteLine(user?.Email + user?.Id);

            if (user == null)
            {
                return Unauthorized("User not found in token");
            }

            var accountInfo = await _context.Users
                .Where(u => u.Id == user.Id)
                .Include(u => u.CodeSnippets)
                .Select(u =>
                new
                {
                    u.Id,
                    u.Email,
                    u.UserName,
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
        public async Task<IActionResult> GetSnippets()
        {
            User? user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized("User not found in token");
            }

            var snippets = await _context.CodeSnippets.Where(s => s.UserId == user.Id).ToListAsync();
            return Ok(snippets);
        }

    }
}
