﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Catcher
{
    /// <summary>
    /// Interception context
    /// </summary>
    public class CatcherContext
    {
        /// <summary>
        /// Arguments from method call
        /// </summary>
        public object[] Args { get; internal set; }

        /// <summary>
        /// Method intercepted
        /// </summary>
        public MethodBase Method { get; internal set; }

        /// <summary>
        /// Indicate if the interceptor must cancel the method execution
        /// </summary>
        public bool Cancel { get; set; }

        public CatcherContext(object[] args, MethodBase method)
        {
            this.Args = args;
            this.Method = method;
            this.Cancel = false;
        }
    }
}
