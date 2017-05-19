using Orange.FbxImporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Orange
{
	public partial class FbxModelImporter
	{
		private string path;
		public Scene scene;

		public FbxModelImporter(string path, TargetPlatform platform)
		{
			scene = Manager.Instance.LoadScene(path);
		}
	}
}
