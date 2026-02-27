using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using System;

namespace HazelnutVeb.Services
{
    public class PushNotificationService
    {
        public async Task SendAsync(string token, string title, string body)
        {
            if (string.IsNullOrEmpty(token)) return;

            try
            {
                var message = new Message()
                {
                    Token = token,
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body
                    }
                };

                await FirebaseMessaging.DefaultInstance.SendAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending push notification: {ex.Message}");
            }
        }
    }
}
