using System;
using System.Collections.Generic;
using System.Text;

namespace Catcher.Core.Example.Interceptor.Services
{
    public class TestSvc2 : ITestSvc2
    {
        public ITestSvc TestSvc { get; set;}

        public void Test3(int a, int b)
        {
            TestSvc.Test1(a, b);
        }

        public void Test4()
        {
            TestSvc.Test2();
        }

        public List<string> Test5()
        {
            return new List<string> { "1", "2" };
        }
    }
}
