using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Catcher.Core.Example.Performance.Models;

namespace Catcher.Core.Example.Performance.Services
{
    public interface IAuditableCalcService: ICalcService
    {
        double GetHypotenuse(PartialTriangle partialTriangle);
    }
}
