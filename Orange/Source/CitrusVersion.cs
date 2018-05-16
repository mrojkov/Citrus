using System;
using System.IO;
using Yuzu;

namespace Orange
{
	class CitrusVersion
	{
		public const string Filename = "citrus_version.json";

		public static CitrusVersion Load()
		{
			var yjd = new Yuzu.Json.JsonDeserializer();
			using (var stream = File.Open(Path.Combine(Toolbox.CalcCitrusDirectory(), Filename), FileMode.Open)) {
				return yjd.FromStream<CitrusVersion>(stream);
			}
		}

		public static void Save(CitrusVersion citrusVersion)
		{
			var yjs = new Yuzu.Json.JsonSerializer();
			using (var stream = File.Open(Path.Combine(Toolbox.CalcCitrusDirectory(), Filename), FileMode.Open)) {
				yjs.ToStream(citrusVersion, stream);
			}
		}

		[YuzuMember]
		public string Version { get; set; }

		[YuzuMember]
		public string BuildNumber { get; set; }

		[YuzuMember]
		public bool IsStandalone { get; set; }
	}
}
