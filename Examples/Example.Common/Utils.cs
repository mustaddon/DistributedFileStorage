using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Example.Common
{
    public class Utils
    {
        public static Random Rnd = new Random();

        public static Stream GenerateTextFileStream(string line = "text", int? count = null)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(string.Join("\n",
                Enumerable.Range(0, count ?? Rnd.Next(1, 1000))
                    .Select(i => $"{i + 1}: {line}"))));
        }
    }
}
