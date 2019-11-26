using System.Threading;
using Yuzu;
using Yuzu.Json;

namespace Tangerine.Core
{
	public class TangerinePersistence
	{
		public static Lime.InternalPersistence Instance => threadLocalInstance.Value;
		private static readonly ThreadLocal<Lime.InternalPersistence> threadLocalInstance = new ThreadLocal<Lime.InternalPersistence>(() => new Lime.InternalPersistence(
			new CommonOptions {
				TagMode = TagMode.Aliases,
				AllowEmptyTypes = true,
				CheckForEmptyCollections = true,
				AllowUnknownFields = true,
			},
			new JsonSerializeOptions {
				ArrayLengthPrefix = false,
				Indent = "\t",
				FieldSeparator = "\n",
				SaveRootClass = true,
				Unordered = true,
				MaxOnelineFields = 8,
				BOM = true,
			}
		));
	}
}
