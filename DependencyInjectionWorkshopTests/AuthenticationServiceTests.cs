using System;
using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            var fakeProfile = Substitute.For<IProfile>();
            var fakeHash = Substitute.For<IHash>();
            var fakeOtpService = Substitute.For<IOtpService>();
            var fakeLogger = Substitute.For<ILogger>();
            var fakeFailCounter = Substitute.For<IFailCounter>();
            var fakeNotification = Substitute.For<INotification>();

            var authenticationService = new AuthenticationService(fakeProfile, fakeHash, fakeOtpService, fakeFailCounter, fakeNotification, fakeLogger);
            fakeProfile.GetPassword("erwin").Returns("hashedPwd");
            fakeHash.Compute("pwd").Returns("hashedPwd");
            fakeOtpService.GetCurrentOtp("erwin").Returns("1234");
            var isValid = authenticationService.Verify("erwin", "pwd", "1234");
            Assert.IsTrue(isValid);
        }
    }
}