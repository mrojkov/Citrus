using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine
{
	public enum AlignObject
	{
		Selection,
		KeyObject,
		Root,
		Parent
	}

	public abstract class AlignObjectHandler : DocumentCommandHandler
	{
		readonly ICommand command;

		public AlignObjectHandler(ICommand command)
		{
			this.command = command;
		}

		protected static string AlignToString(AlignObject align)
		{
			switch (align) {
				case AlignObject.Selection:
					return "Align to Selection";
				case AlignObject.KeyObject:
					return "Align to Key Object";
				case AlignObject.Root:
					return "Align to Root";
				case AlignObject.Parent:
					return "Align to Parent";
				default:
					throw new ArgumentException();
			}
		}

		public static ITexture AlignToTexture(AlignObject align)
		{
			return IconPool.GetTexture($"Tools.{Enum.GetName(typeof(AlignObject), align)}");
		}

		protected void SetTextAndTexture()
		{
			var alignObject = GetAlignObject();
			command.Text = AlignToString(alignObject);
			command.Icon = AlignToTexture(alignObject);
		}

		public abstract AlignObject GetAlignObject();
		public abstract void SetAlignObject(AlignObject alignObject);

		protected class ChangeAlignObject : CommandHandler
		{
			private readonly AlignObject alignObject;
			private readonly AlignObjectHandler alignToHandler;

			public ChangeAlignObject(AlignObject alignObject, AlignObjectHandler alignToHandler)
			{
				this.alignObject = alignObject;
				this.alignToHandler = alignToHandler;
			}

			public override void Execute()
			{
				alignToHandler.SetAlignObject(alignObject);
			}
		}
	}

	public class AlignAndDistributeObjectHandler : AlignObjectHandler
	{
		public static AlignObject AlignObject = AlignPreferences.Instance.AlignObject;

		public AlignAndDistributeObjectHandler(ICommand command) : base(command)
		{
		}

		public override AlignObject GetAlignObject() => AlignObject;
		public override void SetAlignObject(AlignObject alignObject)
		{
			AlignObject = alignObject;
			SetTextAndTexture();
		}

		public override void ExecuteTransaction()
		{
			AlignObjectContextMenu.Create(this);
		}

		private static class AlignObjectContextMenu
		{
			public static void Create(AlignObjectHandler alignToHandler)
			{
				var menu = new Menu();
				var curAlignObject = alignToHandler.GetAlignObject();
				foreach (AlignObject alignObject in Enum.GetValues(typeof(AlignObject))) {
					menu.Add(new Command(AlignToString(alignObject),
						new ChangeAlignObject(alignObject, alignToHandler).Execute) {
						Checked = curAlignObject == alignObject
					});
				}
				menu.Popup();
			}
		}
	}

	public class CenterObjectHandler : AlignObjectHandler
	{
		public static AlignObject CenterAlignObject = AlignObject.Parent;

		public CenterObjectHandler(ICommand command) : base(command)
		{
		}

		public override AlignObject GetAlignObject() => CenterAlignObject;
		public override void SetAlignObject(AlignObject alignObject)
		{
			CenterAlignObject = alignObject;
			SetTextAndTexture();
		}

		public override void ExecuteTransaction()
		{
			CenterAlignObjectContextnMenu.Create(this);
		}

		private static class CenterAlignObjectContextnMenu
		{
			public static void Create(AlignObjectHandler alignToHandler)
			{
				var alignObject = alignToHandler.GetAlignObject();
				new Menu {
					new Command(AlignToString(AlignObject.Parent),
						new ChangeAlignObject(AlignObject.Parent, alignToHandler).Execute)
					{
						Checked = alignObject == AlignObject.Parent
					},
					new Command(AlignToString(AlignObject.Root),
						new ChangeAlignObject(AlignObject.Root, alignToHandler).Execute)
					{
						Checked = alignObject == AlignObject.Root
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

		protected virtual void ToKeyObject()
		{
			throw new NotImplementedException();
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
			switch (AlignAndDistributeObjectHandler.AlignObject) {
				case AlignObject.Selection:
					ToSelection();
					break;
				case AlignObject.KeyObject:
					ToKeyObject();
					break;
				case AlignObject.Root:
					ToRoot();
					break;
				case AlignObject.Parent:
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
			switch (CenterObjectHandler.CenterAlignObject) {
				case AlignObject.Parent:
					ToParent();
					break;
				case AlignObject.Root:
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
