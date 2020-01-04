namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthenticationService
    {
        bool Verify(string accountId, string inputPwd, string otp);
    }
}