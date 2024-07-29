using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using CodeShareBackend.Data;
using CodeShareBackend.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

public class CodeShareHub : Hub
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private static Dictionary<string, string> _connectionsNgroup = new Dictionary<string, string>();



    public CodeShareHub(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<CodeSnippet> JoinGroup(string uniqueId)
    {
        Console.WriteLine("JoinGroup: " + uniqueId);
        if (_connectionsNgroup.ContainsKey(Context.ConnectionId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, _connectionsNgroup[Context.ConnectionId]);
            _connectionsNgroup.Remove(Context.ConnectionId);
        }
        _connectionsNgroup.Add(Context.ConnectionId, uniqueId);
        printConnectionsNgroup();

        await Groups.AddToGroupAsync(Context.ConnectionId, uniqueId);



        var snippet = await _context.CodeSnippets.Include(l => l.SelectedLang ).SingleOrDefaultAsync(s => s.UniqueId == uniqueId);
        return snippet;
    }

    public async Task BroadcastText(string uniqueId, string code, string userId)
    {
        var snippet = await _context.CodeSnippets.Include(u => u.SelectedLang).SingleOrDefaultAsync(s => s.UniqueId == uniqueId);
        if (snippet == null)
        {
            //Console.WriteLine("userId: " + userId);
            snippet = new CodeSnippet { UniqueId = uniqueId, Code = code, UserId = userId == string.Empty ? null : userId };
            _context.CodeSnippets.Add(snippet);
        }
        else
        {
            snippet.Code = code;
            _context.CodeSnippets.Update(snippet);
        }
        await _context.SaveChangesAsync();

        if (_connectionsNgroup.ContainsKey(Context.ConnectionId))
        {
            //Console.WriteLine("wyslano: " + code);
            await Clients.OthersInGroup(_connectionsNgroup[Context.ConnectionId]).SendAsync("ReceivedCode", code);
        }
    }

    public async Task UpdateSnippetLine(string uniqueId, int lineNumber, string newLineContent)
    {
        var snippet = await _context.CodeSnippets.SingleOrDefaultAsync(s => s.UniqueId == uniqueId);

        if (snippet == null)
        {
            throw new HubException("Snippet not found");
        }

        // Split code into lines
        var lines = snippet.Code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        // Check if lineNumber is valid
        if (lineNumber < 0 || lineNumber >= lines.Length)
        {
            throw new HubException("Line number is out of range.");
        }

        // Update the specified line
        lines[lineNumber] = newLineContent;

        // Join lines back into a single string
        snippet.Code = string.Join(Environment.NewLine, lines);

        _context.CodeSnippets.Update(snippet);
        await _context.SaveChangesAsync();

        if (_connectionsNgroup.ContainsKey(Context.ConnectionId))
        {
            await Clients.OthersInGroup(_connectionsNgroup[Context.ConnectionId]).SendAsync("ReceivedCode", snippet.Code);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connectionsNgroup.ContainsKey(Context.ConnectionId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, _connectionsNgroup[Context.ConnectionId]);
            _connectionsNgroup.Remove(Context.ConnectionId);
        }

        printConnectionsNgroup();
        await base.OnDisconnectedAsync(exception);
    }

    void printConnectionsNgroup()
    {
        Console.WriteLine("--------------------------");
        foreach (var item in _connectionsNgroup)
        {
            Console.WriteLine(item.Value + " : " + item.Key);
        }
        Console.WriteLine("--------------------------");

    }
}
