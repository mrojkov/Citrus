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
			Lime.Serialization.GenerateBinaryDeserializers();
			Console.WriteLine("Done. Please rebuild Orange.");
		}
	}
}
