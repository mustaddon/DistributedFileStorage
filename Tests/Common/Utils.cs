using System;
using System.Linq;
using System.Text;

namespace Common
{
    public class Utils
    {
        public static Random Rnd = new();

        public static TestFile GenerateFile(string line = "test", int? count = null)
        {
            return new TestFile
            {
                Name = $"test_{DateTime.Now.Ticks}.txt",
                Content = Encoding.UTF8.GetBytes(string.Join("\n", Enumerable.Range(0, count ?? Rnd.Next(1, 5000)).Select(i => $"{i + 1}: {line}"))),
                Metadata = new TestMetadata { Text = $"text" },
            };
        }
    }
}
