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

        public AuthenticationService(ProfileDao profileDao, Sha256Adapter sha256Adapter, OtpService otpService, FailCounter failCounter, SlackAdapter slackAdapter, NLogAdapter nLogAdapter)
        {
            _profileDao = profileDao;
            _sha256Adapter = sha256Adapter;
            _otpService = otpService;
            _failCounter = failCounter;
            _slackAdapter = slackAdapter;
            _nLogAdapter = nLogAdapter;
        }

        public bool Verify(string accountId, string inputPwd, string otp)
        {
            var isLocked = _failCounter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() {AccountId = accountId};
            }

            var pwdInDb = _profileDao.GetPasswordFromDatabase(accountId);
            var hashedInputPWd = _sha256Adapter.GetHashedPassword(inputPwd);
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
                _slackAdapter.Notify(accountId);
                return false;
            }
        }

        private void RecordFailCountLog(string accountId)
        {
            // record fail count log
            var failedCount = _failCounter.GetFailedCount(accountId);
            _nLogAdapter.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}