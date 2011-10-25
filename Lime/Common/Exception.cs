using System;

namespace Lime
{
	public class Exception : System.Exception
	{
		public Exception (string message) : base(message)
		{
		}

		public Exception (string formatString, object arg0) : base (String.Format (formatString, arg0))
		{
		}

		public Exception (string formatString, object arg0, object arg1) : base (String.Format (formatString, arg0, arg1))
		{
		}
	}
}