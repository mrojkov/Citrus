using System;
using System.IO;

namespace Launcher
{
	internal class LogWriter : TextWriter
	{
		private Action<string> logginAction;

		public LogWriter(Action<string> logginAction)
		{
			this.logginAction = logginAction;
		}

		public override void WriteLine(string value)
		{
			Write(value + '\n');
		}

		public override void Write(string value)
		{
			logginAction(value);
		}

		public override System.Text.Encoding Encoding
		{
			get { throw new NotImplementedException(); }
		}
	}
}