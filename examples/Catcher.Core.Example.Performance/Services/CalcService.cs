using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Catcher.Core.Example.Performance.Services
{
    public class CalcService : ICalcService
    {
        public async Task<int> GetFibonacci(int n)
        {
            int a = 0;
            int b = 1;

            for (int i = 0; i < n; i++)
            {
                int temp = a;
                a = b;
                b = temp + b;
            }
            return await Task.FromResult(a);
        }
    }
}
