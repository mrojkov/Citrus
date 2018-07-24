using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;

namespace Tangerine.UI.SceneView.Processors
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
					if (grid.HitTest(mousePosition, sv.Scene)) {
						foreach (var line in grid.Lines) {
							if (line.HitTest(mousePosition, sv.Scene)) {
								Utils.ChangeCursorIfDefault(MouseCursor.Hand);
								yield return null;
								goto Next;
							}
						}
					}
				}
				Next:
				yield return null;
			}
		}
	}
}
