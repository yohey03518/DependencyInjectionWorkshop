using System;

namespace DependencyInjectionWorkshop.Models
{
    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}