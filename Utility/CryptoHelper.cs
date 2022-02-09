using System.Security.Cryptography;

namespace OktaManualLoginFlow.Utility
{
    public static class CryptoHelper
    {
        public static string GenerateRandomString()
        {
            byte[] randomBytes = RandomNumberGenerator.GetBytes(32);

            return Convert.ToBase64String(randomBytes);
        }
    }
}
