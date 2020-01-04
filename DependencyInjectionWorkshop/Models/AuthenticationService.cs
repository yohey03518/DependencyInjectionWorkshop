using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpService _otpService;
        private readonly FailCounter _failCounter;
        private readonly SlackAdapter _slackAdapter;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _failCounter = new FailCounter();
            _slackAdapter = new SlackAdapter();
        }

        public bool Verify(string accountId, string inputPwd, string otp)
        {
            var httpClient = new HttpClient() {BaseAddress = new Uri("http://joey.com/")};
            var isLocked = _failCounter.GetAccountIsLocked(accountId, httpClient);
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
                _slackAdapter.Notify(accountId);
                return false;
            }
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
}