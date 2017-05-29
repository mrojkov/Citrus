using Lime;

namespace Tangerine.UI.FilesystemView
{
	public interface ISystemIconTextureProvider
	{
		ITexture GetTexture(string path);
	}
}