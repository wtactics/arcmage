using System;
using System.Linq;
using System.Security.Cryptography;

namespace Arcmage.Server.Api.Utils
{
    public static class Hasher
    {

        // hashing password with generated salt
        public static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 16, 1000))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(32);
            }
            byte[] dst = new byte[49];
            Buffer.BlockCopy(salt, 0, dst, 1, 16);
            Buffer.BlockCopy(buffer2, 0, dst, 17, 32);
            return Convert.ToBase64String(dst);
        }

        // verify passwords
        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            byte[] pwdRehased;
            if (hashedPassword == null)
            {
                return false;
            }
            if (password == null)
            {
                return false;
            }
            byte[] src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 49) || (src[0] != 0))
            {
                return false;
            }
            byte[] salt = new byte[16];
            Buffer.BlockCopy(src, 1, salt, 0, 16);
            byte[] hash = new byte[32];
            Buffer.BlockCopy(src, 17, hash, 0, 32);
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, salt, 1000))
            {
                pwdRehased = bytes.GetBytes(32);
            }
            return hash.SequenceEqual(pwdRehased);

        }
    }
}