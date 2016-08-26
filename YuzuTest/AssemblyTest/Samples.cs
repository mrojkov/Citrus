using Yuzu;

namespace YuzuTestAssembly
{
	public class SampleAssemblyBase
	{
		[YuzuMember]
		public short P = 5;
	}

	public class SampleAssemblyDerivedQ : SampleAssemblyBase
	{
		[YuzuMember]
		public short Q = 6;
	}
}
