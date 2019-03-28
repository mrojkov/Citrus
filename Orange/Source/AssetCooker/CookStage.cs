using System;

namespace Orange
{
	public interface ICookStage
	{
		void Action();
		string[] Extensions { get; }
		string ImportedExtension { get; }
		string ExportedExtension { get; }
	}

	public abstract class CookStage: ICookStage
	{
		public string[] Extensions { get; protected set; }
		public string ImportedExtension { get; protected set; }
		public string ExportedExtension { get; protected set; }

		public CookStage()
		{
			SetExtensions();
		}

		public abstract void Action();

		/// <summary>
		/// Order of extensions in Extensions list matters strongly. Do not change in mindlessly
		/// </summary>
		protected abstract void SetExtensions();
	}
}
