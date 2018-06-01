#if OPENGL
using System;

#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES20;
using GLStencilOp = OpenTK.Graphics.ES20.StencilOp;
#else
using OpenTK.Graphics.OpenGL;
using GLStencilOp = OpenTK.Graphics.OpenGL.StencilOp;
#endif

#if iOS || ANDROID
using StencilOpFace = OpenTK.Graphics.ES20.CullFaceMode;
using StencilFuncFace = OpenTK.Graphics.ES20.CullFaceMode;
#elif MAC || MONOMAC
using StencilOpFace = OpenTK.Graphics.OpenGL.StencilFace;
using StencilFuncFace = OpenTK.Graphics.OpenGL.Version20;
#else
using StencilOpFace = OpenTK.Graphics.ES20.StencilFace;
using StencilFuncFace = OpenTK.Graphics.ES20.StencilFace;
#endif

using System.Collections.Generic;
using System.Runtime.InteropServices;

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

	public unsafe static class PlatformRenderer
	{
		public static int DrawCount = 0;

		public static uint CurrentFramebuffer { get; private set; }
		public static uint DefaultFramebuffer { get; private set; }

		private static int maxVertexAttribs;
		private static int maxVertexBufferSlots;
		private static int maxTextureSlots;

		private static VertexInputLayout defaultVertexInputLayout = VertexInputLayout.New(new VertexInputElement[0]);

		private static Viewport viewport = Viewport.Default;
		private static Viewport lastViewport;
		private static bool viewportDirty;

		private static BlendState blendState;
		private static BlendState lastBlendState = new BlendState();
		private static bool blendStateDirty;

		private static DepthState depthState = DepthState.DepthDisabled;
		private static DepthState lastDepthState;
		private static bool depthStateDirty;

		private static ScissorState scissorState = ScissorState.ScissorDisabled;
		private static ScissorState lastScissorState;
		private static bool scissorStateDirty;

		private static StencilState stencilState = StencilState.Default;
		private static StencilState lastStencilState;
		private static bool stencilStateDirty;

		private static ColorWriteMask colorWriteMask = ColorWriteMask.All;
		private static ColorWriteMask lastColorWriteMask;

		private static CullMode cullMode = CullMode.None;
		private static CullMode lastCullMode;

		private static FrontFace frontFace = FrontFace.CW;
		private static FrontFace lastFrontFace;

		private static VertexInputLayout vertexInputLayout;
		private static int lastVertexAttribMask;

		private static VertexBufferView[] vertexBufferViews;
		private static bool vertexAttribsDirty;
		private static int lastBaseVertex;

		private static IndexBufferView indexBufferView;
		private static bool indexBufferDirty;

		private static ITexture[] textures;
		private static int texturesDirtyMask;

		private static ShaderProgram shaderProgram;
		private static bool shaderProgramDirty;

		private static ShaderParams[] shaderParamsArray = new ShaderParams[0];
		private static int shaderParamsArrayCount;

		private static Color4 lastClearColor;
		private static float lastClearDepth;
		private static byte lastClearStencil;

		public static bool OffscreenRendering => CurrentFramebuffer != DefaultFramebuffer;

		public static event Action RenderTargetChanged;

		public static int GetGLESMajorVersion()
		{
#if iOS || ANDROID
			int majorVersion;
			GL.GetInteger((GetPName)33307, out majorVersion);
			return majorVersion;
#else
			return 0;
#endif
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void CheckErrors()
		{
#if ANDROID || iOS
			var errCode = GL.GetErrorCode();
#else
			var errCode = GL.GetError();
#endif
			if (errCode == ErrorCode.NoError)
				return;
			string errors = "";
			do {
				if (errors != "")
					errors += ", ";
				errors += errCode.ToString();
#if ANDROID || iOS
				errCode = GL.GetErrorCode();
#else
				errCode = GL.GetError();
#endif
			} while (errCode != ErrorCode.NoError && errCode != ErrorCode.InvalidOperation);
			throw new Exception("OpenGL error(s): " + errors);
		}

		static PlatformRenderer()
		{
			DefaultFramebuffer = uint.MaxValue;
			CheckFeatures();
			textures = new ITexture[maxTextureSlots];
			vertexBufferViews = new VertexBufferView[maxVertexBufferSlots];
		}

		public static void BeginFrame()
		{
			DrawCount = 0;
			SaveDefaultFramebuffer();
			CurrentFramebuffer = DefaultFramebuffer;
			Reset();
			Clear(ClearOptions.All, Color4.Black);
		}

		private static void SaveDefaultFramebuffer()
		{
			if (DefaultFramebuffer == uint.MaxValue) {
				var p = new int[1];
				GL.GetInteger(GetPName.FramebufferBinding, p);
				DefaultFramebuffer = (uint)p[0];
			}
		}

		private static void CheckFeatures()
		{
			GL.GetInteger(GetPName.MaxTextureImageUnits, out maxTextureSlots);
			GL.GetInteger(GetPName.MaxVertexAttribs, out maxVertexAttribs);
			maxTextureSlots = Math.Min(maxTextureSlots, 32);
			maxVertexAttribs = Math.Min(maxVertexAttribs, 32);
			maxVertexBufferSlots = maxVertexAttribs;
		}

		public static void SetTextureLegacy(int slot, ITexture texture)
		{
			SetTexture(slot, texture?.AtlasTexture);
		}

		public static void SetTexture(int slot, ITexture texture)
		{
			if (textures[slot] != texture) {
				if (texture != null && texture.AtlasTexture != texture)
					throw new InvalidOperationException();
				textures[slot] = texture;
				texturesDirtyMask |= 1 << slot;
			}
		}

		internal static void MarkTextureSlotAsDirty(int slot)
		{
			texturesDirtyMask |= 1 << slot;
		}

		internal static void MarkAllTextureSlotsAsDirty()
		{
			texturesDirtyMask = ~0;
		}

		public static void SetVertexInputLayout(VertexInputLayout layout)
		{
			layout = layout ?? defaultVertexInputLayout;
			if (vertexInputLayout != layout) {
				vertexInputLayout = layout;
				vertexAttribsDirty = true;
			}
		}

		public static void SetVertexBuffer(int slot, VertexBuffer buffer, int offset)
		{
			if (vertexBufferViews[slot].Buffer != buffer ||
				vertexBufferViews[slot].Offset != offset
			) {
				vertexBufferViews[slot] = new VertexBufferView {
					Buffer = buffer,
					Offset = offset
				};
				vertexAttribsDirty = true;
			}
		}

		public static void SetIndexBuffer(IndexBuffer buffer, int offset, IndexFormat format)
		{
			if (indexBufferView.Buffer != buffer) {
				indexBufferDirty = true;
			}
			indexBufferView = new IndexBufferView {
				Buffer = buffer,
				Format = format,
				Offset = offset
			};
		}

		public static void SetShaderProgram(ShaderProgram program)
		{
			if (shaderProgram != program) {
				shaderProgram = program;
				shaderProgramDirty = true;
			}
		}

		private static void ResetShaderProgram()
		{
			GL.UseProgram(0);
			CheckErrors();
			shaderProgram = null;
			shaderProgramDirty = false;
		}

		private static void ResetShaderParams()
		{
			Array.Clear(shaderParamsArray, 0, shaderParamsArray.Length);
			shaderParamsArrayCount = 0;
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
			viewport = value;
			viewportDirty = true;
		}

		public static void SetBlendState(BlendState value)
		{
			blendState = value;
			blendStateDirty = true;
		}

		public static void SetDepthState(DepthState value)
		{
			depthState = value;
			depthStateDirty = true;
		}

		public static void SetStencilState(StencilState value)
		{
			stencilState = value;
			stencilStateDirty = true;
		}

		public static void SetScissorState(ScissorState value)
		{
			scissorState = value;
			scissorStateDirty = true;
		}

		public static void SetColorWriteMask(ColorWriteMask value)
		{
			colorWriteMask = value;
		}

		public static void SetCullMode(CullMode value)
		{
			cullMode = value;
		}

		private static void SetFrontFace(FrontFace value)
		{
			frontFace = value;
		}

		public static void Clear(ClearOptions options, Color4 color)
		{
			Clear(options, color, 1, 0);
		}

		public static void Clear(ClearOptions options, Color4 color, float depth, byte stencil)
		{
			if (options == ClearOptions.None)
				return;
			ApplyClearValues(options, color, depth, stencil, force: false);
			ClearBufferMask clearBufferMask = 0;
			if ((options & ClearOptions.ColorBuffer) != 0) {
				clearBufferMask |= ClearBufferMask.ColorBufferBit;
				if (lastColorWriteMask != ColorWriteMask.All) {
					GL.ColorMask(true, true, true, true);
					CheckErrors();
				}
			}
			if ((options & ClearOptions.DepthBuffer) != 0) {
				clearBufferMask |= ClearBufferMask.DepthBufferBit;
				if (!lastDepthState.WriteEnable) {
					GL.DepthMask(true);
					CheckErrors();
				}
				if (lastViewport.MinDepth != 0 || lastViewport.MaxDepth != 1) {
					GL.DepthRange(0, 1);
					CheckErrors();
				}
			}
			if ((options & ClearOptions.StencilBuffer) != 0) {
				clearBufferMask |= ClearBufferMask.StencilBufferBit;
				if (lastStencilState.WriteMask != 0xff) {
					GL.StencilMask(0xff);
					CheckErrors();
				}
			}
			var needRestoreScissorBounds = false;
			var lastScissorBounds = lastScissorState.Bounds;
			if (lastScissorBounds.X != lastViewport.X ||
				lastScissorBounds.Y != lastViewport.Y ||
				lastScissorBounds.Width != lastViewport.Width ||
				lastScissorBounds.Height != lastViewport.Height
			) {
				GL.Scissor(lastViewport.X, lastViewport.Y, lastViewport.Width, lastViewport.Height);
				CheckErrors();
				needRestoreScissorBounds = true;
			}
			GL.Clear(clearBufferMask);
			CheckErrors();
			if ((options & ClearOptions.ColorBuffer) != 0) {
				if (lastColorWriteMask != ColorWriteMask.All) {
					GL.ColorMask(
						(lastColorWriteMask & ColorWriteMask.Red) != 0,
						(lastColorWriteMask & ColorWriteMask.Green) != 0,
						(lastColorWriteMask & ColorWriteMask.Blue) != 0,
						(lastColorWriteMask & ColorWriteMask.Alpha) != 0);
					CheckErrors();
				}
			}
			if ((options & ClearOptions.DepthBuffer) != 0) {
				if (!lastDepthState.WriteEnable) {
					GL.DepthMask(false);
					CheckErrors();
				}
				if (lastViewport.MinDepth != 0 || lastViewport.MaxDepth != 1) {
					GL.DepthRange(lastViewport.MinDepth, lastViewport.MaxDepth);
					CheckErrors();
				}
			}
			if ((options & ClearOptions.StencilBuffer) != 0) {
				if (lastStencilState.WriteMask != 0xff) {
					GL.StencilMask(lastStencilState.WriteMask);
					CheckErrors();
				}
			}
			if (needRestoreScissorBounds) {
				GL.Scissor(lastScissorBounds.X, lastScissorBounds.Y, lastScissorBounds.Width, lastScissorBounds.Height);
				CheckErrors();
			}
		}

		private static void ApplyState(bool force)
		{
			ApplyViewport(force);
			ApplyBlendState(force);
			ApplyDepthState(force);
			ApplyStencilState(force);
			ApplyScissorState(force);
			ApplyColorWriteMask(force);
			ApplyCullMode(force);
			ApplyFrontFace(force);
		}

		private static void Reset()
		{
			viewport = Viewport.Default;
			blendState = BlendState.Default;
			depthState = DepthState.Default;
			stencilState = StencilState.Default;
			scissorState = ScissorState.Default;
			colorWriteMask = ColorWriteMask.All;
			cullMode = CullMode.None;
			frontFace = FrontFace.CW;
			ApplyState(force: true);
			ApplyClearValues(ClearOptions.All, Color4.Black, 1, 0, force: true);
			ResetTextures();
			ResetVertexBuffers();
			ResetIndexBuffer();
			ResetShaderProgram();
			ResetShaderParams();
		}

		private static void ResetTextures()
		{
			for (var i = 0; i < maxTextureSlots; i++) {
				GL.ActiveTexture(TextureUnit.Texture0 + i);
				CheckErrors();
				GL.BindTexture(TextureTarget.Texture2D, 0);
				CheckErrors();
			}
			Array.Clear(textures, 0, textures.Length);
			texturesDirtyMask = 0;
		}

		private static void ResetVertexBuffers()
		{
			for (var i = 0; i < maxVertexAttribs; i++) {
				GL.DisableVertexAttribArray(i);
				CheckErrors();
			}
			lastVertexAttribMask = 0;
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			CheckErrors();
			Array.Clear(vertexBufferViews, 0, vertexBufferViews.Length);
			vertexInputLayout = defaultVertexInputLayout;
			vertexAttribsDirty = false;
		}

		private static void ResetIndexBuffer()
		{
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
			CheckErrors();
			indexBufferView = new IndexBufferView();
			indexBufferDirty = false;
		}

		private static void ApplyClearValues(ClearOptions options, Color4 color, float depth, byte stencil, bool force)
		{
			if ((options & ClearOptions.ColorBuffer) != 0 && (force || color != lastClearColor)) {
				GL.ClearColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
				CheckErrors();
				lastClearColor = color;
			}
			if ((options & ClearOptions.DepthBuffer) != 0 && (force || depth != lastClearDepth)) {
				GL.ClearDepth(depth);
				CheckErrors();
				lastClearDepth = depth;
			}
			if ((options & ClearOptions.StencilBuffer) != 0 && (force || stencil != lastClearStencil)) {
				GL.ClearStencil(stencil);
				CheckErrors();
				lastClearStencil = stencil;
			}
		}

		private static void ApplyViewport(bool force)
		{
			if (force || viewportDirty) {
				if (force ||
					viewport.X != lastViewport.X ||
					viewport.Y != lastViewport.Y ||
					viewport.Width != lastViewport.Width ||
					viewport.Height != lastViewport.Height
				) {
					GL.Viewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);
					CheckErrors();
					lastViewport.X = viewport.X;
					lastViewport.Y = viewport.Y;
					lastViewport.Width = viewport.Width;
					lastViewport.Height = viewport.Height;
				}
				if (force ||
					viewport.MinDepth != lastViewport.MinDepth ||
					viewport.MaxDepth != lastViewport.MaxDepth
				) {
					GL.DepthRange(viewport.MinDepth, viewport.MaxDepth);
					CheckErrors();
					lastViewport.MinDepth = viewport.MinDepth;
					lastViewport.MaxDepth = viewport.MaxDepth;
				}
				viewportDirty = false;
			}
		}

		private static void ApplyBlendState(bool force)
		{
			if (force || blendStateDirty) {
				if (force || blendState.Enable != lastBlendState.Enable) {
					if (blendState.Enable)
						GL.Enable(EnableCap.Blend);
					else
						GL.Disable(EnableCap.Blend);
					CheckErrors();
					lastBlendState.Enable = blendState.Enable;
				}
				if (force || blendState.Enable) {
					if (force ||
						blendState.ColorBlendFunc != lastBlendState.ColorBlendFunc ||
						blendState.AlphaBlendFunc != lastBlendState.AlphaBlendFunc
					) {
						GL.BlendEquationSeparate(
							blendState.ColorBlendFunc.ToGLBlendEquationMode(),
							blendState.AlphaBlendFunc.ToGLBlendEquationMode());
						CheckErrors();
						lastBlendState.ColorBlendFunc = blendState.ColorBlendFunc;
						lastBlendState.AlphaBlendFunc = blendState.AlphaBlendFunc;
					}
					if (force ||
						blendState.ColorSrcBlend != lastBlendState.ColorSrcBlend ||
						blendState.ColorDstBlend != lastBlendState.ColorDstBlend ||
						blendState.AlphaSrcBlend != lastBlendState.AlphaSrcBlend ||
						blendState.AlphaDstBlend != lastBlendState.AlphaDstBlend
					) {
						GL.BlendFuncSeparate(
							blendState.ColorSrcBlend.ToGLBlendingFactorSrc(),
							blendState.ColorDstBlend.ToGLBlendingFactorDest(),
							blendState.AlphaSrcBlend.ToGLBlendingFactorSrc(),
							blendState.AlphaDstBlend.ToGLBlendingFactorDest());
						CheckErrors();
						lastBlendState.ColorSrcBlend = blendState.ColorSrcBlend;
						lastBlendState.ColorDstBlend = blendState.ColorDstBlend;
						lastBlendState.AlphaSrcBlend = blendState.AlphaSrcBlend;
						lastBlendState.AlphaDstBlend = blendState.AlphaDstBlend;
					}
					if (force || blendState.BlendFactor != lastBlendState.BlendFactor) {
						GL.BlendColor(
							blendState.BlendFactor.R / 255f, blendState.BlendFactor.G / 255f,
							blendState.BlendFactor.B / 255f, blendState.BlendFactor.A / 255f);
						CheckErrors();
						lastBlendState.BlendFactor = blendState.BlendFactor;
					}
				}
				blendStateDirty = false;
			}
		}

		private static void ApplyDepthState(bool force)
		{
			if (force || depthStateDirty) {
				if (force || depthState.Enable != lastDepthState.Enable) {
					if (depthState.Enable)
						GL.Enable(EnableCap.DepthTest);
					else
						GL.Disable(EnableCap.DepthTest);
					CheckErrors();
					lastDepthState.Enable = depthState.Enable;
				}
				if (force || depthState.Enable) {
					if (force || depthState.WriteEnable != lastDepthState.WriteEnable) {
						GL.DepthMask(depthState.WriteEnable);
						CheckErrors();
						lastDepthState.WriteEnable = depthState.WriteEnable;
					}
					if (force || depthState.Comparison != lastDepthState.Comparison) {
						GL.DepthFunc(depthState.Comparison.ToGLDepthFunction());
						CheckErrors();
						lastDepthState.Comparison = depthState.Comparison;
					}
				}
				depthStateDirty = false;
			}
		}

		private static void ApplyStencilState(bool force)
		{
			if (force || stencilStateDirty) {
				if (force || stencilState.Enable != lastStencilState.Enable) {
					if (stencilState.Enable)
						GL.Enable(EnableCap.StencilTest);
					else
						GL.Disable(EnableCap.StencilTest);
					CheckErrors();
					lastStencilState.Enable = stencilState.Enable;
				}
				if (force || stencilState.Enable) {
					if (force || stencilState.WriteMask != lastStencilState.WriteMask) {
						GL.StencilMask(stencilState.WriteMask);
						CheckErrors();
						lastStencilState.WriteMask = stencilState.WriteMask;
					}
					if (force ||
						stencilState.FrontFaceDepthFail != lastStencilState.FrontFaceDepthFail ||
						stencilState.FrontFaceFail != lastStencilState.FrontFaceFail ||
						stencilState.FrontFacePass != lastStencilState.FrontFacePass
					) {
						GL.StencilOpSeparate((StencilOpFace)All.Front,
							stencilState.FrontFaceFail.ToGLStencilOp(),
							stencilState.FrontFaceDepthFail.ToGLStencilOp(),
							stencilState.FrontFacePass.ToGLStencilOp());
						CheckErrors();
						lastStencilState.FrontFaceDepthFail = stencilState.FrontFaceDepthFail;
						lastStencilState.FrontFaceFail = stencilState.FrontFaceFail;
						lastStencilState.FrontFacePass = stencilState.FrontFacePass;
					}
					if (force ||
						stencilState.BackFaceDepthFail != lastStencilState.BackFaceDepthFail ||
						stencilState.BackFaceFail != lastStencilState.BackFaceFail ||
						stencilState.BackFacePass != lastStencilState.BackFacePass
					) {
						GL.StencilOpSeparate((StencilOpFace)All.Back,
							stencilState.BackFaceFail.ToGLStencilOp(),
							stencilState.BackFaceDepthFail.ToGLStencilOp(),
							stencilState.BackFacePass.ToGLStencilOp());
						CheckErrors();
						lastStencilState.BackFaceDepthFail = stencilState.BackFaceDepthFail;
						lastStencilState.BackFaceFail = stencilState.BackFaceFail;
						lastStencilState.BackFacePass = stencilState.BackFacePass;
					}
					if (force ||
						stencilState.FrontFaceComparison != lastStencilState.FrontFaceComparison ||
						stencilState.ReferenceValue != lastStencilState.ReferenceValue ||
						stencilState.ReadMask != lastStencilState.ReadMask
					) {
						GL.StencilFuncSeparate((StencilFuncFace)All.Front,
							stencilState.FrontFaceComparison.ToGLStencilFunction(),
							stencilState.ReferenceValue,
							stencilState.ReadMask);
						CheckErrors();
						lastStencilState.FrontFaceComparison = stencilState.FrontFaceComparison;
					}
					if (force ||
						stencilState.BackFaceComparison != lastStencilState.BackFaceComparison ||
						stencilState.ReferenceValue != lastStencilState.ReferenceValue ||
						stencilState.ReadMask != lastStencilState.ReadMask
					) {
						GL.StencilFuncSeparate((StencilFuncFace)All.Back,
							stencilState.BackFaceComparison.ToGLStencilFunction(),
							stencilState.ReferenceValue,
							stencilState.ReadMask);
						CheckErrors();
						lastStencilState.BackFaceComparison = stencilState.BackFaceComparison;
					}
					lastStencilState.ReferenceValue = stencilState.ReferenceValue;
					lastStencilState.ReadMask = stencilState.ReadMask;
				}
				stencilStateDirty = false;
			}
		}

		private static void ApplyScissorState(bool force)
		{
			if (force || scissorStateDirty) {
				if (force || scissorState.Enable != lastScissorState.Enable) {
					if (scissorState.Enable)
						GL.Enable(EnableCap.ScissorTest);
					else
						GL.Disable(EnableCap.ScissorTest);
					CheckErrors();
					lastScissorState.Enable = scissorState.Enable;
				}
				var bounds = scissorState.Bounds;
				if (force || (scissorState.Enable && bounds != lastScissorState.Bounds)) {
					GL.Scissor(bounds.X, bounds.Y, bounds.Width, bounds.Height);
					CheckErrors();
					lastScissorState.Bounds = bounds;
				}
				scissorStateDirty = false;
			}
		}

		private static void ApplyColorWriteMask(bool force)
		{
			if (force || colorWriteMask != lastColorWriteMask) {
				GL.ColorMask(
					(colorWriteMask & ColorWriteMask.Red) != 0,
					(colorWriteMask & ColorWriteMask.Green) != 0,
					(colorWriteMask & ColorWriteMask.Blue) != 0,
					(colorWriteMask & ColorWriteMask.Alpha) != 0);
				CheckErrors();
				lastColorWriteMask = colorWriteMask;
			}
		}

		private static void ApplyCullMode(bool force)
		{
			if (force || cullMode != lastCullMode) {
				var cullingEnabled = lastCullMode != CullMode.None;
				var needCulling = cullMode != CullMode.None;
				if (force || needCulling != cullingEnabled) {
					if (needCulling)
						GL.Enable(EnableCap.CullFace);
					else
						GL.Disable(EnableCap.CullFace);
					CheckErrors();
				}
				if (needCulling) {
					GL.CullFace(cullMode == CullMode.Back ? CullFaceMode.Back : CullFaceMode.Front);
					CheckErrors();
				}
				lastCullMode = cullMode;
			}
		}

		private static void ApplyFrontFace(bool force)
		{
			var ff = AdjustFrontFace(frontFace);
			if (force || ff != lastFrontFace) {
				GL.FrontFace(ff.ToGLFrontFaceDirection());
				CheckErrors();
				lastFrontFace = ff;
			}
		}

		private static FrontFace AdjustFrontFace(FrontFace frontFace)
		{
			return OffscreenRendering ? frontFace.Invert() : frontFace;
		}

		private static void ApplyTextures()
		{
			if (texturesDirtyMask == 0)
				return;
			for (var i = 0; i < textures.Length; i++) {
				var bit = 1 << i;
				if ((texturesDirtyMask & bit) == 0)
					continue;
				var texture = textures[i];
				GL.ActiveTexture(TextureUnit.Texture0 + i);
				CheckErrors();
				GL.BindTexture(TextureTarget.Texture2D, texture != null && !texture.IsDisposed ? texture.GetHandle() : 0);
				CheckErrors();
				texturesDirtyMask &= ~bit;
				if (texturesDirtyMask == 0)
					break;
			}
			texturesDirtyMask = 0;
		}

		private static void ApplyVertexAttribs(int baseVertex)
		{
			var attribMask = 0;
			if (vertexAttribsDirty || baseVertex != lastBaseVertex) {
				VertexBuffer lastBuffer = null;
				foreach (var element in vertexInputLayout.Elements) {
					var view = vertexBufferViews[element.Slot];
					var buffer = view.Buffer;
					if (buffer == null || buffer.IsDisposed) {
						continue;
					}
					if (buffer != lastBuffer) {
						GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.GetHandle());
						CheckErrors();
						lastBuffer = view.Buffer;
					}
					var offset = view.Offset + baseVertex * element.Stride + element.Offset;
					var format = element.Format.ToGLVertexAttributeFormat();
					GL.VertexAttribPointer(
						element.Attribute, format.NumberOfElements, format.Type,
						format.Normalized, element.Stride, new IntPtr(offset));
					CheckErrors();
					attribMask |= 1 << element.Attribute;
				}
				if (attribMask != lastVertexAttribMask) {
					for (var i = 0; i < maxVertexAttribs; i++) {
						var bit = 1 << i;
						if ((attribMask & bit) == (lastVertexAttribMask & bit))
							continue;
						if ((attribMask & bit) != 0)
							GL.EnableVertexAttribArray(i);
						else
							GL.DisableVertexAttribArray(i);
						CheckErrors();
						lastVertexAttribMask ^= bit;
						if (attribMask == lastVertexAttribMask)
							break;
					}
				}
				lastBaseVertex = baseVertex;
				vertexAttribsDirty = false;
			}
		}

		private static void ApplyIndexBuffer()
		{
			if (indexBufferDirty) {
				var buffer = indexBufferView.Buffer;
				if (buffer != null && !buffer.IsDisposed) {
					GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffer.GetHandle());
					CheckErrors();
				}
				indexBufferDirty = false;
			}
		}

		private static void ApplyShaderProgram()
		{
			if (shaderProgramDirty) {
				if (shaderProgram != null)
					GL.UseProgram(shaderProgram.GetHandle());
				else
					GL.UseProgram(0);
				CheckErrors();
				shaderProgramDirty = false;
			}
		}

		private static void ApplyShaderParams()
		{
			if (shaderProgram != null)
				shaderProgram.SyncParams(shaderParamsArray, shaderParamsArrayCount);
		}

		internal static void BindFramebuffer(uint framebuffer)
		{
			CurrentFramebuffer = framebuffer;
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
			texturesDirtyMask = ~0;
			RenderTargetChanged?.Invoke();
		}

		private static void PreDraw(int baseVertex)
		{
			ApplyState(force: false);
			ApplyTextures();
			ApplyVertexAttribs(baseVertex);
			ApplyIndexBuffer();
			ApplyShaderProgram();
			ApplyShaderParams();
		}

		public static void Draw(PrimitiveTopology topology, int startVertex, int vertexCount)
		{
			PreDraw(0);
			GL.DrawArrays(topology.ToGLPrimitiveType(), startVertex, vertexCount);
			CheckErrors();
			DrawCount++;
		}

		public static void DrawIndexed(PrimitiveTopology topology, int startIndex, int indexCount, int baseVertex = 0)
		{
			PreDraw(baseVertex);
			var indexTotalOffset = indexBufferView.Offset + startIndex * indexBufferView.Format.GetElementSize();
			GL.DrawElements(
				topology.ToGLPrimitiveType(), indexCount,
				indexBufferView.Format.ToGLDrawElementsType(), new IntPtr(indexTotalOffset));
			CheckErrors();
			DrawCount++;
		}
	}

	internal struct VertexBufferView
	{
		public VertexBuffer Buffer;
		public int Offset;
	}

	internal struct IndexBufferView
	{
		public IndexBuffer Buffer;
		public IndexFormat Format;
		public int Offset;
	}
}
#endif
