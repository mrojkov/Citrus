using System;
using Yuzu;

namespace Launcher
{
	class CitrusVersion
	{
		public const string Filename = "citrus_version.json";

		[YuzuMember]
		public string Version { get; set; }

		[YuzuMember]
		public string BuildNumber { get; set; }

		[YuzuMember]
		bool IsStandalone { get; set; }
	}
}
