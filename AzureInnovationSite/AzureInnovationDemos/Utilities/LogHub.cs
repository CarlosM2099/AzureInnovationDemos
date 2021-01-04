using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace AzureInnovationDemos.Utilities
{
    public class LogHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage",$"{message}");
        } 
    }   
}
