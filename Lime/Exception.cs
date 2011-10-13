using System;
using System.Collections.Generic;
using System.Text;

namespace Lime
{
    /// <summary>
    /// Type of exception generated in case of internal engine conflicts, 
    /// This type of exception shouldn't ever be thrown.
    /// </summary>
    public class InternalError : Exception
    {
        public InternalError(string message) : base(message) { }
        public InternalError(string formatString, object arg0) : base(String.Format(formatString, arg0)) { }
        public InternalError(string formatString, object arg0, object arg1) : base(String.Format(formatString, arg0, arg1)) { }
    }

    /// <summary>
    /// Type of exception generated in case of external to application causes, 
    /// e.g. wrong data, IO errors, etc.
    /// </summary>
    public class RuntimeError : Exception
    {
        public RuntimeError(string message) : base(message) { }    
        public RuntimeError(string formatString, object arg0) : base(String.Format(formatString, arg0)) { }
        public RuntimeError(string formatString, object arg0, object arg1) : base(String.Format(formatString, arg0, arg1)) { }
    }
}