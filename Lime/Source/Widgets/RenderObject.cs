using System.Collections.Generic;

namespace Lime
{
	public abstract class RenderObject
	{
		internal bool Free = true;

		public abstract void Render();
	}

	public class RenderObjectList
	{
		private List<RenderObject> objects = new List<RenderObject>();
		
		public void Add(RenderObject obj)
		{
			objects.Add(obj);
		}
		
		public void Clear()
		{
			foreach (var obj in objects) {
				obj.Free = true;
			}
			objects.Clear();
		}
		
		public void Render()
		{
			foreach (var ro in objects) {
				ro.Render();
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
				if (item.Free) {
					item.Free = false;
					return item;
				}
			}
			System.Array.Resize(ref items, items.Length * 2);
			index = items.Length / 2;
			for (int i = index; i < items.Length; i++) {
				items[i] = new T();
			}
			items[index].Free = false;
			return items[index];
		}
	}
}
