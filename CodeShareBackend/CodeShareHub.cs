using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CodeShareBackend
{
    public class CodeShareHub : Hub
    {
        public async Task SendCode(string code)
        {
            await Clients.Others.SendAsync("ReceiveCode", code);
        }
    }

}
