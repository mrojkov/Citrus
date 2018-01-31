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
	public class Decoder : IDisposable
	{
		public int Width => videoFormat.GetInteger(MediaFormat.KeyWidth);
		public int Height => videoFormat.GetInteger(MediaFormat.KeyHeight);
		public bool TextureIsDirty = false;

		private MediaExtractor extractor;

		private MediaCodec videoCodec;
		private MediaFormat videoFormat;
		private int videoTrack = -1;

		private MediaCodec audioCodec;
		private MediaFormat audioFormat;
		private int audioTrack = -1;

		private AudioTrack audio;

		public Decoder(string path, Surface surface)
		{
			extractor = new MediaExtractor();
			extractor.SetDataSource(path);
			for (int i = 0; i < extractor.TrackCount; ++i) {
				var format = extractor.GetTrackFormat(i);
				var mime = format.GetString(MediaFormat.KeyMime);
				if (mime.StartsWith("video/")) {
					videoFormat = format;
					videoTrack = i;
					videoCodec = MediaCodec.CreateDecoderByType(mime);
					extractor.SelectTrack(i);
					videoCodec.Configure(videoFormat, surface, null, MediaCodecConfigFlags.None);
					continue;
				}
				if (mime.StartsWith("audio/")) {
					audioFormat = format;
					audioTrack = i;
					audioCodec = MediaCodec.CreateDecoderByType(mime);
					audioCodec.Configure(audioFormat, null, null, MediaCodecConfigFlags.None);
					extractor.SelectTrack(i);
					var bufferSize = AudioTrack.GetMinBufferSize(44100, ChannelOut.Stereo, Android.Media.Encoding.Pcm16bit);
					audio = new AudioTrack(
						Android.Media.Stream.Music,
						44100,
						ChannelOut.Stereo,
						Android.Media.Encoding.Pcm16bit,
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
					} else {
						codec.QueueInputBuffer(inputIndex, 0, 0, 0, MediaCodecBufferFlags.EndOfStream);
						hasInput = false;
					}
					extractor.Advance();
				}
			}
		}

		public async System.Threading.Tasks.Task Run()
		{
			videoCodec?.Start();
			audioCodec?.Start();
			audio.Play();
			var queueTask = System.Threading.Tasks.Task.Run(() => {
				var hasVideoInput = videoCodec != null;
				var hasAudioInput = audioCodec != null;
				while (hasVideoInput || hasAudioInput) {
					var trackIndex = extractor.SampleTrackIndex;
					if (trackIndex > -1) {
						if (trackIndex == videoTrack) {
							ExtractAndQueueSample(extractor, videoCodec, ref hasVideoInput);
						} else if (trackIndex == audioTrack) {
							ExtractAndQueueSample(extractor, audioCodec, ref hasAudioInput);
						}
					}
				}
			});


			var processVideo = System.Threading.Tasks.Task.Run(() => {
				var info = new BufferInfo();
				var eosReceived = false;
				while (!eosReceived) {
					var outIndex = videoCodec.DequeueOutputBuffer(info, 10000);
					if (outIndex >= 0) {
						videoCodec.ReleaseOutputBuffer(outIndex, true);
						TextureIsDirty = true;
					}

					if (info.Flags.HasFlag(MediaCodecBufferFlags.EndOfStream)) {
						eosReceived = true;
					}
				}
			});

			var processAudio = System.Threading.Tasks.Task.Run(() => {
				var eosReceived = false;
				var info = new BufferInfo();
				while (!eosReceived) {
					var outIndex = audioCodec.DequeueOutputBuffer(info, 10000);
					if (outIndex >= 0) {
						var buffer = audioCodec.GetOutputBuffer(outIndex);
						var numChannels = audioFormat.GetInteger(MediaFormat.KeyChannelCount);
						audio.Write(buffer, info.Size, WriteMode.Blocking);
						audioCodec.ReleaseOutputBuffer(outIndex, false);
					}

					if (info.Flags.HasFlag(MediaCodecBufferFlags.EndOfStream)) {
						eosReceived = true;
					}
				}
			});

			await System.Threading.Tasks.Task.WhenAll(queueTask, processVideo, processVideo);

			videoCodec?.Stop();
			audioCodec?.Stop();
		}

		public void Dispose()
		{
			videoCodec?.Stop();
			audioCodec?.Stop();
			videoCodec?.Release();
			audioCodec?.Release();
			extractor.Release();
		}
	}

	public class SurfaceTextureRenderer {

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

			private const string FragmentShader = @"
				#extension GL_OES_EGL_image_external : require
				precision mediump float;
				varying vec2 v_UV;
				uniform samplerExternalOES u_Texture;
				void main()
				{
					gl_FragColor = texture2D(u_Texture, v_UV);
				}
			";

			private const int TextureStage = 0;

			public SurfaceTextureProgram(): base(GetShaders(), GetAttribLocations(), GetSamplers())
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
		private const TextureTarget TextureTarget = (TextureTarget) All.TextureExternalOes;
		public Surface Surface { get; private set; }

		public SurfaceTextureRenderer()
		{
			program = new SurfaceTextureProgram();
			mesh = new Mesh() {
				IndexBuffer = new IndexBuffer {
					Data = new ushort[] {
						0, 1, 2, 2, 3, 0
					},
					Dynamic = true
				},
				VertexBuffers = new [] {
					new VertexBuffer<Vector2> {
						Data = new Vector2 [] {
							new Vector2 (-1,  1),
							new Vector2( 1,  1),
							new Vector2( 1, -1),
							new Vector2(-1, -1)
						},
						Dynamic = true
					},
					new VertexBuffer<Vector2> {
						Data = new Vector2 [] {
							new Vector2(0, 0),
							new Vector2(1, 0),
							new Vector2(1, 1),
							new Vector2(0, 1)
						},
						Dynamic = true
					}
				},
				Attributes = new [] {
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

	public class VideoPlayer : Image
	{
		private Decoder decoder;
		private SurfaceTextureRenderer renderer;

		private RenderTexture texture;
		
		public VideoPlayer(Widget parentWidget)
		{
			parentWidget.Nodes.Add(this);
			Size = parentWidget.Size;
			Anchors = Anchors.LeftRight | Anchors.TopBottom;
		}

		public override void Update(float delta)
		{
			base.Update(delta);
			var volume = AudioSystem.GetGroupVolume(AudioChannelGroup.Music);
			if (decoder.TextureIsDirty) {
				decoder.TextureIsDirty = false;
				renderer.Render(Texture);
			}
		}

		public void InitPlayer(string sourcePath)
		{
			renderer = new SurfaceTextureRenderer();
			decoder = new Decoder(sourcePath, renderer.Surface);
			texture = new RenderTexture(decoder.Width, decoder.Height);
			Texture = texture;
		}

		public void Start()
		{
			decoder.Run();
		}

		public void Pause()
		{

		}

		public void Stop()
		{

		}

		public override void Dispose()
		{
			base.Dispose();
		}
	}
}
#endif