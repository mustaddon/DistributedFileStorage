using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Mongo
{
    [TestClass()]
    public class Tests : Common.Tests
    {
        public Tests() : base(App.Instance.Value.Services)
        {

        }

    }
}