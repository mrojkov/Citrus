using System.Collections.Generic;

namespace Lime
{
	public abstract class RenderObject
	{
		internal bool Rendered = true;

		public abstract void Render();
	}

	public class RenderObjectList : List<RenderObject>
	{
		public void Render()
		{
			foreach (var ro in this) {
				ro.Render();
				ro.Rendered = true;
			}
		}
	}

	public abstract class WidgetRenderObject : RenderObject
	{
		public Matrix32 LocalToWorldTransform;
		public Blending Blending;
		public ShaderId Shader;

		public void CaptureRenderState(Widget widget)
		{
			LocalToWorldTransform = widget.LocalToWorldTransform;
			Blending = widget.Blending;
			Shader = widget.Shader;
		}

		protected void PrepareRenderState()
		{
			Renderer.Transform1 = LocalToWorldTransform;
			Renderer.Blending = Blending;
			Renderer.Shader = Shader;
		}
	}

	public static class RenderObjectPool<T> where T: RenderObject, new()
	{
		private static T[] items = new T[1] { new T() };
		private static int index;

		public static T Acquire()
		{
			for (int i = 0; i < items.Length; i++) {
				var item = items[index++];
				if (index == items.Length)
					index = 0;
				if (item.Rendered) {
					item.Rendered = false;
					return item;
				}
			}
			System.Array.Resize(ref items, items.Length * 2);
			index = items.Length / 2;
			for (int i = index; i < items.Length; i++) {
				items[i] = new T();
			}
			items[index].Rendered = false;
			return items[index];
		}
	}
}
