using System;
using System.IO;

namespace Launcher
{
	internal class LogWriter : TextWriter
	{
		private LoggingForm loggingForm;

		public LogWriter(LoggingForm loggingForm)
		{
			this.loggingForm = loggingForm;
		}

		public override void WriteLine(string value)
		{
			Write(value + '\n');
		}

		public override void Write(string value)
		{
			loggingForm.Log(value);
		}

		public override System.Text.Encoding Encoding
		{
			get { throw new NotImplementedException(); }
		}
	}
}