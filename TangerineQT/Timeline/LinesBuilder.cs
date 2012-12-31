using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine
{
	public static class LineListExtension
	{
		public static bool AreEqual(this List<AbstractLine> lines1, List<AbstractLine> lines2)
		{
			if (lines1.Count != lines2.Count) {
				return false;
			}
			for (int i = 0; i < lines2.Count; i++) {
				if (lines1[i] != lines2[i]) {
					return false;
				}
			}
			return true;
		}
	}
	//{
	//	private List<AbstractLine> lines = new List<AbstractLine>();

	//	public IEnumerator<AbstractLine> GetEnumerator()
	//	{
	//		return lines.GetEnumerator();
	//	}

	//	public int Count { get { return lines.Count; } }

	//	public AbstractLine this[int index] 
	//	{
	//		get { return lines[index]; }
	//		set { lines[index] = value; } 
	//	}

	//	public void Add(AbstractLine line)
	//	{
	//		lines.Add(line);
	//	}

	//	public static bool AreEqual(List<AbstractLine> lines1, List<AbstractLine> lines2)
	//	{
	//		if (lines1.Count != lines2.Count) {
	//			return false;
	//		}
	//		for (int i = 0; i < lines2.Count; i++) {
	//			if (lines1[i] != lines2[i]) {
	//				return false;
	//			}
	//		}
	//		return true;
	//	}
	//}

	public class LinesBuilder
	{
		Dictionary<string, AbstractLine> cache = new Dictionary<string, AbstractLine>();

		public NodeLine GetNodeLine(Lime.Node node)
		{
			string key = node.Guid.ToString();
			AbstractLine item;
			if (!cache.TryGetValue(key, out item)) {
				item = new NodeLine(node);
				item.CreateWidgets();
				cache[key] = item;
			}
			return item as NodeLine;
		}

		public PropertyLine GetPropertyLine(Lime.Node node, PropertyInfo property)
		{
			string key = node.Guid.ToString() + '#' + property.Name;
			AbstractLine item;
			if (!cache.TryGetValue(key, out item)) {
				item = new PropertyLine(node, property);
				item.CreateWidgets();
				cache[key] = item;
			}
			return item as PropertyLine;
		}

		public List<AbstractLine> BuildLines(Lime.Node container)
		{
			var lines = new List<AbstractLine>();
			foreach (var node in container.Nodes) {
				var nodeLine = GetNodeLine(node);
				lines.Add(nodeLine);
				if (!nodeLine.IsFolded) {
					foreach (var prop in node.GetProperties()) {
						var propLine = GetPropertyLine(node, prop);
						lines.Add(propLine);
					}
				}
			}
			return lines;
		}
	}
}
