#if ANDROID
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Nio;
using OpenTK.Graphics.ES20;
using static Android.Media.MediaCodec;

namespace Lime
{
	public class VideoDecoder : IDisposable
	{
		public int Width => videoFormat.GetInteger(MediaFormat.KeyWidth);
		public int Height => videoFormat.GetInteger(MediaFormat.KeyHeight);
		public bool HasNewTexture = false;
		public bool Looped = false;
		public ITexture Texture => texture;
		public Action OnStart;
		public VideoPlayerStatus Status = VideoPlayerStatus.None;

		private SurfaceTextureRenderer renderer;
		private RenderTexture texture;
		private long currentPosition => stopwatch.ElapsedMilliseconds * 1000;

		private MediaExtractor videoExtractor;
		private MediaCodec videoCodec;
		private MediaFormat videoFormat;
		private int videoTrack = -1;

		private MediaExtractor audioExtractor;
		private MediaCodec audioCodec;
		private MediaFormat audioFormat;
		private int audioTrack = -1;
		private AudioTrack audio;

		private ManualResetEvent checkAudioQueue = new ManualResetEvent(false);
		private ManualResetEvent checkVideoQueue = new ManualResetEvent(false);
		private CancellationTokenSource stopDecodeCancelationTokenSource = new CancellationTokenSource();

		private Stopwatch stopwatch = new Stopwatch();

		private State state;

		private enum State
		{
			Initializing,
			Initialized,
			Started,
			Stoped,
			Paused,
			Finished
		}

		private int SelectTrack(MediaExtractor extractor, string mimeType)
		{
			for (int i = 0; i < extractor.TrackCount; ++i) {
				var format = extractor.GetTrackFormat(i);
				var mime = format.GetString(MediaFormat.KeyMime);
				if (mime.StartsWith(mimeType)) {
					extractor.SelectTrack(i);
					return i;
				}
			}
			return -1;
		}

		public VideoDecoder(string path)
		{
			state = State.Initializing;
			Window.Current.InvokeOnRendering(() => {
				try {
					videoExtractor = new MediaExtractor();
					videoExtractor.SetDataSource(path);
					videoTrack = SelectTrack(videoExtractor, "video/");
					if (videoTrack < 0) {
						throw new System.Exception("Video WOOPS");
					}
					videoFormat = videoExtractor.GetTrackFormat(videoTrack);
					videoCodec = MediaCodec.CreateDecoderByType(videoFormat.GetString(MediaFormat.KeyMime));
					renderer = new SurfaceTextureRenderer();
					videoCodec.Configure(videoFormat, renderer.Surface, null, MediaCodecConfigFlags.None);
					texture = new RenderTexture(Width, Height);
				} catch {
					Lime.Debug.Write("Init video track");
					Status = VideoPlayerStatus.Error;
					return;
				}

				try {
					audioExtractor = new MediaExtractor();
					audioExtractor.SetDataSource(path);
					audioTrack = SelectTrack(audioExtractor, "audio/");
					if (audioTrack < 0) {
						throw new System.Exception("Audio WOOPS");
					}
					audioFormat = audioExtractor.GetTrackFormat(audioTrack);
					var sampleRate = audioFormat.GetInteger(MediaFormat.KeySampleRate);
					audioCodec = MediaCodec.CreateDecoderByType(audioFormat.GetString(MediaFormat.KeyMime));
					audioCodec.Configure(audioFormat, null, null, MediaCodecConfigFlags.None);
					var bufferSize = AudioTrack.GetMinBufferSize(sampleRate, ChannelOut.Stereo, Android.Media.Encoding.Pcm16bit);
					audio = new AudioTrack(
						global::Android.Media.Stream.Music,
						sampleRate,
						ChannelOut.Stereo,
						Android.Media.Encoding.Pcm16bit,
						bufferSize,
						AudioTrackMode.Stream
					);
				} catch {
					Lime.Debug.Write("Init audio track");
					Status = VideoPlayerStatus.Error;
					return;
				}

				state = State.Initialized;
				Status = VideoPlayerStatus.Success;
			});
		}

