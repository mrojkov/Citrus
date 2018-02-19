using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Yuzu;

namespace Tangerine.UI
{
	public enum DockSite
	{
		None,
		Left,
		Top,
		Right,
		Bottom,
		Fill
	}

	public enum DockSeparator
	{
		None,
		Vertical,
		Horizontal
	}

	public static class DockSiteExtensions
	{
		public static DockSeparator GetSeparator(this DockSite site)
		{
			switch (site) {
				case DockSite.Bottom:
				case DockSite.Top:
					return DockSeparator.Horizontal;
				case DockSite.Left:
				case DockSite.Right:
					return DockSeparator.Vertical;
				default:
					return DockSeparator.None;
			}
		}
	}

	public class DockingModel
	{
		public WindowPlacement MainWindow => Placements.First();

		public readonly List<DockPanel> Panels = new List<DockPanel>();

		[YuzuMember]
		public readonly List<WindowPlacement> Placements = new List<WindowPlacement>();

		public IEnumerable<WindowPlacement> VisibleWindows => Placements.Where(p => p.WindowWidget != null);

		private static DockingModel instance;
		public static DockingModel Instance => instance ?? (instance = new DockingModel());

		private DockingModel() { }

		public void Initialize(WindowPlacement main)
		{
			Placements.Add(main);
		}

		public PanelPlacement AddPanel(DockPanel panel, Placement targetPlacement, DockSite site, float stretch)
		{
			var placement = new PanelPlacement { Id = panel.Id, Title = panel.Title };
			DockPanelTo(placement, targetPlacement, site, stretch);
			Panels.Add(panel);
			return placement;
		}

		public PanelPlacement AppendPanelToPlacement(DockPanel panel, SplitPlacement target)
		{
			var placement = new PanelPlacement { Id = panel.Id, Title = panel.Title };
			if (target.FirstChild == null) {
				target.FirstChild = placement;
			} else if (target.SecondChild == null) {
				target.SecondChild = placement;
			} else {
				throw new System.Exception("Unable to append placement to another one; There is no space");
			}
			Panels.Add(panel);
			return placement;
		}

		public void DockPanelTo(PanelPlacement placement, Placement target, DockSite site, float stretch)
		{
			if (site == DockSite.Fill) {
				target.SwitchType(
					panelPlacement => {
						var tabBarPlacement = panelPlacement.Parent as TabBarPlacement;
						if (tabBarPlacement == null) {
							tabBarPlacement = new TabBarPlacement();
							var parent = (SplitPlacement)target.Parent;
							panelPlacement.Unlink();
							tabBarPlacement.Placements.Add(panelPlacement);
							parent.Append(tabBarPlacement);
						}
						tabBarPlacement.Placements.Add(placement);
					},
					tabBarPlacement => tabBarPlacement.Placements.Add(placement),
					splitPlacement => splitPlacement.Append(placement)
				);
			} else {
				if (target.Parent is TabBarPlacement) {
					target = target.Parent;
				}
				var parent = (SplitPlacement)target.Parent;
				var splitPlacement = new SplitPlacement();
				var isFirst = site == DockSite.Left || site == DockSite.Top;
				var wasFirst = parent.FirstChild == target;
				target.Unlink();
				if (isFirst) {
					splitPlacement.Initialize(placement, target, site.GetSeparator(), stretch);
				} else {
					splitPlacement.Initialize(target, placement, site.GetSeparator(), 1 - stretch);
				}
				if (wasFirst) {
					parent.FirstChild = splitPlacement;
				} else {
					parent.SecondChild = splitPlacement;
				}
			}
		}

		public bool IsPanelSingleInWindow(string id) => FindPanelPlacement(id).Root.DescendantPanels().Count() == 1;

		public void AddWindow(WindowPlacement placement) => Placements.Add(placement);

		public void HideWindowPanels(WindowPlacement windowPlacement)
		{
			windowPlacement.Root.HideThisOrDescendantPanels();
			windowPlacement.WindowWidget = null;
		}

