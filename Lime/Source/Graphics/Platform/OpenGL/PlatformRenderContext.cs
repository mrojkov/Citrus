using System;
using System.Collections.Generic;

#if iOS || MAC
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
		private Viewport viewport;
		private BlendState blendState;
		private DepthState depthState;
		private StencilState stencilState;
		private ScissorState scissorState;
		private ColorWriteMask colorWriteMask;
		private CullMode cullMode;
		private FrontFace frontFace;
		private PrimitiveTopology primitiveTopology;
		private PlatformShaderProgram shaderProgram;
		private PlatformVertexInputLayout vertexInputLayout;
		private PlatformTexture2D[] textures;
		private PlatformBuffer[] vertexBuffers;

		private int[] vertexOffsets;
		private PlatformBuffer indexBuffer;
		private int indexOffset;
		private IndexFormat indexFormat;
		private PlatformRenderTexture2D renderTarget;
		
		private bool renderTargetDirty;
		private bool shaderProgramDirty;
		private bool indexBufferDirty;
		private int texturesDirtyMask;
		private long enabledVertexAttribMask;
		private long vertexBuffersDirtyMask;
		private bool vertexInputLayoutDirty;
		private int boundBaseVertex;

		internal int MaxTextureSlots;
		internal int MaxVertexBufferSlots;
		internal int MaxVertexAttributes;
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
			enabledVertexAttribMask = ~0;
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
			GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out MaxTextureSlots);
			GL.GetInteger(GetPName.MaxVertexAttribs, out MaxVertexAttributes);
			MaxTextureSlots = Math.Min(MaxTextureSlots, 32);
			MaxVertexAttributes = Math.Min(MaxVertexAttributes, 64);
			MaxVertexBufferSlots = MaxVertexAttributes;
		}

		public void SetViewport(Viewport vp)
		{
			viewport = vp;
		}

		public void SetBlendState(BlendState state)
		{
			blendState = state;
		}

		public void SetDepthState(DepthState state)
		{
			depthState = state;
		}

		public void SetStencilState(StencilState state)
		{
			stencilState = state;
		}

		public void SetScissorState(ScissorState state)
		{
			scissorState = state;
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

		public void Clear(ClearOptions options, float r, float g, float b, float a, float depth, byte stencil)
		{
			if (options == ClearOptions.None) {
				return;
			}
			EnsureRenderTarget();
			ClearBufferMask glClearBufferMask = 0;
			if ((options & ClearOptions.ColorBuffer) != 0) {
				glClearBufferMask |= ClearBufferMask.ColorBufferBit;
				GL.ColorMask(true, true, true, true);
				GLHelper.CheckGLErrors();
				GL.ClearColor(r, g, b, a);
				GLHelper.CheckGLErrors();
			}
			if ((options & ClearOptions.DepthBuffer) != 0) {
				glClearBufferMask |= ClearBufferMask.DepthBufferBit;
				GL.DepthMask(true);
				GLHelper.CheckGLErrors();
				GL.ClearDepth(depth);
				GLHelper.CheckGLErrors();
			}
			if ((options & ClearOptions.StencilBuffer) != 0) {
				glClearBufferMask |= ClearBufferMask.StencilBufferBit;
				GL.StencilMask(0xff);
				GLHelper.CheckGLErrors();
				GL.ClearStencil(stencil);
				GLHelper.CheckGLErrors();
			}
			GL.Scissor(viewport.X, viewport.Y, viewport.Width, viewport.Height);
			GLHelper.CheckGLErrors();
			GL.Clear(glClearBufferMask);
			GLHelper.CheckGLErrors();
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
			BindState();
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

		private void BindState()
		{
			if (blendState.Enable) {
				GL.Enable(EnableCap.Blend);
				GLHelper.CheckGLErrors();
			} else {
				GL.Disable(EnableCap.Blend);
				GLHelper.CheckGLErrors();
			}
			GL.BlendEquationSeparate(
				(BlendEquationMode)GLHelper.GetGLBlendEquationMode(blendState.ColorBlendFunc),
				(BlendEquationMode)GLHelper.GetGLBlendEquationMode(blendState.AlphaBlendFunc));
			GLHelper.CheckGLErrors();
			GL.BlendFuncSeparate(
				(BlendingFactorSrc)GLHelper.GetGLBlendFactor(blendState.ColorSrcBlend), (BlendingFactorDest)GLHelper.GetGLBlendFactor(blendState.ColorDstBlend),
				(BlendingFactorSrc)GLHelper.GetGLBlendFactor(blendState.AlphaSrcBlend), (BlendingFactorDest)GLHelper.GetGLBlendFactor(blendState.AlphaDstBlend));
			GLHelper.CheckGLErrors();
			GL.BlendColor(
				blendState.BlendFactor.R / 255f,
				blendState.BlendFactor.G / 255f,
				blendState.BlendFactor.B / 255f,
				blendState.BlendFactor.A / 255f);
			GLHelper.CheckGLErrors();
			GL.ColorMask(
				(colorWriteMask & ColorWriteMask.Red) != 0,
				(colorWriteMask & ColorWriteMask.Green) != 0,
				(colorWriteMask & ColorWriteMask.Blue) != 0,
				(colorWriteMask & ColorWriteMask.Alpha) != 0);
			GLHelper.CheckGLErrors();
			if (depthState.Enable) {
				GL.Enable(EnableCap.DepthTest);
				GLHelper.CheckGLErrors();
			} else {
				GL.Disable(EnableCap.DepthTest);
				GLHelper.CheckGLErrors();
			}
			GL.DepthMask(depthState.WriteEnable);
			GLHelper.CheckGLErrors();
			GL.DepthFunc((DepthFunction)GLHelper.GetGLCompareFunc(depthState.Comparison));
			GLHelper.CheckGLErrors();
			if (stencilState.Enable) {
				GL.Enable(EnableCap.StencilTest);
				GLHelper.CheckGLErrors();
			} else {
				GL.Disable(EnableCap.StencilTest);
				GLHelper.CheckGLErrors();
			}
			GL.StencilMask(stencilState.WriteMask);
			GLHelper.CheckGLErrors();
			GL.StencilFuncSeparate(StencilFace.Front,
				(StencilFunction)GLHelper.GetGLCompareFunc(stencilState.FrontFaceComparison), stencilState.ReferenceValue, stencilState.ReadMask);
			GLHelper.CheckGLErrors();
			GL.StencilOpSeparate(StencilFace.Front,
				(GLStencilOp)GLHelper.GetGLStencilOp(stencilState.FrontFaceFail),
				(GLStencilOp)GLHelper.GetGLStencilOp(stencilState.FrontFaceDepthFail),
				(GLStencilOp)GLHelper.GetGLStencilOp(stencilState.FrontFacePass));
			GLHelper.CheckGLErrors();
			GL.StencilFuncSeparate(StencilFace.Back,
				(StencilFunction)GLHelper.GetGLCompareFunc(stencilState.BackFaceComparison), stencilState.ReferenceValue, stencilState.ReadMask);
			GLHelper.CheckGLErrors();
			GL.StencilOpSeparate(StencilFace.Back,
				(GLStencilOp)GLHelper.GetGLStencilOp(stencilState.BackFaceFail),
				(GLStencilOp)GLHelper.GetGLStencilOp(stencilState.BackFaceDepthFail),
				(GLStencilOp)GLHelper.GetGLStencilOp(stencilState.BackFacePass));
			GLHelper.CheckGLErrors();
			if (scissorState.Enable) {
				GL.Enable(EnableCap.ScissorTest);
				GLHelper.CheckGLErrors();
			} else {
				GL.Disable(EnableCap.ScissorTest);
				GLHelper.CheckGLErrors();
			}
			var scissor = scissorState.Bounds;
			GL.Scissor(scissor.X, scissor.Y, scissor.Width, scissor.Height);
			GLHelper.CheckGLErrors();
			if (cullMode != CullMode.None) {
				GL.Enable(EnableCap.CullFace);
				GLHelper.CheckGLErrors();
				GL.CullFace((CullFaceMode)GLHelper.GetGLCullFaceMode(cullMode));
				GLHelper.CheckGLErrors();
			} else {
				GL.Disable(EnableCap.CullFace);
				GLHelper.CheckGLErrors();
			}
			GL.FrontFace((FrontFaceDirection)GLHelper.GetGLFrontFaceDirection(frontFace));
			GLHelper.CheckGLErrors();
			GL.Viewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);
			GLHelper.CheckGLErrors();
			GL.DepthRange(viewport.MinDepth, viewport.MaxDepth);
			GLHelper.CheckGLErrors();
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
			if (vertexInputLayoutDirty || baseVertex != boundBaseVertex) {
				foreach (var binding in vertexInputLayout.GLBindings) {
					vertexBuffersDirtyMask |= 1L << binding.Slot;
				}
				boundBaseVertex = baseVertex;
				vertexInputLayoutDirty = false;
			}
			if (vertexBuffersDirtyMask == 0) {
				return;
			}
			var attribMask = 0L;
			foreach (var binding in vertexInputLayout.GLBindings) {
				var bindingMask = 1 << binding.Slot;
				if ((vertexBuffersDirtyMask & bindingMask) == 0) {
					continue;
				}
				var buffer = vertexBuffers[binding.Slot];
				if (buffer != null && !buffer.Disposed) {
					var offset = vertexOffsets[binding.Slot] + baseVertex * binding.Stride;
					GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.GLBuffer);
					GLHelper.CheckGLErrors();
					foreach (var attrib in binding.Attributes) {
						var effectiveOffset = offset + attrib.Offset;
						GL.EnableVertexAttribArray(attrib.Index);
						GLHelper.CheckGLErrors();
						GL.VertexAttribPointer(
							attrib.Index, attrib.Size, (VertexAttribPointerType)attrib.Type,
							attrib.Normalized, binding.Stride, effectiveOffset);
						GLHelper.CheckGLErrors();
						attribMask |= 1L << attrib.Index;
					}
				}
				vertexBuffersDirtyMask &= ~bindingMask;
			}
			for (var i = 0; attribMask != enabledVertexAttribMask && i < MaxVertexAttributes; i++) {
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
				}
			}
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
