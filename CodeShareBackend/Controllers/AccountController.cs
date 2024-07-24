using CodeShareBackend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeShareBackend.Controllers
{
    [ApiController]
    [Route("account")]
    //[Authorize]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost()]
        public async Task<IActionResult> SetOwner(string uniqueId, string ownerId)
        {
            var snippet = await _context.CodeSnippets.SingleOrDefaultAsync(s => s.UniqueId == uniqueId);
            if (snippet == null)
            {
                return NotFound("Snippet not found");
            }
            snippet.UserId = ownerId;
            await _context.SaveChangesAsync();
            return Ok("Owner set");
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountInfo(string ownerId)
        {
            var accountInfo = await _context.Users
                .Where(u => u.Id == ownerId)
                .Include(u => u.CodeSnippets).Select(u => new { u.Email, u.UserName, CodeSnippets = u.CodeSnippets.Select(cs => cs.UniqueId).ToArray() }).FirstOrDefaultAsync();
            Console.WriteLine(accountInfo);
            return Ok(accountInfo);
        }

        [HttpPost("test")]
        public async Task<IActionResult> GetSnippets(string ownerId)
        {
            var snippets = await _context.CodeSnippets.Where(s => s.UserId == ownerId).ToListAsync();
            return Ok(snippets);
        }

    }
}
