using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CodeShareBackend
{
    public class CodeShareHub : Hub
    {
        public async Task SendCode(string code)
        {
            Debug.WriteLine(code);
            await Clients.Others.SendAsync("ReceiveCode", code);
        }
    }

}
