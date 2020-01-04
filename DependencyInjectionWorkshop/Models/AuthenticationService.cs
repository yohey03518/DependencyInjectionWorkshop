using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao;
        private readonly FailCounter _failCounter;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpService _otpService;
        private readonly NLoggerAdapter _nLoggerAdapter;
        private readonly SlackAdapter _slackAdapter;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _failCounter = new FailCounter();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _nLoggerAdapter = new NLoggerAdapter();
            _slackAdapter = new SlackAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isLockedResponse = _failCounter.GetAccountIsLocked(accountId);
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }

            var passwordFromDb = _profileDao.GetPasswordFromDb(accountId);

            var hashedPassword = _sha256Adapter.GetHashedPassword(password);

            //get otp
            var currentOtp = _otpService.GetCurrentOtp(accountId);

            //compare
            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                _failCounter.ResetFailCount(accountId);

                return true;
            }
            else
            {
                _failCounter.AddFailCount(accountId);

                LogFailCount(accountId);

                _slackAdapter.Notify(accountId);

                return false;
            }
        }

        private void LogFailCount(string accountId)
        {
            var failedCount = _failCounter.GetFailedCount(accountId);
            _nLoggerAdapter.LogInfo($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}