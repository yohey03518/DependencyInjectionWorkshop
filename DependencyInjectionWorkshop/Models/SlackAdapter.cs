﻿using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class SlackAdapter
    {
        public SlackAdapter()
        {
        }

        public void Notify(string accountId)
        {
            string message = $"account: {accountId} try to login failed.";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(messageResponse => { }, "my channel", message, "my bot name");
        }
    }
}