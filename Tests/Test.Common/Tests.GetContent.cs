using DistributedFileStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Common
{
    public abstract partial class Tests
    {
        [TestMethod()]
        public async Task TestGetContentStream()
        {
            var file = Utils.GenerateFile();
            using var stream = new MemoryStream(file.Content);
            var fileId = await _dfs.Add(stream, file.Name, file.Metadata);
            await TestGetContent(file, fileId);
        }

        [TestMethod()]
        public Task TestGetContent()
        {
            return TestGetContent(Utils.GenerateFile());
        }

        [TestMethod()]
        public Task TestGetContentEmpty()
        {
            return TestGetContent(Utils.GenerateFile(null, 0));
        }

        [TestMethod()]
        public Task TestGetContentSmall()
        {
            return TestGetContent(Utils.GenerateFile("small", 1));
        }

        private async Task TestGetContent(TestFile file)
        {
            var fileId = await _dfs.Add(file.Content, file.Name, file.Metadata);
            await TestGetContent(file, fileId);
        }

        private async Task TestGetContent(TestFile file, string fileId)
        {
            try
            {
                var content = new List<byte[]>();
                await foreach (var chunk in _dfs.GetContent(fileId))
                    content.Add(chunk);

                var bytes = content.SelectMany(x => x).ToArray();

                CollectionAssert.AreEqual(file.Content, bytes);
            }
            finally
            {
                await _dfs.Delete(fileId);
            }
        }
    }
}