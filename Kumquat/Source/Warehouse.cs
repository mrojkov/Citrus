using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;
using Kumquat;
using ProtoBuf;
using System.IO;

namespace Kumquat
{
	[ProtoContract(SkipConstructor = true)]
	public class Warehouse
	{
		[ProtoMember(1)]
		public Dictionary<string, Tool> Tools = new Dictionary<string, Tool>();

		public Warehouse(Dictionary<string, Frame> locations)
		{
			foreach (var frame in locations.Values) {
				foreach (var tool in frame.Descendants<Tool>()) {
					if (Tools.Keys.Contains(tool.Id)) {
						throw new Lime.Exception("There are two tools with the same name: {0}", tool.Id);
					} else {
						Tools.Add(tool.Id, tool.DeepCloneFast<Tool>());
					};
				}
			}
		}

		public Tool this[string name]
		{
			get {
				return Tools[name];
			}
		}

	}
}
