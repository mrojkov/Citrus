using System;
using System.IO;
using Lime;
using Lemon;
using MonoMac.Foundation;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Orange
{
	class MainClass
	{
		
		public static void Main (string[] args)
		{
/*			
//			Process.Start ("./../Resources/nvcompress", "~/Documents/Background.png ~/Documents/Background.dds");			
			var input = new Nvidia.TextureTools.InputOptions ();
			var compression = new Nvidia.TextureTools.CompressionOptions ();
			var output = new Nvidia.TextureTools.OutputOptions ();
			
			//input.SetFormat (Nvidia.TextureTools.InputFormat.BGRA_8UB);
			input.SetTextureLayout (Nvidia.TextureTools.TextureType.Texture2D, 32, 32, 1);
			//input.SetMipmapGeneration (false);
			//input.SetMaxExtents (1);
			IntPtr p = Marshal.AllocHGlobal (4 * 64 * 64);
			input.SetMipmapData (p, 32, 32, 1, 1, 1);
			
			compression.SetFormat (Nvidia.TextureTools.Format.DXT5);
			compression.SetQuality (Nvidia.TextureTools.Quality.Fastest);
			output.SetOutputHeader (false);
			output.SetFileName ("/Users/Mike/Documents/Test.dds");
			
			int[] pixels = new int [32 * 32];
			
			//unsafe {
  				//fixed (int* p = pixels) {
    				//IntPtr p1 = new IntPtr(p);
					
					var compressor = new Nvidia.TextureTools.Compressor ();
					var size = compressor.EstimateSize (input, compression);
					compressor.Compress (input, compression, output); 
			//	}
			//}
*/		
			Gtk.Application.Init ();
			MainDialog dlg = new MainDialog ();
			dlg.Run ();
		}
	}
}
