#if !ANDROID && !iOS && !WIN
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Lime
{
	public class VideoDecoder: IDisposable
	{
		public int Width => 0;
		public int Height => 0;
		public bool HasNewTexture = false;
		public bool Looped = false;
		public ITexture Texture => null;
		public Action OnStart;
		public VideoPlayerStatus Status = VideoPlayerStatus.Playing;

		public VideoDecoder(string path)
		{
		}

		public async System.Threading.Tasks.Task Start()
		{
		}

		public void SeekTo(int time)
		{

		}

		public void Stop()
		{
		}

		public void Pause()
		{
		}

		public void Update(float delta)
		{
		}

		public void UpdateTexture()
		{
		}

		public void Dispose()
		{
		}
	}
}
#endif