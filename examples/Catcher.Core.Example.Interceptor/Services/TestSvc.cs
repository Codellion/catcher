using System;
using System.Collections.Generic;
using System.Text;

namespace Catcher.Core.Example.Interceptor.Services
{
    public class TestSvc : ITestSvc
    {
        public String Test1(int a, int b)
        {
            for(var i = a; i < b; i++)
            {
                Console.WriteLine("Principal test 1");
            }

            return "a";
        }

        public void Test2()
        {
            Console.WriteLine("Principal test 2");
        }
    }
}
