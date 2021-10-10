using System;

namespace Common
{
    public class TestMetadata
    {
        public string Text { get; set; }
        public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;

        public override int GetHashCode() => HashCode.Combine(Text, Date);
        public override bool Equals(object obj) => GetHashCode() == (obj as TestMetadata)?.GetHashCode();
    }

    public class TestFile
    {
        public string Name { get; set; }
        public byte[] Content { get; set; }
        public TestMetadata Metadata { get; set; }
    }
}
