using System;
using System.IO;
using Yuzu;
using Yuzu.Json;

namespace Orange
{
	class CitrusVersion
	{
		public const string Filename = "citrus_version.json";

		public static CitrusVersion Load(Stream stream) {
			var yjd = new JsonDeserializer { JsonOptions = new JsonSerializeOptions() { Unordered = true } };
			return yjd.FromStream<CitrusVersion>(stream);
		}

		public static void Save(CitrusVersion citrusVersion, Stream stream)
		{
			var yjs = new JsonSerializer();
			yjs.ToStream(citrusVersion, stream);
		}

		public static CitrusVersion Load()
		{
			using (var stream = File.Open(Path.Combine(Toolbox.CalcCitrusDirectory(), Filename), FileMode.Open)) {
				return Load(stream);
			}
		}

		public static void Save(CitrusVersion citrusVersion)
		{
			using (var stream = File.Open(Path.Combine(Toolbox.CalcCitrusDirectory(), Filename), FileMode.Open)) {
				Save(citrusVersion, stream);
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
