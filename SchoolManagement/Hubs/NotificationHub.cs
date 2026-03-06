using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace SchoolManagement.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}
