using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Game_Cloud
{
    public static class HashHelper
    {
        public static string HashString(string StringData)
        {
            if (StringData == null)
            {
                throw new ArgumentNullException("Password");
            }
            byte[] salt;
            byte[] bytes;
            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(StringData, 16, 1000))
            {
                salt = rfc2898DeriveBytes.Salt;
                bytes = rfc2898DeriveBytes.GetBytes(32);
            }
            byte[] inArray = new byte[49];
            Buffer.BlockCopy((Array)salt, 0, (Array)inArray, 1, 16);
            Buffer.BlockCopy((Array)bytes, 0, (Array)inArray, 17, 32);
            return Convert.ToBase64String(inArray);
        }


        public static bool VerifyHash(string HashedString, string StringData)
        {
            if (HashedString == null)
            {
                throw new ArgumentNullException("HashedPassword");
            }
            if (StringData == null)
            {
                throw new ArgumentNullException("Password");
            }
            byte[] numArray = Convert.FromBase64String(HashedString);
            if (numArray.Length != 49 || (int)numArray[0] != 0)
            {
                return false;
            }
            byte[] salt = new byte[16];
            Buffer.BlockCopy((Array)numArray, 1, (Array)salt, 0, 16);
            byte[] a = new byte[32];
            Buffer.BlockCopy((Array)numArray, 17, (Array)a, 0, 32);
            byte[] bytes;
            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(StringData, salt, 1000))
            {
                bytes = rfc2898DeriveBytes.GetBytes(32);
            }
            return ByteArraysEqual(a, bytes);
        }
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (ReferenceEquals((object)a, (object)b))
            {
                return true;
            }
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }
            bool same = true;
            for (int i = 0; i < a.Length; ++i)
            {
                if (a[i] != b[i])
                {
                    same = false;
                }
            }
            return same;
        }
    }
}
