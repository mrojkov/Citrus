using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.UI.SceneView.Presenters
{
	class NineGridLinePresenter
	{
		public NineGridLinePresenter(SceneView sceneView)
		{
			sceneView.Frame.CompoundPostPresenter.Add(new DelegatePresenter<Widget>(Render));
		}

		private void Render(Widget canvas)
		{
			var grids = Core.Document.Current.SelectedNodes().Editable().OfType<NineGrid>();
			foreach (var grid in grids) {
				foreach (var line in NineGridLine.GetForNineGrid(grid)) {
					line.Render(canvas);
				}
			}
		}
	}
}
