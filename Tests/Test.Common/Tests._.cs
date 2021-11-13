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
            _dfsSettings = serviceProvider.GetRequiredService<DfsSettings>();
        }

        readonly IDistributedFileStorage<TestMetadata> _dfs;
        readonly DfsSettings _dfsSettings;
    }
}