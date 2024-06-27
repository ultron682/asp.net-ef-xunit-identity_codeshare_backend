using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using CodeShareBackend.Data;
using CodeShareBackend.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class CodeShareHub : Hub
{
    private readonly ApplicationDbContext _context;

    public CodeShareHub(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SendCode(string uniqueId, string code)
    {
        var snippet = await _context.CodeSnippets.SingleOrDefaultAsync(s => s.UniqueId == uniqueId);
        if (snippet == null)
        {
            snippet = new CodeSnippet { UniqueId = uniqueId, Code = code };
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
