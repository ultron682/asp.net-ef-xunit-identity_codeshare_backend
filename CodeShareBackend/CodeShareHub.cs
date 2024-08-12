using Microsoft.AspNetCore.SignalR;
using CodeShareBackend.Data;
using CodeShareBackend.Models;
using Microsoft.AspNetCore.Identity;


public class CodeShareHub : Hub
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private static Dictionary<string, string> _connectionsNgroup = new Dictionary<string, string>();


    private static Dictionary<string, Text> Documents = new Dictionary<string, Text>();
    private static Dictionary<string, List<ChangeSet>> DocumentChanges = new Dictionary<string, List<ChangeSet>>();

    public async Task JoinDocument(string documentId)
    {
        if (!Documents.ContainsKey(documentId))
        {
            Documents[documentId] = new Text("Start document");
            DocumentChanges[documentId] = new List<ChangeSet>();
        }

        await Clients.Caller.SendAsync("ReceiveDocument", Documents[documentId].ToString(), DocumentChanges[documentId]);
    }

    public async Task PushUpdate(string documentId, string changeSetJson)
    {
        var changeSet = ChangeSet.FromJSON(changeSetJson);
        var document = Documents[documentId].ApplyChangeSet(changeSet);

        Documents[documentId] = document;
        DocumentChanges[documentId].Add(changeSet);

        await Clients.OthersInGroup(documentId).SendAsync("ReceiveUpdate", changeSetJson);
    }

    public async Task SubscribeDocument(string documentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, documentId);
    }

    public async Task UnsubscribeDocument(string documentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, documentId);
    }



    public CodeShareHub(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    //public async Task<CodeSnippet> JoinGroup(string uniqueId)
    //{
    //    Console.WriteLine("JoinGroup: " + uniqueId + " : " + Context.ConnectionId);
    //    if (_connectionsNgroup.ContainsKey(Context.ConnectionId))
    //    {
    //        await Groups.RemoveFromGroupAsync(Context.ConnectionId, _connectionsNgroup[Context.ConnectionId]);
    //        _connectionsNgroup.Remove(Context.ConnectionId);
    //    }
    //    _connectionsNgroup.Add(Context.ConnectionId, uniqueId);
    //    printConnectionsNgroup();

    //    await Groups.AddToGroupAsync(Context.ConnectionId, uniqueId);

    //    var snippet = await _context.CodeSnippets.Include(l => l.SelectedLang).SingleOrDefaultAsync(s => s.UniqueId == uniqueId);
    //    //Console.WriteLine(snippet?.Code);
    //    return snippet;
    //}

    //public async Task BroadcastText(string uniqueId, string code, string userId)
    //{
    //    var snippet = await _context.CodeSnippets.Include(u => u.SelectedLang).SingleOrDefaultAsync(s => s.UniqueId == uniqueId);
    //    if (snippet == null)
    //    {
    //        //Console.WriteLine("userId: " + userId);
    //        snippet = new CodeSnippet { UniqueId = uniqueId, Code = code, UserId = userId == string.Empty ? null : userId };
    //        _context.CodeSnippets.Add(snippet);
    //    }
    //    else
    //    {
    //        snippet.Code = code;
    //        _context.CodeSnippets.Update(snippet);
    //    }
    //    await _context.SaveChangesAsync();

    //    if (_connectionsNgroup.ContainsKey(Context.ConnectionId))
    //    {
    //        //Console.WriteLine("wyslano: " + code);
    //        await Clients.OthersInGroup(_connectionsNgroup[Context.ConnectionId]).SendAsync("ReceivedCode", code);
    //    }
    //}

    //public async Task UpdateSnippetLine(string uniqueId, int lineNumber, string newLineContent)
    //{
    //    var snippet = await _context.CodeSnippets.SingleOrDefaultAsync(s => s.UniqueId == uniqueId);

    //    if (snippet == null)
    //    {
    //        throw new HubException("Snippet not found");
    //    }

    //    // Split code into lines
    //    var lines = snippet.Code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

    //    // Check if lineNumber is valid
    //    if (lineNumber < 0 || lineNumber >= lines.Length)
    //    {
    //        throw new HubException("Line number is out of range.");
    //    }

    //    // Update the specified line
    //    lines[lineNumber] = newLineContent;
    //    Console.WriteLine(newLineContent + " on " + lineNumber);

    //    // Join lines back into a single string
    //    snippet.Code = string.Join(Environment.NewLine, lines);

    //    _context.CodeSnippets.Update(snippet);
    //    await _context.SaveChangesAsync();

    //    if (_connectionsNgroup.ContainsKey(Context.ConnectionId))
    //    {
    //        await Clients.OthersInGroup(_connectionsNgroup[Context.ConnectionId]).SendAsync("ReceivedCode", snippet.Code);
    //    }
    //}

    //public async Task UpdateSnippetLine(string uniqueId, string userId, int lineNumber, string newLineContent)
    //{
    //    //Console.WriteLine("UpdateSnippetLine" + uniqueId + " " + lineNumber + " " + newLineContent);
    //    var snippet = await _context.CodeSnippets.SingleOrDefaultAsync(s => s.UniqueId == uniqueId);

    //    //if (snippet == null)
    //    //{
    //    //    throw new HubException("Snippet not found");
    //    //}
    //    if (snippet == null)
    //    {
    //        //Console.WriteLine("userId: " + userId);
    //        snippet = new CodeSnippet { UniqueId = uniqueId, Code = newLineContent, UserId = userId == string.Empty ? null : userId };
    //        _context.CodeSnippets.Add(snippet);

    //        Console.WriteLine(newLineContent + " created new " + lineNumber);
    //    }
    //    else
    //    {
    //        // Split code into lines
    //        var lines = snippet!.Code!.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

    //        //foreach (var item in lines)
    //        //{
    //        //    Console.WriteLine(item);
    //        //}

    //        if (lines.Count > 0 && lineNumber < lines.Count)
    //        {
    //            // Update the specified line
    //            lines[lineNumber] = newLineContent;
    //            Console.WriteLine(newLineContent + " on line " + lineNumber);
    //        }
    //        else
    //        {
    //            lines.Add(newLineContent);
    //            Console.WriteLine(newLineContent + " added on line" + lineNumber);
    //        }

    //        snippet.Code = string.Join(Environment.NewLine, lines);

    //        //Console.WriteLine("\nChanged to: \n" + snippet.Code);

    //        _context.CodeSnippets.Update(snippet);
    //    }

    //    await _context.SaveChangesAsync();

    //    if (_connectionsNgroup.ContainsKey(Context.ConnectionId))
    //    {
    //        await Clients.OthersInGroup(_connectionsNgroup[Context.ConnectionId]).SendAsync("ReceivedNewLineCode", lineNumber, newLineContent);
    //    }
    //}

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
