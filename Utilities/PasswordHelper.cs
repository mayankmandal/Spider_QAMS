using System.Security.Cryptography;
using System.Text;

namespace Spider_QAMS.Utilities
{
    public static class PasswordHelper
    {
        // Parameters matching Identity Framework settings
        private const int SaltSize = 16; // 128-bit salt
        private const int KeySize = 32;  // 256-bit key
        private const int Iterations = 10000; // Default iterations for Identity Framework

        // Generate a random salt
        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[SaltSize];
            using (var provider = new RNGCryptoServiceProvider())
            {
                provider.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }
        // Generate a hashed password using PBKDF2 with HMAC-SHA256
        public static string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            using (var deriveBytes = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] keyBytes = deriveBytes.GetBytes(KeySize);
                return Convert.ToBase64String(keyBytes);
            }
        }

        // Verify if a password matches the hashed password
        public static bool VerifyPassword(string password, string salt, string hash)
        {
            var hashedPassword = HashPassword(password, salt);
            return hashedPassword == hash;
        }

    }
}
