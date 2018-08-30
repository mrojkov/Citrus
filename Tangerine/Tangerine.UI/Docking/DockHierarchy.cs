using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Yuzu;

namespace Tangerine.UI.Docking
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

	public enum LinearPlacementDirection
	{
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

	public class DockHierarchy
	{
		public WindowPlacement MainWindow => WindowPlacements.First();

		public readonly List<Panel> Panels = new List<Panel>();

		[YuzuMember]
		public readonly List<WindowPlacement> WindowPlacements = new List<WindowPlacement>();

		public IEnumerable<WindowPlacement> VisibleWindowPlacements => WindowPlacements.Where(p => p.WindowWidget != null);

		private static DockHierarchy instance;
		public static DockHierarchy Instance => instance ?? (instance = new DockHierarchy());

		private DockHierarchy() { }

		public void Initialize(WindowPlacement main)
		{
			WindowPlacements.Add(main);
		}

		public PanelPlacement AddPanel(Panel panel, Placement targetPlacement, DockSite site, float stretch)
		{
			var placement = new PanelPlacement { Id = panel.Id, Title = panel.Title };
			DockPlacementTo(placement, targetPlacement, site, stretch);
			Panels.Add(panel);
			return placement;
		}

		public PanelPlacement AppendPanelToPlacement(Panel panel, LinearPlacement target, float stretch)
		{
			var placement = new PanelPlacement { Id = panel.Id, Title = panel.Title };
			target.Placements.Add(new StretchPlacement(placement, stretch));
			Panels.Add(panel);
			return placement;
		}

		private void DockPlacementTo(TabBarPlacement placement, TabBarPlacement target)
		{
			var placements = placement.Placements.ToList();
			foreach (var panelPlacement in placements) {
				panelPlacement.Unlink();
				target.Placements.Add(panelPlacement);
			}
		}

		private void DockPlacementTo(TabBarPlacement placement, PanelPlacement target)
		{
			var parent = target.Parent;
			if (parent is TabBarPlacement tabBarPlacement) {
				DockPlacementTo(placement, tabBarPlacement);
				return;
			}
			if (parent is StretchPlacement stretchPlacement) {
				target.Unlink();
				placement.Placements.Add(target);
				stretchPlacement.Placement = placement;
			}
		}

		private void DockPlacementTo(PanelPlacement placement, PanelPlacement target)
		{
			if (!(target.Parent is TabBarPlacement tabBarPlacement)) {
				tabBarPlacement = new TabBarPlacement();
				var parent = (StretchPlacement)target.Parent;
				target.Unlink();
				tabBarPlacement.Placements.Add(target);
				parent.Placement = tabBarPlacement;
			}
			tabBarPlacement.Placements.Add(placement);
		}

		public void DockPlacementTo(LinearPlacement placement, PanelPlacement target)
		{
			var parent = target.Parent;
			if (parent is TabBarPlacement tabBarPlacement) {
				DockPlacementTo(placement, tabBarPlacement);
			}
			if (parent is StretchPlacement targetStretchPlacement) {
				DockPlacementTo(placement, (LinearPlacement)targetStretchPlacement.Parent);
			}
		}

		public void DockPlacementTo(LinearPlacement placement, TabBarPlacement target)
		{
			var placements = placement.Placements.ToList();
			foreach (var stretchPlacement in placements) {
				var p = stretchPlacement.Placement;
				p.Unlink();
				stretchPlacement.Unlink();
				DockPlacementTo(p, target, DockSite.Fill, 0);
			}
			placement.Unlink();
		}

		public void DockPlacementTo(LinearPlacement placement, LinearPlacement target, int index = 0, float stretch = 0)
		{
			if (placement.Direction == target.Direction) {
				stretch = 1f / (target.Placements.Count + placement.Placements.Count);
				var placements = placement.Placements.ToList();
				for (int i = placements.Count - 1; i >= 0; --i) {
					target.Placements.Insert(0, placements[i]);
				}
				foreach (var p in target.Placements) {
					p.Stretch = stretch;
				}
				return;
			}
			if (placement.Placements.Count == 1) {
				var stretchPlacement = placement.Placements[0];
				stretchPlacement.Unlink();
				stretchPlacement.Stretch = stretch;
				target.Placements.Insert(index, stretchPlacement);
				return;
			}
			target.Placements.Insert(index, new StretchPlacement(placement, stretch));
		}

		private bool CheckPlacement(LinearPlacement placement, DockSite site, out LinearPlacement newPlacement)
		{
			newPlacement = placement;
			if (site == DockSite.Left || site == DockSite.Right) {
				if (placement.Direction == LinearPlacementDirection.Horizontal) {
					return true;
				}
				newPlacement = new LinearPlacement(LinearPlacementDirection.Horizontal);
				return false;
			}
			if (placement.Direction == LinearPlacementDirection.Vertical) {
				return true;
			}
			newPlacement = new LinearPlacement(LinearPlacementDirection.Vertical);
			return false;
		}

		private LinearPlacement CheckPlacement(LinearPlacement placement, DockSite site)
		{
			if (!CheckPlacement(placement, site, out var newPlacement)) {
				placement.Placements.Add(new StretchPlacement(newPlacement, 1));
			}
			return newPlacement;
		}

		private float AdjustStretch(LinearPlacement linearPlacement, float stretch, int index)
		{
			if (stretch >= 0) {
				return stretch;
			}
			linearPlacement.Placements[index].Stretch *= 0.75f;
			return linearPlacement.Placements[index].Stretch * 0.25f;
		}

		public void DockPlacementTo(Placement placement, Placement target, DockSite site, float stretch)
		{
			if (site == DockSite.Fill) {
				target.SwitchType(
					targetPanelPlacement => {
						switch (placement)
						{
							case TabBarPlacement tabBarPlacement:
								DockPlacementTo(tabBarPlacement, targetPanelPlacement);
								break;
							case PanelPlacement panelPlacement:
								DockPlacementTo(panelPlacement, targetPanelPlacement);
								break;
							case LinearPlacement linearPlacement:
								DockPlacementTo(linearPlacement, targetPanelPlacement);
								break;
						}
					},
					targetTabBarPlacement => {
						switch (placement)
						{
							case TabBarPlacement tabBarPlacement:
								DockPlacementTo(tabBarPlacement, targetTabBarPlacement);
								break;
							case PanelPlacement panelPlacement:
								targetTabBarPlacement.Placements.Add(panelPlacement);
								break;
							case LinearPlacement linearPlacement:
								DockPlacementTo(linearPlacement, targetTabBarPlacement);
								break;
						}
					},
					targetLinearPlacement => {
						switch (placement) {
							case LinearPlacement linearPlacement:
								DockPlacementTo(linearPlacement, targetLinearPlacement);
								break;
							case TabBarPlacement _:
							case PanelPlacement _:
								targetLinearPlacement.Placements.Add(new StretchPlacement(placement,
									AdjustStretch(targetLinearPlacement, stretch, targetLinearPlacement.Placements.Count - 1)));
								break;

						}
					}
				);
				return;
			}
			var parent = target as LinearPlacement;
			int index = 0;
			if (parent == null) {
				if (target.Parent is TabBarPlacement parentTabBarPlacement) {
					target = parentTabBarPlacement;
				}
				if (target.Parent is StretchPlacement parentStretchPlacement) {
					if (!CheckPlacement((LinearPlacement)parentStretchPlacement.Parent, site, out parent)) {
						parentStretchPlacement.Placement = parent;
						stretch = stretch < 0 ? 0.25f : stretch;
						parent.Placements.Add(new StretchPlacement(target, 1 - stretch));
						index = site == DockSite.Bottom || site == DockSite.Right ? 1 : 0;
					} else {
						index = parent.Placements.IndexOf(parentStretchPlacement);
						stretch = AdjustStretch(parent, stretch, index);
						if (site == DockSite.Right || site == DockSite.Bottom) {
							++index;
						}
					}
				}
			} else {
				parent = CheckPlacement(parent, site);
				if (site == DockSite.Right || site == DockSite.Bottom) {
					index = parent.Placements.Count;
				}
				stretch = AdjustStretch(parent, stretch, index);
			}
			if (placement is LinearPlacement lPlacement) {
				DockPlacementTo(lPlacement, parent, index, stretch);
			} else {
				parent.Placements.Insert(index, new StretchPlacement(placement, stretch));
			}
		}

		public bool IsPanelSingleInWindow(string id) => FindPanelPlacement(id).Root.GetPanelPlacements().Count() == 1;

		public void AddWindow(WindowPlacement placement) => WindowPlacements.Add(placement);

		public void HideWindowPanels(WindowPlacement windowPlacement)
		{
			windowPlacement.Root.HideThisOrDescendantPanels();
			windowPlacement.WindowWidget = null;
		}

		public void RemoveWindow(WindowPlacement placement) => WindowPlacements.Remove(placement);

		public PanelPlacement FindPanelPlacement(string id)
		{
			PanelPlacement result = null;
			foreach (var p in WindowPlacements) {
				if ((result = p.Root.FindPanelPlacement(id)) != null) {
					break;
				}
			}
			return result;
		}

		public WindowPlacement GetWindowByPlacement(Placement target)
		{
			foreach (var p in WindowPlacements) {
				if (target.IsDescendantOf(p.Root) || target == p) {
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

		public abstract IEnumerable<PanelPlacement> GetPanelPlacements();

		public abstract void RemovePlacement(Placement placement);

		public abstract PanelPlacement FindPanelPlacement(string id);

		public abstract void HideThisOrDescendantPanels();

		public abstract Placement Clone();

		public abstract bool AnyVisiblePanel();

		public virtual void RemoveRedundantNodes() { }

		public virtual void Replace(Placement old, Placement @new) { }

		public virtual Vector2 CalcGlobalSize()
		{
			var result = Vector2.One;
			if (Parent == null) {
				return result;
			}
			Parent.SwitchType(onLinearPlacement: linearPlacement => {
				float stretch = ((StretchPlacement)this).Stretch;
				result =
					linearPlacement.Direction == LinearPlacementDirection.Vertical
					? new Vector2(1, stretch)
					: new Vector2(stretch, 1);
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
			Action<LinearPlacement> onLinearPlacement = null)
		{
			switch (this)
			{
				case PanelPlacement panelPlacement:
					onPanelPlacement?.Invoke(panelPlacement);
					break;
				case LinearPlacement linearPlacement:
					onLinearPlacement?.Invoke(linearPlacement);
					break;
				case TabBarPlacement tabBarPlacement:
					onTabBarPlacement?.Invoke(tabBarPlacement);
					break;
			}
		}

		public IEnumerable<PanelPlacement> GetVisiblePanelPlacements() =>
			GetPanelPlacements().Where(panelPlacement => !panelPlacement.Hidden);
	}

	public class WindowPlacement : LinearPlacement
	{
		[YuzuOptional]
		public Vector2 Position;

		[YuzuOptional]
		public Vector2 Size;

		[YuzuOptional]
		public WindowState State;

		public WindowWidget WindowWidget;

		public override Placement Clone()
		{
			var result = new WindowPlacement {
				Position = Position,
				Size = Size,
				State = State,
			};
			foreach (var placement in Placements) {
				result.Placements.Add((StretchPlacement)placement.Clone());
			}
			return result;
		}

		public WindowPlacement() : base(LinearPlacementDirection.Vertical)
		{
		}
	}

	public class PanelPlacement : Placement
	{
		[YuzuOptional]
		public string Title;

		[YuzuOptional]
		public string Id;

		[YuzuOptional]
		public bool Hidden;

		public override IEnumerable<PanelPlacement> GetPanelPlacements()
		{
			yield return this;
		}

		public override void RemovePlacement(Placement placement) { }

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

	public class PlacementList<TPlacement> : IList<TPlacement> where TPlacement : Placement
	{
		private readonly Placement owner;
		private readonly List<TPlacement> placements = new List<TPlacement>();

		public PlacementList(Placement owner)
		{
			this.owner = owner;
		}

		public IEnumerator<TPlacement> GetEnumerator() => placements.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(TPlacement item)
		{
			CheckBeforeInsertion(item);
			placements.Add(item);
			if (item != null) {
				item.Parent = owner;
			}
		}

		public void Clear()
		{
			foreach (var placement in placements) {
				placement.Parent = null;
			}
			placements.Clear();
		}

		public bool Contains(TPlacement item) => placements.Contains(item);

		public void CopyTo(TPlacement[] array, int arrayIndex) =>
			placements.Select(p => p.Clone()).ToList().CopyTo(array, arrayIndex);

		public bool Remove(TPlacement item)
		{
			if (!placements.Remove(item)) {
				return false;
			}
			item.Parent = null;
			return true;
		}

		public int Count => placements.Count;
		public bool IsReadOnly => false;
		public int IndexOf(TPlacement item) => placements.IndexOf(item);

		public void Insert(int index, TPlacement item)
		{
			CheckBeforeInsertion(item);
			item.Parent = owner;
			placements.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			placements[index].Parent = null;
			placements.RemoveAt(index);
		}

		public TPlacement this[int index]
		{
			get => placements[index];
			set {
				CheckBeforeInsertion(value);
				value.Parent = owner;
				placements[index] = value;
			}
		}

		private static void CheckBeforeInsertion(TPlacement placement)
		{
			if (placement?.Parent != null) {
				throw new System.Exception("Can't adopt a placement twice. Call placement.Unlink() first");
			}
		}
	}

	public class TabBarPlacement : Placement
	{
		[YuzuOptional]
		public PlacementList<PanelPlacement> Placements { get; }

		public TabBarPlacement()
		{
			Placements = new PlacementList<PanelPlacement>(this);
		}

		public override IEnumerable<PanelPlacement> GetPanelPlacements() => Placements;

		public override PanelPlacement FindPanelPlacement(string id) => Placements.FirstOrDefault(p => p.Id == id);

		public override void HideThisOrDescendantPanels()
		{
			foreach (var p in Placements) {
				p.HideThisOrDescendantPanels();
			}
		}

		public override void RemovePlacement(Placement placement)
		{
			if (placement is PanelPlacement panelPlacement) {
				Placements.Remove(panelPlacement);
			}
		}

		public override void RemoveRedundantNodes()
		{
			if (Placements.Count == 0) {
				Unlink();
				return;
			}
			if (Placements.Count == 1) {
				var placement = Placements[0];
				placement.Unlink();
				Parent.Replace(this, placement);
			}
		}

		public override void Replace(Placement old, Placement @new)
		{
			if (old.Parent == this && @new is PanelPlacement newPlacement) {
				Placements[Placements.IndexOf((PanelPlacement)old)] = newPlacement;
			}
		}

		public override bool AnyVisiblePanel() => Placements.Any(p => p.AnyVisiblePanel());

		public override Placement Clone()
		{
			var clone = new TabBarPlacement();
			foreach (var placement in Placements) {
				clone.Placements.Add((PanelPlacement)placement.Clone());
			}
			return clone;
		}
	}

	public class StretchPlacement : Placement
	{
		private Placement placement;

		[YuzuOptional]
		public Placement Placement
		{
			get => placement;
			set {
				if (value?.Parent != null) {
					throw new ArgumentException("Can't adopt a placement twice. Call Unlink() first.");
				}
				if (placement != null) {
					placement.Parent = null;
				}
				if (value == null) {
					placement = null;
					return;
				}
				placement = value;
				value.Parent = this;
			}
		}

		[YuzuOptional]
		public float Stretch { get; set; }

		public StretchPlacement()
		{
		}

		public StretchPlacement(Placement placement, float stretch)
		{
			Placement = placement;
			Stretch = stretch;
		}

		public override IEnumerable<PanelPlacement> GetPanelPlacements()
		{
			return Placement.GetPanelPlacements();
		}

		public override void RemovePlacement(Placement placement)
		{
			if (placement == Placement) {
				Placement = null;
				placement.Parent = null;
			} else {
				placement.RemovePlacement(placement);
			}
		}

		public override PanelPlacement FindPanelPlacement(string id)
		{
			return Placement?.FindPanelPlacement(id);
		}

		public override void HideThisOrDescendantPanels()
		{
			Placement.HideThisOrDescendantPanels();
		}

		public override Placement Clone()
		{
			return new StretchPlacement(Placement?.Clone(), Stretch);
		}

		public override bool AnyVisiblePanel()
		{
			return Placement.AnyVisiblePanel();
		}

		public override void Replace(Placement old, Placement @new)
		{
			if (old == Placement) {
				Placement = @new;
			}
		}

		public override void RemoveRedundantNodes()
		{
			placement?.RemoveRedundantNodes();
			if (placement == null) {
				Unlink();
			}
		}
	}

	public class LinearPlacement : Placement
	{
		[YuzuOptional]
		public PlacementList<StretchPlacement> Placements { get; }

		[YuzuOptional]
		public LinearPlacementDirection Direction { get; set; }

		public LinearPlacement()
		{
			Placements = new PlacementList<StretchPlacement>(this);
		}

		public LinearPlacement(LinearPlacementDirection direction) : this()
		{
			Direction = direction;
		}

		public override IEnumerable<PanelPlacement> GetPanelPlacements()
		{
			foreach (var placement in Placements) {
				foreach (var panelPlacement in placement.GetPanelPlacements()) {
					yield return panelPlacement;
				}
			}
		}

		public override void RemovePlacement(Placement placement)
		{
			if (Placements.Contains(placement)) {
				Placements.Remove((StretchPlacement)placement);
			} else {
				foreach (var stretchPlacement in Placements) {
					stretchPlacement.RemovePlacement(placement);
				}
			}
		}

		public override PanelPlacement FindPanelPlacement(string id)
		{
			foreach (var placement in Placements) {
				var panel = placement.FindPanelPlacement(id);
				if (panel != null) {
					return panel;
				}
			}
			return null;
		}

		public override void HideThisOrDescendantPanels()
		{
			foreach (var placement in Placements) {
				placement.Placement.HideThisOrDescendantPanels();
			}
		}

		public override Placement Clone()
		{
			var placement = new LinearPlacement(Direction);
			foreach (var stretchPlacement in Placements) {
				placement.Placements.Add((StretchPlacement)stretchPlacement.Clone());
			}
			return placement;
		}

		public override bool AnyVisiblePanel()
		{
			bool result = false;
			foreach (var placement in Placements) {
				result |= placement?.AnyVisiblePanel() ?? false;
			}
			return result;
		}

		public override void Replace(Placement old, Placement @new)
		{
			if (old.Parent == this && @new is StretchPlacement newPlacement) {
				Placements[Placements.IndexOf((StretchPlacement)old)] = newPlacement;
			}
			foreach (var placement in Placements) {
				placement.Replace(old, @new);
			}
		}

		public override void RemoveRedundantNodes()
		{
			int index = 0;
			while (index < Placements.Count) {
				var placement = Placements[index];
				placement.RemoveRedundantNodes();
				if (placement.Parent != null) {
					++index;
				}
			}
			if (Placements.Count == 0) {
				Unlink();
			}
			if (Placements.Count == 1 && Parent != null) {
				var placement = Placements[0].Placement;
				placement.Unlink();
				Parent.Replace(this, placement);
			}
		}
	}

	public class RequestedDockingComponent : NodeComponent
	{
		public Rectangle? Bounds { get; set; }
	}

	public class Panel
	{
		public bool IsUndockable { get; private set; }
		public readonly Widget ContentWidget;
		public readonly string Id;

		public string Title;

		public Panel(string id, string title = null, bool undockable = true)
		{
			Id = id;
			Title = title ?? id;
			IsUndockable = undockable;
			ContentWidget = new Frame { Id = "PanelContent", ClipChildren = ClipMethod.ScissorTest, Layout = new StackLayout() };
		}
	}
}
