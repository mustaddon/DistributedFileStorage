using DistributedFileStorage.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TestEfc
{
    [TestClass()]
    public class DfsTests
    {
        public DfsTests()
        {
            _dfs = Common.App.Value.Services.GetRequiredService<IDistributedFileStorage<TestMetadata>>();
        }

        readonly IDistributedFileStorage<TestMetadata> _dfs;



        [TestMethod()]
        public async Task TestAdd()
        {
            var file = Common.GenerateFile();
            var fileId = await _dfs.Add(file.Content, file.Name, file.Metadata);

            Assert.IsFalse(string.IsNullOrWhiteSpace(fileId));
            await _dfs.Delete(fileId);
        }

        [TestMethod()]
        public async Task TestGetInfo()
        {
            var file = Common.GenerateFile();
            var fileId = await _dfs.Add(file.Content, file.Name, file.Metadata);
            var info = await _dfs.GetInfo(fileId);
            await _dfs.Delete(fileId);

            Assert.IsTrue(info.Name == file.Name, nameof(info.Name));
            Assert.IsTrue(info.Length == file.Content.Length, nameof(info.Length));
            Assert.IsTrue(info.Metadata.Equals(file.Metadata), nameof(info.Metadata));
        }

        [TestMethod()]
        public async Task TestGetInfos()
        {
            var file = Common.GenerateFile();
            file.Metadata = null;

            var fileId = await _dfs.Add(file.Content, file.Name);
            try
            {
                var ids = new[] { fileId }
                    .Concat(Enumerable.Range(0, 3000).Select(i => Guid.NewGuid().ToString("n")))
                    .Concat(new[] { fileId });

                await foreach (var info in _dfs.GetInfos(ids))
                {
                    Assert.IsTrue(info.Name == file.Name, nameof(info.Name));
                    Assert.IsTrue(info.Length == file.Content.Length, nameof(info.Length));
                    Assert.IsNull(info.Metadata, nameof(info.Metadata));
                }
            }
            finally
            {
                await _dfs.Delete(fileId);
            }
        }

        [TestMethod()]
        public async Task TestUpdate()
        {
            var file = Common.GenerateFile();
            var fileId = await _dfs.Add(file.Content, file.Name, file.Metadata);

            file.Name = "updated.txt";
            file.Metadata.Text = "updated";
            await _dfs.Update(fileId, file.Name, file.Metadata);

            var info = await _dfs.GetInfo(fileId);
            await _dfs.Delete(fileId);

            Assert.IsTrue(info.Name == file.Name, nameof(info.Name));
            Assert.IsTrue(info.Length == file.Content.Length, nameof(info.Length));
            Assert.IsTrue(info.Metadata.Equals(file.Metadata), nameof(info.Metadata));
        }

        [TestMethod()]
        public async Task TestGetContentStream()
        {
            var file = Common.GenerateFile();
            using var stream = new MemoryStream(file.Content);
            var fileId = await _dfs.Add(stream, file.Name, file.Metadata);
            await TestGetContent(file, fileId);
        }

        [TestMethod()]
        public Task TestGetContent() => TestGetContent(Common.GenerateFile());

        [TestMethod()]
        public Task TestGetContentEmpty() => TestGetContent(Common.GenerateFile(null, 0));

        [TestMethod()]
        public Task TestGetContentSmall() => TestGetContent(Common.GenerateFile("small", 1));

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

                Assert.IsTrue(bytes.Length == file.Content.Length, nameof(file.Content.Length));
                Assert.IsFalse(bytes.Select((x, i) => file.Content[i] == x).Any(x => !x), nameof(file.Content));
            }
            finally
            {
                await _dfs.Delete(fileId);
            }
        }
    }
}