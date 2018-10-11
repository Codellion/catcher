using System;
using System.Collections.Generic;
using System.Text;

namespace Catcher.Example.Console.Services
{
    public class TestSvc : ITestSvc
    {
        public String Test1(int a, int b)
        {
            for(var i = a; i < b; i++)
            {
                System.Console.WriteLine("Principal test 1");
            }

            return "a";
        }

        public void Test2()
        {
            System.Console.WriteLine("Principal test 2");
        }
    }
}
