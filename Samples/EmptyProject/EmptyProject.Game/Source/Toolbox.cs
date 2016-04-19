using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Lime;

#if iOS
using Foundation;
using MessageUI;
using UIKit;
#endif

namespace EmptyProject
{
	// Некоторые папки могут быть одними и теми же, или находиться друг внутри друга.
	public enum DataDirectoryType
	{
		Documents,
		// На iOS пользователь может покопаться в этой папке через iTunes.

		Library,
		// К содержимому этой папки у пользователя доступа нет, но она тоже бэкапится через iCloud
		// (это можно отключить для отдельных файлов функцией SetSkipBackupAttribute).

		Cache
		// Эта папка может быть очищена системой при нехватке места (но только не во время работы программы),
		// она не бэкапится через iCloud, и её содержимое не обязательно переживает обновление до новой версии приложения.
	}

	public enum ExecutionEnvironment
	{
		QA,
		Stage,
		Production
	}

	public struct NameValuePair<TValue>
	{
		public string Name;
		public TValue Value;

		public NameValuePair (string name, TValue value)
		{
			Name = name;
			Value = value;
		}
	}

	public static partial class Toolbox
	{
		public static NameValuePair<TValue> MakeNameValuePair<TValue>(string name, TValue value)
		{
			return new NameValuePair<TValue>(name, value);
		}

		public static Dictionary<Button, bool> DisableAllButtonsExcept(Widget root, Button btnExcept)
		{
			Dictionary<Button, bool> result = new Dictionary<Button, bool>();
			foreach (Button btn in root.DescendantsOf<Button>()) {
				if (btn != btnExcept) {
					result[btn] = btn.Enabled;
					btn.Enabled = false;
				}
			}
			return result;
		}

		public static Dictionary<Button, bool> DisableAllButtonsExcept(Widget root, HashSet<Button> btnsExcept)
		{
			Dictionary<Button, bool> result = new Dictionary<Button, bool>();
			foreach (Button btn in root.DescendantsOf<Button>()) {
				if (!btnsExcept.Contains(btn)) {
					result[btn] = btn.Enabled;
					btn.Enabled = false;
				}
			}
			return result;
		}

		public static void SetEnabledStateBackForButtons(Dictionary<Button, bool> data)
		{
			foreach (var btn in data.Keys) {
				btn.Enabled = data[btn];
			}
		}

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
		{
			T[] elements = source.ToArray();
			for (int i = elements.Length - 1; i >= 0; i--) {
				// Swap element "dotIndex" with a random earlier element it (or itself)
				// ... except we don't really need to swap it fully, as we can
				// return it immediately, and afterwards it's irrelevant.
				int swapIndex = Mathf.RandomInt(i + 1);
				yield return elements[swapIndex];
				elements[swapIndex] = elements[i];
			}
		}

