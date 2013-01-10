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
		Dictionary<string, TimelineRow> cache = new Dictionary<string, TimelineRow>();

		public TimelineNodeRow GetNodeRow(Lime.Node node)
		{
			string key = node.Guid.ToString();
			TimelineRow item;
			if (!cache.TryGetValue(key, out item)) {
				item = new TimelineNodeRow(node);
				cache[key] = item;
			}
			return item as TimelineNodeRow;
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

		public List<TimelineRow> GetRowsForContainer(Lime.Node container)
		{
			var rows = new List<TimelineRow>();
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
