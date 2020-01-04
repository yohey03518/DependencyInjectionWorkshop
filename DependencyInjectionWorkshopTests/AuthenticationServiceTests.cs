﻿using System;
using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string DefaultAccountId = "erwin";
        private const string HashedPassword = "hashedPassword";
        private const int FailCount = 100;

        [SetUp]
        public void SetUp()
        {
            _otpService = Substitute.For<IOtpService>();
            _logger = Substitute.For<ILogger>();
            _failCounter = Substitute.For<IFailCounter>();
            _hash = Substitute.For<IHash>();
            _notification = Substitute.For<INotify>();
            _profile = Substitute.For<IProfile>();
            _authenticationService =
                new AuthenticationService(_profile, _failCounter, _hash, _otpService, _logger, _notification);
        }

        private IOtpService _otpService;
        private ILogger _logger;
        private IFailCounter _failCounter;
        private IHash _hash;
        private INotify _notification;
        private IProfile _profile;
        private AuthenticationService _authenticationService;

        [Test]
        public void is_valid()
        {
            GivenStoredHashPassword(DefaultAccountId, HashedPassword);
            GivenHashResult("password", HashedPassword);
            GivenOtp(DefaultAccountId, "1234");

            ShouldBeValid(DefaultAccountId, "password", "1234");
        }

        [Test]
        public void is_invalid()
        {
            GivenStoredHashPassword(DefaultAccountId, HashedPassword);
            GivenHashResult("password", HashedPassword);
            GivenOtp(DefaultAccountId, "1234");

            ShouldBeInvalid(DefaultAccountId, "password", "wrong otp");
        }

        [Test]
        public void reset_fail_count_when_valid()
        {
            WhenValidAccountVerify(DefaultAccountId);

            ShouldResetFailCount(DefaultAccountId);
        }


        [Test]
        public void add_fail_count_when_invalid()
        {
            WhenInvalidAccountVerify(DefaultAccountId);

            ShouldAddFailCount(DefaultAccountId);
        }

        [Test]
        public void log_fail_count_when_invalid()
        {
            GivenFailCount(DefaultAccountId, FailCount);
            WhenInvalidAccountVerify(DefaultAccountId);

            LogShouldContains(DefaultAccountId, FailCount);
        }

        [Test]
        public void notify_user_when_invalid()
        {
            WhenInvalidAccountVerify(DefaultAccountId);

            ShouldNotifyUser(DefaultAccountId);
        }

        [Test]
        public void account_is_lock()
        {
            GivenAccountIsLocked(DefaultAccountId, true);
            ShouldThrow<FailedTooManyTimesException>();
        }

        private void ShouldThrow<FailedTooManyTimesException>() where FailedTooManyTimesException : Exception
        {
            TestDelegate action = () => _authenticationService.Verify(DefaultAccountId, "password", "otp");
            Assert.Throws<FailedTooManyTimesException>(action);
        }

        private void GivenAccountIsLocked(string accountId, bool isLocked)
        {
            _failCounter.GetIsLocked(accountId).Returns(isLocked);
        }

        private void ShouldNotifyUser(string accountId)
        {
            _notification.Received(1).Notify(accountId);
        }

        private void LogShouldContains(string accountId, int failCount)
        {
            _logger.Received(1)
                .Info(Arg.Is<string>(message => message.Contains(failCount.ToString()) && message.Contains(accountId)));
        }

        private void GivenFailCount(string accountId, int failCount)
        {
            _failCounter.GetFailedCount(accountId).Returns(failCount);
        }

        private void ShouldAddFailCount(string accountId)
        {
            _failCounter.AddFailCount(accountId);
        }

        private void WhenInvalidAccountVerify(string accountId)
        {
            GivenStoredHashPassword(accountId, HashedPassword);
            GivenHashResult("password", HashedPassword);
            GivenOtp(accountId, "1234");

            _authenticationService.Verify(accountId, "password", "wrong otp");
        }

        private void ShouldResetFailCount(string accountId)
        {
            _failCounter.Received(1).Reset(accountId);
        }

        private void WhenValidAccountVerify(string accountId)
        {
            GivenStoredHashPassword(accountId, HashedPassword);
            GivenHashResult("password", HashedPassword);
            GivenOtp(accountId, "1234");

            _authenticationService.Verify(accountId, "password", "1234");
        }

        private void ShouldBeInvalid(string accountId, string password, string otp)
        {
            var isValid = _authenticationService.Verify(accountId, password, otp);
            Assert.IsFalse(isValid);
        }

        private void ShouldBeValid(string accountId, string password, string otp)
        {
            var isValid = _authenticationService.Verify(accountId, password, otp);
            Assert.IsTrue(isValid);
        }

        private void GivenOtp(string accountId, string otp)
        {
            _otpService.GetCurrentOtp(accountId).Returns(otp);
        }

        private void GivenHashResult(string rawText, string hashResult)
        {
            _hash.Compute(rawText).Returns(hashResult);
        }

        private void GivenStoredHashPassword(string accountId, string hashedpassword)
        {
            _profile.GetPassword(accountId).Returns(hashedpassword);
        }
    }
}