namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator: IAuthenticationService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly INotification _notification;


        public NotificationDecorator(IAuthenticationService authenticationService, INotification notification)
        {
            _authenticationService = authenticationService;
            _notification = notification;
        }

        private void Notify(string accountId)
        {
            _notification.Notify(accountId);
        }

        public bool Verify(string accountId, string inputPwd, string otp)
        {
            var isValid = _authenticationService.Verify(accountId, inputPwd, otp);
            if (!isValid)
            {
                _notification.Notify(accountId);
            }

            return isValid;
        }
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IProfile _profileDao;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly IFailCounter _failCounter;
        private readonly ILogger _logger;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _failCounter = new FailCounter();
            _logger = new NLogAdapter();
        }

        public AuthenticationService(IProfile profileDao, IHash hash, IOtpService otpService,
            IFailCounter failCounter, INotification notification, ILogger logger)
        {
            _profileDao = profileDao;
            _hash = hash;
            _otpService = otpService;
            _failCounter = failCounter;
            _logger = logger;
        }

        public bool Verify(string accountId, string inputPwd, string otp)
        {
            var isLocked = _failCounter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() {AccountId = accountId};
            }

            var pwdInDb = _profileDao.GetPassword(accountId);
            var hashedInputPWd = _hash.Compute(inputPwd);
            var currentOtp = _otpService.GetCurrentOtp(accountId);
            if (pwdInDb == hashedInputPWd && currentOtp == otp)
            {
                _failCounter.Reset(accountId);
                return true;
            }
            else
            {
                _failCounter.AddFailCount(accountId);
                RecordFailCountLog(accountId);
                return false;
            }
        }

        private void RecordFailCountLog(string accountId)
        {
            // record fail count log
            var failedCount = _failCounter.GetFailedCount(accountId);
            _logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}