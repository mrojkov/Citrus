using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Yuzu;

namespace Tangerine.UI.FilesystemView
{
	public class FilesystemUserPreferences : Component
	{
		[YuzuRequired]
		public Dictionary<string, ViewNode> ViewRootPerProjectFile = new Dictionary<string, ViewNode>();

		public ViewNode ViewRoot
		{
			get
			{
				var path = Project.Current.CitprojPath ?? "";
				if (!ViewRootPerProjectFile.ContainsKey(path)) {
					ViewRootPerProjectFile.Add(path, new FSViewNode());
				}
				return ViewRootPerProjectFile[path];
			}
			set
			{
				ViewRootPerProjectFile[Project.Current.CitprojPath ?? ""] = value;
			}
		}

		public static FilesystemUserPreferences Instance => Core.UserPreferences.Instance.Get<FilesystemUserPreferences>();
	}
}