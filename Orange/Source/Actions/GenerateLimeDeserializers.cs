using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace Orange
{
	static partial class Actions
	{
		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Generate Lime deserializers")]
		[ExportMetadata("Priority", 5)]
		public static void GenerateLimeDeserializersAction()
		{
			Lime.Serialization.GenerateBinaryDeserializers();
			Console.WriteLine("Done. Please rebuild Orange.");
		}
	}
}
