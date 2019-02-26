using System;
using System.Collections.Generic;

#if iOS || MAC || ANDROID
using GLStencilOp = Lime.Graphics.Platform.OpenGL.StencilOp;
#else
using OpenTK.Graphics.ES20;
using GLStencilOp = OpenTK.Graphics.ES20.StencilOp;
#endif

namespace Lime.Graphics.Platform.OpenGL
{
	public class PlatformRenderContext : IPlatformRenderContext
	{
		private int glDefaultFramebuffer;
		private Viewport viewport = Viewport.Default;
		private BlendState blendState = BlendState.Default;
		private DepthState depthState = DepthState.Default;
		private StencilState stencilState = StencilState.Default;
		private ScissorState scissorState = ScissorState.Default;
		private ColorWriteMask colorWriteMask = ColorWriteMask.All;
		private CullMode cullMode = CullMode.None;
		private FrontFace frontFace = FrontFace.CW;
		private PrimitiveTopology primitiveTopology = PrimitiveTopology.TriangleList;
		private PlatformShaderProgram shaderProgram;
		private PlatformVertexInputLayout vertexInputLayout;
		private PlatformTexture2D[] textures;
		private PlatformBuffer[] vertexBuffers;
		private int[] vertexOffsets;
		private PlatformBuffer indexBuffer;
		private int indexOffset;
		private IndexFormat indexFormat;
		private PlatformRenderTexture2D renderTarget;

		private bool blendStateDirty;
		private bool depthStateDirty;
		private bool stencilStateDirty;
		private bool scissorStateDirty;
		private bool viewportDirty;
		private bool renderTargetDirty;
		private bool shaderProgramDirty;
		private bool indexBufferDirty;
		private int texturesDirtyMask;
		private long enabledVertexAttribMask;
		private long vertexBuffersDirtyMask;
		private bool vertexInputLayoutDirty;
		private int boundBaseVertex;

		private BlendState boundBlendState;
		private DepthState boundDepthState;
		private StencilState boundStencilState;
		private ScissorState boundScissorState;
		private ColorWriteMask boundColorWriteMask;
		private CullMode boundCullMode;
		private FrontFace boundFrontFace;
		private Viewport boundViewport;
		private Color4 boundClearColor;
		private float boundClearDepth;
		private byte boundClearStencil;

		internal bool SupportsTextureRG;
		internal bool SupportsPackedDepth24Stencil8;
		internal bool SupportsDepth24;
		internal bool SupportsDxt1;
		internal bool SupportsDxt3;
		internal bool SupportsDxt5;
		internal bool SupportsPvrtc1;
		internal bool SupportsPvrtc2;
		internal bool SupportsEtc1;
		internal bool SupportsEtc2;
		internal int GLMajorVersion;
		internal int GLMinorVersion;
		internal bool ESProfile;
		internal bool SupportsInternalFormatBgra8;
		internal bool SupportsExternalFormatBgra8;

		public int MaxTextureSlots { get; private set; }
		public int MaxVertexBufferSlots { get; private set; }
		public int MaxVertexAttributes { get; private set; }

		public PlatformRenderContext()
		{
			CheckFeatures();
			textures = new PlatformTexture2D[MaxTextureSlots];
			vertexBuffers = new PlatformBuffer[MaxVertexBufferSlots];
			vertexOffsets = new int[MaxVertexBufferSlots];
		}

		public void Dispose() { }

		public void Begin(int glFramebuffer)
		{
			glDefaultFramebuffer = glFramebuffer;
			renderTargetDirty = true;
			shaderProgramDirty = true;
			indexBufferDirty = true;
			texturesDirtyMask = ~0;
			vertexInputLayoutDirty = true;
			vertexBuffersDirtyMask = 0;
			enabledVertexAttribMask = 0;
			BindState(true);
			BindClearValues(true, ClearOptions.All, Color4.Black, 1, 0);
		}

		internal void InvalidateTextureBinding(int slot)
		{
			texturesDirtyMask |= 1 << slot;
		}

		internal void InvalidateRenderTargetBinding()
		{
			renderTargetDirty = true;
		}

		internal void InvalidateShaderProgramBinding()
		{
			shaderProgramDirty = true;
		}

		internal void InvalidateBufferBinding(BufferType type)
		{
			indexBufferDirty |= type == BufferType.Index;
		}

