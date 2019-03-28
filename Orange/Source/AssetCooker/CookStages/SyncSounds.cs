using System;
using System.IO;
using Lime;

namespace Orange
{
	class SyncSounds : CookStage
	{
		public SyncSounds() : base()
		{

		}

		protected override void SetExtensions()
		{
			Extensions = new string[] { ".ogg", ".sound" };
			ImportedExtension = Extensions[0];
			ExportedExtension = Extensions[1];
		}

		public override void Action()
		{
			SyncUpdated.Sync(ImportedExtension, ExportedExtension, AssetBundle.Current, Converter);
		}

		private bool Converter(string srcPath, string dstPath)
		{
			using (var stream = new FileStream(srcPath, FileMode.Open)) {
				// All sounds below 100kb size (can be changed with cooking rules) are converted
				// from OGG to Wav/Adpcm
				var rules = AssetCooker.CookingRulesMap[srcPath];
				if (stream.Length > rules.ADPCMLimit * 1024) {
					AssetCooker.AssetBundle.ImportFile(dstPath, stream, 0, ImportedExtension, File.GetLastWriteTime(srcPath),
						AssetAttributes.None, AssetCooker.CookingRulesMap[srcPath].SHA1);
				}
				else {
					Console.WriteLine("Converting sound to ADPCM/IMA4 format...");
					using (var input = new OggDecoder(stream)) {
						using (var output = new MemoryStream()) {
							WaveIMA4Converter.Encode(input, output);
							output.Seek(0, SeekOrigin.Begin);
							AssetCooker.AssetBundle.ImportFile(dstPath, output, 0, ImportedExtension, File.GetLastWriteTime(srcPath),
								AssetAttributes.None, AssetCooker.CookingRulesMap[srcPath].SHA1);
						}
					}
				}
				return true;
			}
		}
	}
}