		private void ExtractAndQueueSample(MediaExtractor extractor, MediaCodec codec, ref bool hasInput)
		{
			if (hasInput) {
				var inputIndex = codec.DequeueInputBuffer(10000);
				if (inputIndex >= 0) {
					var inputBuffer = codec.GetInputBuffer(inputIndex);

					var sampleSize = extractor.ReadSampleData(inputBuffer, 0);
					if (sampleSize > 0) {
						codec.QueueInputBuffer(inputIndex, 0, sampleSize, extractor.SampleTime, MediaCodecBufferFlags.None);
					}
					if (!extractor.Advance() || sampleSize == 0) {
						codec.QueueInputBuffer(inputIndex, 0, 0, 0, MediaCodecBufferFlags.EndOfStream);
						hasInput = false;
					}
				}
			}
		}

		private void SignEndOfStream(MediaCodec codec, ref bool hasInput)
		{
			if (hasInput) {
				var inputIndex = codec.DequeueInputBuffer(10000);
				if (inputIndex >= 0) {
					codec.QueueInputBuffer(inputIndex, 0, 0, 0, MediaCodecBufferFlags.EndOfStream);
					hasInput = false;
				}
			}
		}

		public IEnumerator<object> Start()
		{
			if (state == State.Initializing) {
				yield return null;
			}
			if (state == State.Started) {
				yield break;
			}
			stopDecodeCancelationTokenSource = new CancellationTokenSource();
			var stopDecodeCancelationToken = stopDecodeCancelationTokenSource.Token;
			var hasMoreItemsInQueue = new ManualResetEvent(false);

			do {
				if (state == State.Finished) {
					stopwatch.Reset();
					videoExtractor.SeekTo(0, MediaExtractorSeekTo.ClosestSync);
					audioExtractor.SeekTo(0, MediaExtractorSeekTo.ClosestSync);
					videoCodec?.Configure(videoFormat, renderer.Surface, null, MediaCodecConfigFlags.None);
					audioCodec?.Configure(audioFormat, null, null, MediaCodecConfigFlags.None);
				}

				if (state == State.Finished ||
					state == State.Initialized
				) {
					videoCodec?.Start();
					audioCodec?.Start();
				}
				stopwatch.Start();
				audio?.Play();
				state = State.Started;

				System.Threading.Tasks.Task StartQueueTask(MediaExtractor extractor, MediaCodec codec)
				{
					return System.Threading.Tasks.Task.Run(() => {
						var hasInput = codec != null;
						while (hasInput) {
							stopDecodeCancelationToken.ThrowIfCancellationRequested();
							var trackIndex = extractor.SampleTrackIndex;
							if (trackIndex > -1) {
								ExtractAndQueueSample(extractor, codec, ref hasInput);
							} else {
								SignEndOfStream(codec, ref hasInput);
								Debug.Write("trackIndex <= -1");
							}
						}
						Debug.Write("queueTask: end");
					}, stopDecodeCancelationToken);
				}

				var videoQueueTask = StartQueueTask(videoExtractor, videoCodec);
				var audioQueueTask = StartQueueTask(audioExtractor, audioCodec);

				var processVideo = System.Threading.Tasks.Task.Run(() => {
					var info = new BufferInfo();
					var eosReceived = videoCodec == null;
					while (!eosReceived) {
						stopDecodeCancelationToken.ThrowIfCancellationRequested();
						var outIndex = videoCodec.DequeueOutputBuffer(info, 10000);
						if (outIndex >= 0) {
							var pt = info.PresentationTimeUs;
							while (currentPosition < pt) {
								Thread.Sleep(10);
								if (stopDecodeCancelationToken.IsCancellationRequested) {
									videoCodec.ReleaseOutputBuffer(outIndex, false);
								}
								stopDecodeCancelationToken.ThrowIfCancellationRequested();
							}
							videoCodec.ReleaseOutputBuffer(outIndex, true);
							HasNewTexture = true;
						}

						if (info.Flags.HasFlag(MediaCodecBufferFlags.EndOfStream)) {
							eosReceived = true;
						}
					}
				}, stopDecodeCancelationToken);


				var processAudio = System.Threading.Tasks.Task.Run(() => {
					var eosReceived = audioCodec == null;
					var info = new BufferInfo();
					while (!eosReceived) {
						stopDecodeCancelationToken.ThrowIfCancellationRequested();
						var outIndex = audioCodec.DequeueOutputBuffer(info, 10000);
						if (outIndex >= 0) {
							var buffer = audioCodec.GetOutputBuffer(outIndex);
							var pt = info.PresentationTimeUs;
							while (currentPosition < pt) {
								Thread.Sleep(10);
								stopDecodeCancelationToken.ThrowIfCancellationRequested();
							}
							audio.Write(buffer.Duplicate(), info.Size, WriteMode.Blocking, pt);
							hasMoreItemsInQueue.Set();
							audioCodec.ReleaseOutputBuffer(outIndex, false);
						}

						if (info.Flags.HasFlag(MediaCodecBufferFlags.EndOfStream)) {
							eosReceived = true;
						}
					}
				}, stopDecodeCancelationToken);

				OnStart?.Invoke();

				while (
					(!videoQueueTask.IsCompleted && !videoQueueTask.IsCanceled && !videoQueueTask.IsFaulted) ||
					(!audioQueueTask.IsCompleted && !audioQueueTask.IsCanceled && !audioQueueTask.IsFaulted)) {
					yield return null;
				};
				while (
					(!processVideo.IsCompleted && !processVideo.IsCanceled && !processVideo.IsFaulted) ||
					(!processAudio.IsCompleted && !processAudio.IsCanceled && !processAudio.IsFaulted)) {
					yield return null;
				};
				if (state == State.Started) {
					state = State.Finished;
					videoCodec?.Stop();
					audioCodec?.Stop();
				}
			} while (Looped && !stopDecodeCancelationToken.IsCancellationRequested);
			checkAudioQueue.Set();
			hasMoreItemsInQueue.Set();
		}

