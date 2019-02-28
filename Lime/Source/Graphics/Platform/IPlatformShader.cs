using System;

namespace Lime.Graphics.Platform
{
	public interface IPlatformShader : IDisposable
	{
		ShaderStageMask Stage { get; }
	}

	[Flags]
	public enum ShaderStageMask
	{
		None = 0,
		Vertex = 1 << 0,
		Fragment = 1 << 1
	}
}