		public void End()
		{
			for (var i = 0; i < MaxVertexAttributes; i++) {
				var mask = 1L << i;
				if ((enabledVertexAttribMask & mask) != 0) {
					GL.DisableVertexAttribArray(i);
					GLHelper.CheckGLErrors();
				}
			}
		}

		public IPlatformBuffer CreateBuffer(BufferType bufferType, int size, bool dynamic)
		{
			return new PlatformBuffer(this, bufferType, size, dynamic);
		}

		public IPlatformTexture2D CreateTexture2D(Format format, int width, int height, bool mipmaps, TextureParams textureParams)
		{
			return new PlatformTexture2D(this, format, width, height, mipmaps, textureParams);
		}

		public IPlatformRenderTexture2D CreateRenderTexture2D(Format format, int width, int height, TextureParams textureParams)
		{
			return new PlatformRenderTexture2D(this, format, width, height, textureParams);
		}

		public IPlatformShader CreateShader(ShaderStageMask stage, string source)
		{
			return new PlatformShader(this, stage, source);
		}

		public IPlatformShaderProgram CreateShaderProgram(
			IPlatformShader[] shaders,
			ShaderProgram.AttribLocation[] attribLocation,
			ShaderProgram.Sampler[] samplers)
		{
			return new PlatformShaderProgram(this, shaders, attribLocation, samplers);
		}

		public IPlatformVertexInputLayout CreateVertexInputLayout(
			VertexInputLayoutBinding[] bindings,
			VertexInputLayoutAttribute[] attributes)
		{
			return new PlatformVertexInputLayout(this, bindings, attributes);
		}

		private void CheckFeatures()
		{
			GLHelper.ParseGLVersion(GL.GetString(StringName.Version), out GLMajorVersion, out GLMinorVersion, out ESProfile);
			var glExtensions = new HashSet<string>(GL.GetString(StringName.Extensions).Split(' '));
			SupportsTextureRG = !ESProfile || GLMajorVersion >= 3 || glExtensions.Contains("GL_EXT_texture_rg");
			SupportsPackedDepth24Stencil8 = !ESProfile || GLMajorVersion >= 3 || glExtensions.Contains("GL_OES_packed_depth_stencil");
			SupportsDepth24 = !ESProfile || GLMajorVersion >= 3 || glExtensions.Contains("GL_OES_depth24");
			var supportsS3tc = glExtensions.Contains("GL_EXT_texture_compression_s3tc");
			SupportsDxt1 = supportsS3tc || glExtensions.Contains("GL_EXT_texture_compression_dxt1");
			SupportsDxt3 = supportsS3tc || glExtensions.Contains("GL_ANGLE_texture_compression_dxt3");
			SupportsDxt5 = supportsS3tc || glExtensions.Contains("GL_ANGLE_texture_compression_dxt5");
			SupportsPvrtc1 = glExtensions.Contains("GL_IMG_texture_compression_pvrtc");
			SupportsPvrtc2 = glExtensions.Contains("GL_IMG_texture_compression_pvrtc2");
			SupportsEtc1 = glExtensions.Contains("GL_OES_compressed_ETC1_RGB8_texture");
			SupportsEtc2 = (ESProfile && GLMajorVersion >= 3) || glExtensions.Contains("GL_ARB_ES3_compatibility");
			SupportsInternalFormatBgra8 = ESProfile && glExtensions.Contains("GL_EXT_texture_format_BGRA8888");
			SupportsExternalFormatBgra8 = SupportsInternalFormatBgra8 || !ESProfile || glExtensions.Contains("GL_APPLE_texture_format_BGRA8888");
			GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out var maxTextureSlots);
			GL.GetInteger(GetPName.MaxVertexAttribs, out var maxVertexAttributes);
			MaxTextureSlots = Math.Min(maxTextureSlots, 32);
			MaxVertexAttributes = Math.Min(maxVertexAttributes, 64);
			MaxVertexBufferSlots = MaxVertexAttributes;
		}

		public void SetViewport(Viewport vp)
		{
			viewport = vp;
			viewportDirty = true;
		}

		public void SetBlendState(BlendState state)
		{
			blendState = state;
			blendStateDirty = true;
		}

		public void SetDepthState(DepthState state)
		{
			depthState = state;
			depthStateDirty = true;
		}

