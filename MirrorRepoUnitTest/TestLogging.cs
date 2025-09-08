using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorRepoUnitTest
{
    [TestClass]
    public class TestLogging
    {
        [TestMethod] 
        public void test()
        {
            var super = new SuperClass();
            super.test();
        }
    }


    public class BaseClass
    {
        protected ILog Log = null; // LogManager.GetLogger(this.GetType());
        public BaseClass()
        {
            Log = LogManager.GetLogger(this.GetType());
        }
    }
    public class SuperClass : BaseClass
    {
        public void test()
        {
            Log.Info("It's me: " + this.GetType());
        }
    }
}
