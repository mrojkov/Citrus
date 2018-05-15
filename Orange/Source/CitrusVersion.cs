using System;
using Yuzu;

namespace Orange
{
	class CitrusVersion
	{
		public const string Filename = "citrus_version.json";

		[YuzuMember]
		public string Version { get; set; }

		[YuzuMember]
		public string BuildNumber { get; set; }

		[YuzuMember]
		public bool IsStandalone { get; set; }
	}
}
