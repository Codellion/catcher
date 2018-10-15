
using System;
using System.Collections.Generic;
using System.Text;

namespace Catcher.Core.Example.AutoProperty.Services
{
    public class CarService: ICarService
    {
        public IEngineService engineService { get; set; }
        public Object engineService2 { get; set; }

        public CarService()
        {
            engineService2 = "fdasfd";
        }

        public void CheckSystem()
        {
            engineService.Start();
            engineService.Stop();
        }
    }
}
