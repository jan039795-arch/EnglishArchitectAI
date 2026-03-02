using Microsoft.JSInterop;

namespace EA.Client.Services;

public class NotificationService
{
    private readonly IJSRuntime _js;

    public NotificationService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<string> RequestPermissionAsync()
    {
        try
        {
            return await _js.InvokeAsync<string>("notificationInterop.requestPermission");
        }
        catch
        {
            return "error";
        }
    }

    public async Task<bool> ScheduleReminderAsync(string title, string body, int delayMs)
    {
        try
        {
            return await _js.InvokeAsync<bool>("notificationInterop.scheduleNotification", title, body, delayMs);
        }
        catch
        {
            return false;
        }
    }
}
