using System;
using System.IO;
using Lime.Graphics.Platform;

namespace Lime
{
	public struct BlendState
	{
		public bool Enable;
		public BlendFunc ColorBlendFunc;
		public Blend ColorSrcBlend;
		public Blend ColorDstBlend;
		public BlendFunc AlphaBlendFunc;
		public Blend AlphaSrcBlend;
		public Blend AlphaDstBlend;
		public Color4 BlendFactor;

		public BlendFunc BlendFunc { set { ColorBlendFunc = AlphaBlendFunc = value; } }
		public Blend SrcBlend { set { ColorSrcBlend = AlphaSrcBlend = value; } }
		public Blend DstBlend { set { ColorDstBlend = AlphaDstBlend = value; } }

		public static readonly BlendState Default = new BlendState {
			Enable = true,
			BlendFunc = BlendFunc.Add,
			SrcBlend = Blend.One,
			DstBlend = Blend.Zero
		};
	}

	public struct DepthState
	{
		public bool Enable;
		public bool WriteEnable;
		public CompareFunc Comparison;

		public static readonly DepthState Default = new DepthState {
			Enable = false,
			WriteEnable = true,
			Comparison = CompareFunc.Less
		};

		public static readonly DepthState DepthDisabled = new DepthState {
			Enable = false
		};

		public static readonly DepthState DepthRead = new DepthState {
			Enable = true,
			Comparison = CompareFunc.Less,
			WriteEnable = false
		};

		public static readonly DepthState DepthWrite = new DepthState {
			Enable = true,
			Comparison = CompareFunc.Always,
			WriteEnable = true
		};

		public static readonly DepthState DepthReadWrite = new DepthState {
			Enable = true,
			Comparison = CompareFunc.Less,
			WriteEnable = true
		};
	}

	public struct StencilState
	{
		public bool Enable;
		public byte ReferenceValue;
		public byte ReadMask;
		public byte WriteMask;
		public CompareFunc FrontFaceComparison;
		public StencilOp FrontFaceDepthFail;
		public StencilOp FrontFaceFail;
		public StencilOp FrontFacePass;
		public CompareFunc BackFaceComparison;
		public StencilOp BackFaceDepthFail;
		public StencilOp BackFaceFail;
		public StencilOp BackFacePass;

		public CompareFunc Comparison { set { FrontFaceComparison = BackFaceComparison = value; } }
		public StencilOp DepthFail { set { FrontFaceDepthFail = BackFaceDepthFail = value; } }
		public StencilOp Fail { set { FrontFaceFail = BackFaceFail = value; } }
		public StencilOp Pass { set { FrontFacePass = BackFacePass = value; } }

		public static StencilState Default = new StencilState {
			Enable = false,
			ReferenceValue = 0,
			ReadMask = 0xff,
			WriteMask = 0xff,
			Comparison = CompareFunc.Always,
			DepthFail = StencilOp.Keep,
			Fail = StencilOp.Keep,
			Pass = StencilOp.Keep,
		};
	}

	public struct ScissorState
	{
		public bool Enable;
		public WindowRect Bounds;

		public static readonly ScissorState Default = new ScissorState {
			Enable = false
		};

		public static readonly ScissorState ScissorDisabled = Default;

		public ScissorState(WindowRect bounds)
		{
			Enable = true;
			Bounds = bounds;
		}
	}

	public struct Viewport
	{
		public int X;
		public int Y;
		public int Width;
		public int Height;
		public float MinDepth;
		public float MaxDepth;

		public WindowRect Bounds
		{
			get { return new WindowRect { X = X, Y = Y, Width = Width, Height = Height }; }
			set
			{
				X = value.X;
				Y = value.Y;
				Width = value.Width;
				Height = value.Height;
			}
		}

		public static readonly Viewport Default = new Viewport(0, 0, 0, 0);

		public Viewport(WindowRect bounds)
			: this(bounds.X, bounds.Y, bounds.Width, bounds.Height)
		{ }

		public Viewport(int x, int y, int width, int height)
			: this(x, y, width, height, 0.0f, 1.0f)
		{ }

