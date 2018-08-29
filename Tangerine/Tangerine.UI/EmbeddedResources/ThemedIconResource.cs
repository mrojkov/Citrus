using System.IO;

namespace Tangerine.UI
{
    public class ThemedIconResource : EmbeddedResource
	{
        public ThemedIconResource(string iconId, string assemblyName) : base(iconId, assemblyName) { }

		public override Stream GetResourceStream()
		{
			var assembly = GetAssembly();
			var stream = assembly.GetManifestResourceStream(GetResourceId(themed: true));
			return stream ?? assembly.GetManifestResourceStream(GetResourceId(themed: false));
		}

		private string GetResourceId(bool themed)
		{
			var theme = themed ? $"{(ColorTheme.Current.IsDark ? "Dark" : "Light")}." : string.Empty;
            return $"{AssemblyName}.Resources.Icons.{theme}{ResourceId}.png";
		}
	}
}
