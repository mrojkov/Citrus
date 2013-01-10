using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ProtoBuf;
using Qyoto;

namespace Tangerine
{
	using RecentProjectList = List<RecentProjectAction>;

	public class RecentProjectsManager
	{
		public static RecentProjectsManager Instance = new RecentProjectsManager();

		public string dataFile = Tangerine.GetDataFilePath("RecentProjects.dat");
		public RecentProjectList RecentProjects = new RecentProjectList();

		public void Initialize()
		{
			if (File.Exists(dataFile)) {
				Lime.Serialization.ReadObjectFromFile<RecentProjectList>(dataFile, RecentProjects);
				//RefreshMenu();
			}
		}

		public void Add(string file)
		{
			AddHelper(file);
			Save();
			RefreshMenu();
		}

		public string GetFirstItem()
		{
			if (RecentProjects.Count > 0) {
				return RecentProjects[0].Text;
			}
			return null;
		}

		private void AddHelper(string file)
		{
			RecentProjectAction action;
			var i = RecentProjects.FindIndex(p => p.ProjectFile == file);
			if (i < 0) {
				action = new RecentProjectAction() { ProjectFile = file };
			} else {
				action = RecentProjects[i];
				RecentProjects.RemoveAt(i);
			}
			RecentProjects.Insert(0, action);
		}

		private void RefreshMenu()
		{
			RecentProjectsMenu.Instance.Clear();
			foreach (var project in RecentProjects) {
				RecentProjectsMenu.Instance.Add(project);
			}
		}

		public void Save()
		{
			Lime.Serialization.WriteObjectToFile<RecentProjectList>(dataFile, RecentProjects);
		}
	}

	[ProtoContract]
	public class RecentProjectAction : Action
	{
		[ProtoMember(1)]
		public string ProjectFile { get { return Text; } set { Text = value; } }

		protected override void OnTriggered()
		{
			OpenProjectAction.Instance.OpenProject(Text);
		}
	}
}