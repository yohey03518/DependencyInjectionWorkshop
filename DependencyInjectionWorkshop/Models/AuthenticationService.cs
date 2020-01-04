using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpService _otpService;
        private readonly FailCounter _failCounter;
        private readonly SlackAdapter _slackAdapter;
        private readonly NLogAdapter _nLogAdapter;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _failCounter = new FailCounter();
            _slackAdapter = new SlackAdapter();
            _nLogAdapter = new NLogAdapter();
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

        private void RecordFailCountLog(string accountId, HttpClient httpClient)
        {
            // record fail count log
            var failedCount = _failCounter.GetFailedCount(accountId, httpClient);
            _nLogAdapter.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}