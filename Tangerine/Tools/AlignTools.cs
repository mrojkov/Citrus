using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.UI;

namespace Tangerine
{
	public enum AlignTo
	{
		Selection,
		Root,
		Parent
	}

	public abstract class AlignToHandler : DocumentCommandHandler
	{
		private readonly ICommand command;

		protected AlignToHandler(ICommand command)
		{
			this.command = command;
		}

		protected static string GetDisplayedTextForAlign(AlignTo align)
		{
			switch (align) {
				case AlignTo.Selection:
					return "Align to Selection";
				case AlignTo.Root:
					return "Align to Root";
				case AlignTo.Parent:
					return "Align to Parent";
				default:
					throw new ArgumentException();
			}
		}

		public static Icon GetIconForAlign(AlignTo align)
		{
			return IconPool.GetIcon($"Tools.{Enum.GetName(typeof(AlignTo), align)}");
		}

		protected void SetTextAndTexture()
		{
			var alignTo = GetAlignTo();
			command.Text = GetDisplayedTextForAlign(alignTo);
			command.Icon = GetIconForAlign(alignTo);
		}

		public abstract AlignTo GetAlignTo();
		public abstract void SetAlignTo(AlignTo alignTo);

		protected class ChangeAlignTo : CommandHandler
		{
			private readonly AlignTo alignTo;
			private readonly AlignToHandler alignToHandler;

			public ChangeAlignTo(AlignTo alignTo, AlignToHandler alignToHandler)
			{
				this.alignTo = alignTo;
				this.alignToHandler = alignToHandler;
			}

			public override void Execute()
			{
				alignToHandler.SetAlignTo(alignTo);
			}
		}
	}

	public class AlignAndDistributeToHandler : AlignToHandler
	{
		public static AlignTo AlignTo = AlignPreferences.Instance.AlignTo;

		public AlignAndDistributeToHandler(ICommand command) : base(command)
		{
		}

		public override AlignTo GetAlignTo() => AlignTo;
		public override void SetAlignTo(AlignTo alignTo)
		{
			AlignTo = alignTo;
			SetTextAndTexture();
		}

		public override void ExecuteTransaction()
		{
			AlignObjectContextMenu.Create(this);
		}

		private static class AlignObjectContextMenu
		{
			public static void Create(AlignToHandler alignToHandler)
			{
				var menu = new Menu();
				var currentAlignObject = alignToHandler.GetAlignTo();
				foreach (AlignTo alignObject in Enum.GetValues(typeof(AlignTo))) {
					menu.Add(new Command(GetDisplayedTextForAlign(alignObject),
						new ChangeAlignTo(alignObject, alignToHandler).Execute) {
						Checked = currentAlignObject == alignObject
					});
				}
				menu.Popup();
			}
		}
	}

	public class CenterToHandler : AlignToHandler
	{
		public static AlignTo CenterAlignTo = AlignTo.Parent;

		public CenterToHandler(ICommand command) : base(command)
		{
		}

		public override AlignTo GetAlignTo() => CenterAlignTo;
		public override void SetAlignTo(AlignTo alignTo)
		{
			CenterAlignTo = alignTo;
			SetTextAndTexture();
		}

		public override void ExecuteTransaction()
		{
			CenterAlignObjectContextMenu.Create(this);
		}

		private static class CenterAlignObjectContextMenu
		{
			public static void Create(AlignToHandler alignToHandler)
			{
				var alignTo = alignToHandler.GetAlignTo();
				new Menu {
					new Command(
						GetDisplayedTextForAlign(AlignTo.Parent),
						new ChangeAlignTo(AlignTo.Parent, alignToHandler).Execute
					) {
						Checked = alignTo == AlignTo.Parent
					},
					new Command(
						GetDisplayedTextForAlign(AlignTo.Root),
						new ChangeAlignTo(AlignTo.Root, alignToHandler).Execute
					) {
						Checked = alignTo == AlignTo.Root
					}
				}.Popup();
			}
		}
	}

