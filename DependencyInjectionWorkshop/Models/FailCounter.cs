using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailCounter
    {
        void Reset(string accountId);
        void AddFailCount(string accountId);
        int GetFailedCount(string accountId);
        bool GetIsLocked(string accountId);
    }

    public class FailCounter : IFailCounter
    {
        public FailCounter()
        {
        }

        public void Reset(string accountId)
        {
            var resetResponse = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}
                .PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public void AddFailCount(string accountId)
        {
            //失敗
            var addFailedCountResponse = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}
                .PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public int GetFailedCount(string accountId)
        {
            //紀錄失敗次數 
            var failedCountResponse =
                new HttpClient() {BaseAddress = new Uri("http://joey.com/")}
                    .PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        public bool GetIsLocked(string accountId)
        {
            var isLockedResponse1 = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}
                .PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;

            isLockedResponse1.EnsureSuccessStatusCode();
            var isLockedResponse = isLockedResponse1;
            return isLockedResponse.Content.ReadAsAsync<bool>().Result;
        }
    }
}