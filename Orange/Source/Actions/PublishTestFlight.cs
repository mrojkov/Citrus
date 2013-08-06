using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Web;
using RestSharp;
using System.ComponentModel;

namespace Orange
{	
	public static class PublishTestFlight
	{
		[MenuItem("Publish to TestFlight")]
		public static void PublishTestFlightAction()
		{
			string notifyX = The.Workspace.ProjectJson["TestFlight/Notify"] as string;
			Console.WriteLine("XXXXXXXXXXXXXXXXX '{0}'", notifyX);
			AssetCooker.CookForActivePlatform();
			// XXX
			// if (Actions.BuildGame()) {
			Actions.BuildGame();
			if (true) {
				var apiToken = The.Workspace.ProjectJson["TestFlight/ApiToken"];
				var teamToken = The.Workspace.ProjectJson["TestFlight/TeamToken"];
				var notes = The.Workspace.ProjectJson["BuildNotes"];

				var ipaPath = GetIPAPath();
				Console.WriteLine("Uploading " + ipaPath);

				var testflight = new RestClient("http://testflightapp.com");
				var uploadRequest = new RestRequest("api/builds.json", Method.POST);
				uploadRequest.AddParameter("api_token", apiToken);
				uploadRequest.AddParameter("team_token", teamToken);
				uploadRequest.AddParameter("notes", notes);
				bool notify = The.Workspace.ProjectJson.GetBool("TestFlight/Notify");
				if (notify) {
					uploadRequest.AddParameter("notify", "true");
					uploadRequest.AddParameter("distribution_lists", The.Workspace.ProjectJson.GetBool("DistributionLists"));
				}
				uploadRequest.AddFile("file", ipaPath);

				IRestResponse response = null;
				var bw = new BackgroundWorker();
				bw.DoWork += (s, e) => {
					response = testflight.Execute(uploadRequest);
				};
				bw.RunWorkerCompleted += (s, e) => {
					if (e.Error != null) {
						Console.WriteLine(e.Error);
					}
				};
				bw.RunWorkerAsync();
				WaitWhileBusy(bw);
				if (response != null) {
					if (response.StatusCode == HttpStatusCode.OK) {
						Console.WriteLine("Build uploaded to TestFlight");
					} else {
						Console.WriteLine("Build not uploaded, testflight returned error");
					}
					Console.WriteLine(response.Content);
				}
			}
		}

		private static void WaitWhileBusy(BackgroundWorker worker)
		{
			int i = 0;
			while (worker.IsBusy) {
				if (++i % 40 == 0) {
					Console.Write("\n");
				}
				Console.Write(" . ");
				for (int j = 0; j < 10; j++) {
					System.Threading.Thread.Sleep(100);
					The.UI.ProcessPendingEvents();
				}
			}
			Console.Write("\n");
		}

		private static string GetIPAPath()
		{
			var directory = Path.Combine(Path.GetDirectoryName(The.Workspace.GetSolutionFilePath()), "bin", "iPhone", "Release");
			var dirInfo = new DirectoryInfo(directory);
			var allFiles = dirInfo.EnumerateFiles("*.ipa").ToList();
			allFiles.Sort((a, b) => b.CreationTime.CompareTo(a.CreationTime));
			if (allFiles.Count > 0) {
				var ipaPath = Path.Combine(directory, allFiles[0].FullName);
				return ipaPath;
			}
			return null;
		}
	}
}
