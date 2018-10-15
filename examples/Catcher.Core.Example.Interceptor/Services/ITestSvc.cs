using System;
using System.Collections.Generic;
using System.Text;

namespace Catcher.Core.Example.Interceptor.Services
{
    public interface ITestSvc : IAuditable
    {
        string Test1(int a, int b);
        void Test2();
    }
}
