using System.Text;

namespace Modules.Utils.Scripts.Security
{
    public static class Crypto
    {
        public static string XorEncryptDecrypt(string data, string key)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
                sb.Append((char)(data[i] ^ key[i % key.Length]));
            return sb.ToString();
        }
    }
}
