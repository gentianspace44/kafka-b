using System.Security.Cryptography;
using System.Text;

namespace VPS.Helpers
{
    public static class EncryptionHelper
    {

        public static string Sha256Hash(String inputString)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.ASCII;
                Byte[] result = hash.ComputeHash(enc.GetBytes(inputString));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
    }
}
