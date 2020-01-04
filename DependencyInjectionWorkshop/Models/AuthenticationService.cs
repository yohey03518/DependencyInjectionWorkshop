using System;
using System.Net.Http;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpService _otpService;
        private readonly FailCounter _failCounter;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _failCounter = new FailCounter();
        }

        public bool Verify(string accountId, string inputPwd, string otp)
        {
            var httpClient = new HttpClient() {BaseAddress = new Uri("http://joey.com/")};
            var isLocked = GetAccountIsLocked(accountId, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() {AccountId = accountId};
            }

            var pwdInDb = _profileDao.GetPasswordFromDatabase(accountId);
            var hashedInputPWd = _sha256Adapter.GetHashedPassword(inputPwd);
            var currentOtp = _otpService.GetCurrentOtp(accountId, httpClient);
            if (pwdInDb == hashedInputPWd && currentOtp == otp)
            {
                _failCounter.Reset(accountId, httpClient);
                return true;
            }
            else
            {
                _failCounter.AddFailCount(accountId, httpClient);
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
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}