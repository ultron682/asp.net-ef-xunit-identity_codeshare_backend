using Microsoft.AspNetCore.SignalR;
using CodeShareBackend.Data;
using CodeShareBackend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Xml.Linq;


public class CodeShareHub : Hub {
    private readonly ApplicationDbContext _context;
    private readonly UserManager<UserCodeShare> _userManager;

    private static Dictionary<string, string> _connectionsNgroup = [];

    private static Dictionary<string, Text> CurrentOpenDocuments = [];

    public CodeShareHub(ApplicationDbContext context, UserManager<UserCodeShare> userManager) {
        _context = context;
        _userManager = userManager;
    }

    public async Task JoinToDocument(string uniqueId) {
        Console.WriteLine("JoinDocument: " + uniqueId + " : " + Context.ConnectionId);

        if (_connectionsNgroup.ContainsKey(Context.ConnectionId)) {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, _connectionsNgroup[Context.ConnectionId]);
            _connectionsNgroup.Remove(Context.ConnectionId);
        }

        _connectionsNgroup.Add(Context.ConnectionId, uniqueId);

        await Groups.AddToGroupAsync(Context.ConnectionId, uniqueId);
        printConnectionsNgroup();

        var snippet = await _context.CodeSnippets.Include(l => l.SelectedLang).Select(
            s => new { s.Code, s.ExpiryDate, s.Id, s.SelectedLang, s.UniqueId, owner = (s.User != null) ? new { nickname = s.User!.UserName, userId = s.User!.Id } : null, s.ReadOnly }).SingleOrDefaultAsync(s => s.UniqueId == uniqueId);

        if (!CurrentOpenDocuments.ContainsKey(uniqueId)) {
            CurrentOpenDocuments[uniqueId] = new Text(snippet == null ? string.Empty : snippet.Code!);
        }

        await Clients.Caller.SendAsync("ReceiveDocument", snippet);
    }

    public async Task PushUpdate(string changeSetJson, string userId) {
        if (_connectionsNgroup.ContainsKey(Context.ConnectionId) == false) {
            Console.WriteLine("Connection not in group");
            return;
        }

        var changeSet = ChangeSet.FromJSON(changeSetJson);
        if (changeSet != null) {
            var uniqueId = _connectionsNgroup[Context.ConnectionId];
            //Console.WriteLine(uniqueId);

            var document = CurrentOpenDocuments[uniqueId].ApplyChangeSet(changeSet);

            CurrentOpenDocuments[uniqueId] = document;

            var snippet = await _context.CodeSnippets.Include(u => u.SelectedLang).SingleOrDefaultAsync(s => s.UniqueId == uniqueId);

            if (snippet?.ReadOnly == true && userId != snippet.UserId)
                return;

            if (snippet == null) {
                //Console.WriteLine("userId: " + userId);
                snippet = new CodeSnippet { UniqueId = uniqueId, Code = document.ToString(), UserId = (userId == string.Empty ? null : userId) };
                _context.CodeSnippets.Add(snippet);
            }
            else {
                snippet.Code = document.ToString();
                _context.CodeSnippets.Update(snippet);
            }


            if (_connectionsNgroup.ContainsKey(Context.ConnectionId))
                await Clients.OthersInGroup(uniqueId).SendAsync("ReceiveUpdate", changeSetJson);

            await _context.SaveChangesAsync();
        }
        else {
            Console.WriteLine("Invalid ChangeSet JSON (null)");
        }
    }

    public async Task ChangeCodeSnippetProperty(string property, string value, string userId) {
        var uniqueId = _connectionsNgroup[Context.ConnectionId];

        var snippet = await _context.CodeSnippets.Include(u => u.SelectedLang).SingleOrDefaultAsync(s => s.UniqueId == uniqueId);

        if (snippet == null)
            return;

        if (snippet.UserId == null || snippet.UserId != userId) // only owner can change doc property
            return;

        switch (property) {
            case "lang": {
                    if (snippet != null) {
                        snippet.SelectedLang = _context.ProgLanguages.SingleOrDefault(s => s.Name == value);
                        _context.CodeSnippets.Update(snippet);
                    }

                    await _context.SaveChangesAsync();
                }
                break;

            case "readOnly": {
                    if (snippet != null) {
                        snippet.ReadOnly = (value == "true");
                        _context.CodeSnippets.Update(snippet);
                    }

                    await _context.SaveChangesAsync();
                }
                break;
        }

        if (_connectionsNgroup.ContainsKey(Context.ConnectionId))
            await Clients.OthersInGroup(uniqueId).SendAsync("OnCodeSnippetPropertyChange", property, value);
    }

    public override async Task OnDisconnectedAsync(Exception? exception) {
        if (_connectionsNgroup.ContainsKey(Context.ConnectionId)) {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, _connectionsNgroup[Context.ConnectionId]);
            _connectionsNgroup.Remove(Context.ConnectionId);
        }

        printConnectionsNgroup();
        await base.OnDisconnectedAsync(exception);
    }

    void printConnectionsNgroup() {
        Console.WriteLine("\n--------------------------");
        foreach (var item in _connectionsNgroup) {
            Console.WriteLine(item.Value + " : " + item.Key);
        }
        Console.WriteLine("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^\n");
    }
}
