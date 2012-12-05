using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;

namespace Kumquat
{
	public class GameScreen : Frame
	{
		public static GameScreen Instance;

		public Location CurrentLocation = null;

		public GameScreen(string name)
			: base(name)
		{
			Instance = this;
		}

		public void OpenLocation(string name)
		{
			var locationPlace = this["LocationPlace"];
			locationPlace.Nodes.Clear();

			CurrentLocation = Location.Create(name);
			locationPlace.AddNode(CurrentLocation);
		}

		public void GoToLocation(string name)
		{
			CurrentLocation = null;

			Frame fade = (Frame)this["Fade"];
			if (fade.CurrentAnimation != "Show")
				fade.RunAnimation("Show");

			fade.AnimationStopped += () => {
				OpenLocation(name);
				fade.RunAnimation("Hide");
			};
		}

	}
}
