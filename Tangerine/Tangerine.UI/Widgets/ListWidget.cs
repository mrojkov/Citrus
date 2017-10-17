using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime;

namespace Tangerine.UI
{
	public class ListWidget<TItem> : Widget
	{
		private Func<TItem, Widget> rowBuilder;
		private readonly Dictionary<TItem, Widget> widgetCache;

		public ListWidget(Func<TItem, Widget> rowBuilder, ObservableCollection<TItem> source)
		{
			Source = source;
			source.CollectionChanged += CollectionChanged;
			this.rowBuilder = rowBuilder;
			widgetCache = new Dictionary<TItem, Widget>();
			Rebuild();
		}

		private void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (footer != null) {
				Nodes.Remove(footer);
			}
			if (e.OldItems != null) {
				foreach (TItem item in e.OldItems) {
					Nodes.Remove(widgetCache[item]);
					widgetCache.Remove(item);
				}
			}

			if (e.NewItems != null) {
				foreach (TItem item in e.NewItems) {
					var row = rowBuilder(item);
					widgetCache.Add(item, row);
					AddNode(row);
				}
			}
			if (footer != null) {
				AddNode(footer);
			}
		}

		private void Rebuild()
		{
			widgetCache.Clear();
			Nodes.Clear();
			if (header != null)
				AddNode(header);
			foreach (var item in Source) {
				var row = rowBuilder(item);
				widgetCache.Add(item, row);
				AddNode(row);
			}
			if (footer != null)
				AddNode(footer);
		}

		public Widget header;
		public Widget Header
		{
			get { return header; }
			set
			{
				if (header != null) {
					Nodes.Remove(header);
				}

				header = value;
				if (header != null) {
					Nodes.Push(header);
				}
			}
		}

		public Widget footer;
		public Widget Footer
		{
			get { return footer; }
			set
			{
				if (footer != null) {
					Nodes.Remove(footer);
				}
				footer = value;
				if (footer != null) {
					Nodes.Add(footer);
				}


			}
		}

		public ObservableCollection<TItem> Source { get; private set; }
	}
}