		public void RemoveWindow(WindowPlacement placement) => Placements.Remove(placement);

		public PanelPlacement FindPanelPlacement(string id)
		{
			PanelPlacement result = null;
			foreach (var p in Placements) {
				if ((result = p.Root.FindPanelPlacement(id)) != null) {
					break;
				}
			}
			return result;
		}

		public WindowPlacement GetWindowByPlacement(Placement target)
		{
			foreach (var p in Placements) {
				if (target.IsDescendantOf(p.Root)) {
					return p;
				}
			}
			return null;
		}

		public WindowPlacement GetWindowByPlacement(string id) => GetWindowByPlacement(FindPanelPlacement(id));
	}

	public abstract class Placement
	{

		public Placement Parent { get; set; }

		public Placement Root => Parent?.Root ?? this;

		public abstract IEnumerable<PanelPlacement> DescendantPanels();

		public abstract void RemovePlacement(Placement placement);

		public abstract PanelPlacement FindPanelPlacement(string id);

		public abstract void HideThisOrDescendantPanels();

		public abstract Placement Clone();

		public abstract bool AnyVisiblePanel();

		public virtual void RemoveRedundantNodes()
		{
		}

		public virtual Vector2 CalcGlobalSize()
		{
			var result = Vector2.One;
			if (Parent == null) {
				return result;
			}
			Parent.SwitchType(onSplitPlacememnt: splitPlacement => {
				var stretch = ((SplitPlacement)Parent).Stretch;
				if (splitPlacement.FirstChild != this) {
					stretch = 1f - stretch;
				}
				result = splitPlacement.Separator == DockSeparator.Horizontal ? new Vector2(1, stretch) : new Vector2(stretch, 1);
			});
			return Parent.CalcGlobalSize() * result;
		}

		public void Unlink()
		{
			Parent?.RemovePlacement(this);
		}

		public bool IsDescendantOf(Placement root)
		{
			var placement = Parent;
			while (placement != null) {
				if (placement == root) {
					return true;
				}
				placement = placement.Parent;
			}
			return false;
		}

		public void SwitchType(
			Action<PanelPlacement> onPanelPlacement = null,
			Action<TabBarPlacement> onTabBarPlacement = null,
			Action<SplitPlacement> onSplitPlacememnt = null)
		{
			if (this is PanelPlacement) {
				onPanelPlacement?.Invoke(this as PanelPlacement);
			} else if (this is SplitPlacement) {
				onSplitPlacememnt?.Invoke(this as SplitPlacement);
			} else if (this is TabBarPlacement) {
				onTabBarPlacement?.Invoke(this as TabBarPlacement);
			}
		}
	}

	public class WindowPlacement
	{
		[YuzuMember]
		public Vector2 Position;

		[YuzuMember]
		public Vector2 Size;

		[YuzuMember]
		public SplitPlacement Root;

		[YuzuMember]
		public WindowState State;

		public WindowWidget WindowWidget;

		public WindowPlacement Clone()
		{
			return new WindowPlacement {
				Position = Position,
				Size = Size,
				State = State,
				Root = (SplitPlacement)Root.Clone()
			};
		}
	}

	public class PanelPlacement : Placement
	{
		[YuzuMember]
		public string Title;

		[YuzuMember]
		public string Id;

		[YuzuMember]
		public bool Hidden;

		public override IEnumerable<PanelPlacement> DescendantPanels() => new PanelPlacement[] { };

		public override void RemovePlacement(Placement placement)
		{
		}

		public override PanelPlacement FindPanelPlacement(string id) => id == Title ? this : null;

		public override bool AnyVisiblePanel() => !Hidden;

		public override void HideThisOrDescendantPanels() => Hidden = true;

		public override Placement Clone()
		{
			var clone = (Placement)MemberwiseClone();
			clone.Parent = null;
			return clone;
		}
	}

	public class PanelPlacementList : IList<PanelPlacement>
	{
		private readonly TabBarPlacement owner;
		private readonly List<PanelPlacement> list;