		// Actually, it's Shuffle, but the method Shuffle already exists
		public static void Randomize<T>(this IList<T> list)
		{
			Random rng = new Random();
			int n = list.Count;
			while (n > 1) {
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		public static void PlayLoopedAnimation(Node node, string animation)
		{
			if (node.CurrentAnimation != animation || node.IsStopped) {
				node.TryRunAnimation(animation);
			}
		}

		public static void SetAnimationSoundsPitch(Lime.Node node, float pitch)
		{
			if (node is Lime.Audio) {
				(node as Lime.Audio).Pitch = pitch;
			}
			foreach (var child in node.Nodes) {
				SetAnimationSoundsPitch(child, pitch);
			}
		}

		public static bool MoveTo(ref Vector2 val, Vector2 to, float speed, float acceleration, float dt)
		{
			Vector2 dir = to - val;
			float dist = dir.Length;
			float spd = speed + acceleration * dist;
			float passed = spd * dt;
			if (passed >= dist) {
				val = to;
				return true;
			} else {
				val += dir / dist * passed;
				return false;
			}
		}

		public static bool ForRandomIn<T>(IEnumerable<T> source, Action<T> callback)
		{
			int cnt = 0;
			foreach (T t in source) {
				cnt++;
			}
			if (cnt > 0) {
				int num = Mathf.RandomInt(cnt);
				foreach (T t in source) {
					if (num == 0) {
						callback(t);
						return true;
					}
					num--;
				}
			}
			return false;
		}

		public static bool ForRandomIn<T>(IEnumerable<T> source1, IEnumerable<T> source2, Action<T> callback)
		{
			if (ForRandomIn(source1, callback)) {
				return true;
			} else {
				return ForRandomIn(source2, callback);
			}
		}

		public static float SineSoftMotion(float t)
		{
			if (t < 0.0f)
				return 0.0f;
			if (t > 1.0f)
				return 1.0f;
			return Mathf.Sin((t - 0.5f) * Mathf.Pi) * 0.5f + 0.5f;
		}

		public static bool HasAnimationPassedMarkerOrStopped(Node node, string markerId)
		{
			Marker marker = node.Markers.TryFind(markerId);
			return (marker != null && node.AnimationFrame >= marker.Frame) || node.IsStopped;
		}

		public static IEnumerator<object> Timer(float time, Action onFinish)
		{
			yield return time;
			if (onFinish != null) {
				onFinish.Invoke();
			}
		}

		public static IEnumerator<object> WaitMarkerPass(Widget ani, string marker, Action onFinish)
		{
			while (!Toolbox.HasAnimationPassedMarkerOrStopped(ani, marker)) {
				yield return 0;
			}
			if (onFinish != null) {
				onFinish.Invoke();
			}
		}

		public static IEnumerator<object> FadeOutAndKill(Widget widget, float time)
		{
			yield return FadeOut(widget, time);
			widget.Unlink();
		}

		public static IEnumerator<object> FadeOut(Widget widget, float time)
		{
			Color4 color = widget.Color;
			float totalTime = time;
			UpdateHandler updateProc = (float delta) => {
				time -= delta;
				widget.Color = color * Color4.FromFloats(1, 1, 1, (time / totalTime).Clamp(0, 1));
			};
			widget.Updating += updateProc;
			while (time > 0) {
				yield return 0;
			}
			widget.Updating -= updateProc;
		}

		public static IEnumerator<object> FadeIn(Widget widget, float totalTime)
		{
			Color4 color = widget.Color;
			widget.Color = color * Color4.FromFloats(1.0f, 1.0f, 1.0f, 0);
			yield return FadeIn(widget, color, totalTime);
		}

		public static IEnumerator<object> FadeIn(Widget widget, Color4 color, float totalTime)
		{
			float time = 0;
			UpdateHandler updateProc = (float delta) => {
				time += delta;
				widget.Color = color * Color4.FromFloats(1, 1, 1, (time / totalTime).Clamp(0, 1));
			};
			widget.Updating += updateProc;
			while (time < totalTime) {
				yield return 0;
			}
			widget.Updating -= updateProc;
		}

		public static float Spline(float t, float[] keys, float[] values)
		{
			if (keys.Length == 0 || keys.Length != values.Length)
				throw new Lime.Exception("Toolbox.Spline: invalid params");
			if (values.Length == 1)
				return values[0];
			int last = keys.Length - 1;
			if (t <= keys[0])
				return values[0];
			if (t >= keys[last])
				return values[last];
			if (last == 1)
				return Mathf.CatmullRomSpline((t - keys[0]) / (keys[1] - keys[0]), values[0], values[0], values[1], values[1]);
			if (t <= keys[1])
				return Mathf.CatmullRomSpline((t - keys[0]) / (keys[1] - keys[0]), values[0], values[0], values[1], values[2]);
			if (t >= keys[last - 1])
				return Mathf.CatmullRomSpline((t - keys[last - 1]) / (keys[last] - keys[last - 1]), values[last - 2], values[last - 1], values[last], values[last]);
			for (int i = 2; i < last; i++) {
				if (t < keys[i])
					return Mathf.CatmullRomSpline((t - keys[i - 1]) / (keys[i] - keys[i - 1]), values[i - 2], values[i - 1], values[i], values[i + 1]);
			}
			return values[last - 1];
		}

		public static void SetProgressBar(Widget bar, string markerMin, string markerMax, double fraction)
		{
			int minTime = bar.Markers.Find(markerMin).Time;
			int maxTime = bar.Markers.Find(markerMax).Time;
			bar.AnimationTime = (minTime + (int)(fraction * (maxTime - minTime))).Clamp(minTime, maxTime);
			bar.IsStopped = true;
		}

		public static float NormalizeRotation(float value)
		{
			while (value < 0)
				value += 360;
			while (value >= 360)
				value -= 360;
			return value;
		}

		public static List<string> ReadStrings(Stream stream)
		{
			List<string> result = new List<string>();
			using (var r = new StreamReader(stream)) {
				while (true) {
					string line = r.ReadLine();
					if (line == null) {
						break;
					}
					result.Add(line.Trim());
				}
			}
			return result;
		}

		static Vertex[] quadVertices = new Vertex[4];

		static public void DrawQuad(ITexture texture1, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color4 color)
		{
			quadVertices[0] = new Vertex { Pos = a, Color = color, UV1 = Vector2.Zero };
			quadVertices[1] = new Vertex { Pos = b, Color = color, UV1 = new Vector2(1.0f, 0) };
			quadVertices[2] = new Vertex { Pos = c, Color = color, UV1 = Vector2.One };
			quadVertices[3] = new Vertex { Pos = d, Color = color, UV1 = new Vector2(0, 1.0f) };
			Renderer.DrawTriangleFan(texture1, null, quadVertices, quadVertices.Length);
		}

		static public void DrawQuad(Vector2 leftTop, Vector2 size, Color4 color)
		{
			DrawQuad(null, leftTop, leftTop + new Vector2(size.X, 0), leftTop + size, leftTop + new Vector2(0, size.Y), color);
		}

		static public void AddDebugInfoMenu(Lime.PopupMenu.Menu menu)
		{
			Lime.PopupMenu.Menu debugInfoSubMenu = new Lime.PopupMenu.Menu(Widget.MaxLayer, 200);
			debugInfoSubMenu.Add(new Lime.PopupMenu.StringItem("Off", () => {
				The.Application.DebugInfoMode = DebugInfoMode.Off;
			}));
			debugInfoSubMenu.Add(new Lime.PopupMenu.StringItem("FPS Only", () => {
				The.Application.DebugInfoMode = DebugInfoMode.FPS_Only;
			}));

			menu.Add(new Lime.PopupMenu.Separator());
			menu.Add(new Lime.PopupMenu.StringItem("Debug Info", debugInfoSubMenu));
		}

		public static int RoundTo(this int x, int y)
		{
			return (x + y / 2) / y * y;
		}

		public static void DeleteAllParticles(Node n)
		{
			if (n is ParticleEmitter) {
				(n as ParticleEmitter).DeleteAllParticles();
			} else {
				foreach (Node child in n.Nodes) {
					DeleteAllParticles(child);
				}
			}
		}

		public static string GetDataDirectory(DataDirectoryType type)
		{
#if iOS
			NSSearchPathDirectory dir;
			switch(type) {
				case DataDirectoryType.Documents:
					dir = NSSearchPathDirectory.DocumentDirectory;
					break;
				case DataDirectoryType.Library:
					dir = NSSearchPathDirectory.LibraryDirectory;
					break;
				case DataDirectoryType.Cache:
					dir = NSSearchPathDirectory.CachesDirectory;
					break;
				default:
					throw new Lime.Exception("Invalid DataDirectoryType");
			}
			var url = NSFileManager.DefaultManager.GetUrls(dir, NSSearchPathDomain.User)[0];
			return url.Path;
#else
			return Lime.Environment.GetDataDirectory("Game Forest", Application.AppTitle, "1.0");
#endif
		}

		public static void SetSkipBackupAttribute(string fileName, bool skipBackup)
		{
#if iOS
			NSFileManager.SetSkipBackupAttribute(fileName, skipBackup);
#endif
		}

		public static bool GetSkipBackupAttribute(string fileName)
		{
#if iOS
			return NSFileManager.GetSkipBackupAttribute(fileName);
#endif
			return false;
		}

#if ANDROID
		static ExecutionEnvironment? executionEnvironment;

		public static ExecutionEnvironment GetExecutionEnvironment()
		{
			if (executionEnvironment.HasValue) {
				return executionEnvironment.Value;
			}
			var activity = Lime.ActivityDelegate.Instance.Activity;
			if (activity == null) {
				return ExecutionEnvironment.Production;
			}
			using (var fs = activity.Assets.Open("E")) {
				using (var r = new StreamReader(fs)) {
					var s = r.ReadLine();
					if (s[0] == 'Q') {
						executionEnvironment = ExecutionEnvironment.QA;
					} else if (s[0] == 'S') {
						executionEnvironment = ExecutionEnvironment.Stage;
					} else {
						executionEnvironment = ExecutionEnvironment.Production;
					}
				}
			}
			return executionEnvironment.Value;
		}
#endif

		public static bool CheckiOSVersion(int major, int minor) // greater or equals
		{
#if iOS
			return UIDevice.CurrentDevice.CheckSystemVersion(major, minor);
#else
			return true;
#endif
		}

		public static IntVector2 CeilingVector(Vector2 p)
		{
			return new IntVector2((int)Math.Ceiling(p.X), (int)Math.Ceiling(p.Y));
		}

		public static T Random<T>(this IList<T> l)
		{
			return l[Mathf.RandomInt(0, l.Count - 1)];
		}

		public static void PropagateTextOverflowMode(Node root, TextOverflowMode mode)
		{
			var text = root as IText;
			if (text != null) {
				text.OverflowMode = mode;
			}
			foreach (var child in root.Nodes) {
				PropagateTextOverflowMode(child, mode);
			}
		}

		public static void PropagateAnimation(Node root, string markerId)
		{
			if (!root.TryRunAnimation(markerId)) {
				foreach (var child in root.Nodes) {
					PropagateAnimation(child, markerId);
				}
			}
		}

		public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
		{
			return GetValueOrDefault(dict, key, default(TValue));
		}

		public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue @default)
		{
			TValue value;
			return dict.TryGetValue(key, out value) ? value : @default;
		}

		public static T[] GetEnumValues<T>()
		{
			return (T[])Enum.GetValues(typeof(T));
		}

		public static T Last<T>(this IList<T> l)
		{
			return l[l.Count - 1];
		}

#if UNITY
		public static bool TryParseEnum<T>(string valueToParse, out T returnValue)
		{
			returnValue = default(T); 
			if (Enum.IsDefined(typeof(T), valueToParse)) {
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
				returnValue = (T)converter.ConvertFromString(valueToParse);
				return true;
			}
			return false;
		}
#else
		public static bool TryParseEnum<T>(string valueToParse, out T returnValue) where T : struct
		{
			return Enum.TryParse<T>(valueToParse, out returnValue);
		}
#endif

	}
}
