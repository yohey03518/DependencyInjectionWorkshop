using System;
using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string DefaultAccountId = "erwin";
        private const string HashedPassword = "hashedPwd";
        private const string InputPassword = "pwd";
        private const int FailCount = 91;

        private AuthenticationService _authenticationService;

        private IProfile _fakeProfile;

        private IHash _fakeHash;

        private IOtpService _fakeOtpService;

        private ILogger _fakeLogger;

        private IFailCounter _fakeFailCounter;

        private INotification _fakeNotification;

        [SetUp]
        public void SetUp()
        {
            _fakeProfile = Substitute.For<IProfile>();
            _fakeHash = Substitute.For<IHash>();
            _fakeOtpService = Substitute.For<IOtpService>();
            _fakeLogger = Substitute.For<ILogger>();
            _fakeFailCounter = Substitute.For<IFailCounter>();
            _fakeNotification = Substitute.For<INotification>();
            _authenticationService = new AuthenticationService(_fakeProfile, _fakeHash, _fakeOtpService,
                _fakeFailCounter, _fakeNotification, _fakeLogger);
        }

        [Test]
        public void is_valid()
        {
            GivenPasswordInDb(DefaultAccountId, HashedPassword);
            GivenHashedPassword(InputPassword, HashedPassword);
            GivenOtp(DefaultAccountId, "1234");

            ShouldBeValid(DefaultAccountId, InputPassword, "1234");
        }

        [Test]
        public void is_invalid()
        {
            GivenPasswordInDb(DefaultAccountId, HashedPassword);
            GivenHashedPassword(InputPassword, HashedPassword);
            GivenOtp(DefaultAccountId, "1234");

            ShouldBeInvalid(DefaultAccountId, InputPassword, "wrong otp");
        }

        [Test]
        public void reset_fail_count_when_valid()
        {
            WhenLoginValid(DefaultAccountId);

            ShouldResetFailCount(DefaultAccountId);
        }

        [Test]
        public void add_fail_count_when_invalid()
        {
            WhenAccountInvalid(DefaultAccountId);

            ShouldAddFailCount(DefaultAccountId);
        }

        [Test]
        public void log_fail_count_when_invalid()
        {
            _fakeFailCounter.GetFailedCount(DefaultAccountId).Returns(FailCount);
            WhenAccountInvalid(DefaultAccountId);

            LogShouldContains(DefaultAccountId, FailCount.ToString());
        }

        [Test]
        public void notify_when_invalid()
        {
            WhenAccountInvalid(DefaultAccountId);

            ShouldNotify(DefaultAccountId);
        }

        private void ShouldNotify(string accountId)
        {
            _fakeNotification.Received(1).Notify(accountId);
        }

        private void LogShouldContains(string accountId, string failCount)
        {
            _fakeLogger.Received(1).Info(Arg.Is<string>(message =>
                message.Contains(accountId) && message.Contains(failCount)));
        }

        private void ShouldAddFailCount(string accountId)
        {
            _fakeFailCounter.Received(1).AddFailCount(accountId);
        }

        private void WhenAccountInvalid(string accountId)
        {
            GivenPasswordInDb(accountId, HashedPassword);
            GivenHashedPassword(InputPassword, HashedPassword);
            GivenOtp(accountId, "1234");

            _authenticationService.Verify(accountId, InputPassword, "wrong otp");
        }

        private void ShouldResetFailCount(string accountId)
        {
            _fakeFailCounter.Received(1).Reset(accountId);
        }

        private void WhenLoginValid(string accountId)
        {
            GivenPasswordInDb(accountId, HashedPassword);
            GivenHashedPassword(InputPassword, HashedPassword);
            GivenOtp(accountId, "1234");

            _authenticationService.Verify(accountId, InputPassword, "1234");
        }


        private void ShouldBeInvalid(string accountId, string inputPassword, string otp)
        {
            var isValid = _authenticationService.Verify(accountId, inputPassword, otp);
            Assert.IsFalse(isValid);
        }

        private void ShouldBeValid(string accountId, string inputPwd, string otp)
        {
            var isValid = _authenticationService.Verify(accountId, inputPwd, otp);
            Assert.IsTrue(isValid);
        }

        private void GivenOtp(string accountId, string otp)
        {
            _fakeOtpService.GetCurrentOtp(accountId).Returns(otp);
        }

        private void GivenHashedPassword(string inputPwd, string hashedPassword)
        {
            _fakeHash.Compute(inputPwd).Returns(hashedPassword);
        }

        private void GivenPasswordInDb(string accountId, string passwordInDb)
        {
            _fakeProfile.GetPassword(accountId).Returns(passwordInDb);
        }
    }
}