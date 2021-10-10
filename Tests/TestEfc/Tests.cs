using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestEfc
{
    [TestClass()]
    public class Tests : Common.Tests
    {
        public Tests() : base(App.Instance.Value.Services)
        {

        }

    }
}