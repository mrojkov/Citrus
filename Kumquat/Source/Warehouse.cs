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
		public Dictionary<string, Widget> Tools = new Dictionary<string, Widget>();

		public Warehouse(Dictionary<string, Frame> locations)
		{
			foreach (var frame in locations.Values) {
				foreach (var tool in frame.Descendants<Tool>()) {
					if (Tools.Keys.Contains(tool.Id)) {
						var mes = String.Format("Два тула с одинаковым именем: {0}", tool.Id);
						throw new Lime.Exception(mes);
					} else {
						Tools.Add(tool.Id, tool.DeepCloneFast<Widget>());
					};
				}
			}
		}

		public Widget this[string name]
		{
			get {
				return Tools[name];
			}
		}

	}
}
