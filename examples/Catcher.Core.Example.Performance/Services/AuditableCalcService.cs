
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Catcher.Core.Example.Performance.Models;

namespace Catcher.Core.Example.Performance.Services
{
    public class AuditableCalcService : CalcService, IAuditableCalcService
    {
        public double GetHypotenuse(PartialTriangle partialTriangle)
        {
            return Math.Sqrt((
                partialTriangle.Catheti1 * partialTriangle.Catheti1 
                + partialTriangle.Catheti2 * partialTriangle.Catheti2));
        }
    }
}
