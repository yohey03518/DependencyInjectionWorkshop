using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfileDao _profileDao;
        private readonly IFailCounter _failCounter;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly ILogger _logger;
        private readonly INotify _notification;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _failCounter = new FailCounter();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _logger = new NLogAdapter();
            _notification = new SlackAdapter();
        }

        public AuthenticationService(IProfileDao profileDao, IFailCounter failCounter, IHash hash, IOtpService otpService, ILogger logger, INotify notification)
        {
            _profileDao = profileDao;
            _failCounter = failCounter;
            _hash = hash;
            _otpService = otpService;
            _logger = logger;
            _notification = notification;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isLockedResponse = _failCounter.GetAccountIsLocked(accountId);
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }

            var passwordFromDb = _profileDao.GetPassword(accountId);

            var hashedPassword = _hash.GetHash(password);

            //get otp
            var currentOtp = _otpService.GetCurrentOtp(accountId);

            //compare
            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                _failCounter.Reset(accountId);

                return true;
            }
            else
            {
                _failCounter.AddFailCount(accountId);

                LogFailCount(accountId);

                _notification.Notify(accountId);

                return false;
            }
        }

        private void LogFailCount(string accountId)
        {
            var failedCount = _failCounter.GetFailedCount(accountId);
            _logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}