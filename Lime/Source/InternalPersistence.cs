using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SharpVulkan;
using Yuzu;
using Yuzu.Binary;
using Yuzu.Json;

namespace Lime
{
	public class InternalPersistence : Persistence
	{
		internal static InternalPersistence Current => stackOfCurrent.Value.Count > 0 ? stackOfCurrent.Value.Peek() : null;
		private static void PushCurrent(InternalPersistence persistence) => stackOfCurrent.Value.Push(persistence);
		private static InternalPersistence PopCurrent() => stackOfCurrent.Value.Pop();

		private static readonly ThreadLocal<Stack<InternalPersistence>> stackOfCurrent = new ThreadLocal<Stack<InternalPersistence>>(() => new Stack<InternalPersistence>());

		private readonly Stack<string> pathStack = new Stack<string>();

		private static readonly Regex conflictRegex = new Regex("^<<<<<<<.*?^=======.*?^>>>>>>>",
			RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.Singleline);

		public static InternalPersistence Instance => threadLocalInstance.Value;
		private static readonly ThreadLocal<InternalPersistence>  threadLocalInstance = new ThreadLocal<InternalPersistence>(() => new InternalPersistence());

		public InternalPersistence()
		{

		}

		public InternalPersistence(CommonOptions yuzuCommonOptions, JsonSerializeOptions yuzuJsonOptions)
			: base(yuzuCommonOptions, yuzuJsonOptions)
		{ }

		internal string ShrinkPath(string path)
		{
			if (pathStack.Count == 0 || string.IsNullOrEmpty(path)) {
				return path;
			}
			var d = GetCurrentSerializationDirectory() + '/';
			return path.StartsWith(d) ? path.Substring(d.Length) : '/' + path;
		}

		internal string ExpandPath(string path)
		{
			if (pathStack.Count == 0 || string.IsNullOrEmpty(path)) {
				return path;
			}
			return (path[0] == '/') ? path.Substring(1) : GetCurrentSerializationDirectory() + '/' + path;
		}

		internal string GetCurrentSerializationPath()
		{
			return pathStack.Peek();
		}

		private string GetCurrentSerializationDirectory()
		{
			var path = Path.GetDirectoryName(pathStack.Peek());
			if (!string.IsNullOrEmpty(path)) {
				path = AssetPath.CorrectSlashes(path);
			}
			return path;
		}

		public override void WriteObject<T>(string path, Stream stream, T instance, Format format)
		{
			pathStack.Push(path);
			PushCurrent(this);
			try {
				base.WriteObject<T>(path, stream, instance, format);
			} finally {
				pathStack.Pop();
				PopCurrent();
			}
		}

		public override void WriteObject<T>(string path, Stream stream, T instance, AbstractSerializer serializer)
		{
			pathStack.Push(path);
			PushCurrent(this);
			try {
				base.WriteObject<T>(path, stream, instance, serializer);
			} finally {
				pathStack.Pop();
				PopCurrent();
			}
		}

		public override T ReadObject<T>(string path, Stream stream, object obj = null)
		{
			pathStack.Push(path);
			PushCurrent(this);
			try {
				return base.ReadObject<T>(path, stream, obj);
			} catch {
				if (!stream.CanSeek) {
					var ms = new MemoryStream();
					stream.CopyTo(ms);
					ms.Seek(0, SeekOrigin.Begin);
					stream = ms;
				}
				if (!CheckBinarySignature(stream) && HasConflicts(path, stream)) {
					throw new InvalidOperationException($"{path} has git conflicts");
				} else {
					throw;
				}
			} finally {
				pathStack.Pop();
				PopCurrent();
			}
		}

		private bool HasConflicts(string path, Stream stream)
		{
			stream.Seek(0, SeekOrigin.Begin);
			using (var reader = new StreamReader(stream)) {
				return conflictRegex.IsMatch(reader.ReadToEnd());
			}
		}
	}
}
