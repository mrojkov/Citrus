using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;

namespace Kumquat
{
    public class Location : Frame
	{
		#region static

		public new static Location Create(string name)
		{
			Location location = null;
			Type type = Type.GetType("RoyalCandy.Locations." + name);
			if (type == null) {
				location = new Location();
			} else {
				location = (Location)type.InvokeMember("new", System.Reflection.BindingFlags.CreateInstance, null, null, null);
			}
			location.LoadContent(name);
			location.OnCreate();
			return location;
		}

		private static string GetPath(string name)
		{
			return String.Format("Locations/{0}/{1}.scene", name, name);
		}

		public static bool Exist(string name)
		{
			return AssetsBundle.Instance.FileExists(GetPath(name));
		}

		#endregion

		private void LoadContent(string name)
		{
			var frame = new Frame(GetPath(name));
			this.Nodes.Add(frame);
		}

		public virtual void OnCreate()
		{
		}

		public virtual void ExitAreaClick(ExitArea exitArea) {
			var name = exitArea.ExitTo;
			if (Location.Exist(name)) {
				GameScreen.Instance.GoToLocation(name);
			} else {
				Console.WriteLine("No loсation: " + name);
			}
		}

    }
}
