using System;

public class Application
{
	[STAThread]
	public static void Main(string[] args)
	{
		if (Array.IndexOf(args, "--GenerateSerializationAssembly") >= 0) {
			Lime.Environment.GenerateSerializationAssembly("Serializer", typeof(EmptyProject.ApplicationData));
			return;
		}

		//Lime.Serialization.Serializer = new Serializer();

		var options = new Lime.Application.StartupOptions();
		options.DecodeAudioInSeparateThread = false;

		var app = new EmptyProject.Application(args, options);
		using (var gameView = new Lime.GameView(app)) {
			bool _30FPS = Array.IndexOf(args, "--30FPS") >= 0;
			double fps = _30FPS ? 30 : 60;
			gameView.Run(fps, fps);
		}
	}
}