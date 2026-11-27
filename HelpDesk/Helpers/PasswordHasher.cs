using System;
using System.Security.Cryptography;

namespace HelpDesk.Helpers
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public static string HashPassword(string password)
        {
            byte[] salt = new byte[SaltSize];

            using (RandomNumberGenerator rng =
                   RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash;

            using (Rfc2898DeriveBytes pbkdf2 =
                   new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                hash = pbkdf2.GetBytes(HashSize);
            }

            return Iterations + "." +
                   Convert.ToBase64String(salt) + "." +
                   Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(
            string password,
            string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(storedHash))
                return false;

            string[] parts = storedHash.Split('.');

            if (parts.Length != 3)
                return false;

            int iterations;

            if (!int.TryParse(parts[0], out iterations) ||
                iterations < 10000 || iterations > 1000000)
                return false;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] expectedHash = Convert.FromBase64String(parts[2]);
                byte[] actualHash;

                using (Rfc2898DeriveBytes pbkdf2 =
                       new Rfc2898DeriveBytes(password, salt, iterations))
                {
                    actualHash = pbkdf2.GetBytes(expectedHash.Length);
                }

                if (actualHash.Length != expectedHash.Length)
                    return false;

                int difference = 0;
                for (int i = 0; i < actualHash.Length; i++)
                    difference |= actualHash[i] ^ expectedHash[i];

                return difference == 0;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}