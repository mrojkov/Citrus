#if !UNITY
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Lime
{
	public class OgvDecoder : IDisposable
	{
		Stream stream;
		int streamHandle;
		Lemon.Api.FileSystem fileSystem;
		IntPtr ogvHandle;
		static StreamMap streamMap = new StreamMap();

		public Size FrameSize { get; private set; }

		public OgvDecoder(Stream stream)
		{
			this.stream = stream;
			fileSystem = new Lemon.Api.FileSystem {
				ReadFunc = OgvRead, CloseFunc = OgvClose,
				SeekFunc = OgvSeek, TellFunc = OgvTell
			};
			streamHandle = streamMap.Allocate(stream);
			ogvHandle = Lemon.Api.OgvCreate(streamHandle, fileSystem);
			if (ogvHandle.ToInt32() == 0) {
				throw new Lime.Exception("Failed to open Ogv/Theora file");
			}
			FrameSize = new Size(Lemon.Api.OgvGetVideoWidth(ogvHandle),
				Lemon.Api.OgvGetVideoHeight(ogvHandle));
		}

		public void Dispose()
		{
			Lemon.Api.OgvDispose(ogvHandle);
			streamMap.Release(streamHandle);
			ogvHandle = new IntPtr(0);
			streamHandle = 0;
		}

		public bool DecodeFrame()
		{
			return Lemon.Api.OgvDecodeFrame(ogvHandle) == 0;
		}

		public double GetPlaybackTime()
		{
			return Lemon.Api.OgvGetPlaybackTime(ogvHandle);
		}

		public void FillTextureRGB8(Color4[] pixels, int width, int height)
		{
			var yPlane = Lemon.Api.OgvGetBuffer(ogvHandle, 0);
			var uPlane = Lemon.Api.OgvGetBuffer(ogvHandle, 1);
			var vPlane = Lemon.Api.OgvGetBuffer(ogvHandle, 2);
			unsafe {
				fixed (Color4* p = &pixels[0]) {
					Lemon.Api.DecodeRGBX8((IntPtr)p, 
						yPlane.Data, uPlane.Data, vPlane.Data,
						yPlane.Width, yPlane.Height, yPlane.Stride, uPlane.Stride,
						width * 4, 0);
				}
			}
		}

#if iOS
		[MonoTouch.MonoPInvokeCallback(typeof(Lemon.Api.ReadCallback))]
#endif
		private static uint OgvRead(IntPtr buffer, uint size, uint nmemb, int handle)
		{
			byte[] block = new byte[1024 * 16];
			int actualCount = 0;
			int requestCount = (int)(size * nmemb);
			while (true) {
				var stream = streamMap[handle];
				int read = stream.Read(block, 0, Math.Min(block.Length, requestCount - actualCount));
				if (read == 0)
					break;
				Marshal.Copy(block, 0, (IntPtr)(buffer.ToInt64() + actualCount), read);
				actualCount += read;
			}
			return (uint)actualCount;
		}

#if iOS
		[MonoTouch.MonoPInvokeCallback(typeof(Lemon.Api.TellCallback))]
#endif
		private static int OgvTell(int handle)
		{
			var stream = streamMap[handle];
			return (int)stream.Position;
		}
		
#if iOS
		[MonoTouch.MonoPInvokeCallback(typeof(Lemon.Api.SeekCallback))]
#endif
		private static int OgvSeek(int handle, long offset, SeekOrigin whence)
		{
			var stream = streamMap[handle];
			return (int)stream.Seek(offset, whence);
		}
		
#if iOS
		[MonoTouch.MonoPInvokeCallback(typeof(Lemon.Api.CloseCallback))]
#endif
		private static int OgvClose(int handle)
		{
			var stream = streamMap[handle];
			stream.Close();
			return 0;
		}
	}
}
#endif
