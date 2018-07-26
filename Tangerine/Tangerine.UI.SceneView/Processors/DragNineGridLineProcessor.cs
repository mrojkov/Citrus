using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	class DragNineGridLineProcessor : ITaskProvider
	{
		static SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				var grids = Document.Current.SelectedNodes().Editable().OfType<NineGrid>();
				var mousePosition = sv.MousePosition;
				foreach (var grid in grids) {
					foreach (var line in NineGridLine.GetForNineGrid(grid)) {
						if (line.HitTest(mousePosition, sv.Scene)) {
							Utils.ChangeCursorIfDefault(MouseCursor.Hand);
							if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
								yield return Drag(line);
							}
							goto Next;
						}
					}
				}
				Next:
				yield return null;
			}
		}

		private IEnumerator<object> Drag(NineGridLine nineGridLine)
		{
			var transform = sv.Scene.CalcTransitionToSpaceOf(Document.Current.Container.AsWidget);
			var initMousePos = sv.MousePosition * transform;
			var dir = nineGridLine.GetDirection(sv.Scene);
			float value = nineGridLine.Value;
			var nineGrid = nineGridLine.Owner;
			var propertyName = nineGridLine.PropertyName;
			var maxValue = nineGridLine.MaxValue;
			var size = nineGridLine.TextureSize;
			float clipTolerance = 15 / size;
			float[] clipPositions = { 0, maxValue, maxValue / 3, maxValue / 3 * 2 };
			using(Document.Current.History.BeginTransaction()) {
				while (sv.Input.IsMousePressed()) {
					Document.Current.History.RollbackTransaction();

					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					var curMousePos = sv.MousePosition * transform;
					var diff = Vector2.DotProduct((curMousePos - initMousePos), dir) / size;
					if (Mathf.Abs(diff) > Mathf.ZeroTolerance) {
						float newValue = value + diff;
						if (sv.Input.IsKeyPressed(Key.Shift)) {
							foreach (var origin in clipPositions) {
								newValue = newValue.Snap(origin, clipTolerance);
							}
						}
						Core.Operations.SetAnimableProperty.Perform(nineGrid,
							propertyName, newValue, CoreUserPreferences.Instance.AutoKeyframes);
					}
					yield return null;
				}
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.CommitTransaction();
			}
		}
	}
}
