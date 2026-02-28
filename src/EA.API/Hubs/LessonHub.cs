using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EA.API.Hubs;

[Authorize]
public class LessonHub : Hub
{
    public async Task SendProgress(string lessonId, int score)
    {
        await Clients.User(Context.UserIdentifier!)
            .SendAsync("ReceiveProgress", lessonId, score);
    }
}
