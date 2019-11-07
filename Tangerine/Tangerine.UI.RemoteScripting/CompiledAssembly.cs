using RemoteScripting;

namespace Tangerine.UI.RemoteScripting
{
	public class CompiledAssembly
	{
		public static CompiledAssembly Instance { get; set; }

		public byte[] RawBytes;
		public byte[] PdbRawBytes;
		public PortableAssembly PortableAssembly;
	}
}
