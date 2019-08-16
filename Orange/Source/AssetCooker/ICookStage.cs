using System;
using System.Collections.Generic;

namespace Orange
{
	public interface ICookStage
	{
		void Action();
		IEnumerable<string> ImportedExtensions { get; }
		IEnumerable<string> BundleExtensions { get; }
		int GetOperationsCount();
	}

	public abstract class AssetCookerCookStage
	{
		public AssetCookerCookStage(AssetCooker assetCooker)
		{
			this.AssetCooker = assetCooker;
		}

		protected readonly AssetCooker AssetCooker;
	}
}
