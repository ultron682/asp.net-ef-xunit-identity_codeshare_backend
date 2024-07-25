using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using CodeShareBackend.Data;
using CodeShareBackend.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using static System.Net.Mime.MediaTypeNames;

public class CodeShareHub : Hub
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private static Dictionary<string, string> connectionsNgroup = new Dictionary<string, string>();



    public CodeShareHub(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task BroadcastText(string uniqueId, string code, string userId)
    {
        var snippet = await _context.CodeSnippets.SingleOrDefaultAsync(s => s.UniqueId == uniqueId);
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

        if (connectionsNgroup.ContainsKey(Context.ConnectionId))
        {
            Console.WriteLine("wyslano: " + code);
            await Clients.OthersInGroup(connectionsNgroup[Context.ConnectionId]).SendAsync("ReceivedCode", uniqueId, code);
        }
    }

    //public async Task<string> GetCode(string uniqueId)
    //{
    //    var snippet = await _context.CodeSnippets.SingleOrDefaultAsync(s => s.UniqueId == uniqueId);
    //    return snippet?.Code ?? string.Empty;
    //}

    //public override async Task OnDisconnectedAsync(Exception exception)
    //{
    //    if (connectionsNgroup.ContainsKey(Context.ConnectionId))
    //    {
    //        await Groups.RemoveFromGroupAsync(Context.ConnectionId, connectionsNgroup[Context.ConnectionId]);
    //        connectionsNgroup.Remove(Context.ConnectionId);
    //    }
    //    await base.OnDisconnectedAsync(exception);
    //}

    //public async Task BroadcastText(string text)
    //{
    //    if (connectionsNgroup.ContainsKey(Context.ConnectionId))
    //    {
    //        await Clients.OthersInGroup(connectionsNgroup[Context.ConnectionId]).SendAsync("ReceiveText", text);
    //    }
    //}

    public async Task<string> JoinGroup(string uniqueId)
    {
        Console.WriteLine("Joining group: " + uniqueId);

        if (connectionsNgroup.ContainsKey(Context.ConnectionId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, connectionsNgroup[Context.ConnectionId]);
            connectionsNgroup.Remove(Context.ConnectionId);
        }
        connectionsNgroup.Add(Context.ConnectionId, uniqueId);
        await Groups.AddToGroupAsync(Context.ConnectionId, uniqueId);

        var snippet = await _context.CodeSnippets.SingleOrDefaultAsync(s => s.UniqueId == uniqueId);
        return snippet?.Code ?? string.Empty;
    }
}
