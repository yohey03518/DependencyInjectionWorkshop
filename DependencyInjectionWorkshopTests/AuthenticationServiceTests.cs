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
            var otpService = Substitute.For<IOtpService>();
            var logger = Substitute.For<ILogger>();
            var failCounter = Substitute.For<IFailCounter>();
            var hash = Substitute.For<IHash>();
            var notification = Substitute.For<INotify>();
            var profile = Substitute.For<IProfile>();

            var authenticationService =
                new AuthenticationService(profile, failCounter, hash, otpService, logger, notification);

            profile.GetPassword("erwin").Returns("hashedPassword");
            hash.Compute("password").Returns("hashedPassword");
            otpService.GetCurrentOtp("erwin").Returns("1234");
            var isValid = authenticationService.Verify("erwin", "password", "1234");
            Assert.IsTrue(isValid);
        }
    }
}