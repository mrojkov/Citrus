using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Orange.Source
{
	abstract class BuildSystem
	{
		protected string BuilderPath = "";
		protected string SlnPath;
		protected string Args = "";
		protected TargetPlatform Platform;
		public string Configuration;

		private Dictionary<TargetPlatform, string> fileEndings = new Dictionary<TargetPlatform, string> {
#if WIN
			{TargetPlatform.Desktop, ".Win.sln"},
#elif MAC
			{TargetPlatform.Desktop, ".Mac.sln"},
#endif
			{TargetPlatform.Android, ".Android.csproj"},
			{TargetPlatform.iOS, ".iOS.sln"}
		};
		
		public BuildSystem(string projectDirectory, string projectName, TargetPlatform platform, string customSolution)
		{
			Platform = platform;

            if (customSolution != null)
                SlnPath = Path.Combine(projectDirectory, customSolution);
            else
                SlnPath = Path.Combine(projectDirectory, projectName + fileEndings[platform]);
		}

		public abstract int Execute(StringBuilder output = null);
		protected abstract void DecorateBuild();
		protected abstract void DecorateClean();
		protected abstract void DecorateConfiguration();
		
		public void PrepareForBuild()
		{
			DecorateBuild();
			DecorateConfiguration();
		}

		public void PrepareForClean()
		{
			DecorateClean();
			DecorateConfiguration();
		}
	}
}
