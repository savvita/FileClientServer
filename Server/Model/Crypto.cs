using System.Text;

namespace Server.Model
{
    internal class Crypto
    {
        private static readonly string key = "qwerty";

        /// <summary>
        /// Encrypt the input string
        /// </summary>
        /// <param name="text">Text to be encrypted</param>
        /// <returns>Encrypted text</returns>
        internal static string Encrypt(string text)
        {
            StringBuilder sb = new StringBuilder();
            int k = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char ch = (char)((int)text[i] + (int)key[k] % (int)' ');

                if (ch > 'z')
                {
                    ch = (char)((int)' ' + (int)ch - (int)'z');
                }
                sb.Append(ch);

                k++;
                if (k >= key.Length)
                    k = 0;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Decrypt the input string
        /// </summary>
        /// <param name="text">Text to be decrypted</param>
        /// <returns>Decrypted text</returns>
        internal static string Decrypt(string text)
        {
            StringBuilder sb = new StringBuilder();
            int k = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char ch = (char)((int)text[i] - (int)key[k] % (int)' ');

                if (ch < ' ')
                {
                    ch = (char)((int)'z' - ((int)' ' - (int)ch));
                }
                sb.Append(ch);

                k++;
                if (k >= key.Length)
                    k = 0;
            }

            return sb.ToString();
        }
    }
}
