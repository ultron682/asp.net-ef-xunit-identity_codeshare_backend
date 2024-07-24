using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using CodeShareBackend.Data;
using CodeShareBackend.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

public class CodeShareHub : Hub
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public CodeShareHub(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task SendCode(string uniqueId, string code, string userId)
    {
        var snippet = await _context.CodeSnippets.SingleOrDefaultAsync(s => s.UniqueId == uniqueId);
        if (snippet == null)
        {
            //Console.WriteLine("userId: " + userId);
            snippet = new CodeSnippet { UniqueId = uniqueId, Code = code, UserId = userId };
            _context.CodeSnippets.Add(snippet);
        }
        else
        {
            snippet.Code = code;
            _context.CodeSnippets.Update(snippet);
        }
        await _context.SaveChangesAsync();

        await Clients.Others.SendAsync("ReceiveCode", uniqueId, code);
    }

    public async Task<string> GetCode(string uniqueId)
    {
        var snippet = await _context.CodeSnippets.SingleOrDefaultAsync(s => s.UniqueId == uniqueId);
        return snippet?.Code ?? string.Empty;
    }
}
