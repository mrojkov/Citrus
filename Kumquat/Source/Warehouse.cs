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

		public Warehouse()
		{
			var prefix = "Location";
			var arr = AssetsBundle.Instance.EnumerateFiles();
	
			arr = arr.Where(x => 
				x.Substring(0, prefix.Length) == prefix && 
				Path.GetExtension(x) == ".scene"
			).ToArray();

			foreach (var path in arr)
				FindTools(new Frame(path));
		}

		public Widget this[string name]
		{
			get {
				return Tools[name];
			}
		}

		private void FindTools(Node root)
		{
			foreach (var tool in root.Descendants<Tool>()) {
				if (Tools.Keys.Contains(tool.Id)) {
					var mes = String.Format("Два тула с одинаковым именем: {0}", tool.Id);
					throw new Lime.Exception(mes);
				} else {
					Tools.Add(tool.Id, tool.DeepCloneFast<Widget>());
				};
			}
		}

	}
}
