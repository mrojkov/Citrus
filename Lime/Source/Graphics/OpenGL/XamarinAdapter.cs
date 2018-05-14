#if OPENGL && !MAC && !MONOMAC
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.ES20;

#if WIN
namespace Lime
{
	static class FramebufferAttachment
	{
		public const FramebufferSlot ColorAttachment0 = FramebufferSlot.ColorAttachment0;
	}
}
#elif ANDROID
namespace Lime
{
	static class BufferUsageHint
	{
		public const BufferUsage DynamicDraw = BufferUsage.DynamicDraw;
		public const BufferUsage StaticDraw = BufferUsage.StaticDraw;
	}

	static class FramebufferAttachment
	{
		public const FramebufferSlot ColorAttachment0 = FramebufferSlot.ColorAttachment0;
	}

	static class GenerateMipmapTarget
	{
		public const TextureTarget Texture2D = TextureTarget.Texture2D;
	}
}
#elif iOS
namespace Lime
{
	static class FramebufferAttachment
	{
		public const FramebufferSlot ColorAttachment0 = FramebufferSlot.ColorAttachment0;
	}

	static class BufferUsageHint
	{
		public const BufferUsage DynamicDraw = BufferUsage.DynamicDraw;
		public const BufferUsage StaticDraw = BufferUsage.StaticDraw;
	}

/*
	static class VertexAttribPointerType
	{
		public const All Float = All.Float;
		public const All UnsignedByte = All.UnsignedByte;
	}

	static class DrawElementsType
	{
		public const All UnsignedShort = All.UnsignedShort;
	}

	static class GetPName	static class BufferUsageHint
	{
		public const All DynamicDraw = All.DynamicDraw;
	}

	{
		public const All FramebufferBinding = All.FramebufferBinding;
	}

	static class EnableCap
	{
		public const All Blend = All.Blend;
		public const All ScissorTest = All.ScissorTest;
	}

	static class ClearBufferMask
	{
		public const int ColorBufferBit = (int)All.ColorBufferBit;
	}

	static class TextureUnit
	{
		public const All Texture0 = All.Texture0;
	}

	static class TextureTarget
	{
		public const All Texture2D = All.Texture2D;
	}

	static class GenerateMipmapTarget
	{
		public const All Texture2D = All.Texture2D;
	}

	static class BlendingFactorSrc
	{
		public const All One = All.One;
		public const All SrcAlpha = All.SrcAlpha;
		public const All DstColor = All.DstColor;
	}

	static class BlendingFactorDest
	{
		public const All OneMinusSrcAlpha = All.OneMinusSrcAlpha;
		public const All One = All.One;
		public const All Zero = All.Zero;
	}

	static class TextureParameterName
	{
		public const All TextureMinFilter = All.TextureMinFilter;
		public const All TextureMagFilter = All.TextureMagFilter;
		public const All TextureWrapS = All.TextureWrapS;
		public const All TextureWrapT = All.TextureWrapT;
	}

	static class PixelInternalFormat
	{
		public const int Rgb = (int)All.Rgb;
		public const int Rgba = (int)All.Rgba;
	}

	static class PixelFormat
	{
		public const All Rgb = All.Rgb;
		public const All Rgba = All.Rgba;
	}

	static class PixelType
	{
		public const All UnsignedByte = All.UnsignedByte;
		public const All UnsignedShort565 = All.UnsignedShort565;
	}

	static class FramebufferTarget
	{
		public const All Framebuffer = All.Framebuffer;
	}

	static class FramebufferErrorCode
	{
		public const All FramebufferComplete = All.FramebufferComplete;
	}

	static class ShaderType
	{
		public const All FragmentShader = All.FragmentShader;
		public const All VertexShader = All.VertexShader;
	}

	static class ShaderParameter
	{
		public const All CompileStatus = All.CompileStatus;
		public const All InfoLogLength = All.InfoLogLength;
	}

	static class BufferTarget
	{
		public const All ArrayBuffer = All.ArrayBuffer;
		public const All ElementArrayBuffer = All.ElementArrayBuffer;
	}

	static class ErrorCode
	{
		public const All NoError = All.NoError;
	}

	static class BufferUsageHint
	{
		public const All DynamicDraw = All.DynamicDraw;
	}
*/
}
#endif
#endif