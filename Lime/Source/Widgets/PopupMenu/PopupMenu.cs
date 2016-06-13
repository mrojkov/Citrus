using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Lime.PopupMenu
{
	/// <summary>
	/// Контекстное отладочное меню
	/// </summary>
	public class Menu : ICollection<MenuItem>
	{
		RoundedRectangle rectangle = new RoundedRectangle();

		public event Action Hidden;
		public Frame Frame = new Frame { Tag = "$PopupMenu.cs" };
		private List<MenuItem> items = new List<MenuItem>();
		private int layer;
		private int maxHeight;
		private int itemWidth;
		private Widget container;

		public Vector2 Scale { get { return Frame.Scale; } set { Frame.Scale = value; } }

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="layer">Слой (свойство виджета Layer), на котором будет рисоваться меню</param>
		/// <param name="itemWidth">Ширина меню в пикселях</param>
		/// <param name="maxHeight">Максимально возможная высота меню в пикселях</param>
		public Menu(Widget container, int layer = RenderChain.LayerCount - 1, int itemWidth = 350, int maxHeight = 768)
		{
			this.container = container;
			this.layer = layer;
			this.maxHeight = maxHeight;
			this.itemWidth = itemWidth;
			Frame.AddNode(rectangle);
			Frame.Tasks.Add(RefreshMenuTask());
		}

		/// <summary>
		/// Показывает меню
		/// </summary>
		public void Show()
		{
			container.PushNode(Frame);
			Frame.CenterOnParent();
			Frame.Layer = layer;
			Frame.Input.CaptureAll();
		}

		/// <summary>
		/// Прячет меню
		/// </summary>
		public void Hide()
		{
			Frame.Input.Release();
			Frame.Unlink();
			if (Hidden != null) {
				Hidden();
			}
		}

		IEnumerator<object> RefreshMenuTask()
		{
			Frame.Input.CaptureMouse();
			while (true) {
				int visibleCount = items.Count(i => i.Visible);
				float totalItemsHeigth = MenuItem.Height * visibleCount;
				int columnsCount = Math.Max((totalItemsHeigth / maxHeight).Ceiling(), 1);
				int itemsPerColumn = ((float)visibleCount / columnsCount).Ceiling();
				if (Frame.Parent != null) {
					Frame.CenterOnParent();
				}
				Frame.Height = MenuItem.Height * itemsPerColumn + MenuItem.Height;
				Frame.Width = itemWidth * columnsCount;
				if (Frame.Height * Frame.Scale.Y > Window.Current.ClientSize.Y - 60) {
					Frame.Scale = (Window.Current.ClientSize.Y - 60) / Frame.Height * Vector2.One;
				}
				if (Frame.Width * Frame.Scale.X > Window.Current.ClientSize.X - 60) {
					Frame.Scale = (Window.Current.ClientSize.X - 60) / Frame.Width * Vector2.One;
				}
				UpdateBackground();
				UpdateItems(itemsPerColumn);
				yield return null;
				if (Frame.Input.WasMousePressed() && !Frame.IsMouseOver()) {
					Hide();
					yield break;
				}
			}
		}

		private void UpdateItems(int itemsPerColumn)
		{
			var i = 0;
			foreach (var item in items) {
				item.Frame.Visible = item.Visible;
				if (!item.Visible) {
					continue;
				}
				int column = i / itemsPerColumn;
				int row = i % itemsPerColumn;
				item.Frame.X = column * itemWidth;
				item.Frame.Y = row * MenuItem.Height + MenuItem.Height / 2;
				item.Frame.Width = itemWidth;
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
			items.CopyTo(array, arrayIndex);
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