		public Viewport(int x, int y, int width, int height, float minDepth, float maxDepth)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
			MinDepth = minDepth;
			MaxDepth = maxDepth;
		}
	}

	public enum BlendFunc
	{
		Add,
		Subtract,
		ReverseSubtract
	}

	public enum Blend
	{
		One,
		Zero,
		Factor,
		SrcColor,
		SrcAlpha,
		SrcAlphaSaturation,
		DstColor,
		DstAlpha,
		InverseFactor,
		InverseSrcColor,
		InverseSrcAlpha,
		InverseDstColor,
		InverseDstAlpha
	}

	public enum CompareFunc
	{
		Always,
        Never,
		Less,
		LessEqual,
		Greater,
		GreaterEqual,
		Equal,
		NotEqual
	}
	
	public enum StencilOp
	{
		Keep,
		Zero,
		Replace,
		Increment,
		IncrementSaturation,
		Decrement,
        DecrementSaturation,
		Invert,
	}

	public enum CullMode
	{
		None,
		Back,
		Front,
	}

	public enum FrontFace
	{
		CW,
		CCW
	}

	[Flags]
	public enum ColorWriteMask
	{
		None = 0,
		Red = 1,
		Green = 2,
		Blue = 4,
		Alpha = 8,
		All = Red | Green | Blue | Alpha
	}

	[Flags]
	public enum ClearOptions
	{
		None = 0,
		ColorBuffer = 1,
		DepthBuffer = 2,
		StencilBuffer = 4,
		All = ColorBuffer | DepthBuffer | StencilBuffer
	}

	public enum PrimitiveTopology
	{
		TriangleList,
		TriangleStrip
	}

	public enum IndexFormat
	{
		Index16Bits
	}

	public static class IndexFormatExtensions
	{
		public static int GetSize(this IndexFormat format)
		{
			switch (format) {
				case IndexFormat.Index16Bits:
					return 2;
				default:
					throw new ArgumentException(nameof(format));
			}
		}
	}

	public static class PlatformRenderer
	{
		private static ITexture[] textures;
		private static VertexBuffer[] vertexBuffers;
		private static int[] vertexOffsets;
		private static IndexBuffer indexBuffer;
		private static int indexOffset;
		private static IndexFormat indexFormat;
		private static ShaderProgram shaderProgram;
		private static ShaderParams[] shaderParamsArray = new ShaderParams[0];
		private static int shaderParamsArrayCount;

		internal static IPlatformRenderContext Context { get; private set; }

		public static RenderTexture CurrentRenderTarget { get; private set; }
		public static int DrawCount { get; private set; } = 0;
		public static bool OffscreenRendering => CurrentRenderTarget != null;
		public static event Action RenderTargetChanged;

		public static event Action ContextLost;

		internal static void RaiseContextLost() => ContextLost?.Invoke();

		public static bool PipelineCacheSupported => Context.PipelineCacheSupported;

		internal static void Initialize(IPlatformRenderContext context)
		{
			Context = context;
			textures = new ITexture[Context.MaxTextureSlots];
			vertexBuffers = new VertexBuffer[Context.MaxVertexBufferSlots];
			vertexOffsets = new int[Context.MaxVertexBufferSlots];
		}

		public static void BeginFrame()
		{
			DrawCount = 0;
			Reset();
		}

		public static void SetTextureLegacy(int slot, ITexture texture)
		{
			SetTexture(slot, texture?.AtlasTexture);
		}

		public static void SetTexture(int slot, ITexture texture)
		{
			Context.SetTexture(slot, texture?.GetPlatformTexture());
			textures[slot] = texture;
		}

		public static void SetVertexInputLayout(VertexInputLayout layout)
		{
			Context.SetVertexInputLayout(layout?.GetPlatformLayout());
		}

		public static void SetVertexBuffer(int slot, VertexBuffer buffer, int offset)
		{
			Context.SetVertexBuffer(slot, buffer?.GetPlatformBuffer(), offset);
			vertexBuffers[slot] = buffer;
			vertexOffsets[slot] = offset;
		}

		public static void SetIndexBuffer(IndexBuffer buffer, int offset, IndexFormat format)
		{
			Context.SetIndexBuffer(buffer?.GetPlatformBuffer(), offset, format);
			indexBuffer = buffer;
			indexOffset = offset;
			indexFormat = format;
		}

		public static void SetShaderProgram(ShaderProgram program)
		{
			shaderProgram = program;
			Context.SetShaderProgram(shaderProgram?.GetPlatformProgram());
		}

		public static void SetShaderParams(ShaderParams[] paramsArray)
		{
			SetShaderParams(paramsArray, paramsArray.Length);
		}

		public static void SetShaderParams(ShaderParams[] paramsArray, int count)
		{
			if (shaderParamsArray.Length < count) {
				shaderParamsArray = new ShaderParams[count];
			}
			Array.Copy(paramsArray, shaderParamsArray, count);
			shaderParamsArrayCount = count;
		}

		public static void SetViewport(Viewport value)
		{
			Context.SetViewport(value);
		}

		public static void SetBlendState(BlendState value)
		{
			Context.SetBlendState(value);
		}

		public static void SetDepthState(DepthState value)
		{
			Context.SetDepthState(value);
		}

		public static void SetStencilState(StencilState value)
		{
			Context.SetStencilState(value);
		}

		public static void SetScissorState(ScissorState value)
		{
			Context.SetScissorState(value);
		}

		public static void SetColorWriteMask(ColorWriteMask value)
		{
			Context.SetColorWriteMask(value);
		}

		public static void SetCullMode(CullMode value)
		{
			Context.SetCullMode(value);
		}

		public static void SetFrontFace(FrontFace value)
		{
			Context.SetFrontFace(value);
		}

		public static void Clear(ClearOptions options, Color4 color)
		{
			Clear(options, color, 1, 0);
		}

		public static void Clear(ClearOptions options, Color4 color, float depth, byte stencil)
		{
			Context.Clear(options, color, depth, stencil);
		}

		private static void Reset()
		{
			// FIXME : Reset vertex buffers, textures, index buffer, ...
			SetViewport(Viewport.Default);
			SetBlendState(BlendState.Default);
			SetDepthState(DepthState.Default);
			SetStencilState(StencilState.Default);
			SetScissorState(ScissorState.Default);
			SetColorWriteMask(ColorWriteMask.All);
			SetCullMode(CullMode.None);
			SetFrontFace(FrontFace.CW);
			SetShaderProgram(null);
			SetVertexInputLayout(null);
			SetRenderTarget(null);
			Array.Clear(shaderParamsArray, 0, shaderParamsArray.Length);
			shaderParamsArrayCount = 0;
		}

		internal static void RebindIndexBuffer(Buffer buffer)
		{
			if (indexBuffer == buffer) {
				Context.SetIndexBuffer(buffer?.GetPlatformBuffer(), indexOffset, indexFormat);
			}
		}

		internal static void RebindVertexBuffer(Buffer buffer)
		{
			for (var slot = 0; slot < vertexBuffers.Length; slot++) {
				if (vertexBuffers[slot] == buffer) {
					Context.SetVertexBuffer(slot, buffer?.GetPlatformBuffer(), vertexOffsets[slot]);
				}
			}
		}

		internal static void RebindTexture(ITexture texture)
		{
			for (var slot = 0; slot < textures.Length; slot++) {
				if (textures[slot] == texture) {
					Context.SetTexture(slot, texture?.GetPlatformTexture());
				}
			}
		}

		internal static void SetRenderTarget(RenderTexture texture)
		{
			CurrentRenderTarget = texture;
			Context.SetRenderTarget(CurrentRenderTarget?.GetPlatformTexture());
			RenderTargetChanged?.Invoke();
		}

		private static void PreDraw(PrimitiveTopology topology)
		{
			if (shaderProgram != null)
				shaderProgram.SyncParams(shaderParamsArray, shaderParamsArrayCount);
			Context.SetPrimitiveTopology(topology);
		}

		public static void Draw(PrimitiveTopology topology, int startVertex, int vertexCount)
		{
			PreDraw(topology);
			Context.Draw(startVertex, vertexCount);
			DrawCount++;
		}

		public static void DrawIndexed(PrimitiveTopology topology, int startIndex, int indexCount, int baseVertex = 0)
		{
			PreDraw(topology);
			Context.DrawIndexed(startIndex, indexCount, baseVertex);
			DrawCount++;
		}

		public static byte[] GetPipelineCacheData()
		{
			return Context.GetPipelineCacheData();
		}

		public static bool SetPipelineCacheData(byte[] data)
		{
			return Context.SetPipelineCacheData(data);
		}
	}
}
