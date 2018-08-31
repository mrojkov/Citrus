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

	public enum LinearPlacementDirection
	{
		Vertical,
		Horizontal
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
			target.Placements.Add(placement);
			target.Stretches.Add(stretch);
			Panels.Add(panel);
			return placement;
		}

		private static void DockPlacementTo(TabBarPlacement placement, TabBarPlacement target)
		{
			var placements = placement.Placements.ToList();
			foreach (var panelPlacement in placements) {
				panelPlacement.Unlink();
				target.Placements.Add(panelPlacement);
			}
		}

		private static void DockPlacementTo(TabBarPlacement placement, PanelPlacement target)
		{
			var parent = target.Parent;
			if (parent is TabBarPlacement tabBarPlacement) {
				DockPlacementTo(placement, tabBarPlacement);
				return;
			}
			if (parent is LinearPlacement linearPlacement) {
				int index = linearPlacement.Placements.IndexOf(target);
				target.Parent = null;
				placement.Placements.Add(target);
				linearPlacement.Placements[index] = placement;
			}
		}

		private static void DockPlacementTo(PanelPlacement placement, PanelPlacement target)
		{
			if (!(target.Parent is TabBarPlacement tabBarPlacement)) {
				tabBarPlacement = new TabBarPlacement();
				var parent = (LinearPlacement)target.Parent;
				target.Parent = null;
				int index = parent.Placements.IndexOf(target);
				tabBarPlacement.Placements.Add(target);
				parent.Placements[index] = tabBarPlacement;
			}
			tabBarPlacement.Placements.Add(placement);
		}

		private void DockPlacementTo(LinearPlacement placement, PanelPlacement target)
		{
			var parent = target.Parent;
			if (parent is TabBarPlacement tabBarPlacement) {
				DockPlacementTo(placement, tabBarPlacement);
			}
			if (parent is LinearPlacement targetLinearPlacement) {
				DockPlacementTo(placement, targetLinearPlacement);
			}
		}

		private void DockPlacementTo(LinearPlacement placement, TabBarPlacement target)
		{
			var placements = placement.Placements.ToList();
			foreach (var p in placements) {
				DockPlacementTo(p, target, DockSite.Fill, 0);
			}
		}

		private static void DockPlacementTo(LinearPlacement placement, LinearPlacement target, int index = 0, float stretch = 0)
		{
			if (placement.Direction == target.Direction) {
				var placements = placement.Placements.ToList();
				var stretches = placement.Stretches;
				for (int i = placements.Count - 1; i >= 0; --i) {
					target.Placements.Insert(index, placements[i]);
					target.Stretches.Insert(index, stretches[i] * stretch);
				}
				return;
			}
			if (placement.Placements.Count == 1) {
				var p = placement.Placements[0];
				p.Unlink();
				target.Stretches.Insert(index, stretch);
				target.Placements.Insert(index, p);
				return;
			}
			target.Placements.Insert(index, placement);
			target.Stretches.Insert(index, stretch);
		}

		private static bool CheckPlacement(LinearPlacement placement, DockSite site, out LinearPlacement newPlacement)
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

		private static float AdjustStretch(LinearPlacement linearPlacement, float stretch, int index)
		{
			if (stretch >= 0) {
				return stretch;
			}
			linearPlacement.Stretches[index] *= 0.75f;
			return linearPlacement.Stretches[index] * 0.25f;
		}

		private static void ReplacePlacement(Placement old, LinearPlacement @new, DockSite site, ref float stretch, ref int index)
		{
			var placement = old;
			index = site == DockSite.Bottom || site == DockSite.Right ? 1 : 0;
			stretch = stretch < 0 ? 0.25f : stretch;
			if (old.Parent != null) {
				var parentLinearPlacement = (LinearPlacement)old.Parent;
				parentLinearPlacement.Replace(old, @new);
			} else {
				placement = ((LinearPlacement)old).Placements[0];
				old.Replace(placement, @new);
			}
			@new.Placements.Add(placement);
			@new.Stretches.Add(1 - stretch);
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
								targetLinearPlacement.Placements.Add(placement);
								targetLinearPlacement.Stretches.Add(
									AdjustStretch(targetLinearPlacement, stretch, targetLinearPlacement.Placements.Count - 1));
								break;
						}
					}
				);
				return;
			}
			int index = 0;
			if (!(target is LinearPlacement parent)) {
				if (target.Parent is TabBarPlacement parentTabBarPlacement) {
					target = parentTabBarPlacement;
				}
				if (!CheckPlacement((LinearPlacement)target.Parent, site, out parent)) {
					ReplacePlacement(target, parent, site, ref stretch, ref index);
				}
				else {
					index = parent.Placements.IndexOf(target);
					stretch = AdjustStretch(parent, stretch, index);
					if (site == DockSite.Right || site == DockSite.Bottom) {
						++index;
					}
				}
			} else {
				if (!CheckPlacement(parent, site, out var newPlacement)) {
					ReplacePlacement(parent, newPlacement, site, ref stretch, ref index);
				} else {
					if (site == DockSite.Right || site == DockSite.Bottom) {
						index = parent.Placements.Count;
					}
					stretch = AdjustStretch(parent, stretch, index);
				}
			}
			if (placement is LinearPlacement lPlacement) {
				DockPlacementTo(lPlacement, parent, index, stretch);
			} else {
				parent.Placements.Insert(index, placement);
				parent.Stretches.Insert(index, stretch);
			}
		}

		public void AddWindow(WindowPlacement placement) => WindowPlacements.Add(placement);

		public static void HideWindowPanels(WindowPlacement windowPlacement)
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

		public Vector2 CalcGlobalSize()
		{
			var result = Vector2.One;
			if (Parent == null) {
				return result;
			}
			Parent.SwitchType(onLinearPlacement: linearPlacement => {
				float stretch = linearPlacement.Stretches[linearPlacement.Placements.IndexOf(this)];
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
			for (int i = 0; i < Placements.Count; ++i) {
				result.Placements.Add(Placements[i].Clone());
				result.Stretches.Add(Stretches[i]);
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

	public class LinearPlacement : Placement
	{
		[YuzuOptional]
		public PlacementList<Placement> Placements { get; }

		[YuzuOptional]
		public List<float> Stretches { get; }

		[YuzuOptional]
		public LinearPlacementDirection Direction { get; set; }

		public LinearPlacement()
		{
			Placements = new PlacementList<Placement>(this);
			Stretches = new List<float>();
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
				int index = Placements.IndexOf(placement);
				Placements.RemoveAt(index);
				Stretches.RemoveAt(index);
				NormalizeStretches();
			} else {
				foreach (var p in Placements) {
					p.RemovePlacement(placement);
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
				placement.HideThisOrDescendantPanels();
			}
		}

		public override Placement Clone()
		{
			var result = new LinearPlacement(Direction);
			for (int i = 0; i < Placements.Count; ++i) {
				result.Placements.Add(Placements[i].Clone());
				result.Stretches.Add(Stretches[i]);
			}
			return result;
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
			if (old.Parent == this) {
				old.Parent = null;
				Placements[Placements.IndexOf(old)] = @new;
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
				var placement = Placements[0];
				placement.Unlink();
				Parent.Replace(this, placement);
			}
		}

		public void NormalizeStretches()
		{
			if (Stretches.Count == 0) {
				return;
			}
			float total = Stretches.Aggregate((v1, v2) => v1 + v2);
			for (int i = 0; i < Stretches.Count; ++i) {
				Stretches[i] /= total;
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
