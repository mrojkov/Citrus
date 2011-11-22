using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace Lime
{
	public struct Sound
	{
		internal FMOD.Sound instance;
		internal FMOD.Channel channel;
	}

	public static class AudioSystem
	{
		const int MaxChannels = 16;
		static FMOD.System fmod;
		static Dictionary<int, Stream> openedStreams = new Dictionary<int, Stream> ();
		static int streamId = 1;
		static FMOD.FILE_OPENCALLBACK openCallback = OpenCallback;
		static FMOD.FILE_CLOSECALLBACK closeCallback = CloseCallback;
		static FMOD.FILE_READCALLBACK readCallback = ReadCallback;
		static FMOD.FILE_SEEKCALLBACK seekCallback = SeekCallback;

		static void Assert (FMOD.RESULT result)
		{
			if (result != FMOD.RESULT.OK) {
				throw new Lime.Exception ("FMOD error: {0}", result.ToString ());
			}
		}

		public static void Initialize ()
		{
			Assert (FMOD.Factory.System_Create (ref fmod));
			if (fmod.init (MaxChannels, FMOD.INITFLAG.NORMAL, new IntPtr (0)) != FMOD.RESULT.OK) {
				Assert (fmod.setOutput (FMOD.OUTPUTTYPE.NOSOUND));
				Assert (fmod.init (MaxChannels, FMOD.INITFLAG.NORMAL, new IntPtr (0)));
			}
			Assert (fmod.setFileSystem (openCallback, closeCallback, readCallback, seekCallback, 2048));
		}

		static FMOD.RESULT OpenCallback (string name, int unicode, ref uint filesize, ref IntPtr handle, ref IntPtr userdata)
		{
			if (!AssetsBundle.Instance.FileExists (name))
				return FMOD.RESULT.ERR_FILE_NOTFOUND;
			var stream = AssetsBundle.Instance.OpenFile (name);
			filesize = (uint)stream.Length;
			handle = new IntPtr (streamId++);
			openedStreams [(int)handle] = stream;
			return FMOD.RESULT.OK;
		}

		static FMOD.RESULT CloseCallback (IntPtr handle, IntPtr userdata)
		{
			var stream = openedStreams [(int)handle];
			stream.Close ();
			openedStreams.Remove ((int)handle);
			return FMOD.RESULT.OK;
		}

		static FMOD.RESULT ReadCallback(IntPtr handle, IntPtr buffer, uint sizebytes, ref uint bytesread, IntPtr userdata)
		{
			var stream = openedStreams [(int)handle];
			byte [] buffer2 = new byte [sizebytes];
			bytesread = (uint)stream.Read (buffer2, 0, (int)sizebytes);
			Marshal.Copy(buffer2, 0, buffer, (int)bytesread);
			return FMOD.RESULT.OK;
		}

		static FMOD.RESULT SeekCallback(IntPtr handle, int pos, IntPtr userdata)
		{
			var stream = openedStreams [(int)handle];
			stream.Seek (pos, SeekOrigin.Begin);
			return FMOD.RESULT.OK;
		}

		public static Sound CreateSound (string path, bool streaming)
		{
			var sound = new Sound ();
			var mode = FMOD.MODE.SOFTWARE | FMOD.MODE.LOWMEM | FMOD.MODE._2D | FMOD.MODE.LOOP_NORMAL | FMOD.MODE.UNICODE;
			if (streaming) {
				Assert (fmod.createStream (path, mode, ref sound.instance));
			} else {
				Assert (fmod.createSound (path, mode | FMOD.MODE.CREATECOMPRESSEDSAMPLE, ref sound.instance));
			}
			//sound.channel.setChannelGroup
			fmod.playSound (FMOD.CHANNELINDEX.FREE, sound.instance, false, ref sound.channel);
			return sound;
		}
	}
}
