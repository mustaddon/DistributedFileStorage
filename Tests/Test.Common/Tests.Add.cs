using DistributedFileStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Test.Common
{
    public abstract partial class Tests
    {
        [TestMethod()]
        public async Task TestAdd()
        {
            var file = Utils.GenerateFile();
            var fileId = await _dfs.Add(file.Content, file.Name, file.Metadata);

            Assert.IsFalse(string.IsNullOrWhiteSpace(fileId));
            await _dfs.Delete(fileId);
        }
    }
}