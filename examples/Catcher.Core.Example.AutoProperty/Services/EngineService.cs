using System;
using System.Collections.Generic;
using System.Text;

namespace Catcher.Core.Example.AutoProperty.Services
{
    public class EngineService: IEngineService
    {
        public void Start()
        {
            System.Console.WriteLine("ENGINE START: OK");
        }
        public void Stop()
        {
            System.Console.WriteLine("ENGINE STOP: OK");
        }
    }
}
