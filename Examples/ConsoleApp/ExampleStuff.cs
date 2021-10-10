using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApp
{
    class MyCustomMetadata
    {
        public string? Author { get; set; }
        public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
    }

    class Example
    {
        public static Random Rnd = new();

        public static Stream GenerateTextFileStream(string line = "text", int? count = null)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(string.Join("\n", 
                Enumerable.Range(0, count ?? Rnd.Next(1, 1000))
                    .Select(i => $"{i + 1}: {line}"))));
        }
    }
}
