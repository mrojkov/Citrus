using System;
using System.Collections.Generic;
using Yuzu;

namespace Calamansi
{
	public class CalamansiConfig
	{
		public class CalamansiOption
		{
			[YuzuOptional]
			public string Font { get; set; }
			[YuzuMember]
			public string Charset { get; set; }
		}

		[YuzuMember]
		public string Localization;
		[YuzuMember]
		public CalamansiOption Main;
		[YuzuMember]
		public List<CalamansiOption> AlwaysInclude = new List<CalamansiOption>();
		[YuzuMember]
		public List<CalamansiOption> AlwaysExclude= new List<CalamansiOption>();
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
