#if OPENGL
using System;

#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif
using ProtoBuf;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Lime
{
	unsafe static class PlatformRenderer
	{
		public static uint CurrentFramebuffer { get; private set; }
		public static uint DefaultFramebuffer { get; private set; }
		private static Blending blending;
		private static ShaderProgram shaderProgram;
		private static bool premultipliedAlphaMode;
		private static CullMode cullMode;

		// First texture pair is used for creation mask effect, second pair - for representing ETC1 alpha channel
		private static readonly uint[] textures = new uint[4];

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

		public static void SetProjectionMatrix(Matrix44 matrix)
		{
			shaderProgram = null;
		}

		public static void SetShader(ShaderId value, ShaderProgram customShaderProgram)
		{
			int numTextures = textures[1] != 0 ? 2 : (textures[0] != 0 ? 1 : 0);
			var flags = ShaderFlags.None;
#if ANDROID
			if (textures[2] != 0) {
				flags |= ShaderFlags.UseAlphaTexture1;
			}
			if (textures[3] != 0) {
				flags |= ShaderFlags.UseAlphaTexture2;
			}
#endif
			if (!premultipliedAlphaMode && (blending == Blending.Burn || blending == Blending.Darken)) {
				flags |= ShaderFlags.PremultiplyAlpha;
			}
			var program = value == ShaderId.Custom ? customShaderProgram : ShaderPrograms.Instance.GetShaderProgram(value, numTextures, flags);
			if (shaderProgram != program) {
				shaderProgram = program;
				shaderProgram.Use();
				var projection = Renderer.Projection;
				// OpenGL has a nice peculiarity: for render targets we must flip Y axis.
				if (IsOffscreen()) {
					FlipProjectionYAxis(ref projection);
				}
				shaderProgram.LoadMatrix(program.ProjectionMatrixUniformId, projection);
			}
		}

		private static void FlipProjectionYAxis(ref Matrix44 matrix)
		{
			matrix.M22 = -matrix.M22;
			matrix.M32 = -matrix.M32;
			matrix.M42 = -matrix.M42;
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
			SetShader(ShaderId.Diffuse, null);
			ClearRenderTarget(0, 0, 0, 0);
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
		
		public static void ClearRenderTarget(float r, float g, float b, float a)
		{
			GL.ClearColor(r, g, b, a);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}

		public static void SetTexture(ITexture texture, int stage)
		{
			var handle = texture != null ? texture.GetHandle() : 0;
			SetTexture(handle, stage);
#if ANDROID
			// Only Android supports ETC1 without embedded alpha channel
			if (texture != null) {
				var alphaTexture = texture.AlphaTexture;
				if (alphaTexture != null) {
					SetTexture(alphaTexture.GetHandle(), stage + 2);
					return;
				}
			}
			SetTexture(0, stage + 2);
#endif
		}

		internal static void SetTexture(uint glTexNum, int stage)
		{
			if (glTexNum == textures[stage])
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

		public static void SetBlending(Blending value)
		{
			if (value == blending && premultipliedAlphaMode == Renderer.PremultipliedAlphaMode) {
				return;
			}
			premultipliedAlphaMode = Renderer.PremultipliedAlphaMode;
			blending = value;
			switch (blending) {
				case Blending.Inherited:
				case Blending.Alpha:
					GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
					break;
				case Blending.Add:
				case Blending.Glow:
					GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
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
			PlatformRenderer.CheckErrors();
		}

		private static bool IsOffscreen()
		{
			return CurrentFramebuffer != DefaultFramebuffer;
		}
	}
}
#endif