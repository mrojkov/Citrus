using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public partial class ListView : ScrollViewWithSlider, IList<Widget>
	{
		public bool ManualItemsPositioning;

		public ListView(Frame frame, ScrollDirection scrollDirection = ScrollDirection.Vertical, bool processChildrenFirst = false)
			: this(frame, null, scrollDirection, processChildrenFirst)
		{
		}

		public ListView(Frame frame, Widget slider, ScrollDirection scrollDirection = ScrollDirection.Vertical, bool processChildrenFirst = false)
			: base(frame, slider, scrollDirection, processChildrenFirst)
		{
			Content.ReverseOrderRendering = true;
			Content.Tasks.Add(RefreshLayoutTask());
		}

		public void Refresh()
		{
			int itemCount = 0;
			float p = 0;
			foreach (var node in Content.Nodes) {
				var item = node.AsWidget;
				if (item.Visible) {
					itemCount++;
					if (ScrollDirection == ScrollDirection.Vertical) {
						if (!ManualItemsPositioning) {
							item.Y = p;
						}
						p += item.Height;
					} else {
						if (!ManualItemsPositioning) {
							item.X = p;
						}
						p += item.Width;
					}
				}
			}
			ContentLength = p;
		}

		public void ScrollToItem(Widget widget, bool instantly = false)
		{
			ScrollTo(ProjectToScrollAxis(widget.CalcPositionInSpaceOf(Content)), instantly);
		}

		private IEnumerator<object> RefreshLayoutTask()
		{
			while (true) {
				Refresh();
				yield return null;
			}
		}
	
		#region IList<Widget> implementation
		public void Add(Widget item)
		{
			PrepareWidgetBeforeInsertion(item);
			Content.AddNode(item);
			Refresh();
		}

		public void AddRange(IEnumerable<Widget> items)
		{
			foreach (var i in items) {
				PrepareWidgetBeforeInsertion(i);
				Content.AddNode(i);
			}
			Refresh();
		}

		private void PrepareWidgetBeforeInsertion(Widget item)
		{
			item.Pivot = Vector2.Zero;
			if (ScrollDirection == ScrollDirection.Vertical) {
				item.X = 0;
				item.Width = Content.Width;
				item.Anchors = Anchors.LeftRight;
			} else {
				item.Y = 0;
				item.Height = Content.Height;
				item.Anchors = Anchors.TopBottom;
			}
		}

		public void Clear()
		{
			Content.Nodes.Clear();
			ScrollPosition = 0;
		}

		public void Insert(int index, Widget item)
		{
			PrepareWidgetBeforeInsertion(item);
			Content.Nodes.Insert(index, item);
			Refresh();
		}

		public bool Contains(Widget item)
		{
			return Content.Nodes.Contains(item);
		}

		public void CopyTo(Widget[] array, int arrayIndex)
		{
			Content.Nodes.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return Content.Nodes.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(Widget item)
		{
			if (Content.Nodes.Contains(item)) {
				Content.Nodes.Remove(item);
				Refresh();
				return true;
			}
			return false;
		}

		public int RemoveAll(Predicate<Widget> match)
		{
			int c = 0;
			foreach (var i in this.ToList()) {
				if (match(i)) {
					Remove(i);
					c++;
				}
			}
			return c;
		}

		public IEnumerator<Widget> GetEnumerator()
		{
			foreach (var n in Content.Nodes) {
				yield return n.AsWidget;
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return Content.Nodes.GetEnumerator();
		}

		public int IndexOf(Widget item)
		{
			return Content.Nodes.IndexOf(item);
		}

		public void RemoveAt(int index)
		{
			Content.Nodes.RemoveAt(index);
			Refresh();
		}

		public Widget this[int index]
		{
			get { return Content.Nodes[index].AsWidget; }
			set { Content.Nodes[index] = value; }
		}

		#endregion
	}
}
