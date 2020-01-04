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
        private const string Otp = "1234";

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
            GivenOtp(DefaultAccountId, Otp);

            ShouldBeValid(DefaultAccountId, InputPassword, Otp);
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