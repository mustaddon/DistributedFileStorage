using DistributedFileStorage;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Test.Common
{
    public abstract partial class Tests
    {
        public Tests(IServiceProvider serviceProvider)
        {
            _dfs = serviceProvider.GetRequiredService<IDistributedFileStorage<TestMetadata>>();
        }

        readonly IDistributedFileStorage<TestMetadata> _dfs;
        readonly DfsSettings _dfsSettings = new();
    }
}