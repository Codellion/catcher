using System;
using System.Collections.Generic;
using System.Text;

namespace Catcher.Core.Example.Performance.Services
{
    public interface IPerformanceTestService
    {
        void TestWithoutInterceptor();
        void TestWithInterceptor();
    }
}
