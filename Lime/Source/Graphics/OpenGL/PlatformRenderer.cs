#if OPENGL
using System;

#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES20;
using GLStencilOp = OpenTK.Graphics.ES20.StencilOp;
#else
using OpenTK.Graphics.OpenGL;
using GLStencilOp = OpenTK.Graphics.OpenGL.StencilOp;
#endif
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Lime
{
	public enum StencilFunc : byte
	{
		Never,
		Less,
		Equal,
		LEqual,
		Greater,
		NotEqual,
		GEqual,
		Always
	}
	
	public enum StencilOp : byte
	{
		Zero,
		Invert,
		Keep,
		Replace,
		Incr,
		Decr,
		IncrWrap,
		DecrWrap
	}
	
	[StructLayout(LayoutKind.Explicit)]
	public struct StencilParams : IEquatable<StencilParams>
	{
		[FieldOffset(0)]
		public bool EnableTest;
		
		[FieldOffset(1)]
		public byte ReferenceValue;
		
		[FieldOffset(2)]
		public byte ReadMask;
		
		[FieldOffset(3)]
		public byte WriteMask;
		
		[FieldOffset(4)]
		public StencilFunc Comp;
		
		[FieldOffset(5)]
		public StencilOp Pass;
		
		[FieldOffset(6)]
		public StencilOp Fail;
		
		[FieldOffset(7)]
		public StencilOp ZFail;
		
		[FieldOffset(0)]
		private ulong raw;
		
		public static StencilParams Default = new StencilParams {
			raw = 0,
			EnableTest = false,
			ReferenceValue = 0,
			ReadMask = 255,
			WriteMask = 255,
			Comp = StencilFunc.Always,
			Pass = StencilOp.Keep,
			Fail = StencilOp.Keep,
			ZFail = StencilOp.Keep
		};
		
		public bool Equals(StencilParams other)
		{
			return raw == other.raw;
		}
	}	

	public unsafe static class PlatformRenderer
	{
		public static uint CurrentFramebuffer { get; private set; }
		public static uint DefaultFramebuffer { get; private set; }
		private static Blending blending;
		private static ShaderProgram shaderProgram;
		private static bool premultipliedAlphaMode;
		private static CullMode cullMode;

		// First texture pair is used for creation mask effect, second pair - for representing ETC1 alpha channel
		private static readonly uint[] textures = new uint[4];

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
			while (errCode != ErrorCode.NoError) {
				if (errors != "")
					errors += ", ";
				errors += errCode.ToString();
#if ANDROID || iOS
				errCode = GL.GetErrorCode();
#else
				errCode = GL.GetError();
#endif
			}
			throw new Exception("OpenGL error(s): " + errors);
		}

		public static void InvalidateShaderProgram()
		{
			shaderProgram = null;
		}

		public static void SetShaderProgram(ShaderProgram program)
		{
			if (shaderProgram != program) {
				shaderProgram = program;
				shaderProgram.Use();
				shaderProgram.LoadMatrix(program.ProjectionMatrixUniformId, FixupWVP(Renderer.WorldViewProjection));
			}
		}

		static PlatformRenderer()
		{
			DefaultFramebuffer = uint.MaxValue;
		}

		public static void BeginFrame()
		{
			SaveDefaultFramebuffer();
			CurrentFramebuffer = DefaultFramebuffer;
			GL.Enable(EnableCap.Blend);
			blending = Blending.None;
			premultipliedAlphaMode = false;
			shaderProgram = null;
			SetBlending(Blending.Inherited);
			SetShaderProgram(ShaderPrograms.Instance.GetShaderProgram(ShaderId.Diffuse, 0, ShaderOptions.None));
			Clear(ClearTarget.All, 0, 0, 0, 0);
			CheckErrors();
		}

		private static void SaveDefaultFramebuffer()
		{
			if (DefaultFramebuffer == uint.MaxValue) {
				var p = new int[1];
				GL.GetInteger(GetPName.FramebufferBinding, p);
				DefaultFramebuffer = (uint)p[0];
			}
		}

		public static void Clear(ClearTarget targets, float r, float g, float b, float a)
		{
			if (targets == ClearTarget.None) {
				return;
			}
			ClearBufferMask clearBufferMask = 0;
			if ((targets & ClearTarget.ColorBuffer) != 0) {
				GL.ClearColor(r, g, b, a);
				clearBufferMask |= ClearBufferMask.ColorBufferBit;
			}
			if ((targets & ClearTarget.DepthBuffer) != 0) {
				clearBufferMask |= ClearBufferMask.DepthBufferBit;
			}
			if ((targets & ClearTarget.StencilBuffer) != 0) {
				clearBufferMask |= ClearBufferMask.StencilBufferBit;
			}
			GL.Clear(clearBufferMask);
		}
		
		private static GLStencilOp[] stencilOpMap = {
			GLStencilOp.Zero,
			GLStencilOp.Invert,
			GLStencilOp.Keep,
			GLStencilOp.Replace,
			GLStencilOp.Incr,
			GLStencilOp.Decr,
			GLStencilOp.IncrWrap,
			GLStencilOp.DecrWrap
		};
		
		private static StencilFunction[] stencilFuncMap = {
			StencilFunction.Never,
			StencilFunction.Less,
			StencilFunction.Equal,
			StencilFunction.Lequal,
			StencilFunction.Greater,
			StencilFunction.Notequal,
			StencilFunction.Gequal,
			StencilFunction.Always
		};
		
		public unsafe static void SetStencilParams(StencilParams p)
		{
			if (p.EnableTest) {
				GL.Enable(EnableCap.StencilTest);
			} else {
				GL.Disable(EnableCap.StencilTest);
			}
			GL.StencilOp(stencilOpMap[(int)p.Fail], stencilOpMap[(int)p.ZFail], stencilOpMap[(int)p.Pass]);
			GL.StencilFunc(stencilFuncMap[(int)p.Comp], p.ReferenceValue, p.ReadMask);
			GL.StencilMask(p.WriteMask);
		}

		public static void SetTexture(ITexture texture, int stage)
		{
			var handle = texture != null ? texture.GetHandle() : 0;
			SetTexture(handle, stage);
		}

		internal static void SetTexture(uint glTexNum, int stage, bool force = false)
		{
			if (glTexNum == textures[stage] && !force)
				return;
			if (stage > 0) {
				GL.ActiveTexture(TextureUnit.Texture0 + stage);
				GL.BindTexture(TextureTarget.Texture2D, glTexNum);
				GL.ActiveTexture(TextureUnit.Texture0);
			} else {
				GL.BindTexture(TextureTarget.Texture2D, glTexNum);
			}
			// This is a temporary workaround for Amazon Game Circle.
			// Once Amazon UI plate with playername gets hidden, the game looses textures.
			// Thank you, Amazon!
#if ANDROID
			if (Renderer.AmazonBindTextureWorkaround) {
				if (GL.GetErrorCode() != ErrorCode.NoError) {
					GLObjectRegistry.Instance.DiscardObjects();
				}
			}
#endif
			textures[stage] = glTexNum;
		}

		private static Stack<uint> textureStack = new Stack<uint>();

		public static void PushTexture(uint handle, int stage)
		{
			textureStack.Push(textures[stage]);
			SetTexture(handle, stage);
		}

		public static void PopTexture(int stage)
		{
			SetTexture(textureStack.Pop(), stage);
		}

		public static void InvalidateTexture(uint handle)
		{
			for (int i = 0; i < textures.Length; i++) {
				if (textures[i] == handle) {
					textures[i] = 0;
				}
			}
		}

		public static void RebindTextures()
		{
			for (int i = 0; i < textures.Length; i++) {
				SetTexture(textures[i], i, force: true);
			}
		}

		public static void SetViewport(WindowRect value)
		{
			GL.Viewport(value.X, value.Y, value.Width, value.Height);
		}

		public static void SetScissorRectangle(WindowRect value)
		{
			GL.Scissor(value.X, value.Y, value.Width, value.Height);
		}

		public static void EnableScissorTest(bool value)
		{
			if (value) {
				GL.Enable(EnableCap.ScissorTest);
			} else {
				GL.Disable(EnableCap.ScissorTest);
			}
		}

		public static void EnableZTest(bool value)
		{
			if (value) {
				GL.Enable(EnableCap.DepthTest);
				CheckErrors();
			} else {
				GL.Disable(EnableCap.DepthTest);
				CheckErrors();
			}
		}

		public static void EnableZWrite(bool value)
		{
			GL.DepthMask(value);
			CheckErrors();
		}
		
		public static void EnableColorWrite(ColorMask mask)
		{
			GL.ColorMask(
				(mask & ColorMask.Red) != 0, (mask & ColorMask.Green) != 0,
				(mask & ColorMask.Blue) != 0, (mask & ColorMask.Alpha) != 0);
			CheckErrors();
		}

		public static void SetBlending(Blending value, bool premultipliedAlpha = false)
		{
			if (value == blending && premultipliedAlphaMode == premultipliedAlpha) {
				return;
			}
			premultipliedAlphaMode = premultipliedAlpha;
			blending = value;
			switch (blending) {
				case Blending.Inherited:
				case Blending.Alpha:
					if (premultipliedAlphaMode) {
						GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
					} else {
						GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
					}
					break;
				case Blending.LcdTextFirstPass:
					GL.BlendFunc(BlendingFactorSrc.Zero, BlendingFactorDest.OneMinusSrcColor);
					break;
				case Blending.LcdTextSecondPass:
					GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
					break;
				case Blending.Add:
				case Blending.Glow:
					if (premultipliedAlphaMode) {
						GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
					} else {
						GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
					}
					break;
				case Blending.Burn:
				case Blending.Darken:
					GL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.OneMinusSrcAlpha);
					break;
				case Blending.Modulate:
					GL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.Zero);
					break;
				case Blending.Opaque:
					GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.Zero);
					break;
			}
			CheckErrors();
		}

		public static void BindFramebuffer(uint framebuffer)
		{
			CurrentFramebuffer = framebuffer;
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
			SetCullMode(cullMode);
			shaderProgram = null;
		}

		public static void SetCullMode(CullMode value)
		{
			cullMode = value;
			if (cullMode != CullMode.None) {
				GL.Enable(EnableCap.CullFace);
			} else {
				GL.Disable(EnableCap.CullFace);
				return;
			}
			GL.CullFace(CullFaceMode.Back);
			if (cullMode == CullMode.CullClockwise) {
				GL.FrontFace(IsOffscreen() ? FrontFaceDirection.Cw : FrontFaceDirection.Ccw);
			} else {
				GL.FrontFace(IsOffscreen() ? FrontFaceDirection.Ccw : FrontFaceDirection.Cw);
			}
			CheckErrors();
		}

		private static bool IsOffscreen()
		{
			return CurrentFramebuffer != DefaultFramebuffer;
		}

		public static Matrix44 FixupWVP(Matrix44 projection)
		{
			// OpenGL has a nice peculiarity: for render targets we must flip Y axis.
			if (IsOffscreen()) {
				projection *= Matrix44.CreateScale(new Vector3(1, -1, 1));
			}
			return projection;
		}

		public static void DrawTriangles(IMesh mesh, int startIndex, int count)
		{
			(mesh as Mesh).Bind();
			int offset = startIndex * sizeof(short);
#if MAC || MONOMAC || ANDROID
			GL.DrawElements(BeginMode.Triangles, count, DrawElementsType.UnsignedShort, (IntPtr)offset);
#else
			GL.DrawElements(PrimitiveType.Triangles, count, DrawElementsType.UnsignedShort, (IntPtr)offset);
#endif
			CheckErrors();
			Renderer.DrawCalls++;
		}
	}
}
#endif