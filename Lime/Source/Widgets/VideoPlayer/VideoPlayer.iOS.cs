#if iOS
using AVFoundation;
using CoreMedia;
using CoreVideo;
using Foundation;
using OpenTK.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lime
{
	public class Decoder : IDisposable
	{
		AVAsset asset;
		AVPlayerItem playerItem;
		AVPlayer player;
		AVPlayerItemVideoOutput videoOutput;
		double currentPosition;
		public float Width { get; private set; }
		public float Height { get; private set; }
		public double Duration;
		ManualResetEvent checkVideoEvent;
		CVPixelBuffer currentPixelBuffer;
		object locker = new object();

		private enum State
		{
			Initialized,
			Started,
			Stoped,
			Paused,
			Finished
		};

		private State state = State.Initialized;

		public bool Looped { get; set; }

		private CancellationTokenSource stopDecodeCancelationTokenSource = new CancellationTokenSource();

		public Decoder(string path)
		{
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
			asset = playerItem.Asset;
			var videoTracks = asset.GetTracks(AVMediaTypes.Video);
			var size = videoTracks[0].NaturalSize;
			Width = (float)size.Width;
			Height = (float)size.Height;
			Duration = asset.Duration.Seconds;
			currentPosition = 0;
			checkVideoEvent = new ManualResetEvent(false);
			//prepare audio
		}

		public async System.Threading.Tasks.Task Start()
		{
			if (state == State.Started) {
				return;
			}

			do {
				if (state == State.Finished) {
					currentPosition = 0;
					player.Seek(CMTime.FromSeconds(0, 1000));
				}
				stopDecodeCancelationTokenSource = new CancellationTokenSource();
				var stopDecodeCancelationToken = stopDecodeCancelationTokenSource.Token;
				player.Play();
				player.Volume = 1.0f;
				state = State.Started;
				var workTask = System.Threading.Tasks.Task.Run(() => {
					while (true) {
						var time = CMTime.FromSeconds(currentPosition, 1000);
						while (!videoOutput.HasNewPixelBufferForItemTime(time)) {
							checkVideoEvent.WaitOne();
							checkVideoEvent.Reset();
							stopDecodeCancelationToken.ThrowIfCancellationRequested();
							time = CMTime.FromSeconds(currentPosition, 1000);
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
						}
					};
				}, stopDecodeCancelationToken);
				try {
					await workTask;
				} catch (OperationCanceledException e) {
					Debug.Write("VideoPlayer: work task canceled!");
				}
			} while (Looped && state == State.Finished);
			Debug.Write("Video player loop ended!");
		}

		public void Pause()
		{
			if (state == State.Started) {
				state = State.Paused;
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
			currentPosition = 0;
			player.Pause();
			stopDecodeCancelationTokenSource.Cancel();
			checkVideoEvent.Set();
		}

		public void Update(float delta)
		{
			if (state == State.Started) {
				currentPosition += delta;
				checkVideoEvent.Set();
			}
		}

		public void UpdateTexture(Texture2D texture)
		{
			lock (locker) {
				if (currentPixelBuffer != null) {
					var pb = currentPixelBuffer;
					currentPixelBuffer = null;
					pb.Lock(CVPixelBufferLock.None);
					var addr = pb.BaseAddress;
					texture.LoadImage(addr, (int)Width, (int)Height, (PixelFormat)All.Bgra);
					pb.Unlock(CVPixelBufferLock.None);
					pb.Dispose();
				}
			}
		}

		public void Dispose()
		{

		}
	}

	public class VideoPlayer : Image
	{
		public bool Looped { get => decoder.Looped ; set { decoder.Looped = value;  } }
		private Decoder decoder;
		public VideoPlayer (Widget parentWidget)
		{
			parentWidget.Nodes.Add (this);
			Size = parentWidget.Size;
			Anchors = Anchors.LeftRight | Anchors.TopBottom;
			Texture = new Texture2D();
		}

		public override void Update (float delta)
		{
			base.Update (delta);
			decoder.Update(delta);
			decoder.UpdateTexture((Texture2D)Texture);
		}

		public void InitPlayer (string sourcePath)
		{
			decoder = new Decoder(sourcePath);
		}

		public void Start ()
		{
			decoder.Start();
		}

		public void Pause ()
		{
			decoder.Pause();
		}

		public void Stop ()
		{
			decoder.Stop();
		}

		public override void Dispose ()
		{
			decoder.Dispose();
			decoder = null;
			base.Dispose ();
		}
	}
}
#endif