		public void SetStencilState(StencilState state)
		{
			stencilState = state;
			stencilStateDirty = true;
		}

		public void SetScissorState(ScissorState state)
		{
			scissorState = state;
			scissorStateDirty = true;
		}

		public void SetColorWriteMask(ColorWriteMask mask)
		{
			colorWriteMask = mask;
		}

		public void SetCullMode(CullMode mode)
		{
			cullMode = mode;
		}

		public void SetFrontFace(FrontFace face)
		{
			frontFace = face;
		}

		public void SetPrimitiveTopology(PrimitiveTopology topology)
		{
			primitiveTopology = topology;
		}

		public void SetShaderProgram(IPlatformShaderProgram program)
		{
			if (shaderProgram != program) {
				shaderProgram = (PlatformShaderProgram)program;
				shaderProgramDirty = true;
			}
		}

		public void SetVertexInputLayout(IPlatformVertexInputLayout layout)
		{
			if (vertexInputLayout != layout) {
				vertexInputLayout = (PlatformVertexInputLayout)layout;
				vertexInputLayoutDirty = true;
			}
		}

		public void SetTexture(int slot, IPlatformTexture2D texture)
		{
			if (textures[slot] != texture) {
				textures[slot] = (PlatformTexture2D)texture;
				texturesDirtyMask |= 1 << slot;
			}
		}

		public void SetVertexBuffer(int slot, IPlatformBuffer buffer, int offset)
		{
			if (vertexBuffers[slot] != buffer || vertexOffsets[slot] != offset) {
				vertexBuffers[slot] = (PlatformBuffer)buffer;
				vertexOffsets[slot] = offset;
				vertexBuffersDirtyMask |= 1L << slot;
			}
		}

		public void SetIndexBuffer(IPlatformBuffer buffer, int offset, IndexFormat format)
		{
			if (indexBuffer != buffer) {
				indexBuffer = (PlatformBuffer)buffer;
				indexBufferDirty = true;
			}
			indexOffset = offset;
			indexFormat = format;
		}

		public void SetRenderTarget(IPlatformRenderTexture2D texture)
		{
			if (renderTarget != texture) {
				renderTarget = (PlatformRenderTexture2D)texture;
				renderTargetDirty = true;
			}
		}

		public void Clear(ClearOptions options, Color4 color, float depth, byte stencil)
		{
			if (options == ClearOptions.None || viewport.Width == 0 || viewport.Height == 0) {
				return;
			}
			EnsureRenderTarget();
			BindClearValues(false, options, color, depth, stencil);
			ClearBufferMask glClearBufferMask = 0;
			if ((options & ClearOptions.ColorBuffer) != 0) {
				glClearBufferMask |= ClearBufferMask.ColorBufferBit;
				if (boundColorWriteMask != ColorWriteMask.All) {
					GL.ColorMask(true, true, true, true);
					GLHelper.CheckGLErrors();
					boundColorWriteMask = ColorWriteMask.All;
				}
			}
			if ((options & ClearOptions.DepthBuffer) != 0) {
				glClearBufferMask |= ClearBufferMask.DepthBufferBit;
				if (!boundDepthState.WriteEnable) {
					GL.DepthMask(true);
					GLHelper.CheckGLErrors();
					boundDepthState.WriteEnable = true;
					depthStateDirty = true;
				}
			}
			if ((options & ClearOptions.StencilBuffer) != 0) {
				glClearBufferMask |= ClearBufferMask.StencilBufferBit;
				if (boundStencilState.WriteMask != 0xff) {
					GL.StencilMask(0xff);
					GLHelper.CheckGLErrors();
					boundStencilState.WriteMask = 0xff;
					stencilStateDirty = true;
				}
			}
			if (!boundScissorState.Enable) {
				GL.Enable(EnableCap.ScissorTest);
				GLHelper.CheckGLErrors();
				boundScissorState.Enable = true;
				scissorStateDirty = true;
			}
			if (boundScissorState.Bounds != viewport.Bounds) {
				GL.Scissor(viewport.X, viewport.Y, viewport.Width, viewport.Height);
				GLHelper.CheckGLErrors();
				boundScissorState.Bounds = viewport.Bounds;
				scissorStateDirty = true;
			}
			GL.Clear(glClearBufferMask);
			GLHelper.CheckGLErrors();
		}