		public void Stop()
		{
			if (state == State.Stoped || state == State.Finished || state == State.Initialized || state == State.Initializing) {
				return;
			}
			Pause();
			state = State.Stoped;
			videoExtractor?.SeekTo(0, MediaExtractorSeekTo.ClosestSync);
			audioExtractor?.SeekTo(0, MediaExtractorSeekTo.ClosestSync);
			audioCodec?.Flush();
			videoCodec?.Flush();
		}

		public void Pause()
		{
			if (state == State.Started) {
				state = State.Paused;
				stopwatch.Stop();
				stopDecodeCancelationTokenSource.Cancel();
				checkAudioQueue.Set();
				checkVideoQueue.Set();
				audio?.Pause();
			}
		}

		public void Update(float delta)
		{
			if (state == State.Started) {
				checkAudioQueue.Set();
				checkVideoQueue.Set();
			}
		}

		public void UpdateTexture()
		{
			if (HasNewTexture) {
				HasNewTexture = false;
				renderer.Render(Texture);
			}
		}

		public void Dispose()
		{
			stopDecodeCancelationTokenSource.Cancel();
			Window.Current.InvokeOnRendering(() => {
				if (videoCodec != null) {
					videoCodec.Stop();
					videoCodec.Dispose();
					videoCodec = null;
				}
				if (audioCodec != null) {
					audioCodec.Stop();
					audioCodec.Dispose();
					audioCodec = null;
				}
				if (audio != null) {
					audio.Dispose();
					audio = null;
				}
				videoExtractor?.Dispose();
				videoExtractor = null;
				audioExtractor?.Dispose();
				audioExtractor = null;
			});
		}

		private class SurfaceTextureRenderer
		{
			public class SurfaceTextureMaterial : IMaterial
			{
				private readonly BlendState blendState;
				private readonly ShaderParams[] shaderParamsArray;
				private readonly ShaderParams shaderParams;

				public float Strength { get; set; } = 1f;

				public int PassCount => 1;

				public SurfaceTextureMaterial()
				{
					blendState = BlendState.Default;
					shaderParams = new ShaderParams();
					shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
				}

				public void Apply(int pass)
				{
					PlatformRenderer.SetBlendState(blendState);
					PlatformRenderer.SetShaderProgram(SurfaceTextureProgram.GetInstance());
					PlatformRenderer.SetShaderParams(shaderParamsArray);
				}

				public void Invalidate() { }

				public IMaterial Clone() => new SurfaceTextureMaterial();
			}

			private class SurfaceTextureProgram : ShaderProgram
			{
				private const string VertexShader = @"
				attribute vec4 a_Position;
				attribute vec4 a_UV;
				varying vec2 v_UV;
				void main()
				{
					gl_Position = a_Position;
					v_UV = vec2(a_UV.x, 1.0 - a_UV.y);
				}
			";

				private const string FragmentShader = "#extension GL_OES_EGL_image_external : require" + @"

				precision mediump float;
				varying vec2 v_UV;
				uniform samplerExternalOES u_Texture;
				void main()
				{
					gl_FragColor = texture2D(u_Texture, v_UV);
				}
			";

