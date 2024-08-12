using Microsoft.AspNetCore.SignalR;
using CodeShareBackend.Data;
using CodeShareBackend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


public class CodeShareHub : Hub
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    private static Dictionary<string, string> _connectionsNgroup = [];

    private static Dictionary<string, Text> CurrentOpenDocuments = [];
    private static Dictionary<string, List<ChangeSet>> CurrentDocumentChanges = [];

    public CodeShareHub(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task JoinDocument(string uniqueId)
    {
        Console.WriteLine("JoinDocument: " + uniqueId + " : " + Context.ConnectionId);

        if (_connectionsNgroup.ContainsKey(Context.ConnectionId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, _connectionsNgroup[Context.ConnectionId]);
            _connectionsNgroup.Remove(Context.ConnectionId);
        }
        _connectionsNgroup.Add(Context.ConnectionId, uniqueId);

        await Groups.AddToGroupAsync(Context.ConnectionId, uniqueId);
        printConnectionsNgroup();

        var snippet = await _context.CodeSnippets.Include(l => l.SelectedLang).SingleOrDefaultAsync(s => s.UniqueId == uniqueId);

        if (!CurrentOpenDocuments.ContainsKey(uniqueId))
        {
            CurrentOpenDocuments[uniqueId] = new Text(snippet == null ? string.Empty : snippet.Code!);
            CurrentDocumentChanges[uniqueId] = [];
        }

        await Clients.Caller.SendAsync("ReceiveDocument", snippet);
    }

    public async Task PushUpdate(string changeSetJson, string userId)
    {
        if (_connectionsNgroup.ContainsKey(Context.ConnectionId) == false)
        {
            Console.WriteLine("Connection not in group");
            return;
        }

        var changeSet = ChangeSet.FromJSON(changeSetJson);
        if (changeSet != null)
        {
            var uniqueId = _connectionsNgroup[Context.ConnectionId];
            //Console.WriteLine(uniqueId);

            var document = CurrentOpenDocuments[uniqueId].ApplyChangeSet(changeSet);

            CurrentOpenDocuments[uniqueId] = document;
            CurrentDocumentChanges[uniqueId].Add(changeSet);

            if (_connectionsNgroup.ContainsKey(Context.ConnectionId))
                await Clients.OthersInGroup(uniqueId).SendAsync("ReceiveUpdate", changeSetJson);

            var snippet = await _context.CodeSnippets.Include(u => u.SelectedLang).SingleOrDefaultAsync(s => s.UniqueId == uniqueId);
            if (snippet == null)
            {
                //Console.WriteLine("userId: " + userId);
                snippet = new CodeSnippet { UniqueId = uniqueId, Code = document.ToString(), UserId = (userId == string.Empty ? null : userId) };
                _context.CodeSnippets.Add(snippet);
            }
            else
            {
                snippet.Code = document.ToString();
                _context.CodeSnippets.Update(snippet);
            }

            await _context.SaveChangesAsync();
        }
        else
        {
            Console.WriteLine("Invalid ChangeSet JSON (null)");
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
        Console.WriteLine("\n--------------------------");
        foreach (var item in _connectionsNgroup)
        {
            Console.WriteLine(item.Value + " : " + item.Key);
        }
        Console.WriteLine("--------------------------\n");
    }
}
