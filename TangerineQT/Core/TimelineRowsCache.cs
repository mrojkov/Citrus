using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Tangerine
{
	[ProtoContract]
	public class TimelineRowsCache
	{
		[ProtoMember(1)]
		Dictionary<string, Timeline.Row> cache = new Dictionary<string, Timeline.Row>();

		public Timeline.NodeRow GetNodeRow(Lime.Node node)
		{
			string key = node.Guid.ToString();
			Timeline.Row item;
			if (!cache.TryGetValue(key, out item)) {
				item = new Timeline.NodeRow(node);
				cache[key] = item;
			}
			return item as Timeline.NodeRow;
		}

		//public PropertyLine GetPropertyRow(Lime.Node node, PropertyInfo property)
		//{
		//	string key = node.Guid.ToString() + '#' + property.Name;
		//	AbstractLineView item;
		//	if (!cache.TryGetValue(key, out item)) {
		//		item = new PropertyLine(node, property);
		//		item.CreateWidgets();
		//		cache[key] = item;
		//	}
		//	return item as PropertyLine;
		//}

		public List<Timeline.Row> GetRowsForContainer(Lime.Node container)
		{
			var rows = new List<Timeline.Row>();
			int i = 0;
			foreach (var node in container.Nodes) {
				var nodeRow = GetNodeRow(node);
				nodeRow.Index = i++;
				rows.Add(nodeRow);
				//if (!nodeLine.IsFolded) {
				//	foreach (var prop in node.GetProperties()) {
				//		var propLine = GetPropertyLine(node, prop);
				//		lines.Add(propLine);
				//	}
				//}
			}
			return rows;
		}
	}
}
