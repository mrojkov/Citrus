#if iOS
using AVFoundation;
using CoreMedia;
using CoreVideo;
using Foundation;
using OpenTK.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lime
{
	public class VideoDecoder : IDisposable
	{
		public bool Looped;
		public double Duration { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		public bool HasNewTexture { get; private set; }
		public ITexture Texture { get => texture; }
		public Action OnStart;
		public VideoPlayerStatus Status = VideoPlayerStatus.Success;

		private Texture2D texture;

		private double currentPosition;
		private AVPlayerItem playerItem;
		private AVPlayer player;
		private AVPlayerItemVideoOutput videoOutput;
		private ManualResetEvent checkVideoEvent;
		private CVPixelBuffer currentPixelBuffer;

		private object locker = new object();
		private CancellationTokenSource stopDecodeCancelationTokenSource = new CancellationTokenSource();
		private State state = State.Initialized;

		private Stopwatch stopwatch = new Stopwatch();

		private enum State
		{
			Initialized,
			Started,
			Stoped,
			Paused,
			Finished
		};

		public VideoDecoder(string path)
		{
			texture = new Texture2D();
			playerItem = AVPlayerItem.FromUrl(NSUrl.FromString(path));
			NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, (notification) => {
				state = State.Finished;
				stopDecodeCancelationTokenSource.Cancel();
				checkVideoEvent.Set();
				Debug.Write("Play to end");
			}, playerItem);
			Debug.Write(string.Format("{0}:\n", playerItem.DebugDescription));
			player = new AVPlayer(playerItem);
			Debug.Write(string.Format("DecoderInitialized!"));
			videoOutput = new AVPlayerItemVideoOutput(new CoreVideo.CVPixelBufferAttributes() {
				//TODO: change to CoreVideo.CVPixelFormatType.CV420YpCbCr8BiPlanarFullRange to avoid convertation from YUV -> BGRA on processor and move it to GPU
				//required: changes in fragment shader
				//example: https://developer.apple.com/library/content/samplecode/GLCameraRipple/Listings/GLCameraRipple_Shaders_Shader_fsh.html#//apple_ref/doc/uid/DTS40011222-GLCameraRipple_Shaders_Shader_fsh-DontLinkElementID_9
				PixelFormatType = CoreVideo.CVPixelFormatType.CV32BGRA,
			});
			playerItem.AddOutput(videoOutput);
			var asset = playerItem.Asset;
			var videoTracks = asset.GetTracks(AVMediaTypes.Video);
			var size = videoTracks[0].NaturalSize;
			Width = (int)size.Width;
			Height = (int)size.Height;
			Duration = asset.Duration.Seconds;
			stopwatch.Reset();
			checkVideoEvent = new ManualResetEvent(false);
			//prepare audio
		}

		public IEnumerator<object> Start()
		{
			if (state == State.Started) {
				yield break;
			}
			stopwatch.Start();
			do {
				if (state == State.Finished) {
					stopwatch.Restart();
					player.Seek(CMTime.FromSeconds(0, 1000));
				}
				stopDecodeCancelationTokenSource = new CancellationTokenSource();
				var stopDecodeCancelationToken = stopDecodeCancelationTokenSource.Token;
				player.Play();
				state = State.Started;

				OnStart?.Invoke();

				var workTask = System.Threading.Tasks.Task.Run(() => {
					while (true) {
						var time = player.CurrentTime;
						while (!videoOutput.HasNewPixelBufferForItemTime(time)) {
							checkVideoEvent.WaitOne();
							checkVideoEvent.Reset();
							stopDecodeCancelationToken.ThrowIfCancellationRequested();
							time = player.CurrentTime;
						}
						var timeForDisplay = default(CMTime);
						var pixelBuffer = videoOutput.CopyPixelBuffer(time, ref timeForDisplay);
						while (currentPosition < timeForDisplay.Seconds) {
							checkVideoEvent.WaitOne();
							checkVideoEvent.Reset();
							stopDecodeCancelationToken.ThrowIfCancellationRequested();
						}
						lock (locker) {
							if (currentPixelBuffer != null) {
								currentPixelBuffer.Dispose();
								currentPixelBuffer = null;
							}
							currentPixelBuffer = pixelBuffer;
							HasNewTexture = true;
						}
					};
				}, stopDecodeCancelationToken);
				while (!workTask.IsCompleted && !workTask.IsCanceled && !workTask.IsFaulted) {
					yield return null;
				};
			} while (Looped && state == State.Finished);
			Debug.Write("Video player loop ended!");
		}

		public void Pause()
		{
			if (state == State.Started) {
				state = State.Paused;
				stopwatch.Stop();
				player.Pause();
				stopDecodeCancelationTokenSource.Cancel();
				checkVideoEvent.Set();
			}
		}

		public void Stop()
		{
			if (state == State.Stoped || state == State.Finished || state == State.Initialized) {
				return;
			}
			state = State.Stoped;
			player.Seek(CMTime.FromSeconds(0, 1000));
			stopwatch.Reset();
			player.Pause();
			stopDecodeCancelationTokenSource.Cancel();
			checkVideoEvent.Set();
		}

		public void Update(float delta)
		{
			if (state == State.Started) {
				currentPosition = stopwatch.ElapsedMilliseconds;
				checkVideoEvent.Set();
			}
		}

		public void UpdateTexture()
		{
			if (HasNewTexture) {
				HasNewTexture = false;
				lock (locker) {
					var pb = currentPixelBuffer;
					if (pb != null) {
						pb.Lock(CVPixelBufferLock.None);
						var addr = pb.BaseAddress;
						texture.LoadImage(addr, Width, Height, (PixelFormat)All.Bgra);
						pb.Unlock(CVPixelBufferLock.None);
					}
				}
			}
		}

		public void Dispose()
		{
			Pause();
			if (player != null) {
				player.Dispose();
				player = null;
			}
			if (videoOutput != null) {
				videoOutput.Dispose();
				videoOutput = null;
			}
			if (playerItem != null) {
				playerItem.Dispose();
				playerItem = null;
			}
			lock (locker) {
				if (currentPixelBuffer != null) {
					currentPixelBuffer.Dispose();
					currentPixelBuffer = null;
				}
			}
			if (texture != null) {
				texture.Dispose();
				texture = null;
			}
			HasNewTexture = false;
		}
	}
}
#endif