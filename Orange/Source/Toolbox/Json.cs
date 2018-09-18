using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Orange
{
	public class Json
	{
		private readonly string sourcePath;

		public JObject JObject { get; }
		public dynamic AsDynamic { get; }

		public object this[string path] => GetValue<object>(path);

		public Json(string sourcePath)
		{
			this.sourcePath = sourcePath;
			var json = File.ReadAllText(sourcePath);
			JObject = JObject.Parse(json);
			AsDynamic = JsonConvert.DeserializeObject(json);
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
			var json = JObject;
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
			var array = JObject.GetValue(name) as JArray;
			if (array == null) {
				array = new JArray();
				JObject.Add(name, array);
			}
			array.Add(JObject.FromObject(target));
		}

		public void RemoveFromArray(string name, object target)
		{
			if (name == null)
				return;
			var array = JObject.GetValue(name) as JArray;
			var targetJson = JObject.FromObject(target);
			if (array != null) {
				array.Remove(array.FirstOrDefault(v => v.ToString() == targetJson.ToString()));
			}
		}

		public void RewriteOrigin()
		{
			File.WriteAllText(sourcePath, JObject.ToString());
		}
	}
}
