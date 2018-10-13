using System;
using System.Collections.Generic;
using System.Text;

namespace Catcher.Core.Example.Console.Services
{
    public class TestSvc2 : ITestSvc2
    {
        public void Test3(int a, int b)
        {
            for(var i = a; i < b; i++)
            {
                System.Console.WriteLine("Principal test 1");
            }

        }

        public void Test4()
        {
            System.Console.WriteLine("Principal test 2");
        }
    }
}
