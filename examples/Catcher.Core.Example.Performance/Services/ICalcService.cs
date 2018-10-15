using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Catcher.Core.Example.Performance.Services
{
    public interface ICalcService
    {
        Task<int> GetFibonacci(int n);
    }
}
