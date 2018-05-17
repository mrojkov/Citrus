using System;
using System.IO;
using Yuzu;
using Yuzu.Json;

namespace Orange
{
	class CitrusVersion
	{
		public const string Filename = "citrus_version.json";

		public static CitrusVersion Load()
		{
			var yjd = new JsonDeserializer { JsonOptions = new JsonSerializeOptions() { Unordered = true } };
			using (var stream = File.Open(Path.Combine(Toolbox.CalcCitrusDirectory(), Filename), FileMode.Open)) {
				return yjd.FromStream<CitrusVersion>(stream);
			}
		}

		public static void Save(CitrusVersion citrusVersion)
		{
			var yjs = new JsonSerializer();
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
