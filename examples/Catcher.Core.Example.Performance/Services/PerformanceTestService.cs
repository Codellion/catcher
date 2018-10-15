using System;
using System.Collections.Generic;
using System.Text;

namespace Catcher.Core.Example.Performance.Services
{
    public class PerformanceTestService : IPerformanceTestService
    {
        private readonly ICalcService calcService;
        private readonly IAuditableCalcService auditableCalcService;

        const int FibonacciLimit = 100000000;
        const int TestLimit = 100;

        public PerformanceTestService(ICalcService calcService, IAuditableCalcService auditableCalcService)
        {
            this.calcService = calcService;
            this.auditableCalcService = auditableCalcService;
        }

        public void TestWithoutInterceptor()
        {
            for(var i=0; i< TestLimit; i++)
            {
                var res = calcService.GetFibonacci(FibonacciLimit);

                if (res.Result == 0)
                {
                    throw new Exception("No returned data");
                }
            }
        }

        public void TestWithInterceptor()
        {
            for (var i = 0; i < TestLimit; i++)
            {
                var res = auditableCalcService.GetFibonacci(FibonacciLimit);

                if(res.Result == 0)
                {
                    throw new Exception("No returned data");
                }
            }
        }
    }
}
