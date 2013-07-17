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
			var apiToken = The.Workspace.GetProjectAttribute("TestFlightApiToken");
			var teamToken = The.Workspace.GetProjectAttribute("TestFlightTeamToken");
			var notes = The.Workspace.GetProjectAttribute("BuildNotes");

			var ipaPath = GetIPAPath();
			Console.WriteLine("Uploading " + ipaPath);

			var testflight = new RestClient("http://testflightapp.com");
			var uploadRequest = new RestRequest("api/builds.json", Method.POST);
			uploadRequest.AddParameter("api_token", apiToken);
			uploadRequest.AddParameter("team_token", teamToken);
			uploadRequest.AddParameter("notes", notes);
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
					Toolbox.ProcessPendingEvents();
				}
			}
			Console.Write("\n");
		}

		private static string GetIPAPath()
		{
			var ipa = "F:\\Work\\Zx3\\Kill3.iOS\\bin\\iPhone\\Release\\ZZZ-6.32_NoData.ipa";
			//var ipa = Path.Combine(Path.GetDirectoryName(The.Workspace.GetSolutionFilePath()), "bin", "iPhone", "Release", "ZZZ-6.32.ipa");
			return ipa;
		}
	}
}