		private void BindClearValues(bool force, ClearOptions options, Color4 color, float depth, byte stencil)
		{
			if ((options & ClearOptions.ColorBuffer) != 0) {
				if (force || color != boundClearColor) {
					GL.ClearColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
					GLHelper.CheckGLErrors();
					boundClearColor = color;
				}
			}
			if ((options & ClearOptions.DepthBuffer) != 0) {
				if (force || depth != boundClearDepth) {
					GL.ClearDepth(depth);
					GLHelper.CheckGLErrors();
					boundClearDepth = depth;
				}
			}
			if ((options & ClearOptions.StencilBuffer) != 0) {
				if (force || stencil != boundClearStencil) {
					GL.ClearStencil(stencil);
					GLHelper.CheckGLErrors();
					boundClearStencil = stencil;
				}
			}
		}

		public void Draw(int startVertex, int vertexCount)
		{
			PreDraw(0);
			GL.DrawArrays((PrimitiveType)GLHelper.GetGLPrimitiveType(primitiveTopology), startVertex, vertexCount);
			GLHelper.CheckGLErrors();
		}

		public void DrawIndexed(int startIndex, int indexCount, int baseVertex)
		{
			PreDraw(baseVertex);
			var effectiveOffset = indexOffset + startIndex * indexFormat.GetSize();
			GL.DrawElements(
				(PrimitiveType)GLHelper.GetGLPrimitiveType(primitiveTopology), indexCount,
				(DrawElementsType)GLHelper.GetGLDrawElementsType(indexFormat), new IntPtr(effectiveOffset));
			GLHelper.CheckGLErrors();
		}

		public void Flush()
		{
			GL.Flush();
			GLHelper.CheckGLErrors();
		}

		public void Finish()
		{
			GL.Finish();
			GLHelper.CheckGLErrors();
		}

		private void PreDraw(int baseVertex)
		{
			EnsureRenderTarget();
			BindState(false);
			BindShaderProgram();
			BindTextures();
			BindVertexAttributes(baseVertex);
			BindIndexBuffer();
		}

