using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using System.Linq;
using System.Threading.Tasks;

namespace TestMongo
{
    [TestClass()]
    public class Tests : Common.Tests
    {
        public Tests() : base(App.Instance.Value.Services)
        {

        }

    }
}