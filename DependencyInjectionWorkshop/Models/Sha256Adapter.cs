using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public interface IHash
    {
        string GetHashedPassword(string inputPwd);
    }

    public class Sha256Adapter : IHash
    {
        public Sha256Adapter()
        {
        }

        public string GetHashedPassword(string inputPwd)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(inputPwd));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashedInputPWd = hash.ToString();
            return hashedInputPWd;
        }
    }
}