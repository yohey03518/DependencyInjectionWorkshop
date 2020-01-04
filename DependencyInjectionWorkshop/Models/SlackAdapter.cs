using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public interface INotify
    {
        void Notify(string accountId);
    }

    public class SlackAdapter : INotify
    {
        public SlackAdapter()
        {
        }

        public void Notify(string accountId)
        {
            //notify
            string message = $"account:{accountId} try to login failed";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(messageResponse => { }, "my channel", message, "my bot name");
        }
    }
}