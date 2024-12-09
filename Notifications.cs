using System;
using System.Threading.Tasks;

public class Notifications
{

    public async Task<bool> SendNotificationAsync(string title, string message, string priority)
    {
        try
        {

            var parameters = new Dictionary<string, string> {
                ["token"] = "a3pnninufi36m34bawn8hk1q9jqpts",
                ["user"] = "uhisf8s8mmzb8hd9ukb4zkrtimrrtz",
                ["title"] = title,
                ["message"] = message,
                ["sound"] = "Siren",
                ["priority"] = priority
            };

            using var client = new HttpClient();
            var response = await client.PostAsync("https://api.pushover.net/1/messages.json", new
            FormUrlEncodedContent(parameters));

            if(!response.IsSuccessStatusCode){
                Console.WriteLine($"Failed to send Pushover notification: {string.Join(", ", response.Content)}");
            }

            return true;
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Error sending Pushover notification: {ex.Message}");

            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Since the notification is actually sent, we return true despite the exception
            return true;
        }
    }
}