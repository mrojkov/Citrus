using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Orange
{
	static class UpgradeOrange
	{
		[MenuItem("Upgrade Orange")]
		public static void UpgradeOrangeAction()
		{
			var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Toolbox.GetApplicationDirectory())));
#if MAC
			path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(path)));
#endif
			var builder = new SolutionBuilder(TargetPlatform.Desktop, path, "Orange");
			builder.SvnUpdate();
#if WIN
			CsprojSynchronization.SynchronizeProject(Path.Combine(path, "Orange.Win.csproj"));
#elif MAC
			CsprojSynchronization.SynchronizeProject(Path.Combine(path, "Orange.Mac.csproj"));
#endif
			if (builder.Build()) {
				Subversion.Commit(path, "");
			}
		}
	}
}
