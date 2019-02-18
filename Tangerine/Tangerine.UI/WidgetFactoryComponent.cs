using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime;

namespace Tangerine.UI
{
	public class WidgetFactoryComponent<TItem> : NodeComponent
	{
		private Func<TItem, Widget> rowBuilder;
		protected Widget Container { get; }
		protected readonly Dictionary<TItem, Widget> widgetCache;

		public WidgetFactoryComponent(Func<TItem, Widget> rowBuilder, ObservableCollection<TItem> source)
		{
			Source = source;
			source.CollectionChanged += OnCollectionChanged;
			this.rowBuilder = rowBuilder;
			Container = new Widget { Id = "Container", Layout = new VBoxLayout() };
			widgetCache = new Dictionary<TItem, Widget>();
			Rebuild();
		}

		public override void Dispose()
		{
			Source.CollectionChanged -= OnCollectionChanged;
			base.Dispose();
		}

		protected override void OnOwnerChanged(Node oldOwner)
		{
			oldOwner?.Nodes.Clear();
			Owner?.AddNode(Container);
		}

		protected void Rebuild()
		{
			widgetCache.Clear();
			Container.Nodes.Clear();
			foreach (var item in Source) {
				var row = rowBuilder(item);
				widgetCache.Add(item, row);
				Container.AddNode(row);
			}
		}

		protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
				case NotifyCollectionChangedAction.Add:
					var idx = e.NewStartingIndex;
					foreach (TItem item in e.NewItems) {
						var row = rowBuilder(item);
						widgetCache.Add(item, row);
						Container.Nodes.Insert(idx++, row);
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (TItem item in e.OldItems) {
						Container.Nodes.Remove(widgetCache[item]);
						widgetCache.Remove(item);
					}
					break;
				case NotifyCollectionChangedAction.Reset:
					widgetCache.Clear();
					Container.Nodes.Clear();
					break;
			}
		}

		public ObservableCollection<TItem> Source { get; private set; }
	}
}
