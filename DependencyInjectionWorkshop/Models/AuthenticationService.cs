using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string accountId, string inputPwd, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var isLocked = GetAccountIsLocked(accountId, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }

            var pwdInDb = GetPasswordFromDatabase(accountId);
            var hashedInputPWd = GetHashedPassword(inputPwd);
            var currentOtp = GetCurrentOtp(accountId, httpClient);
            if (pwdInDb == hashedInputPWd && currentOtp == otp)
            {
                var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
                resetResponse.EnsureSuccessStatusCode();
                return true;
            }
            else
            {
                AddFailCount(accountId, httpClient);
                RecordFailCountLog(accountId, httpClient);
                Notify(accountId);
                return false;
            }
        }

        private static bool GetAccountIsLocked(string accountId, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }

        private static void Notify(string accountId)
        {
            string message = $"account: {accountId} try to login failed.";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(messageResponse => { }, "my channel", message, "my bot name");
        }

        private static void RecordFailCountLog(string accountId, HttpClient httpClient)
        {
            // record fail count log
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
            failedCountResponse.EnsureSuccessStatusCode();
            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }

        private static void AddFailCount(string accountId, HttpClient httpClient)
        {
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        private static string GetCurrentOtp(string accountId, HttpClient httpClient)
        {
            var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            return currentOtp;
        }

        private static string GetHashedPassword(string inputPwd)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(inputPwd));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashedInputPWd = hash.ToString();
            return hashedInputPWd;
        }

        private static string GetPasswordFromDatabase(string accountId)
        {
            string pwdInDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                pwdInDb = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return pwdInDb;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}