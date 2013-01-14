using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime.PopupMenu
{
	public class Menu : ICollection<MenuItem>
	{
		RoundedRectangle rectangle = new RoundedRectangle();

		public event BareEventHandler Hidden;
		public Frame Frame = new Frame();
		List<MenuItem> items = new List<MenuItem>();
		int layer;

		public Menu(int layer)
		{
			this.layer = layer;
			Frame.AddNode(rectangle);
			Frame.Updating += Frame_Updating;
			Frame.DialogMode = true;
		}

		public void Show()
		{
			World.Instance.PushNode(Frame);
			Frame.CenterOnParent();
			Frame.Layer = layer;
		}

		public void Hide()
		{
			Frame.Unlink();
			if (Hidden != null) {
				Hidden();
			}
		}

		void Frame_Updating(float delta)
		{
			if (Input.WasMousePressed()) {
				if (!Frame.HitTest(Input.MousePosition)) {
					Hide();
				}
			}
			Frame.Width = MenuItem.Width;
			Frame.Height = MenuItem.Height * Count + MenuItem.Height;
			UpdateBackground();
			UpdateItems();
		}

		private void UpdateItems()
		{
			int i = 0;
			foreach (var item in items) {
				item.Frame.Y = i * MenuItem.Height + MenuItem.Height / 2;
				item.Frame.Width = MenuItem.Width;
				item.Frame.Height = MenuItem.Height;
				i++;
			}
		}

		private void UpdateBackground()
		{
			rectangle.CornerRadius = MenuItem.Height / 3;
			rectangle.Size = Frame.Size;
		}

		#region ICollection<MenuItem>

		public void Add(MenuItem item)
		{
			item.Menu = this;
			items.Add(item);
			Frame.Nodes.Push(item.Frame);
		}

		public void Clear()
		{
			foreach (var item in items.ToArray()) {
				Remove(item);
			}
		}

		public bool Contains(MenuItem item)
		{
			return items.Contains(item);
		}

		public void CopyTo(MenuItem[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(MenuItem item)
		{
			Frame.Nodes.Remove(item.Frame);
			return items.Remove(item);
		}

		public IEnumerator<MenuItem> GetEnumerator()
		{
			return items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return items.GetEnumerator();
		}

		public int Count { get { return items.Count; } }

		public bool IsReadOnly { get { return false; } }
		#endregion
	}
}