		public int Count => list.Count;

		public bool IsReadOnly => false;

		public PanelPlacement this[int index]
		{
			get
			{
				return list[index];
			}

			set
			{
				list[index] = value;
			}
		}

		public PanelPlacementList(TabBarPlacement owner)
		{
			list = new List<PanelPlacement>();
			this.owner = owner;
		}

		public int IndexOf(PanelPlacement item) => list.IndexOf(item);

		public void RemoveAt(int index)
		{
			list[index].Unlink();
		}

		public void Add(PanelPlacement item)
		{
			CheckBeforeInsertion(item);
			list.Add(item);
			item.Parent = owner;
		}

		public void Clear()
		{
			list.ForEach(p => p.Unlink());
			list.Clear();
		}

		public bool Contains(PanelPlacement item) => list.Contains(item);

		public void CopyTo(PanelPlacement[] array, int arrayIndex)
		{
			list.Select(p => p.Clone()).ToList().CopyTo(array, arrayIndex);
		}

		public bool Remove(PanelPlacement item)
		{
			if (list.Remove(item)) {
				item.Parent = null;
				return true;
			}
			return false;
		}

		public IEnumerator<PanelPlacement> GetEnumerator() => list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

		public void Insert(int index, PanelPlacement item)
		{
			CheckBeforeInsertion(item);
			list.Insert(index, item);
			item.Parent = owner;
		}

		public void Clone(TabBarPlacement newOwner)
		{
			if (Count > 0) {
				foreach (var placement in this) {
					newOwner.Placements.Add((PanelPlacement)placement.Clone());
				}
			}
		}

		private void CheckBeforeInsertion(Placement placement)
		{
			if (placement?.Parent != null) {
				throw new System.Exception("Can't adopt a placement twice. Call placement.Unlink() first");
			}
		}
	}

	public class TabBarPlacement : Placement
	{
		[YuzuMember]
		public PanelPlacementList Placements { get; private set; }

		public TabBarPlacement()
		{
			Placements = new PanelPlacementList(this);
		}

		public override IEnumerable<PanelPlacement> DescendantPanels() => Placements;

		public override PanelPlacement FindPanelPlacement(string id) => Placements.FirstOrDefault(p => p.Id == id);

		public override void HideThisOrDescendantPanels()
		{
			foreach (var p in Placements) {
				p.HideThisOrDescendantPanels();
			}
		}

		public override void RemovePlacement(Placement placement)
		{
			if (placement is PanelPlacement) {
				Placements.Remove((PanelPlacement)placement);
			}
		}

		public override void RemoveRedundantNodes()
		{
			if (Placements.Count == 0) {
				Unlink();
			} else if (Placements.Count == 1) {
				((SplitPlacement)Parent).Replace(this, Placements.First());
			}
		}

		public override bool AnyVisiblePanel() => Placements.Any(p => p.AnyVisiblePanel());

		public override Placement Clone()
		{
			var clone = new TabBarPlacement();
			Placements.Clone(clone);
			return clone;
		}
	}

	public class SplitPlacement : Placement
	{
		private Placement firstChild;
		private Placement secondChild;

		[YuzuMember]
		public DockSeparator Separator;

		[YuzuMember]
		public float Stretch = 1;

		[YuzuMember]
		public Placement FirstChild
		{
			get
			{
				return firstChild;
			}

			set
			{
				CheckBeforeInsertion(firstChild);
				firstChild = value;
				if (firstChild != null) {
					firstChild.Parent = this;
				}
			}
		}

		private void CheckBeforeInsertion(Placement placement)
		{
			if (placement?.Parent != null) {
				throw new System.Exception("Can't adopt a placement twice. Call placement.Unlink() first");
			}
		}

		[YuzuMember]
		public Placement SecondChild
		{
			get
			{
				return secondChild;
			}

			set
			{
				CheckBeforeInsertion(secondChild);
				secondChild = value;
				if (secondChild != null) {
					secondChild.Parent = this;
				}
			}
		}