		private void EnsureRenderTarget()
		{
			if (renderTargetDirty) {
				var glFramebuffer = renderTarget != null ? renderTarget.GLFramebuffer : glDefaultFramebuffer;
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, glFramebuffer);
				GLHelper.CheckGLErrors();
				renderTargetDirty = false;
			}
		}

		private void BindState(bool force)
		{
			BindBlendState(force);
			BindDepthState(force);
			BindStencilState(force);
			BindScissorState(force);
			BindColorWriteMask(force);
			BindCullMode(force);
			BindFrontFace(force);
			BindViewport(force);
		}

		private void BindBlendState(bool force)
		{
			blendStateDirty |= force;
			if (!blendStateDirty) {
				return;
			}
			if (force || blendState.Enable != boundBlendState.Enable) {
				if (blendState.Enable) {
					GL.Enable(EnableCap.Blend);
					GLHelper.CheckGLErrors();
				} else {
					GL.Disable(EnableCap.Blend);
					GLHelper.CheckGLErrors();
				}
				boundBlendState.Enable = blendState.Enable;
			}
			if (force || blendState.Enable) {
				if (force ||
					blendState.ColorBlendFunc != boundBlendState.ColorBlendFunc ||
					blendState.AlphaBlendFunc != boundBlendState.AlphaBlendFunc
				) {
					GL.BlendEquationSeparate(
						(BlendEquationMode)GLHelper.GetGLBlendEquationMode(blendState.ColorBlendFunc),
						(BlendEquationMode)GLHelper.GetGLBlendEquationMode(blendState.AlphaBlendFunc));
					GLHelper.CheckGLErrors();
					boundBlendState.ColorBlendFunc = blendState.ColorBlendFunc;
					boundBlendState.AlphaBlendFunc = blendState.AlphaBlendFunc;
				}
				if (force ||
					blendState.ColorSrcBlend != boundBlendState.ColorSrcBlend ||
					blendState.ColorDstBlend != boundBlendState.ColorDstBlend ||
					blendState.AlphaSrcBlend != boundBlendState.AlphaSrcBlend ||
					blendState.AlphaDstBlend != boundBlendState.AlphaDstBlend
				) {
					GL.BlendFuncSeparate(
						(BlendingFactorSrc)GLHelper.GetGLBlendFactor(blendState.ColorSrcBlend), (BlendingFactorDest)GLHelper.GetGLBlendFactor(blendState.ColorDstBlend),
						(BlendingFactorSrc)GLHelper.GetGLBlendFactor(blendState.AlphaSrcBlend), (BlendingFactorDest)GLHelper.GetGLBlendFactor(blendState.AlphaDstBlend));
					GLHelper.CheckGLErrors();
					boundBlendState.ColorSrcBlend = blendState.ColorSrcBlend;
					boundBlendState.ColorDstBlend = blendState.ColorDstBlend;
					boundBlendState.AlphaSrcBlend = blendState.AlphaSrcBlend;
					boundBlendState.AlphaDstBlend = blendState.AlphaDstBlend;
				}
				if (force || blendState.BlendFactor != boundBlendState.BlendFactor) {
					GL.BlendColor(
						blendState.BlendFactor.R / 255f,
						blendState.BlendFactor.G / 255f,
						blendState.BlendFactor.B / 255f,
						blendState.BlendFactor.A / 255f);
					GLHelper.CheckGLErrors();
					boundBlendState.BlendFactor = blendState.BlendFactor;
				}
			}
			blendStateDirty = false;
		}

		private void BindColorWriteMask(bool force)
		{
			if (force || colorWriteMask != boundColorWriteMask) {
				GL.ColorMask(
					(colorWriteMask & ColorWriteMask.Red) != 0,
					(colorWriteMask & ColorWriteMask.Green) != 0,
					(colorWriteMask & ColorWriteMask.Blue) != 0,
					(colorWriteMask & ColorWriteMask.Alpha) != 0);
				GLHelper.CheckGLErrors();
				boundColorWriteMask = colorWriteMask;
			}
		}

		private void BindDepthState(bool force)
		{
			depthStateDirty |= force;
			if (!depthStateDirty) {
				return;
			}
			if (force || depthState.Enable != boundDepthState.Enable) {
				if (depthState.Enable) {
					GL.Enable(EnableCap.DepthTest);
					GLHelper.CheckGLErrors();
				} else {
					GL.Disable(EnableCap.DepthTest);
					GLHelper.CheckGLErrors();
				}
				boundDepthState.Enable = depthState.Enable;
			}
			if (force || depthState.Enable) {
				if (force || depthState.WriteEnable != boundDepthState.WriteEnable) {
					GL.DepthMask(depthState.WriteEnable);
					GLHelper.CheckGLErrors();
					boundDepthState.WriteEnable = depthState.WriteEnable;
				}
				if (force || depthState.Comparison != boundDepthState.Comparison) {
					GL.DepthFunc((DepthFunction)GLHelper.GetGLCompareFunc(depthState.Comparison));
					GLHelper.CheckGLErrors();
					boundDepthState.Comparison = depthState.Comparison;
				}
			}
			depthStateDirty = false;
		}

		private void BindStencilState(bool force)
		{
			stencilStateDirty |= force;
			if (!stencilStateDirty) {
				return;
			}
			if (force || stencilState.Enable != boundStencilState.Enable) {
				if (stencilState.Enable) {
					GL.Enable(EnableCap.StencilTest);
					GLHelper.CheckGLErrors();
				} else {
					GL.Disable(EnableCap.StencilTest);
					GLHelper.CheckGLErrors();
				}
				boundStencilState.Enable = stencilState.Enable;
			}
			if (force || stencilState.Enable) {
				if (force || stencilState.WriteMask != boundStencilState.WriteMask) {
					GL.StencilMask(stencilState.WriteMask);
					GLHelper.CheckGLErrors();
					boundStencilState.WriteMask = stencilState.WriteMask;
				}
				var funcDirty = force ||
					stencilState.ReferenceValue != boundStencilState.ReferenceValue ||
					stencilState.ReadMask != boundStencilState.ReadMask;
				if (funcDirty || stencilState.FrontFaceComparison != boundStencilState.FrontFaceComparison) {
					GL.StencilFuncSeparate(StencilFace.Front,
						(StencilFunction)GLHelper.GetGLCompareFunc(stencilState.FrontFaceComparison), stencilState.ReferenceValue, stencilState.ReadMask);
					GLHelper.CheckGLErrors();
					boundStencilState.FrontFaceComparison = stencilState.FrontFaceComparison;
				}
				if (funcDirty || stencilState.BackFaceComparison != boundStencilState.BackFaceComparison) {
					GL.StencilFuncSeparate(StencilFace.Back,
						(StencilFunction)GLHelper.GetGLCompareFunc(stencilState.BackFaceComparison), stencilState.ReferenceValue, stencilState.ReadMask);
					GLHelper.CheckGLErrors();
					boundStencilState.BackFaceComparison = stencilState.BackFaceComparison;
				}
				boundStencilState.ReferenceValue = stencilState.ReferenceValue;
				boundStencilState.ReadMask = stencilState.ReadMask;
				if (force ||
					stencilState.FrontFaceFail != boundStencilState.FrontFaceFail ||
					stencilState.FrontFaceDepthFail != boundStencilState.FrontFaceDepthFail ||
					stencilState.FrontFacePass != boundStencilState.FrontFacePass
				) {
					GL.StencilOpSeparate(StencilFace.Front,
						(GLStencilOp)GLHelper.GetGLStencilOp(stencilState.FrontFaceFail),
						(GLStencilOp)GLHelper.GetGLStencilOp(stencilState.FrontFaceDepthFail),
						(GLStencilOp)GLHelper.GetGLStencilOp(stencilState.FrontFacePass));
					GLHelper.CheckGLErrors();
					boundStencilState.FrontFaceFail = stencilState.FrontFaceFail;
					boundStencilState.FrontFaceDepthFail = stencilState.FrontFaceDepthFail;
					boundStencilState.FrontFacePass = stencilState.FrontFacePass;
				}
				if (force ||
					stencilState.BackFaceFail != boundStencilState.BackFaceFail ||
					stencilState.BackFaceDepthFail != boundStencilState.BackFaceDepthFail ||
					stencilState.BackFacePass != boundStencilState.BackFacePass
				) {
					GL.StencilOpSeparate(StencilFace.Back,
						(GLStencilOp)GLHelper.GetGLStencilOp(stencilState.BackFaceFail),
						(GLStencilOp)GLHelper.GetGLStencilOp(stencilState.BackFaceDepthFail),
						(GLStencilOp)GLHelper.GetGLStencilOp(stencilState.BackFacePass));
					GLHelper.CheckGLErrors();
					boundStencilState.BackFaceFail = stencilState.BackFaceFail;
					boundStencilState.BackFaceDepthFail = stencilState.BackFaceDepthFail;
					boundStencilState.BackFacePass = stencilState.BackFacePass;
				}
			}
			stencilStateDirty = false;
		}

		private void BindScissorState(bool force)
		{
			scissorStateDirty |= force;
			if (!scissorStateDirty) {
				return;
			}
			if (force || scissorState.Enable != boundScissorState.Enable) {
				if (scissorState.Enable) {
					GL.Enable(EnableCap.ScissorTest);
					GLHelper.CheckGLErrors();
				} else {
					GL.Disable(EnableCap.ScissorTest);
					GLHelper.CheckGLErrors();
				}
				boundScissorState.Enable = scissorState.Enable;
			}
			if (force || (scissorState.Enable && scissorState.Bounds != boundScissorState.Bounds)) {
				var bounds = scissorState.Bounds;
				GL.Scissor(bounds.X, bounds.Y, bounds.Width, bounds.Height);
				GLHelper.CheckGLErrors();
				boundScissorState.Bounds = bounds;
			}
			scissorStateDirty = false;
		}

		private void BindCullMode(bool force)
		{
			if (force || cullMode != boundCullMode) {
				var enabled = boundCullMode != CullMode.None;
				var required = cullMode != CullMode.None;
				if (force || required != enabled) {
					if (required) {
						GL.Enable(EnableCap.CullFace);
						GLHelper.CheckGLErrors();
					} else {
						GL.Disable(EnableCap.CullFace);
						GLHelper.CheckGLErrors();
					}
				}
				if (required) {
					GL.CullFace((CullFaceMode)GLHelper.GetGLCullFaceMode(cullMode));
					GLHelper.CheckGLErrors();
				}
				boundCullMode = cullMode;
			}
		}

		private void BindFrontFace(bool force)
		{
			if (force || frontFace != boundFrontFace) {
				GL.FrontFace((FrontFaceDirection)GLHelper.GetGLFrontFaceDirection(frontFace));
				GLHelper.CheckGLErrors();
				boundFrontFace = frontFace;
			}
		}

		private void BindViewport(bool force)
		{
			viewportDirty |= force;
			if (!viewportDirty) {
				return;
			}
			if (force ||
				viewport.X != boundViewport.X ||
				viewport.Y != boundViewport.Y ||
				viewport.Width != boundViewport.Width ||
				viewport.Height != boundViewport.Height
			) {
				GL.Viewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);
				GLHelper.CheckGLErrors();
				boundViewport.X = viewport.X;
				boundViewport.Y = viewport.Y;
				boundViewport.Width = viewport.Width;
				boundViewport.Height = viewport.Height;
			}
			if (force ||
				viewport.MinDepth != boundViewport.MinDepth ||
				viewport.MaxDepth != boundViewport.MaxDepth
			) {
				GL.DepthRange(viewport.MinDepth, viewport.MaxDepth);
				GLHelper.CheckGLErrors();
				boundViewport.MinDepth = viewport.MinDepth;
				boundViewport.MaxDepth = viewport.MaxDepth;
			}
			viewportDirty = false;
		}

		private void BindTextures()
		{
			foreach (var slot in shaderProgram.TextureSlots) {
				var slotMask = 1 << slot;
				if ((texturesDirtyMask & slotMask) != 0) {
					var texture = textures[slot];
					var glTexture = texture != null && !texture.Disposed ? texture.GLTexture : 0;
					GL.ActiveTexture(TextureUnit.Texture0 + slot);
					GLHelper.CheckGLErrors();
					GL.BindTexture(TextureTarget.Texture2D, glTexture);
					GLHelper.CheckGLErrors();
					texturesDirtyMask &= ~slotMask;
				}
			}
		}

		private void BindShaderProgram()
		{
			if (shaderProgramDirty) {
				GL.UseProgram(shaderProgram.GLProgram);
				GLHelper.CheckGLErrors();
				shaderProgramDirty = false;
			}
			shaderProgram.SyncUniforms();
		}

		private void BindVertexAttributes(int baseVertex)
		{
			var dirtyBindingMask = vertexBuffersDirtyMask & vertexInputLayout.BindingMask;
			if (vertexInputLayoutDirty || baseVertex != boundBaseVertex) {
				dirtyBindingMask |= vertexInputLayout.BindingMask;
			}
			if (dirtyBindingMask != 0) {
				foreach (var binding in vertexInputLayout.GLBindings) {
					var bindingMask = 1 << binding.Slot;
					if ((dirtyBindingMask & bindingMask) != 0) {
						var buffer = vertexBuffers[binding.Slot];
						var offset = vertexOffsets[binding.Slot] + baseVertex * binding.Stride;
						GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.GLBuffer);
						GLHelper.CheckGLErrors();
						foreach (var attrib in binding.Attributes) {
							var effectiveOffset = offset + attrib.Offset;
							GL.VertexAttribPointer(
								attrib.Index, attrib.Size, (VertexAttribPointerType)attrib.Type,
								attrib.Normalized, binding.Stride, effectiveOffset);
							GLHelper.CheckGLErrors();
						}
						dirtyBindingMask &= ~bindingMask;
						if (dirtyBindingMask == 0) {
							break;
						}
					}
				}
			}
			var attribMask = vertexInputLayout.AttributeMask;
			if (attribMask != enabledVertexAttribMask) {
				for (var i = 0; i < MaxVertexAttributes; i++) {
					var mask = 1L << i;
					if ((attribMask & mask) != (enabledVertexAttribMask & mask)) {
						if ((attribMask & mask) != 0) {
							GL.EnableVertexAttribArray(i);
							GLHelper.CheckGLErrors();
						} else {
							GL.DisableVertexAttribArray(i);
							GLHelper.CheckGLErrors();
						}
						enabledVertexAttribMask ^= mask;
						if (enabledVertexAttribMask == attribMask) {
							break;
						}
					}
				}
			}
			vertexBuffersDirtyMask &= ~vertexInputLayout.BindingMask;
			vertexInputLayoutDirty = false;
			boundBaseVertex = baseVertex;	
		}

		private void BindIndexBuffer()
		{
			if (indexBufferDirty) {
				var glBuffer = indexBuffer != null && !indexBuffer.Disposed ? indexBuffer.GLBuffer : 0;
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer.GLBuffer);
				GLHelper.CheckGLErrors();
				indexBufferDirty = false;
			}
		}

		public FormatFeatures GetFormatFeatures(Format format)
		{
			var features = FormatFeatures.None;
			switch (format) {
				case Format.R8_SScaled:
				case Format.R8_SNorm:
				case Format.R8_UScaled:
				case Format.R8G8_SScaled:
				case Format.R8G8_SNorm:
				case Format.R8G8_UScaled:
				case Format.R8G8B8_SScaled:
				case Format.R8G8B8_SNorm:
				case Format.R8G8B8_UScaled:
				case Format.R8G8B8A8_SScaled:
				case Format.R8G8B8A8_SNorm:
				case Format.R8G8B8A8_UScaled:
				case Format.R16_SScaled:
				case Format.R16_SNorm:
				case Format.R16_UScaled:
				case Format.R16_UNorm:
				case Format.R16G16_SScaled:
				case Format.R16G16_SNorm:
				case Format.R16G16_UScaled:
				case Format.R16G16_UNorm:
				case Format.R16G16B16_SScaled:
				case Format.R16G16B16_SNorm:
				case Format.R16G16B16_UScaled:
				case Format.R16G16B16A16_SScaled:
				case Format.R16G16B16A16_SNorm:
				case Format.R16G16B16A16_UScaled:
				case Format.R32_SFloat:
				case Format.R32G32_SFloat:
				case Format.R32G32B32_SFloat:
				case Format.R32G32B32A32_SFloat:
					features |= FormatFeatures.VertexBuffer;
					break;
				case Format.R8_UNorm:
				case Format.R8G8_UNorm:
					if (SupportsTextureRG) {
						features |= FormatFeatures.Sample;
						features |= FormatFeatures.RenderTarget;
					}
					features |= FormatFeatures.VertexBuffer;
					break;
				case Format.R8G8B8_UNorm:
				case Format.R8G8B8A8_UNorm:
					features |= FormatFeatures.Sample;
					features |= FormatFeatures.RenderTarget;
					features |= FormatFeatures.VertexBuffer;
					break;
				case Format.B8G8R8A8_UNorm:
					if (SupportsInternalFormatBgra8 || SupportsExternalFormatBgra8) {
						features |= FormatFeatures.Sample;
						features |= FormatFeatures.RenderTarget;
					}
					break;
				case Format.R5G6B5_UNorm_Pack16:
				case Format.R5G5B5A1_UNorm_Pack16:
				case Format.R4G4B4A4_UNorm_Pack16:
					features |= FormatFeatures.Sample;
					features |= FormatFeatures.RenderTarget;
					break;
				case Format.BC1_RGB_UNorm_Block:
				case Format.BC1_RGBA_UNorm_Block:
					if (SupportsDxt1) {
						features |= FormatFeatures.Sample;
					}
					break;
				case Format.BC2_UNorm_Block:
					if (SupportsDxt3) {
						features |= FormatFeatures.Sample;
					}
					break;
				case Format.BC3_UNorm_Block:
					if (SupportsDxt5) {
						features |= FormatFeatures.Sample;
					}
					break;
				case Format.ETC1_R8G8B8_UNorm_Block:
					if (SupportsEtc1 || SupportsEtc2) {
						features |= FormatFeatures.Sample;
					}
					break;
				case Format.ETC2_R8G8B8_UNorm_Block:
				case Format.ETC2_R8G8B8A1_UNorm_Block:
				case Format.ETC2_R8G8B8A8_UNorm_Block:
					if (SupportsEtc2) {
						features |= FormatFeatures.Sample;
					}
					break;
				case Format.PVRTC1_2Bpp_UNorm_Block:
				case Format.PVRTC1_4Bpp_UNorm_Block:
					if (SupportsPvrtc1) {
						features |= FormatFeatures.Sample;
					}
					break;
				case Format.PVRTC2_2Bpp_UNorm_Block:
				case Format.PVRTC2_4Bpp_UNorm_Block:
					if (SupportsPvrtc2) {
						features |= FormatFeatures.Sample;
					}
					break;
			}
			return features;
		}
	}
}
