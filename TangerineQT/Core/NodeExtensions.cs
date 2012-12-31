using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine
{
	public static class NodeExtensions
	{
		public static List<PropertyInfo> GetProperties(this Lime.Node node)
		{
			var result = new List<PropertyInfo>();
			var props = node.GetType().GetProperties();
			foreach (var p in props) {
				var tangAttr = p.GetAttribute<Lime.TangerinePropertyAttribute>();
				if (tangAttr != null) {
					result.Add(p);
				}
			}
			return result;
		}

		public static T GetAttribute<T>(this PropertyInfo propInfo) where T : Attribute
		{
			foreach (var a in propInfo.GetCustomAttributes(false)) {
				if (a is T) {
					return a as T;
				}
			}
			return null;
		}
	}
}
