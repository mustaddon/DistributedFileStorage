using DistributedFileStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Common
{
    public abstract partial class Tests
    {
        [TestMethod()]
        public async Task TestUpdate()
        {
            var file = Utils.GenerateFile();
            var fileId = await _dfs.Add(file.Content, file.Name, file.Metadata);

            file.Name = "updated.txt";
            file.Metadata.Text = "updated";
            await _dfs.Update(fileId, file.Name, file.Metadata);

            var info = await _dfs.GetInfo(fileId);
            await _dfs.Delete(fileId);

            Assert.AreEqual(file.Name, info.Name);
            Assert.AreEqual(file.Content.Length, info.Length);
            Assert.AreEqual(file.Metadata, info.Metadata);
        }

    }
}