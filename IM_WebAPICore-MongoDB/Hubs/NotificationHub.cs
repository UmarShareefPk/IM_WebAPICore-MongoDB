using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM_DataAccess.DataService;
using Microsoft.AspNetCore.SignalR;


namespace IM.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly IUserService _userService;
        private static List<string> users = new List<string>();
        public NotificationHub(IUserService userService)
        {
            _userService = userService;
        }

        public override Task OnConnectedAsync()
        {
            users.Add(Context.ConnectionId);
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            users.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public void Send( string message)
        {
            string s = "iGD7rER_rNOT44SNeTLbfA";
            List<string> listData = new List<string>();
            listData.Add(s);
            s = "Mm65FIntNoSYFzhtvvXmuw";
            listData.Add(s);
            IReadOnlyList<string> readOnlyData = listData.AsReadOnly();
            // Call the broadcastMessage method to update clients.
            Clients.Client(s).SendAsync("ReceiveMessage", message + " | " + DateTime.Now);
           //Clients.All.SendAsync("ReceiveMessage", message + " | " + DateTime.Now);

        }

        public async Task SendIncidentUpdate(string incidentId , string userId)
        {
            List<string> hubIds = await _userService.GetHubIdsAsync(incidentId , userId);

            if (hubIds is null)
                return;           

            foreach(string id in hubIds)
            {
                try
                {
                    await Clients.Client(id).SendAsync("UpdateNotifications", incidentId);                 
                }
                catch(Exception ex)  {  }
            }
        }

        public async Task SendMessageAsync(string conversationId, string userId, object newMessage, bool isNewConversation)
        {
            string hubId = await _userService.GetHubIdByUserIdAsync(userId);

            if (isNewConversation)
            {
                await Clients.Client(hubId).SendAsync("ReceiveNewConversation", newMessage);
            }
            else
            {
                await Clients.Client(hubId).SendAsync("ReceiveNewMessage", newMessage);
            }

        }

    }
}
