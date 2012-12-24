using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lime;

namespace Kumquat
{

	public class CodeGenerator
	{
		private string Directory;
		private string ProjectName;
		private Dictionary<string, Frame> Locations;
		List<string> lines = new List<string>();

		public CodeGenerator(string directory, string projectName, Dictionary<string, Frame> locations)
		{
			Directory = directory;
			ProjectName = projectName;
			Locations = locations;
		}

		private delegate void BodyDelegate();

		public void Start()
		{
			var sourcePath = String.Format(@"{0}\{1}.Game\Source\", Directory, ProjectName);
			var generatedPath = sourcePath + "Generated";
			var locationsPath = sourcePath + "Locations";

			AddLocationCollectionHeader();
			foreach (string locPath in Locations.Keys) {
				var name = Path.GetFileNameWithoutExtension(locPath);
				var line = "\t\tpublic Location NAME { get { return Dictionary[\"NAME\"]; } }";
				lines.Add(line.Replace("NAME", name));
			}
			AddFooter();
			System.IO.File.WriteAllLines(generatedPath + "\\LocationCollection.cs", lines);

			foreach (var loc in Locations) {
				var className = Path.GetFileNameWithoutExtension(loc.Key);
				if (!File.Exists(locationsPath + "\\" + className + ".cs"))
					CreateLocationFile(locationsPath, className);

				var frame = loc.Value;
				CreateLocationFile(generatedPath, className, () => {
					var line = "\t\tpublic Item @NAME { get { return Items[\"NAME\"]; } }";
					HashSet<string> names = new HashSet<string>();
					foreach (Area area in frame.Descendants<Area>()) {
						if (!names.Contains(area.Id)) {
							names.Add(area.Id);
							lines.Add(line.Replace("NAME", area.Id));
						} else {
							Console.WriteLine("WARNING: Duplicate '{0}' on '{1}'", area.Id, className);
						}
					}
				});
			}

		}

		private void CreateLocationFile(string path, string className, BodyDelegate bodyFunc = null)
		{
			lines.Clear();
			if (className == "LocationCollection") {
				AddLocationCollectionHeader();
			} else {
				AddLocationHeader(className);
			}
			if( bodyFunc != null )
				bodyFunc();
			AddFooter();
			System.IO.File.WriteAllLines(path + "\\" + className + ".cs", lines);
		}

		private void AddLocationCollectionHeader()
		{
			var header = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PROJECT_NAME
{
	public partial class LocationCollection
	{";
			lines.Add(header.Replace("PROJECT_NAME", ProjectName));
		}

		private void AddLocationHeader(string className)
		{
			var header = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;
using Kumquat;
using ProtoBuf;

namespace PROJECT_NAME.Locations
{
	using R = IEnumerator<object>;

	[ProtoContract]
	public partial class CLASS_NAME : Location
	{";
			header = header.Replace("PROJECT_NAME", ProjectName);
			header = header.Replace("CLASS_NAME", className);
			lines.Add(header);
		}

		private void AddFooter()
		{
			lines.Add("\t}");
			lines.Add("}");
		}

	}

}
