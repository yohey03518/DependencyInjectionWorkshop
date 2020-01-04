﻿using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public class Sha256Adapter
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