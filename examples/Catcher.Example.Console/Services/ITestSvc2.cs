using System;
using System.Collections.Generic;
using System.Text;

namespace Catcher.Example.Console.Services
{
    public interface ITestSvc2: IAuditable
    {
        void Test3(int a, int b);
        void Test4();
    }
}
