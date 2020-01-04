﻿namespace DependencyInjectionWorkshop.Models
{
    public class NLogAdapter
    {
        public NLogAdapter()
        {
        }

        public void Info(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}