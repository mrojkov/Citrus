using Lime;

namespace Tangerine.UI.FilesystemView
{
	public static class FilesystemViewExtensions
	{
		public static Splitter MakeSplitter(this SplitterType type)
		{
			return type == SplitterType.Horizontal ? (Splitter)new ThemedHSplitter() : new ThemedVSplitter();
		}
	}
}