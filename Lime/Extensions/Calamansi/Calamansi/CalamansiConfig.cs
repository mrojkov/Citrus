using System.Collections.Generic;
using Yuzu;

namespace Calamansi
{
	public class CalamansiConfig
	{
		public class CalamansiOptions
		{
			[YuzuMember]
			public string Font { get; set; }
			[YuzuMember]
			public string Charset { get; set; }
		}

		[YuzuMember]
		public readonly List<CalamansiOptions> Options = new List<CalamansiOptions>();
		[YuzuMember]
		public int Height { get; set; }
		[YuzuMember]
		public int Padding { get; set; }
		[YuzuMember]
		public bool SDF { get; set; }
		[YuzuMember]
		public float SDFScale { get; set; } = 0.25f;
	}
}
