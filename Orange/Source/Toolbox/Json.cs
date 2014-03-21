using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	public class Json
	{
		JObject obj;
		string sourcePath;

		public Json(JObject obj, string sourcePath = null)
		{
			this.obj = obj;
			this.sourcePath = sourcePath;
		}

		public object this[string path]
		{
			get { return GetValue(path); }
		}

		public object GetValue(string path, object @default = null)
		{
			JObject json = obj;
			JValue result = null;
			foreach (var element in path.Split('/')) {
				JToken token = null;
				if (json == null || !json.TryGetValue(element, out token)) {
					if (@default != null) {
						return @default;
					}
					throw new Lime.Exception("{0} is not defined in {1}", path, sourcePath);
				}
				result = token as JValue;
				json = token as JObject;
			}
			return result.Value;
		}

		public bool GetBool(string path)
		{
			var value = GetValue(path);
			if (value is bool) {
				return (bool)value;
			} else {
				throw new Lime.Exception("Invalid boolean value: {0} in {1}", value, sourcePath);
			}
		}
	}
}
