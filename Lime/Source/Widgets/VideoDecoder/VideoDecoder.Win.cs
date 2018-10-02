#if WIN
using MFDecoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lime
{
	public class ReadWriteStream : Stream
	{
		private readonly MemoryStream stream;
		private readonly object locker = new object();

		public ReadWriteStream()
		{
			stream = new MemoryStream();
		}

		public override bool CanRead => true;

		public override bool CanSeek => false;

		public override bool CanWrite => true;

		public override long Length => stream.Length;

		public override long Position {
			get { return readPosition; }
			set { throw new NotImplementedException(); }
		}

		private long readPosition;
		private long writePosition;

		public override void Flush()
		{
			lock (locker) {
				stream.Flush();
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			lock (locker) {
				stream.Position = readPosition;
				int read = stream.Read(buffer, offset, count);
				readPosition = stream.Position;
				return read;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			lock (locker) {
				stream.Position = writePosition;
				stream.Write(buffer, offset, count);
				writePosition = stream.Position;
			}
		}
	}

	public class AudioPlayer
	{
		private ReadWriteStream stream;
		private long writePosition = 0;
		private Sound sound;
		public AudioPlayer()
		{
			stream = new ReadWriteStream();
			sound = PlatformAudioSystem.Play(stream, AudioChannelGroup.Music);
		}

		public void Bump()
		{
			//sound.Bump();
		}

		public void Write(byte[] pcm)
		{
			stream.Write(pcm, 0, pcm.Length);
		}
	}

	public class VideoDecoder : IDisposable
	{
		public int Width => width;
		public int Height => height;
		public bool HasNewTexture = false;
		public bool Looped = false;
		public ITexture Texture => texture;
		public Action OnStart;

		private Texture2D lumaTexture;
		private Texture2D chromaTexture;
		private RenderTexture texture;
		private static YUVtoRGBMaterial material;
		private static Mesh<VertexPosUV> mesh;

		private AudioPlayer audioPlayer;

		private MFDecoder.Decoder decoder;
		private int width;
		private int height;
		private CancellationTokenSource stopDecodeCancelationTokenSource;
		private ManualResetEvent checkVideo;
		private ManualResetEvent checkAudio;
		private ManualResetEvent checkQueue;

		private Queue<Sample> audioQueue;
		private Queue<Sample> videoQueue;
		private const int MaxAudioQueue = 100;
		private const int MaxVideoQueue = 100;

		private long currentPosition;

		private Sample currentVideoSample;

		private State state;

		private enum State
		{
			Initialized,
			Started,
			Stoped,
			Paused,
			Finished
		}

		private class Initializer
		{
			public Initializer()
			{
				MFDecoder.MediaFoundation.Initialize();
			}

			~Initializer()
			{
				MediaFoundation.Shutdown();
			}
		}

		private static Initializer finalizer = new Initializer();

		public VideoDecoder(string path)
		{
			decoder = new MFDecoder.Decoder(path);
			checkVideo = new ManualResetEvent(false);
			checkAudio = new ManualResetEvent(false);
			checkQueue = new ManualResetEvent(false);
			state = State.Initialized;
			width = decoder.GetWidth();
			height = decoder.GetHeight();
			lumaTexture = new Texture2D();
			chromaTexture = new Texture2D();
			texture = new RenderTexture(width, height);
			audioQueue = new Queue<Sample>();
			videoQueue = new Queue<Sample>();
			if (material == null) {
				material = new YUVtoRGBMaterial();
				mesh = new Mesh<VertexPosUV>();
				mesh.Indices = new ushort[] {
					0, 1, 2, 2, 3, 0
				};
				mesh.Vertices = new VertexPosUV[] {
					new VertexPosUV() { UV1 = new Vector2(0, 0), Pos = new Vector2(-1,  1) },
					new VertexPosUV() { UV1 = new Vector2(1, 0), Pos = new Vector2( 1,  1) },
					new VertexPosUV() { UV1 = new Vector2(1, 1), Pos = new Vector2( 1, -1) },
					new VertexPosUV() { UV1 = new Vector2(0, 1), Pos = new Vector2(-1, -1) },
				};
				mesh.AttributeLocations = new[] { ShaderPrograms.Attributes.Pos1, ShaderPrograms.Attributes.UV1 };
				mesh.DirtyFlags = MeshDirtyFlags.All;
			}
			audioPlayer = new AudioPlayer();
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 32)]
		public struct VertexPosUV
		{
			public Vector2 Pos;
			public Vector2 UV1;
		}

		public async System.Threading.Tasks.Task Start()
		{
			if (state == State.Started) {
				return;
			}
			stopDecodeCancelationTokenSource = new CancellationTokenSource();
			var token = stopDecodeCancelationTokenSource.Token;
			do {
				if (state == State.Finished) {
					currentPosition = 0;
					decoder.SetPosition(0);
				}
				state = State.Started;
				bool queueTaskCompleted = false;
				OnStart?.Invoke();
				var queueTask = System.Threading.Tasks.Task.Run(() => {
					try {
						var audioIsEnded = false;
						var videoIsEnded = false;
						while (!audioIsEnded || !videoIsEnded) {
							var sample = decoder.ReadSample();
							if (sample == null) {
								Debug.Write("VideoDecoder: Sample is null");
								break;
							}
							Queue<Sample> queue;
							int maxItems;
							ManualResetEvent resetEvent;
							if (sample.IsAudio) {
								queue = audioQueue;
								maxItems = MaxAudioQueue;
								resetEvent = checkAudio;
								if (sample.IsEos) {
									audioIsEnded = true;
									continue;
								}
							} else {
								queue = videoQueue;
								maxItems = MaxVideoQueue;
								resetEvent = checkVideo;
								if (sample.IsEos) {
									videoIsEnded = true;
									continue;
								}
							}
							while (queue.Count >= maxItems) {
								checkQueue.WaitOne();
								checkQueue.Reset();
								token.ThrowIfCancellationRequested();
							}
							queue.Enqueue(sample);
							resetEvent.Set();
						}
					} finally {
						queueTaskCompleted = true;
					}
				}, token);

				var videoTask = System.Threading.Tasks.Task.Run(() => {
					while (!queueTaskCompleted || videoQueue.Count > 0) {
						while (videoQueue.Count > 0) {
							var sample = videoQueue.Dequeue();
							while (currentPosition < sample.PresentationTime) {
								checkVideo.WaitOne();
								checkVideo.Reset();
								token.ThrowIfCancellationRequested();
							}
							currentVideoSample = sample;
							HasNewTexture = true;
							checkQueue.Set();
						}
					}
				}, token);

				var audioTask = System.Threading.Tasks.Task.Run(() => {
					while (!queueTaskCompleted || audioQueue.Count > 0) {
						while (audioQueue.Count > 0) {
							var sample = audioQueue.Dequeue();
							while (currentPosition < sample.PresentationTime) {
								checkAudio.WaitOne();
								checkAudio.Reset();
								token.ThrowIfCancellationRequested();
							}
							//currentVideoSample = sample;
							if (!sample.IsEos) {
								audioPlayer.Write(sample.Data);
							}
							checkQueue.Set();
						}
					}
				}, token);
				try {
					await queueTask;
					await System.Threading.Tasks.Task.WhenAll(audioTask, videoTask);
				} catch (System.OperationCanceledException e) {
					Debug.Write("VideoPlayer: queueTask canceled!");
				}
				
				if (!token.IsCancellationRequested) {
					state = State.Finished;
				}
			} while (Looped && !token.IsCancellationRequested);
		}

		public void Stop()
		{
			if (state == State.Stoped || state == State.Finished || state == State.Initialized) {
				return;
			}
			Pause();
			state = State.Stoped;
			currentPosition = 0;
			decoder.SetPosition(0);
			audioQueue.Clear();
			videoQueue.Clear();
		}

		public void Pause()
		{
			if (state == State.Started) {
				state = State.Paused;
				stopDecodeCancelationTokenSource.Cancel();
				checkAudio.Set();
				checkVideo.Set();
				checkQueue.Set();
			}
		}

		public void Update(float delta)
		{
			if (state == State.Started) {
				currentPosition += (long)(delta * 10000000);
				checkAudio.Set();
				checkVideo.Set();
				audioPlayer.Bump();
			}
		}

		public void UpdateTexture()
		{
			if (HasNewTexture) {
				HasNewTexture = false;
				var sample = currentVideoSample;
				var pinnedArray = GCHandle.Alloc(sample.Data, GCHandleType.Pinned);
				var pointer = pinnedArray.AddrOfPinnedObject();
				var pixelInternalFormat = (OpenTK.Graphics.ES20.PixelInternalFormat.Luminance); //OpenTK.Graphics.ES20.PixelInternalFormat.Luminance
				lumaTexture.LoadImage(pointer, width, height, pixelInternalFormat, OpenTK.Graphics.ES20.PixelFormat.Luminance);


				pixelInternalFormat = (OpenTK.Graphics.ES20.PixelInternalFormat.LuminanceAlpha);
				var pixelFormat = (OpenTK.Graphics.ES20.PixelFormat.LuminanceAlpha);
				chromaTexture.LoadImage(pointer + width * height, width / 2, height / 2, pixelInternalFormat, pixelFormat);
				pinnedArray.Free();

				RendererWrapper.Current.PushState(RenderState.Viewport | RenderState.Shader | RenderState.Blending);
				RendererWrapper.Current.Viewport = new Viewport(0, 0, texture.ImageSize.Width, texture.ImageSize.Height);
				RendererWrapper.Current.PushRenderTarget(texture);
				PlatformRenderer.SetTexture(0, lumaTexture);
				PlatformRenderer.SetTexture(1, chromaTexture);
				material.Apply(0);
				mesh.DrawIndexed(0, mesh.Indices.Length);
				RendererWrapper.Current.PopRenderTarget();
				RendererWrapper.Current.PopState();
			}
		}

		public void Dispose()
		{
		}



		public class YUVtoRGBMaterial : IMaterial
		{
			private readonly BlendState blendState;
			private readonly ShaderParams[] shaderParamsArray;
			private readonly ShaderParams shaderParams;

			public float Strength { get; set; } = 1f;

			public int PassCount => 1;

			public YUVtoRGBMaterial()
			{
				blendState = BlendState.Default;
				shaderParams = new ShaderParams();
				shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			}

			public void Apply(int pass)
			{
				PlatformRenderer.SetBlendState(blendState);
				PlatformRenderer.SetShaderProgram(YUVtoRGBProgram.GetInstance());
				PlatformRenderer.SetShaderParams(shaderParamsArray);
			}

			public void Invalidate() { }

			public IMaterial Clone() => new YUVtoRGBMaterial();
		}

		private class YUVtoRGBProgram : ShaderProgram
		{
			private const string VertexShader = @"
				attribute vec4 a_Position;
				attribute vec2 a_UV;
				varying vec2 v_UV;
				void main()
				{
					gl_Position = a_Position;
					v_UV = vec2(a_UV.x, 1.0 - a_UV.y);
				}
			";

			private const string FragmentShader = @"
				
				uniform sampler2D u_SamplerY;
				uniform sampler2D u_SamplerUV;
				varying highp vec2 v_UV;

				void main()
				{
					mediump vec3 yuv;
					lowp vec3 rgb;
					yuv.x = texture2D(u_SamplerY, v_UV).r;
					yuv.yz = texture2D(u_SamplerUV, v_UV).ra - vec2(0.5, 0.5);
					// Using BT.709 which is the standard for HDTV
					
					rgb = mat3( 1, 1, 1,
								0, -0.18732, 1.8556,
								1.57481, -0.46813, 0
					) * yuv;
					
					gl_FragColor = vec4(rgb, 1);
				}
			";

			private const int LumaTextureStage = 0;
			private const int ChromaTextureStage = 1;

			private static YUVtoRGBProgram instance;

			public static YUVtoRGBProgram GetInstance() => instance ?? (instance = new YUVtoRGBProgram());

			public YUVtoRGBProgram() : base(GetShaders(), GetAttribLocations(), GetSamplers())
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
					new Sampler { Name = "u_SamplerY", Stage = LumaTextureStage },
					new Sampler { Name = "u_SamplerUV", Stage = ChromaTextureStage }
				};
			}
		}
	}
}
#endif
