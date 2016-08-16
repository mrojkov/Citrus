using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	static partial class Actions
	{
		[MenuItem("Generate Lime deserializers")]
		public static void GenerateLimeDeserializersAction()
		{
			GenerateLimeDeserializers();
		}

		public static bool GenerateLimeDeserializers()
		{
			Lime.Serialization.GenerateDeserializers();
			Console.WriteLine("Done. Please rebuild Orange.");
			return true;
		}
	}
}
