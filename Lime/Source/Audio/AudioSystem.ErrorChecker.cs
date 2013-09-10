using System;
#if OPENAL
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
#endif

namespace Lime
{
	public static partial class AudioSystem
	{
		public class ErrorChecker : IDisposable
		{
			string comment;
			bool throwException;

			public ErrorChecker(string comment = null, bool throwException = true)
			{
				this.comment = comment;
				this.throwException = throwException;
				// Clear current error
				AL.GetError();
			}

			void IDisposable.Dispose()
			{
#if OPENAL
				var error = AL.GetError();
				if (error != ALError.NoError) {
					string message = "OpenAL error: " + AL.GetErrorString(error);
					if (comment != null) {
						message += string.Format(" ({0})", comment);
					}
					if (throwException) {
						throw new Exception(message);
					} else {
						Logger.Write(comment);
					}
				}
#endif
			}
		}
	}
}