				private const int TextureStage = 0;

				private static SurfaceTextureProgram instance;

				public static SurfaceTextureProgram GetInstance() => instance ?? (instance = new SurfaceTextureProgram());

				public SurfaceTextureProgram() : base(GetShaders(), GetAttribLocations(), GetSamplers())
				{
				}

				private static Shader[] GetShaders()
				{
					return new Shader[] {
						new VertexShader(VertexShader),
						new FragmentShader(FragmentShader)
					};
				}

				private static AttribLocation[] GetAttribLocations()
				{
					return new AttribLocation[] {
						new AttribLocation { Name = "a_Position", Index = ShaderPrograms.Attributes.Pos1 },
						new AttribLocation { Name = "a_UV", Index = ShaderPrograms.Attributes.UV1 }
					};
				}

				private static Sampler[] GetSamplers()
				{
					return new Sampler[] {
						new Sampler { Name = "u_Texture", Stage = TextureStage }
					};
				}
			}

			private SurfaceTextureMaterial material;
			private Mesh<VertexPosUV> mesh;

			private uint surfaceTextureId;
			private SurfaceTexture surfaceTexture;
			public Surface Surface { get; private set; }

			//https://developer.android.com/reference/android/graphics/SurfaceTexture.html
			private const TextureTarget TextureTarget = (TextureTarget)All.TextureExternalOes; //<- IMPORTANT for surface texture!!!

			private static VertexPosUV[] meshVertexes = new VertexPosUV[] {
					new VertexPosUV() { UV1 = new Vector2(0, 0), Pos = new Vector2(-1, 1) },
					new VertexPosUV() { UV1 = new Vector2(1, 0), Pos = new Vector2(1, 1) },
					new VertexPosUV() { UV1 = new Vector2(1, 1), Pos = new Vector2(1, -1) },
					new VertexPosUV() { UV1 = new Vector2(0, 1), Pos = new Vector2(-1, -1) },
				};

			public SurfaceTextureRenderer()
			{
				material = new SurfaceTextureMaterial();
				mesh = new Mesh<VertexPosUV>();
				mesh.Indices = new ushort[] {
					0, 1, 2, 2, 3, 0
				};
				mesh.Vertices = meshVertexes;
				mesh.AttributeLocations = new[] { ShaderPrograms.Attributes.Pos1, ShaderPrograms.Attributes.UV1 };
				mesh.DirtyFlags = MeshDirtyFlags.All;
				CreateSurface();
			}

			[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 32)]
			public struct VertexPosUV
			{
				public Vector2 Pos;
				public Vector2 UV1;
			}

			private void PushTexture()
			{
				PlatformRenderer.SetTexture(0, null);
				GL.ActiveTexture(TextureUnit.Texture0);
				PlatformRenderer.CheckErrors();
				GL.BindTexture(TextureTarget, surfaceTextureId);
			}

			private void PopTexture()
			{
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget, 0);
				PlatformRenderer.SetTexture(0, null);
			}

			private void CreateSurface()
			{
				surfaceTextureId = (uint)GL.GenTexture();
				PushTexture();
				GL.TexParameter(TextureTarget, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
				GL.TexParameter(TextureTarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
				GL.TexParameter(TextureTarget, TextureParameterName.TextureWrapS, (int)OpenTK.Graphics.ES20.TextureWrapMode.ClampToEdge);
				GL.TexParameter(TextureTarget, TextureParameterName.TextureWrapT, (int)OpenTK.Graphics.ES20.TextureWrapMode.ClampToEdge);
				surfaceTexture = new SurfaceTexture((int)surfaceTextureId);
				Surface = new Surface(surfaceTexture);
				PopTexture();
			}

			public void Render(ITexture target)
			{
				if (target != null) {
					RendererWrapper.Current.PushState(RenderState.Viewport | RenderState.Shader | RenderState.Blending);
					RendererWrapper.Current.Viewport = new Viewport(0, 0, target.ImageSize.Width, target.ImageSize.Height);
					RendererWrapper.Current.PushRenderTarget(target);
					Render();
					RendererWrapper.Current.PopRenderTarget();
					RendererWrapper.Current.PopState();
				} else {
					Render();
				}
			}

			public void Render()
			{
				surfaceTexture.UpdateTexImage();
				PushTexture();
				material.Apply(0);
				mesh.DrawIndexed(0, mesh.Indices.Length);
				PopTexture();

			}
		}
	}
}
#endif
