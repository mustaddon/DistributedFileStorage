using DistributedFileStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Common
{
    public abstract partial class Tests
    {
        [TestMethod()]
        public async Task TestGetInfo()
        {
            var file = Utils.GenerateFile();
            var fileId = await _dfs.Add(file.Content, file.Name, file.Metadata);
            var info = await _dfs.GetInfo(fileId);
            await _dfs.Delete(fileId);

            Assert.AreEqual(file.Name,info.Name);
            Assert.AreEqual(file.Content.Length, info.Length);
            Assert.AreEqual(file.Metadata, info.Metadata);
        }

        [TestMethod()]
        public async Task TestGetInfos()
        {
            var file = Utils.GenerateFile();
            file.Metadata = null;

            var fileId = await _dfs.Add(file.Content, file.Name);
            try
            {
                var ids = new[] { fileId }
                    .Concat(Enumerable.Range(0, 3000).Select(i => _dfsSettings.IdGenerator()))
                    .Concat(new[] { fileId });

                await foreach (var info in _dfs.GetInfos(ids))
                {
                    Assert.AreEqual(file.Name, info.Name);
                    Assert.AreEqual(file.Content.Length, info.Length);
                    Assert.AreEqual(file.Metadata, info.Metadata);
                }
            }
            finally
            {
                await _dfs.Delete(fileId);
            }
        }

    }
}