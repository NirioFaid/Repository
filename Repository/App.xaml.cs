using System;
using System.Text;
using System.Windows;

namespace Repository
{
    public partial class App : Application
    {
        public static string Crypt(string text, string key)
        {
            char[] arr1 = text.ToCharArray();
            Array.Reverse(arr1);
            byte[] txt = Encoding.Default.GetBytes(new string(arr1));
            byte[] ckey = Encoding.Default.GetBytes(key);
            byte[] res = new byte[text.Length];
            for (int i = 0; i < txt.Length; i++)
                res[i] = (byte)(txt[i] ^ key[i % key.Length]);
            char[] arr2 = Encoding.Default.GetString(res).ToCharArray();
            Array.Reverse(arr2);
            return new string(arr2);
        }
    }
}
