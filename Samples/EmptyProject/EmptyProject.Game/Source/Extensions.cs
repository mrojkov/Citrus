using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;

namespace EmptyProject
{
	public static class WidgetExtensions
	{
		public static Button Button(this Node node, string id)
		{
			return node.Find<Button>(id);
		}

		public static Image Image(this Node node, string id)
		{
			return node.Find<Image>(id);
		}

		public static void InterpolateBetweenMarkers(this Node node, string start, string end, float value)
		{
			var m1 = node.Markers.Find(start);
			var m2 = node.Markers.Find(end);
			node.AnimationTime = (int)Mathf.Lerp(value, m1.Time, m2.Time);
		}

		public static bool SwitchAnimation(this Node node, string animation)
		{
			if (node.CurrentAnimation != animation) {
				node.RunAnimation(animation);
				return true;
			}
			return false;
		}

		public static bool IsAnimationBeforeMarker(this Node node, string markerId)
		{
			var marker = node.Markers.Find(markerId);
			return node.AnimationFrame < marker.Frame;
		}

		public static bool HasAnimationPassedMarker(this Node node, string markerId)
		{
			var marker = node.Markers.Find(markerId);
			return node.AnimationFrame >= marker.Frame;
		}

		public static void InsertAfter(this Node widget, Node target)
		{
			widget.Unlink();
			Node parent = target.Parent;
			parent.Nodes.Insert(parent.Nodes.IndexOf(target) + 1, widget);
		}

		public static void InsertBefore(this Node widget, Node target)
		{
			widget.Unlink();
			Node parent = target.Parent;
			parent.Nodes.Insert(parent.Nodes.IndexOf(target), widget);
		}

		public static IEnumerable<T> DescendantsOf<T>(this Node root, string id = null) where T : Node
		{
			foreach (Node node in root.Nodes) {
				T result = node as T;
				if (result != null && (id == null || result.Id == id))
					yield return result;

				if (node.Nodes.Count > 0)
					foreach (var n in DescendantsOf<T>(node, id))
						yield return n;
			}
		}

		public static Node DeepCloneFastInPlace(this Node node)
		{
			Node result = node.DeepCloneFast();
			node.Parent.Nodes.Insert(node.Parent.Nodes.IndexOf(node) + 1, result);
			return result;
		}

		public static void DraggableClicked(this Widget button, Action clicked)
		{
			(button as Button).Draggable = true;
			button.Clicked = clicked;
		}

		public static void RescaleToFitWidth(this RichText rt, float heightScaleFactor = 0.5f)
		{
			float initialWidth = rt.Width;
			rt.Width = 9999;
			Vector2 sz = rt.MeasureText().Size;
			if (sz.X > initialWidth) {
				rt.Width = sz.X + 20;
				float scaleX = initialWidth / sz.X;
				float scaleY = Mathf.Lerp(heightScaleFactor, 1, scaleX);
				rt.Scale = new Vector2(scaleX, scaleY);
			} else {
				rt.Width = initialWidth;
			}
		}

	}

	static class DelegateExtensions
	{
		public static void SafeInvoke(this Action handler)
		{
			if (handler != null) {
				handler();
			}
		}

		public static void SafeInvoke(this UpdateHandler handler, float delta)
		{
			if (handler != null) {
				handler(delta);
			}
		}

		public static void SafeInvoke<T>(this Action<T> handler, T value)
		{
			if (handler != null) {
				handler(value);
			}
		}
	}

	public static class OtherExtensions
	{
		/// <summary>
		/// Преобразует строку в число
		/// </summary>
		/// <param name="defaultValue">Значение по умолчанию. Если не получиться преобразовать строку в число, функция вернет этот результат</param>
		public static int ToInt(this string s, int defaultValue)
		{
			int result;
			if (!int.TryParse(s, out result)) {
				result = defaultValue;
			}

			return result;
		}

		/// <summary>
		/// Преобразует миллисекунды в строку, показывающую время
		/// </summary>
		public static string ToTimeString(this int value)
		{
			int totalSecs = value / 1000;
			int hours = totalSecs / 3600;
			totalSecs -= hours * 3600;

			int mins = totalSecs / 60;
			totalSecs -= mins * 60;

			int secs = totalSecs;

			if (hours > 0)
				return string.Format("{0}:{1:D2}:{2:D2}", hours, mins, secs);
			else
				return string.Format("{0:D2}:{1:D2}", mins, secs);
		}
	}
}