	public abstract class AlignTool : DocumentCommandHandler
	{
		protected abstract void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb);
		protected abstract void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb);

		protected static Rectangle NormalizedAABB(Rectangle aabb, Widget container)
		{
			return new Rectangle(aabb.Left / container.Width, aabb.Top / container.Height,
				aabb.Right / container.Width, aabb.Bottom / container.Height);
		}

		protected virtual void ToSelection()
		{
			var container = (Widget)Core.Document.Current.Container;
			if (container == null) {
				return;
			}
			var nodes = Core.Document.Current.SelectedNodes();
			Rectangle aabb;
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				HandleWidgets(container, nodes, aabb);
				HandlePointObjects(container, nodes, NormalizedAABB(aabb, container));
			}
		}

		protected virtual void ToRoot()
		{
			var container = (Widget)Core.Document.Current.RootNode;
			if (container == null) {
				return;
			}
			var nodes = Core.Document.Current.SelectedNodes();
			Rectangle aabb = new Rectangle(0, 0, container.Width, container.Height);
			HandleWidgets(container, nodes, aabb);
			HandlePointObjects(container, nodes, NormalizedAABB(aabb, container));
		}

		protected virtual void ToParent()
		{
			var container = (Widget)Core.Document.Current.Container;
			if (container == null) {
				return;
			}
			var nodes = Core.Document.Current.SelectedNodes();
			Rectangle aabb = new Rectangle(0, 0, container.Width, container.Height);
			HandleWidgets(container, nodes, aabb);
			HandlePointObjects(container, nodes, NormalizedAABB(aabb, container));
		}

		public override void ExecuteTransaction()
		{
			switch (AlignAndDistributeToHandler.AlignTo) {
				case AlignTo.Selection:
					ToSelection();
					break;
				case AlignTo.Root:
					ToRoot();
					break;
				case AlignTo.Parent:
					ToParent();
					break;
				default:
					throw new ArgumentException();
			}
		}
	}

	public abstract class CenterTool : AlignTool
	{
		protected override void ToParent()
		{
			var container = (Widget)Core.Document.Current.Container;
			if (container == null) {
				return;
			}
			var nodes = Core.Document.Current.SelectedNodes();
			Rectangle aabb;
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				HandleWidgets(container, nodes, aabb);
				HandlePointObjects(container, nodes, NormalizedAABB(aabb, container));
			}
		}

		protected override void ToRoot()
		{
			var container = (Widget)Core.Document.Current.RootNode;
			if (container == null) {
				return;
			}
			var nodes = Core.Document.Current.SelectedNodes();
			Rectangle aabb;
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				HandleWidgets(container, nodes, aabb);
				HandlePointObjects(container, nodes, NormalizedAABB(aabb, container));
			}
		}

		public override void ExecuteTransaction()
		{
			switch (CenterToHandler.CenterAlignTo) {
				case AlignTo.Parent:
					ToParent();
					break;
				case AlignTo.Root:
					ToRoot();
					break;
				default:
					throw new ArgumentException();
			}
		}
	}

	public class CenterHorizontally : CenterTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var widget in nodes.Editable().OfType<Widget>()) {
				var p = widget.Position;
				float d = (container.Width - aabb.Width) / 2 - aabb.A.X;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.X += d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var po in nodes.Editable().OfType<PointObject>()) {
				var p = po.Position;
				float d = (1 - aabb.Width) / 2 - aabb.A.X;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.X += d;
					Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
				}
			}
		}
	}

	public class CenterVertically : CenterTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var widget in nodes.Editable().OfType<Widget>()) {
				var p = widget.Position;
				float d = (container.Height - aabb.Height) / 2 - aabb.A.Y;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.Y += d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var po in nodes.Editable().OfType<PointObject>()) {
				var p = po.Position;
				float d = (1 - aabb.Height) / 2 - aabb.A.Y;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.Y += d;
					Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
				}
			}
		}
	}

	public class AlignCentersHorizontally : AlignTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var widget in nodes.Editable().OfType<Widget>()) {
				var p = widget.Position;
				float d = aabb.Center.X - widget.CalcAABBInSpaceOf(container).Center.X;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.X += d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var po in nodes.Editable().OfType<PointObject>()) {
				var p = po.Position;
				if (Mathf.Abs(p.X - aabb.Center.X) > Mathf.ZeroTolerance) {
					p.X = aabb.Center.X;
					Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
				}
			}
		}
	}

	public class AlignCentersVertically : AlignTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var widget in nodes.Editable().OfType<Widget>()) {
				var p = widget.Position;
				float d = aabb.Center.Y - widget.CalcAABBInSpaceOf(container).Center.Y;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.Y += d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var po in nodes.Editable().OfType<PointObject>()) {
				var p = po.Position;
				if (Mathf.Abs(p.Y - aabb.Center.Y) > Mathf.ZeroTolerance) {
					p.Y = aabb.Center.Y;
					Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
				}
			}
		}
	}

	public class AlignTop : AlignTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var widget in nodes.Editable().OfType<Widget>()) {
				var p = widget.Position;
				float d = widget.CalcAABBInSpaceOf(container).Top - aabb.Top;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.Y -= d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var po in nodes.Editable().OfType<PointObject>()) {
				var p = po.Position;
				if (Mathf.Abs(p.Y - aabb.Top) > Mathf.ZeroTolerance) {
					p.Y = aabb.Top;
					Core.Operations.SetAnimableProperty.Perform(po, nameof(PointObject.Position), p, false);
				}
			}
		}
	}

	public class AlignBottom : AlignTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var widget in nodes.Editable().OfType<Widget>()) {
				var p = widget.Position;
				float d = widget.CalcAABBInSpaceOf(container).Bottom - aabb.Bottom;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.Y -= d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var po in nodes.Editable().OfType<PointObject>()) {
				var p = po.Position;
				if (Mathf.Abs(p.Y - aabb.Bottom) > Mathf.ZeroTolerance) {
					p.Y = aabb.Bottom;
					Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
				}
			}
		}
	}

	public class AlignLeft : AlignTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var widget in nodes.Editable().OfType<Widget>()) {
				var p = widget.Position;
				float d = widget.CalcAABBInSpaceOf(container).Left - aabb.Left;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.X -= d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var po in nodes.Editable().OfType<PointObject>()) {
				var p = po.Position;
				if (Mathf.Abs(p.X - aabb.Left) > Mathf.ZeroTolerance) {
					p.X = aabb.Left;
					Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
				}
			}
		}
	}

	public class AlignRight : AlignTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var widget in nodes.Editable().OfType<Widget>()) {
				var p = widget.Position;
				float d = widget.CalcAABBInSpaceOf(container).Right - aabb.Right;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.X -= d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var po in nodes.Editable().OfType<PointObject>()) {
				var p = po.Position;
				if (Mathf.Abs(p.X - aabb.Right) > Mathf.ZeroTolerance) {
					p.X = aabb.Right;
					Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
				}
			}
		}
	}

	public abstract class DistributeTool : AlignTool { }

	public class DistributeCenterVertically : DistributeTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			var widgets = nodes.Editable().OfType<Widget>().OrderBy(w => w.CalcAABBInSpaceOf(container).Center.Y);
			if (widgets.Count() == 0) {
				return;
			}
			float minY = aabb.Top + widgets.First().CalcAABBInSpaceOf(container).Height / 2;
			float maxY = aabb.Bottom - widgets.Last().CalcAABBInSpaceOf(container).Height / 2;
			float step = (maxY - minY) / (widgets.Count() - 1);
			float Y = minY;
			foreach (var widget in widgets) {
				var p = widget.Position;
				float d = p.Y - Y;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.Y -= d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
				Y += step;
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			var objects = nodes.Editable().OfType<PointObject>().OrderBy(po => po.Position.Y);
			float step = (aabb.Bottom - aabb.Top) / (objects.Count() - 1);
			float Y = aabb.Top;
			foreach (var obj in objects) {
				var p = obj.Position;
				if (Mathf.Abs(p.Y - Y) > Mathf.ZeroTolerance) {
					p.Y = Y;
					Core.Operations.SetAnimableProperty.Perform(obj, nameof(PointObject.Position), p, false);
				}
				Y += step;
			}
		}
	}

	public class DistributeCenterHorizontally : DistributeTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			var widgets = nodes.Editable().OfType<Widget>().OrderBy(w => w.CalcAABBInSpaceOf(container).Center.X);
			if (widgets.Count() == 0) {
				return;
			}
			float minX = aabb.Left + widgets.First().CalcAABBInSpaceOf(container).Width / 2;
			float maxX = aabb.Right - widgets.Last().CalcAABBInSpaceOf(container).Width / 2;
			float step = (maxX - minX) / (widgets.Count() - 1);
			float X = minX;
			foreach (var widget in widgets) {
				var p = widget.Position;
				float d = p.X - X;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.X -= d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
				X += step;
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			var objects = nodes.Editable().OfType<PointObject>().OrderBy(po => po.Position.X);
			float step = (aabb.Right - aabb.Left) / (objects.Count() - 1);
			float X = aabb.Left;
			foreach (var obj in objects) {
				var p = obj.Position;
				if (Mathf.Abs(p.X - X) > Mathf.ZeroTolerance) {
					p.X = X;
					Core.Operations.SetAnimableProperty.Perform(obj, nameof(PointObject.Position), p, false);
				}
				X += step;
			}
		}
	}

	public class DistributeLeft : DistributeTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			var widgets = nodes.Editable().OfType<Widget>().OrderBy(w => w.CalcAABBInSpaceOf(container).Left);
			if (widgets.Count() == 0) {
				return;
			}
			float minX = aabb.Left;
			float maxX = aabb.Right - widgets.Last().CalcAABBInSpaceOf(container).Width;
			float step = (maxX - minX) / (widgets.Count() - 1);
			float X = minX;
			foreach (var widget in widgets) {
				var p = widget.Position;
				float d = p.X - widget.CalcAABBInSpaceOf(container).Width / 2 - X;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.X -= d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
				X += step;
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			var objects = nodes.Editable().OfType<PointObject>().OrderBy(po => po.Position.X);
			float step = (aabb.Right - aabb.Left) / (objects.Count() - 1);
			float X = aabb.Left;
			foreach (var obj in objects) {
				var p = obj.Position;
				if (Mathf.Abs(p.X - X) > Mathf.ZeroTolerance) {
					p.X = X;
					Core.Operations.SetAnimableProperty.Perform(obj, nameof(PointObject.Position), p, false);
				}
				X += step;
			}
		}
	}

	public class DistributeRight : DistributeTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			var widgets = nodes.Editable().OfType<Widget>().OrderBy(w => w.CalcAABBInSpaceOf(container).Right);
			if (widgets.Count() == 0) {
				return;
			}
			float minX = aabb.Left + widgets.First().CalcAABBInSpaceOf(container).Width;
			float maxX = aabb.Right;
			float step = (maxX - minX) / (widgets.Count() - 1);
			float X = minX;
			foreach (var widget in widgets) {
				var p = widget.Position;
				float d = p.X + widget.CalcAABBInSpaceOf(container).Width / 2 - X;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.X -= d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
				X += step;
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			var objects = nodes.Editable().OfType<PointObject>().OrderBy(po => po.Position.X);
			float step = (aabb.Right - aabb.Left) / (objects.Count() - 1);
			float X = aabb.Left;
			foreach (var obj in objects) {
				var p = obj.Position;
				if (Mathf.Abs(p.X - X) > Mathf.ZeroTolerance) {
					p.X = X;
					Core.Operations.SetAnimableProperty.Perform(obj, nameof(PointObject.Position), p, false);
				}
				X += step;
			}
		}
	}

	public class DistributeTop : DistributeTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			var widgets = nodes.Editable().OfType<Widget>().OrderBy(w => w.CalcAABBInSpaceOf(container).Top);
			if (widgets.Count() == 0) {
				return;
			}
			float minY = aabb.Top;
			float maxY = aabb.Bottom - widgets.Last().CalcAABBInSpaceOf(container).Height;
			float step = (maxY - minY) / (widgets.Count() - 1);
			float Y = minY;
			foreach (var widget in widgets) {
				var p = widget.Position;
				float d = p.Y - widget.CalcAABBInSpaceOf(container).Height / 2 - Y;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.Y -= d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
				Y += step;
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			var objects = nodes.Editable().OfType<PointObject>().OrderBy(po => po.Position.Y);
			float step = (aabb.Bottom - aabb.Top) / (objects.Count() - 1);
			float Y = aabb.Top;
			foreach (var obj in objects) {
				var p = obj.Position;
				if (Mathf.Abs(p.Y - Y) > Mathf.ZeroTolerance) {
					p.Y = Y;
					Core.Operations.SetAnimableProperty.Perform(obj, nameof(PointObject.Position), p, false);
				}
				Y += step;
			}
		}
	}

	public class DistributeBottom : DistributeTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			var widgets = nodes.Editable().OfType<Widget>().OrderBy(w => w.CalcAABBInSpaceOf(container).Bottom);
			if (widgets.Count() == 0) {
				return;
			}
			float minY = aabb.Top + widgets.First().CalcAABBInSpaceOf(container).Height;
			float maxY = aabb.Bottom;
			float step = (maxY - minY) / (widgets.Count() - 1);
			float Y = minY;
			foreach (var widget in widgets) {
				var p = widget.Position;
				float d = p.Y + widget.CalcAABBInSpaceOf(container).Height / 2 - Y;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.Y -= d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
				Y += step;
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			var objects = nodes.Editable().OfType<PointObject>().OrderBy(po => po.Position.Y);
			float step = (aabb.Bottom - aabb.Top) / (objects.Count() - 1);
			float Y = aabb.Top;
			foreach (var obj in objects) {
				var p = obj.Position;
				if (Mathf.Abs(p.Y - Y) > Mathf.ZeroTolerance) {
					p.Y = Y;
					Core.Operations.SetAnimableProperty.Perform(obj, nameof(PointObject.Position), p, false);
				}
				Y += step;
			}
		}
	}
}
