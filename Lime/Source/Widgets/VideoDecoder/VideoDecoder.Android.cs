#if ANDROID
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		private SurfaceTextureRenderer renderer;
		private RenderTexture texture;
		private long currentPosition = 0;
		private MediaExtractor extractor;

		private MediaCodec videoCodec;
		private MediaFormat videoFormat;
		private int videoTrack = -1;

		private MediaCodec audioCodec;
		private MediaFormat audioFormat;
		private int audioTrack = -1;
		private AudioTrack audio;

		private class AudioSample
		{
			public byte[] Buffer;
			public long PresentationTime;
			public int Size;
		}

		private Queue<AudioSample> audioQueue = new Queue<AudioSample>();
		private ManualResetEvent checkAudioQueue = new ManualResetEvent(false);
		private ManualResetEvent checkVideoQueue = new ManualResetEvent(false);
		private CancellationTokenSource stopDecodeCancelationTokenSource = new CancellationTokenSource();

		private State state;

		private enum State
		{
			Initialized,
			Started,
			Stoped,
			Paused,
			Finished
		}

		public VideoDecoder(string path)
		{
			
			state = State.Initialized;
			extractor = new MediaExtractor();
			extractor.SetDataSource(path);
			for (int i = 0; i < extractor.TrackCount; ++i) {
				var format = extractor.GetTrackFormat(i);
				var mime = format.GetString(MediaFormat.KeyMime);
				if (mime.StartsWith("video/")) {
					videoFormat = format;
					videoTrack = i;
					videoCodec = MediaCodec.CreateDecoderByType(mime);
					renderer = new SurfaceTextureRenderer();
					videoCodec.Configure(videoFormat, renderer.Surface, null, MediaCodecConfigFlags.None);
					extractor.SelectTrack(i);
					texture = new RenderTexture(Width, Height);
					continue;
				}
				if (mime.StartsWith("audio/")) {
					audioFormat = format;
					audioTrack = i;
					audioCodec = MediaCodec.CreateDecoderByType(mime);
					audioCodec.Configure(audioFormat, null, null, MediaCodecConfigFlags.None);
					extractor.SelectTrack(i);
					var bufferSize = AudioTrack.GetMinBufferSize(44100, ChannelOut.Stereo, global::Android.Media.Encoding.Pcm16bit);
					audio = new AudioTrack(
						global::Android.Media.Stream.Music,
						44100,
						ChannelOut.Stereo,
						global::Android.Media.Encoding.Pcm16bit,
						bufferSize,
						AudioTrackMode.Stream
					);
					continue;
				}
			}
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

		public async System.Threading.Tasks.Task Start()
		{
			if (state == State.Started) {
				return;
			}
			stopDecodeCancelationTokenSource = new CancellationTokenSource();
			var stopDecodeCancelationToken = stopDecodeCancelationTokenSource.Token;
			var audioFinished = false;
			var hasMoreItemsInQueue = new ManualResetEvent(false);
			var audioDequeueTask = System.Threading.Tasks.Task.Run(() => {
				while (!audioFinished) {
					hasMoreItemsInQueue.WaitOne();
					hasMoreItemsInQueue.Reset();
					stopDecodeCancelationToken.ThrowIfCancellationRequested();
					while (audioQueue.Count > 0) {
						var audioSample = audioQueue.Dequeue();
						var pt = audioSample.PresentationTime / 1000000f;
						while (currentPosition < pt) {
							checkAudioQueue.WaitOne();
							checkAudioQueue.Reset();
							stopDecodeCancelationToken.ThrowIfCancellationRequested();
						}
						audio.Write(audioSample.Buffer, 0, audioSample.Size);
					}
				}
			}, stopDecodeCancelationToken);

			
			do {
				if (state == State.Finished) {
					currentPosition = 0;
					extractor.SeekTo(0, MediaExtractorSeekTo.ClosestSync);
					videoCodec?.Configure(videoFormat, renderer.Surface, null, MediaCodecConfigFlags.None);
					audioCodec?.Configure(audioFormat, null, null, MediaCodecConfigFlags.None);
				}
				
				if (state == State.Finished ||
					state == State.Initialized
				) {
					videoCodec?.Start();
					audioCodec?.Start();
				}
				audio?.Play();
				state = State.Started;

				var queueTask = System.Threading.Tasks.Task.Run(() => {
					var hasVideoInput = videoCodec != null;
					var hasAudioInput = audioCodec != null;
					while (hasVideoInput || hasAudioInput) {
						stopDecodeCancelationToken.ThrowIfCancellationRequested();
						var trackIndex = extractor.SampleTrackIndex;
						if (trackIndex > -1) {
							if (trackIndex == videoTrack) {
								ExtractAndQueueSample(extractor, videoCodec, ref hasVideoInput);
							} else if (trackIndex == audioTrack) {
								ExtractAndQueueSample(extractor, audioCodec, ref hasAudioInput);
							}
						} else {
							SignEndOfStream(videoCodec, ref hasVideoInput);
							SignEndOfStream(audioCodec, ref hasAudioInput);
							Debug.Write("trackIndex <= -1");
						}
					}
					Debug.Write("queueTask: end");
				}, stopDecodeCancelationToken);


				var processVideo = System.Threading.Tasks.Task.Run(() => {
					var info = new BufferInfo();
					var eosReceived = videoCodec == null;
					while (!eosReceived) {
						stopDecodeCancelationToken.ThrowIfCancellationRequested();
						var outIndex = videoCodec.DequeueOutputBuffer(info, 10000);
						if (outIndex >= 0) {
							var pt = info.PresentationTimeUs;
							while (currentPosition < pt) {
								checkVideoQueue.WaitOne();
								checkVideoQueue.Reset();
								if(stopDecodeCancelationToken.IsCancellationRequested) {
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
							var numChannels = audioFormat.GetInteger(MediaFormat.KeyChannelCount);
							var array = new byte[buffer.Remaining()];
							buffer.Get(array);
							audioQueue.Enqueue(new AudioSample() {
								Buffer = array,
								Size = info.Size,
								PresentationTime = info.PresentationTimeUs
							});
							//audio.Write(buffer.Duplicate(), info.Size, WriteMode.NonBlocking);
							hasMoreItemsInQueue.Set();
							audioCodec.ReleaseOutputBuffer(outIndex, false);
						}

						if (info.Flags.HasFlag(MediaCodecBufferFlags.EndOfStream)) {
							eosReceived = true;
						}
					}
				}, stopDecodeCancelationToken);
				try {
					await queueTask;
				} catch (System.OperationCanceledException e) {
					Debug.Write("VideoPlayer: queueTask canceled");
				}
				await System.Threading.Tasks.Task.WhenAll(processVideo, processAudio);
				if (state == State.Started) {
					state = State.Finished;
				}
				videoCodec?.Stop();
				audioCodec?.Stop();
			} while (Looped && !stopDecodeCancelationToken.IsCancellationRequested);
			audioFinished = true;
			checkAudioQueue.Set();
			hasMoreItemsInQueue.Set();
		}

		public void Stop()
		{
			if (state == State.Stoped || state == State.Finished || state == State.Initialized) {
				return;
			}
			Pause();
			state = State.Stoped;
			currentPosition = 0;
			extractor.SeekTo(0, MediaExtractorSeekTo.ClosestSync);
			audioCodec?.Flush();
			videoCodec?.Flush();
		}

		public void Pause()
		{
			if (state == State.Started) {
				state = State.Paused;
				stopDecodeCancelationTokenSource.Cancel();
				checkAudioQueue.Set();
				checkVideoQueue.Set();
				audio?.Pause();
			}
		}

		public void Update(float delta)
		{
			if (state == State.Started) {
				currentPosition += (long)(delta * 1000000);
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
			extractor.Dispose();
			extractor = null;
		}

		private class SurfaceTextureRenderer
		{

			private class SurfaceTextureProgram : ShaderProgram
			{
				private const string VertexShader = @"
				uniform mat4 matProjection;
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

			private SurfaceTextureProgram program;
			private Mesh mesh;

			private uint surfaceTextureId;
			private SurfaceTexture surfaceTexture;
			public Surface Surface { get; private set; }

			//https://developer.android.com/reference/android/graphics/SurfaceTexture.html
			private const TextureTarget TextureTarget = (TextureTarget)All.TextureExternalOes; //<- IMPORTANT for surface texture!!!

			public SurfaceTextureRenderer()
			{
				program = new SurfaceTextureProgram();
				mesh = new Mesh() {
					IndexBuffer = new IndexBuffer {
						Data = new ushort[] {
						0, 1, 2, 2, 3, 0
					},
					},
					VertexBuffers = new[] {
					new VertexBuffer<Vector2> {
						Data = new Vector2 [] {
							new Vector2 (-1,  1),
							new Vector2( 1,  1),
							new Vector2( 1, -1),
							new Vector2(-1, -1)
						},
					},
					new VertexBuffer<Vector2> {
						Data = new Vector2 [] {
							new Vector2(0, 0),
							new Vector2(1, 0),
							new Vector2(1, 1),
							new Vector2(0, 1)
						},
					}
				},
					Attributes = new[] {
					new [] { ShaderPrograms.Attributes.Pos1 },
					new [] { ShaderPrograms.Attributes.UV1 }
				}
				};
				CreateSurface();
			}

			private void PushTexture()
			{
				PlatformRenderer.PushTexture(0, 0);
				PlatformRenderer.CheckErrors();
				GL.BindTexture(TextureTarget, surfaceTextureId);
			}

			private void PopTexture()
			{
				GL.BindTexture(TextureTarget, 0);
				PlatformRenderer.PopTexture(0);
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
					var savedViewport = Renderer.Viewport;
					Renderer.Viewport = new WindowRect { X = 0, Y = 0, Width = target.ImageSize.Width, Height = target.ImageSize.Height };
					target.SetAsRenderTarget();
					Render();
					target.RestoreRenderTarget();
					Renderer.Viewport = savedViewport;
				} else {
					Render();
				}
			}

			public void Render()
			{

				surfaceTexture.UpdateTexImage();
				PushTexture();
				program.Use();
				PlatformRenderer.DrawTriangles(mesh, 0, mesh.IndexBuffer.Data.Length);
				PopTexture();

			}
		}
	}
}
#endif