		public override IEnumerable<PanelPlacement> DescendantPanels()
		{
			var list = new List<PanelPlacement>();
			AddDescendantPanels(list, FirstChild);
			AddDescendantPanels(list, SecondChild);
			return list;
		}

		private void AddDescendantPanels(List<PanelPlacement> list, Placement child)
		{
			if (child == null) {
				return;
			}
			if (child is PanelPlacement) {
				list.Add((PanelPlacement)child);
			} else {
				list.AddRange(child.DescendantPanels());
			}
		}

		public override PanelPlacement FindPanelPlacement(string id)
		{
			return FirstChild?.FindPanelPlacement(id) ?? SecondChild?.FindPanelPlacement(id);
		}

		public override void RemovePlacement(Placement placement)
		{
			if (placement == FirstChild) {
				FirstChild.Parent = null;
				FirstChild = null;
			} else if (placement == SecondChild) {
				SecondChild.Parent = null;
				SecondChild = null;
			} else {
				FirstChild?.RemovePlacement(placement);
				SecondChild?.RemovePlacement(placement);
			}
		}

		public bool RemovePanel(string panelId)
		{
			PanelPlacement panel;
			(panel = FindPanelPlacement(panelId))?.Unlink();
			return panel != null;
		}

		public override void HideThisOrDescendantPanels()
		{
			FirstChild?.HideThisOrDescendantPanels();
			SecondChild?.HideThisOrDescendantPanels();
		}

		public void Replace(Placement child, Placement newChild)
		{
			if (FirstChild == child) {
				FirstChild?.Unlink();
				FirstChild = newChild;
			} else if (SecondChild == child) {
				SecondChild?.Unlink();
				SecondChild = newChild;
			}
		}

		public override void RemoveRedundantNodes()
		{
			if (FirstChild == null && SecondChild != null) {
				ReplaceThisWith(SecondChild);
			} else if (SecondChild == null && FirstChild != null) {
				ReplaceThisWith(FirstChild);
			} else if (FirstChild == null && SecondChild == null) {
				Parent.RemovePlacement(this);
			}
			FirstChild?.RemoveRedundantNodes();
			SecondChild?.RemoveRedundantNodes();
		}

		private void ReplaceThisWith(Placement placement) => (Parent as SplitPlacement)?.Replace(this, placement);

		public override bool AnyVisiblePanel() => (FirstChild?.AnyVisiblePanel() ?? false) || (SecondChild?.AnyVisiblePanel() ?? false);

		public override Placement Clone()
		{
			return new SplitPlacement {
				Stretch = Stretch,
				Separator = Separator,
				FirstChild = FirstChild?.Clone(),
				SecondChild = SecondChild?.Clone()
			};
		}

		public void Initialize(Placement first, Placement second, DockSeparator separator, float stretch)
		{
			FirstChild?.Unlink();
			SecondChild?.Unlink();
			FirstChild = first;
			SecondChild = second;
			Separator = separator;
			Stretch = stretch;
		}

		public void Append(Placement placement)
		{
			CheckBeforeInsertion(placement);
			if (FirstChild == null) {
				FirstChild = placement;
			} else if (SecondChild == null) {
				SecondChild = placement;
			} else {
				throw new System.Exception("Unable to append placement to SplitPlacement");
			}
		}
	}

	public class RequestedDockingComponent : NodeComponent
	{
		public Rectangle? Bounds { get; set; }
	}

	public class DockPanel
	{
		public bool IsUndockable { get; private set; }
		public readonly Widget ContentWidget;
		public readonly string Id;

		public string Title;

		public DockPanel(string id, string title = null, bool undockable = true)
		{
			Id = id;
			Title = title ?? id;
			IsUndockable = undockable;
			ContentWidget = new Frame { Id = "PanelContent", ClipChildren = ClipMethod.ScissorTest, Layout = new StackLayout() };
		}
	}
}
