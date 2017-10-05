using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Orange
{
	public class Json
	{
		private readonly JObject obj;
		private readonly string sourcePath;

		public Json(JObject obj, string sourcePath = null)
		{
			this.obj = obj;
			this.sourcePath = sourcePath;
		}

		public object this[string path]
		{
			get { return GetValue<object>(path); }
		}

		public T[] GetArray<T>(string path, T[] @default = null)
		{
			JToken result = null;

			foreach (var token in GetPathTokens(path)) {
				if (token == null) {
					return @default;
				}

				result = token;
			}

			var array = result as JArray;
			return array == null ? null : array.ToObject<T[]>();
		}

		public T GetValue<T>(string path, T @default = default(T))
		{
			JToken result = null;

			foreach (var token in GetPathTokens(path)) {
				if (token == null)
					return @default;

				result = token;
			}

			var value = result as JValue;
			return value == null ? default(T) : (T) value.Value;
		}

		private IEnumerable<JToken> GetPathTokens(string path)
		{
			var json = obj;
			foreach (var element in path.Split('/')) {
				if (json == null)
					throw new Lime.Exception("{0} is not defined in {1}", path, sourcePath);

				JToken token;
				json.TryGetValue(element, out token);
				yield return token;

				json = token as JObject;
			}
		}

		public void AddToArray(string name, object target)
		{
			if (name == null)
				return;
			var array = obj.GetValue(name) as JArray;
			if (array == null) {
				array = new JArray();
				obj.Add(name, array);
			}
			array.Add(JObject.FromObject(target));
		}

		public void RemoveFromArray(string name, object target)
		{
			if (name == null)
				return;
			var array = obj.GetValue(name) as JArray;
			var targetJson = JObject.FromObject(target);
			if (array != null) {
				array.Remove(array.FirstOrDefault(v => v.ToString() == targetJson.ToString()));
			}
		}

		public void RewriteOrigin()
		{
			File.WriteAllText(sourcePath, obj.ToString());
		}
	}
}
