using System;

namespace Example.Common
{
    public class ExampleMetadata
    {
        public string? Author { get; set; }
        public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
    }
}
